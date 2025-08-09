using MySqlX.XDevAPI.Common;
using Org.BouncyCastle.Crypto.Agreement.Srp;
using Org.BouncyCastle.Utilities;

using Solnet.Programs;
using Solnet.Programs.Models.TokenProgram;
using Solnet.Rpc;
using Solnet.Rpc.Builders;
using Solnet.Rpc.Models;
using Solnet.Wallet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using YC.Common.ShareUtils;
using YC.SolanaSdkService;
using YC.SolanaSdkService.DTO;

namespace SolanaTest
{
    public class TokenLockingServiceTest
    {
        public Wallet _ownsWallet { get; set; }//测试使用的钱包

        public Wallet _secondWallet { get; set; }//测试钱包2
        public Wallet _threeWallet { get; set; }//测试钱包3
        public IRpcClient _rpcClient { get; set; }//测试请求的PRC

        public TokenLockingServiceTest()
        {
            string filePath = AppDomain.CurrentDomain.BaseDirectory + "\\Resources\\walletInfo.txt";
            string filePath2 = AppDomain.CurrentDomain.BaseDirectory + "\\Resources\\walletInfo2.txt";
            string filePath3 = AppDomain.CurrentDomain.BaseDirectory + "\\Resources\\walletInfo3.txt";
            _ownsWallet = BaseService.InitWallet(filePath);
            _secondWallet = BaseService.InitWallet(filePath2);
            _threeWallet = BaseService.InitWallet(filePath3);
            _rpcClient = ClientFactory.GetClient(Cluster.DevNet);//定义全局RPC
        }

        #region 第三个钱包多签测试

        /// 流程1. 初始化一个钱包，往里面打入lamports和指定token
        /// 流程2. 先使用普通交易，确认交易正常
        /// 流程3. 将指定签名账户作为签名账户，然后锁定测试交易是否ok（其他方不签名）
        /// 流程4. 使用其他方一起签名，看一下交易是否成功


        /// <summary>
        /// 给初始化钱包 打入指定lamport 用于测试交易所需
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task InitWalletLamportsTest()
        {
            var walletLamportsBalance = await BaseService.GetAccountLamportsBalanceAsync(_threeWallet.Account);
            var result = await BaseService.TransferLamportsAsync(_ownsWallet.Account, _threeWallet.GetAccount(1233), 100000000, "给新的钱包充值lamports=10000000");
            await Task.Delay(5000);
           
        }

        /// <summary>
        /// 针对指定token 操作，转入指定得到Token
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task InitWalletCustomTokenTest()
        {
            var mintAccount = _ownsWallet.GetAccount(322);//用于铸造币的用户
            var ownerAccount = _ownsWallet.Account;//管理授权用户
            var initalAccount = _ownsWallet.GetAccount(321);//代币用户

            var threeTokenAccount = _threeWallet.GetAccount(10);
            var payAccount = _threeWallet.Account;
            var sourceAssociatedTokenAccount = await BaseService.GetAssociatedTokenAccountAsync(ownerAccount, mintAccount.PublicKey, ownerAccount);
            var destinationAssociatedTokenAccount = await BaseService.GetAssociatedTokenAccountAsync(_threeWallet.Account, mintAccount.PublicKey, payAccount);
            var sourcetokenBalanceRes1 = await BaseService.GetTokenBalanceAsync(initalAccount);//通过直接账户,铸币时候使用
            var sourcetokenBalanceRes2 = await BaseService.GetTokenBalanceAsync(sourceAssociatedTokenAccount.Data);//通过关联代币账户。这个账户没有token

            var destinationBalanceRes1 = await BaseService.GetTokenBalanceAsync(threeTokenAccount);//通过普通账户
            if (destinationBalanceRes1.Message == "Invalid param: could not find account")
            {
                var createdRes = await BaseService.CreateAccountByRegisteAsync(payAccount, threeTokenAccount, mintAccount);
                destinationBalanceRes1 = await BaseService.GetTokenBalanceAsync(threeTokenAccount);//通过普通账户
            }
            var destinationBalanceRes2 = await BaseService.GetTokenBalanceAsync(destinationAssociatedTokenAccount.Data);//通过关联代币账户。这个账户没有token

            if (destinationBalanceRes2.Data.AmountDouble <= 0)
            {
                //转账平常要用的再打开，只是查询时候就不要打开
                var result1 = await SPLTokenService.TransferSPLTokenAsync(
                     mintAccount, ownerAccount,
                     initalAccount,
                     destinationAssociatedTokenAccount.Data, 100000);
            }
            if (destinationBalanceRes1.Data.AmountDouble <= 10000000)
            {
                var result2 = await SPLTokenService.TransferSPLTokenAsync(
                mintAccount, ownerAccount,
                initalAccount,
                threeTokenAccount, 10000000);
            }
            await Task.Delay(5000);

        }

        /// <summary>
        /// 将第三个钱包指定Token 转移给李四
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task ThreeWalletTransTokenTest()
        {
            var mintAccount = _ownsWallet.GetAccount(322);//用于铸造币的用户
            var ownerAccount = _ownsWallet.Account;//管理授权用户
            var initalAccount = _ownsWallet.GetAccount(321);//代币用户

            var threeTokenAccount = _threeWallet.GetAccount(10);
            var payAccount = _threeWallet.Account;
            // 获取李四的关联代币账户
            var lisiAssociatedTokenAccount = AssociatedTokenAccountProgram.DeriveAssociatedTokenAccount(_secondWallet.Account.PublicKey, mintAccount.PublicKey);
            var getLisiAtaInfo = await _rpcClient.GetAccountInfoAsync(lisiAssociatedTokenAccount);
            var lsTokenBalance = await _rpcClient.GetTokenAccountBalanceAsync(lisiAssociatedTokenAccount);
            var lsAmount = lsTokenBalance.Result.Value.AmountDouble;
            var transResult = await SPLTokenService.TransferSPLTokenAsync(mintAccount, payAccount, threeTokenAccount, lisiAssociatedTokenAccount, 500);
            var lsTokenBalance2 = await _rpcClient.GetTokenAccountBalanceAsync(lisiAssociatedTokenAccount);
        }

        /// <summary>
        /// 创建多签账户（第三个钱包版本）
        /// 要使用第三个钱包重新创建一个
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task MultisigToken1Test()
        {

            Solnet.Wallet.Account mintAccount = _ownsWallet.GetAccount(322);//用于铸造币的用户
            
            ///第三个钱包主账户
            var payerAccount = _threeWallet.Account;//管理授权用户
            // 创建三个用于多签的钱包
            var multiSignature = _threeWallet.GetAccount(61);
            var signer1 = _threeWallet.GetAccount(12);
            var signer2 = _threeWallet.GetAccount(13);
            var signer3 = _threeWallet.GetAccount(14);
            var signer4 = _threeWallet.GetAccount(15);
            var signer5 = _threeWallet.GetAccount(16);
            // 多签账户所需签名数和总签名者数
            var m = 3;
            var n = 5;
            ///多方签名集合
            List<Solnet.Wallet.PublicKey> signers = new List<Solnet.Wallet.PublicKey>() {  signer1, signer2, signer3,signer4,
                                                                                                                      signer5
            };

            ulong minBalanceForExemptionMultiSig =
                  _rpcClient.GetMinimumBalanceForRentExemption(TokenProgram.MultisigAccountDataSize).Result;

            var lastestBlock = await BaseService.GetLatestBlockHashAsync();
            var msgData = new TransactionBuilder().SetRecentBlockHash(lastestBlock.Data)
                 .SetFeePayer(payerAccount)
                 .AddInstruction(SystemProgram.CreateAccount(//和免租金额 lamports有关
                     payerAccount.PublicKey,
                     multiSignature,
                     minBalanceForExemptionMultiSig,
                     TokenProgram.MultisigAccountDataSize,
                     TokenProgram.ProgramIdKey))
                 .AddInstruction(TokenProgram.InitializeMultiSignature(
                     multiSignature.PublicKey,
                     signers,
                     m)).CompileMessage();
            Message msg = Message.Deserialize(Convert.ToBase64String(msgData));
            Transaction tx = Transaction.Populate(msg,
                new List<byte[]>
                {
                    payerAccount.Sign(msgData),
                    multiSignature.Sign(msgData),
                });

            var test = _rpcClient.SimulateTransaction(tx.Serialize());
            var createAccountSignature = await _rpcClient.SendTransactionAsync(tx.Serialize());
            Console.WriteLine($"Multisig account creation signature: {createAccountSignature.Result}");
        }

        /// <summary>
        /// 构建多签名，使用该关联账户，给多签名的关联账户打钱
        /// 【一次事务交易中构建多笔转账】(第三个钱包版本)
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task MultiSignatureAssociatedTokenTrans1Test()
        {
            Solnet.Wallet.Account mintAccount = _ownsWallet.GetAccount(322);//用于铸造币的用户

            ///第三个钱包主账户
            var payerAccount = _threeWallet.Account;//管理授权用户
            var threeTokenAccount = _threeWallet.GetAccount(10);//存储token 

            // 创建五个用于多签的账户
            var multiSignature = _threeWallet.GetAccount(61);
            var signer1 = _threeWallet.GetAccount(12);
            var signer2 = _threeWallet.GetAccount(13);
            var signer3 = _threeWallet.GetAccount(14);
            var signer4 = _threeWallet.GetAccount(15);
            var signer5 = _threeWallet.GetAccount(16);

            ///获取多方签名者的关联账户
            var multiSignatureAssociatedTokenAccount = await BaseService.GetAssociatedTokenAccountAsync(multiSignature,mintAccount,payerAccount);

            await Task.Delay(10000);//等待10秒，自动创建关联代币账户，需要网络同步时间
            var multiSignatureTokenBalance = await BaseService.GetTokenBalanceAsync(multiSignatureAssociatedTokenAccount.Data);
            var signer1TokenBalance = await BaseService.GetTokenBalanceAsync(signer1);//通过普通账户
            var signer2TokenBalance = await BaseService.GetTokenBalanceAsync(signer2);//通过普通账户
            var signer3TokenBalance = await BaseService.GetTokenBalanceAsync(signer3);//通过普通账户

            //批量给signer1，signer2，signer3 转账，但打包在一个交易里面。之后查看每个人的账户余额
            //var createdRes1 = await BaseService.CreateAccountByRegisteAsync(payerAccount, signer1, mintAccount);
            //var createdRes2 = await BaseService.CreateAccountByRegisteAsync(payerAccount, signer2, mintAccount);
            //var createdRes3 = await BaseService.CreateAccountByRegisteAsync(payerAccount, signer3, mintAccount);

            //await Task.Delay(10000);//创建注册账户需要一定的时间同步
            #region 构建转账交易
            // 创建转移 Token 的指令
            var transferInstruction = TokenProgram.Transfer(
                threeTokenAccount,
                multiSignatureAssociatedTokenAccount.Data,
                1000000,
                payerAccount);

            // 创建转移 Token 的指令
            var transferInstruction1 = TokenProgram.Transfer(
                threeTokenAccount,
                signer1,
                10000,
                payerAccount);
            var transferInstruction2 = TokenProgram.Transfer(
                threeTokenAccount,
                signer2,
                10000,
                payerAccount);
            var transferInstruction3 = TokenProgram.Transfer(
                threeTokenAccount,
                signer3,
                10000,
                payerAccount);

            // 获取最新的区块哈希
            var getLastestBlockHash = await BaseService.GetLatestBlockHashAsync();

            // 构建转移交易（一次性转三笔）
            var msgData = new TransactionBuilder()
               .AddInstruction(transferInstruction)
               .AddInstruction(transferInstruction1)
               .AddInstruction(transferInstruction2)
               .AddInstruction(transferInstruction3)
               .SetRecentBlockHash(getLastestBlockHash.Data)
               .SetFeePayer(payerAccount.PublicKey);
            byte[] signData = msgData.Build(new List<Solnet.Wallet.Account>() { payerAccount });

            // 发送转移交易
            var transferSignature = _rpcClient.SendTransaction(signData);
           
            #endregion

        }

        /// <summary>
        /// 验证 multiSignature 在没有其他人交易签名情况下，是否可以转账成功？
        /// 虽然是使用了关联代币账户
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task MultiSignatureTrans1Test()
        {
            Solnet.Wallet.Account mintAccount = _ownsWallet.GetAccount(322);//用于铸造币的用户

            ///第三个钱包主账户
            var payerAccount = _threeWallet.Account;//管理授权用户
            var threeTokenAccount = _threeWallet.GetAccount(10);//存储token 

            var multiSignature = _threeWallet.GetAccount(31);
            var signer1 = _threeWallet.GetAccount(12);
            var signer2 = _threeWallet.GetAccount(13);
            var signer3 = _threeWallet.GetAccount(14);
            var signer4 = _threeWallet.GetAccount(15);
            var signer5 = _threeWallet.GetAccount(16);
            ///多方签名集合{需要验证签名 在满足m的情况，传入签名个数和后面的签名要一一对应，网络是通过传入签名去验签的}
            List<Solnet.Wallet.PublicKey> signers = new List<Solnet.Wallet.PublicKey>() {multiSignature, signer1, signer2, signer3,signer4, signer5 
            };
            var lisiAssociatedTokenAccount = AssociatedTokenAccountProgram.DeriveAssociatedTokenAccount(_secondWallet.Account.PublicKey, mintAccount.PublicKey);
            ///获取多方签名者的关联账户
            var multiSignatureAssociatedTokenAccount = await BaseService.GetAssociatedTokenAccountAsync(multiSignature, mintAccount, payerAccount);
            var multiSignatureTokenBalance = await BaseService.GetTokenBalanceAsync(multiSignatureAssociatedTokenAccount.Data);
            #region 构建转账交易

            // 创建转移 Token 的指令
            var transferInstruction = TokenProgram.Transfer(
                multiSignatureAssociatedTokenAccount.Data,
                lisiAssociatedTokenAccount,
                1000,
                multiSignature,signers);
            
            // 获取最新的区块哈希
            var getLastestBlockHash = await BaseService.GetLatestBlockHashAsync();

            // 构建转移交易
            var msgData = new TransactionBuilder()
               .AddInstruction(transferInstruction)
               .SetRecentBlockHash(getLastestBlockHash.Data)
               .SetFeePayer(payerAccount.PublicKey);
            byte[] signData = msgData.Build(new List<Solnet.Wallet.Account>() { payerAccount, signer1, signer2, signer3});

            // 发送转移交易
            var transferSignature = _rpcClient.SendTransaction(signData);

            #endregion

        }

        [Fact]
        public async Task GetMultiSignAccountTest() {
            var multiSignature = _threeWallet.GetAccount(11);
            var multiSigAccountInfo = await _rpcClient.GetAccountInfoAsync(multiSignature.PublicKey);
            if (multiSigAccountInfo.WasSuccessful)
            {
                var multiSigData = MultiSignatureAccount.Deserialize(Convert.FromBase64String(multiSigAccountInfo.Result.Value.Data[0]));
                //Console.WriteLine($"阈值: {multiSigData.}");
                Console.WriteLine("签名者列表:");
                foreach (var signer in multiSigData.Signers)
                {
                    Console.WriteLine(signer);
                }
            }
        }

        [Fact]
        public async Task MultisigTrans1Test()
        {
            Solnet.Wallet.Account mintAccount = _ownsWallet.GetAccount(322);//用于铸造币的用户
            Solnet.Wallet.Account ownsAccount = _ownsWallet.Account;//用于管理的用户
            ///第三个钱包主账户
            var payerAccount = _threeWallet.Account;//管理授权用户
            var threeTokenAccount = _threeWallet.GetAccount(10);//存储token 
            var payerAccountBalance = await BaseService.GetAccountLamportsBalanceAsync(payerAccount);

            // 创建五个用于多签的账户
            var multiSignature = _threeWallet.GetAccount(61);
            var signer1 = _threeWallet.GetAccount(12);
            var signer2 = _threeWallet.GetAccount(13);
            var signer3 = _threeWallet.GetAccount(14);
            var signer4 = _threeWallet.GetAccount(15);
            var signer5 = _threeWallet.GetAccount(16);

            var signer1TokenBalance = await BaseService.GetTokenBalanceAsync(signer1);//通过普通账户
            var signer2TokenBalance = await BaseService.GetTokenBalanceAsync(signer2);//通过普通账户
            var signer3TokenBalance = await BaseService.GetTokenBalanceAsync(signer3);//通过普通账户
            
            ///multiSignature 关联代币账户的余额查询
            var multiSignatureAssociatedTokenAccount = await BaseService.GetAssociatedTokenAccountAsync(multiSignature, mintAccount, payerAccount);
            var multiSignatureTokenBalance = await BaseService.GetTokenBalanceAsync(multiSignatureAssociatedTokenAccount.Data);

            ///lisi 关联代币账户的余额查询
            var lisiAssociatedTokenAccount = AssociatedTokenAccountProgram.DeriveAssociatedTokenAccount(_secondWallet.Account.PublicKey, mintAccount.PublicKey);
            var lsTokenBalance = await _rpcClient.GetTokenAccountBalanceAsync(lisiAssociatedTokenAccount);
            ///多方签名集合{需要验证签名 在满足m的情况，传入签名个数和后面的签名要一一对应，网络是通过传入签名去验签的}
            List<Solnet.Wallet.PublicKey> signers = new List<Solnet.Wallet.PublicKey>() {

                //signer1,
                signer2,
                signer3,
                signer4,
                //signer5
            };

            ulong amount = 1000;
            try
            {
                #region 接口调用版本
                // 关联账户的交易，需要钱包主账户支付Fee
                var waitSign = await SPLTokenService.BuildTransferSPLTokenWaitMultiSignAsync(payerAccount,
                    multiSignatureAssociatedTokenAccount.Data,
                    lisiAssociatedTokenAccount,
                    amount,
                    multiSignature,
                    signers);
                List<byte[]> signatures = new List<byte[]>();
                signatures.Add(payerAccount.Sign(waitSign.Data));
                //signatures.Add(multiSignature.Sign(waitSign.Data));
                //signatures.Add(signer1.Sign(waitSign.Data));//获得各自的签名
                signatures.Add(signer2.Sign(waitSign.Data));//获得各自的签名
                signatures.Add(signer3.Sign(waitSign.Data));
                signatures.Add(signer4.Sign(waitSign.Data));//获得各自的签名
                //signatures.Add(signer5.Sign(waitSign.Data));//获得各自的签名
              
                Solnet.Rpc.Models.Message msg = Solnet.Rpc.Models.Message.Deserialize(Convert.ToBase64String(waitSign.Data));
                payerAccount.Verify(waitSign.Data, payerAccount.Sign(waitSign.Data));
                var tx = Transaction.Populate(msg, signatures);
                var t = tx.VerifySignatures();
                var resut = await SPLTokenService.TransferSPLTokenByMultiSignAsync(waitSign.Data, signatures, payerAccount);

                ///2,3,5 组合成功
                //1,3,5 组合成功，先选择 m个数的各种组合，看谁可以成功
                #endregion
                #region 测试，不知道成功失败
                // 创建转移 Token 的指令
                // var transferInstruction = TokenProgram.Transfer(
                //     multiSignatureAssociatedTokenAccount.Data,
                //     lisiAssociatedTokenAccount,
                //     amount,
                //     multiSignature, signers);
                // var getLastestBlockHash = await  BaseService.GetLatestBlockHashAsync();
                // Transaction tx = new()
                // {
                //     FeePayer = payerAccount,
                //     RecentBlockHash = getLastestBlockHash.Data
                // };
                // tx.Add(transferInstruction);
                //var msgData= tx.CompileMessage();
                // //tx.PartialSign(new List<Solnet.Wallet.Account> { payerAccount,multiSignature, signer1, signer2, signer3 });
                // tx.AddSignature(payerAccount.PublicKey, payerAccount.Sign(msgData));
                // //tx.AddSignature(multiSignature.PublicKey, multiSignature.Sign(msgData));
                // tx.AddSignature(signer1.PublicKey, signer1.Sign(msgData));
                // //tx.AddSignature(signer2.PublicKey, signer2.Sign(msgData));
                // tx.AddSignature(signer3.PublicKey, signer3.Sign(msgData));
                // tx.AddSignature(signer4.PublicKey, signer4.Sign(msgData));
                // //tx.AddSignature(signer5.PublicKey, signer5.Sign(msgData));
                // var vts= tx.VerifySignatures();
                // byte[] serializedBytes = tx.Serialize();
                // var test = _rpcClient.SimulateTransaction(serializedBytes);

                // // 发送转移交易
                // var transferSignature = _rpcClient.SendTransaction(serializedBytes); 
                #endregion

            }
            catch (Exception ex)
            {

                throw;
            }

        }



        #endregion



        /// <summary>
        /// 创建多签账户
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task MultisigTokenTest()
        {

            // 张三账户
            Solnet.Wallet.Account mintAccount = _ownsWallet.GetAccount(322);//用于铸造币的用户
            var ownerAccount = _ownsWallet.Account;//管理授权用户

            var haveAccount = _ownsWallet.GetAccount(321);//代币用户;
            // 创建三个用于多签的钱包
            var multiSignature = _ownsWallet.GetAccount(1121);
            var signer2 = _ownsWallet.GetAccount(1115);
            var signer3 = _ownsWallet.GetAccount(1116);
            //var signer4 = _ownsWallet.GetAccount(1117);
            //var signer5 = _ownsWallet.GetAccount(1118);
            //var signer6 = _ownsWallet.GetAccount(1119);

            // 多签账户所需签名数和总签名者数
            var m = 2;
            var n = 5;
            ///多方签名集合
            List<Solnet.Wallet.PublicKey> signers = new List<Solnet.Wallet.PublicKey>() {signer2, signer3,// signer4, signer5, signer6
            };

            ulong minBalanceForExemptionMultiSig =
                  _rpcClient.GetMinimumBalanceForRentExemption(TokenProgram.MultisigAccountDataSize).Result;

            var lastestBlock = await BaseService.GetLatestBlockHashAsync();
            var msgData = new TransactionBuilder().SetRecentBlockHash(lastestBlock.Data)
                 .SetFeePayer(ownerAccount)
                 .AddInstruction(SystemProgram.CreateAccount(
                     ownerAccount.PublicKey,
                     multiSignature,
                     minBalanceForExemptionMultiSig,
                     TokenProgram.MultisigAccountDataSize,
                     TokenProgram.ProgramIdKey))
                 .AddInstruction(TokenProgram.InitializeMultiSignature(
                     multiSignature.PublicKey,
                     signers,
                     m)).CompileMessage();
            Message msg = Message.Deserialize(Convert.ToBase64String(msgData));
            Transaction tx = Transaction.Populate(msg,
                new List<byte[]>
                {
                    ownerAccount.Sign(msgData),
                    multiSignature.Sign(msgData),
                });

            var test = _rpcClient.SimulateTransaction(tx.Serialize());
            var createAccountSignature = await _rpcClient.SendTransactionAsync(tx.Serialize());
            Console.WriteLine($"Multisig account creation signature: {createAccountSignature.Result}");
        }

        /// <summary>
        /// 构建多签名，使用该关联账户，给多签名的关联账户打钱
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task MultiSignatureAssociatedTokenTransTest()
        {
            // 张三账户
            Solnet.Wallet.Account mintAccount = _ownsWallet.GetAccount(322);//用于铸造币的用户
            Solnet.Wallet.Account ownerAccount = _ownsWallet.GetAccount(0);//管理员用户
            var zhangsanAssociatedTokenAccount = AssociatedTokenAccountProgram.DeriveAssociatedTokenAccount(_ownsWallet.Account.PublicKey, mintAccount.PublicKey);
            Solnet.Wallet.Account initalAccount = _ownsWallet.GetAccount(321);//代币用户
            var multiSignature = _ownsWallet.GetAccount(1121);

            var multiSignatureAssociatedTokenAccount = AssociatedTokenAccountProgram.DeriveAssociatedTokenAccount(multiSignature.PublicKey, mintAccount.PublicKey);

            var getMultiSiAtaInfo = await _rpcClient.GetAccountInfoAsync(multiSignatureAssociatedTokenAccount);
            if (getMultiSiAtaInfo.Result.Value == null)
            {
                var multiSigA = await BaseService.GetAssociatedTokenAccountAsync(multiSignature, mintAccount, ownerAccount);
                var multiSiTokenBalance1 = await _rpcClient.GetTokenAccountBalanceAsync(multiSigA.Data);
            }
            //交易共识需要时间，要等
            var res = await SPLTokenService.TransferSPLTokenAsync(mintAccount, ownerAccount, initalAccount, multiSignatureAssociatedTokenAccount, 100000);
            var multiSiTokenBalance = await _rpcClient.GetTokenAccountBalanceAsync(multiSignatureAssociatedTokenAccount);
        }

        [Fact]
        public async Task MultisigTrans3Test()
        {
            // 张三账户
            Solnet.Wallet.Account mintAccount = _ownsWallet.GetAccount(322);//用于铸造币的用户
            Solnet.Wallet.Account ownerAccount = _ownsWallet.GetAccount(0);//管理员用户
            Solnet.Wallet.Account initalAccount = _ownsWallet.GetAccount(321);//代币用户

            var multiSignature = _ownsWallet.GetAccount(1120);
            var signer2 = _ownsWallet.GetAccount(1115);
            var signer3 = _ownsWallet.GetAccount(1116);
            var signer4 = _ownsWallet.GetAccount(1117);
            var signer5 = _ownsWallet.GetAccount(1118);
            var signer6 = _ownsWallet.GetAccount(1119);

            var multiSignLamportsBalance = await BaseService.GetAccountLamportsBalanceAsync(multiSignature);
            ulong minBalanceForExemptionMultiSig =
                  _rpcClient.GetMinimumBalanceForRentExemption(TokenProgram.MultisigAccountDataSize).Result;
            if (multiSignLamportsBalance.Data < minBalanceForExemptionMultiSig)
            {
                var transferLamportRes = await BaseService.TransferLamportsAsync(ownerAccount, multiSignature, 10000000);
            }

            ///多方签名集合
            List<Solnet.Wallet.PublicKey> signers = new List<Solnet.Wallet.PublicKey>() { 
                signer2, signer3, 
                signer4,
                //signer5,
                //signer6
            };

            ///获取多方签名者的关联账户
            var multiSignatureAssociatedTokenAccount = AssociatedTokenAccountProgram.DeriveAssociatedTokenAccount(multiSignature.PublicKey, mintAccount.PublicKey);

            // 获取李四的关联代币账户
            var lisiAssociatedTokenAccount = AssociatedTokenAccountProgram.DeriveAssociatedTokenAccount(_secondWallet.Account.PublicKey, mintAccount.PublicKey);
            var getLisiAtaInfo = await _rpcClient.GetAccountInfoAsync(lisiAssociatedTokenAccount);
            var lsTokenBalance = await _rpcClient.GetTokenAccountBalanceAsync(lisiAssociatedTokenAccount);
            #region 处理关联账户
            if (getLisiAtaInfo?.Result?.Value == null)
            {
                // 如果李四的关联代币账户不存在，创建它
                var createAtaInstruction = AssociatedTokenAccountProgram.CreateAssociatedTokenAccount(
                     _secondWallet.Account.PublicKey,
                     _secondWallet.Account.PublicKey,
                    mintAccount.PublicKey
                );
            }
            #endregion 
            ulong amount = 1000;
            try
            {
                ///关联账户的交易，需要钱包主账户支付Fee
                var waitSign = await SPLTokenService.BuildTransferSPLTokenWaitMultiSignAsync(ownerAccount, multiSignatureAssociatedTokenAccount, lisiAssociatedTokenAccount, amount, multiSignature, signers);

                //2,3,4 可以
                //2,3,6 可以
                //2,3,5 不可以
                //4,5,6 就不可以

                List<byte[]> signatures = new List<byte[]>();
                signatures.Add(ownerAccount.Sign(waitSign.Data));
                ///signatures.Add(multiSignature.Sign(waitSign.Data));
                signatures.Add(signer2.Sign(waitSign.Data));//获得各自的签名
                signatures.Add(signer3.Sign(waitSign.Data));
               signatures.Add(signer4.Sign(waitSign.Data));//获得各自的签名
                //signatures.Add(signer5.Sign(waitSign.Data));
                //signatures.Add(signer6.Sign(waitSign.Data));//获得各自的签名

                var resut = await SPLTokenService.TransferSPLTokenByMultiSignAsync(waitSign.Data, signatures);
            }
            catch (Exception ex)
            {

                throw;
            }

        }




        #region 测试封存版本，成功的版本
        ///// <summary>
        ///// 创建多签账户
        ///// </summary>
        ///// <returns></returns>
        //[Fact]
        //public async Task MultisigTokenTest()
        //{

        //    // 张三账户
        //    Solnet.Wallet.Account mintAccount = _ownsWallet.GetAccount(322);//用于铸造币的用户
        //    var ownerAccount = _ownsWallet.Account;//管理授权用户

        //    var haveAccount = _ownsWallet.GetAccount(321);//代币用户;
        //    // 创建三个用于多签的钱包
        //    var multiSignature = _ownsWallet.GetAccount(1114);
        //    var signer2 = _ownsWallet.GetAccount(1115);
        //    var signer3 = _ownsWallet.GetAccount(1116);

        //    // 多签账户所需签名数和总签名者数
        //    var m = 2;
        //    var n = 3;
        //    ///多方签名集合
        //    List<Solnet.Wallet.PublicKey> signers = new List<Solnet.Wallet.PublicKey>() { multiSignature, signer2, signer3 };

        //    ulong minBalanceForExemptionMultiSig =
        //          _rpcClient.GetMinimumBalanceForRentExemption(TokenProgram.MultisigAccountDataSize).Result;

        //    var lastestBlock = await BaseService.GetLatestBlockHashAsync();
        //    var msgData = new TransactionBuilder().SetRecentBlockHash(lastestBlock.Data)
        //         .SetFeePayer(ownerAccount)
        //         .AddInstruction(SystemProgram.CreateAccount(
        //             ownerAccount.PublicKey,
        //             multiSignature,
        //             minBalanceForExemptionMultiSig,
        //             TokenProgram.MultisigAccountDataSize,
        //             TokenProgram.ProgramIdKey))
        //         .AddInstruction(TokenProgram.InitializeMultiSignature(
        //             multiSignature.PublicKey,
        //             signers,
        //             m)).CompileMessage();
        //    Message msg = Message.Deserialize(Convert.ToBase64String(msgData));
        //    Transaction tx = Transaction.Populate(msg,
        //        new List<byte[]>
        //        {
        //            ownerAccount.Sign(msgData),
        //            multiSignature.Sign(msgData),
        //        });

        //    var test = _rpcClient.SimulateTransaction(tx.Serialize());
        //    var createAccountSignature = await _rpcClient.SendTransactionAsync(tx.Serialize());
        //    Console.WriteLine($"Multisig account creation signature: {createAccountSignature.Result}");
        //}

        ///// <summary>
        ///// 构建多签名，使用该关联账户，给多签名的关联账户打钱
        ///// </summary>
        ///// <returns></returns>
        //[Fact]
        //public async Task MultiSignatureAssociatedTokenTransTest()
        //{
        //    // 张三账户
        //    Solnet.Wallet.Account mintAccount = _ownsWallet.GetAccount(322);//用于铸造币的用户
        //    Solnet.Wallet.Account ownerAccount = _ownsWallet.GetAccount(0);//管理员用户
        //    var zhangsanAssociatedTokenAccount = AssociatedTokenAccountProgram.DeriveAssociatedTokenAccount(_ownsWallet.Account.PublicKey, mintAccount.PublicKey);
        //    Solnet.Wallet.Account initalAccount = _ownsWallet.GetAccount(321);//代币用户
        //    var multiSignature = _ownsWallet.GetAccount(1114);

        //    var multiSignatureAssociatedTokenAccount = AssociatedTokenAccountProgram.DeriveAssociatedTokenAccount(multiSignature.PublicKey, mintAccount.PublicKey);

        //    var getMultiSiAtaInfo = await _rpcClient.GetAccountInfoAsync(multiSignatureAssociatedTokenAccount);
        //    if (getMultiSiAtaInfo.Result.Value == null)
        //    {
        //        var multiSigA = await BaseService.GetAssociatedTokenAccountAsync(multiSignature, mintAccount, ownerAccount);
        //        var multiSiTokenBalance1 = await _rpcClient.GetTokenAccountBalanceAsync(multiSigA.Data);
        //    }

        //    var res = await SPLTokenService.TransferSPLTokenAsync(mintAccount, ownerAccount, initalAccount, multiSignatureAssociatedTokenAccount, 100000);
        //    var multiSiTokenBalance = await _rpcClient.GetTokenAccountBalanceAsync(multiSignatureAssociatedTokenAccount);
        //}

        [Fact]
        public async Task MultisigTransTest()
        {
            // 张三账户
            Solnet.Wallet.Account mintAccount = _ownsWallet.GetAccount(322);//用于铸造币的用户
            Solnet.Wallet.Account ownerAccount = _ownsWallet.GetAccount(0);//管理员用户
            Solnet.Wallet.Account initalAccount = _ownsWallet.GetAccount(321);//代币用户

            var multiSignature = _ownsWallet.GetAccount(1114);

            var multiSignLamportsBalance = await BaseService.GetAccountLamportsBalanceAsync(multiSignature);
            ulong minBalanceForExemptionMultiSig =
                  _rpcClient.GetMinimumBalanceForRentExemption(TokenProgram.MultisigAccountDataSize).Result;
            if (multiSignLamportsBalance.Data < minBalanceForExemptionMultiSig)
            {
                var transferLamportRes = await BaseService.TransferLamportsAsync(ownerAccount, multiSignature, 10000000);
            }
            var signer2 = _ownsWallet.GetAccount(1115);
            var signer3 = _ownsWallet.GetAccount(1116);

            ///多方签名集合
            List<Solnet.Wallet.PublicKey> signers = new List<Solnet.Wallet.PublicKey>() { multiSignature, signer2, signer3 };

            ///获取多方签名者的关联账户
            var multiSignatureAssociatedTokenAccount = AssociatedTokenAccountProgram.DeriveAssociatedTokenAccount(multiSignature.PublicKey, mintAccount.PublicKey);

            // 获取李四的关联代币账户
            var lisiAssociatedTokenAccount = AssociatedTokenAccountProgram.DeriveAssociatedTokenAccount(_secondWallet.Account.PublicKey, mintAccount.PublicKey);
            var getLisiAtaInfo = await _rpcClient.GetAccountInfoAsync(lisiAssociatedTokenAccount);
            var lsTokenBalance = await _rpcClient.GetTokenAccountBalanceAsync(lisiAssociatedTokenAccount);
            #region 处理关联账户
            if (getLisiAtaInfo.Result.Value == null)
            {
                // 如果李四的关联代币账户不存在，创建它
                var createAtaInstruction = AssociatedTokenAccountProgram.CreateAssociatedTokenAccount(
                     _secondWallet.Account.PublicKey,
                     _secondWallet.Account.PublicKey,
                    mintAccount.PublicKey
                );
            }
            #endregion 
            ulong amount = 1000;
            try
            {
                ///关联账户的交易，需要钱包主账户支付Fee
                var waitSign = await SPLTokenService.BuildTransferSPLTokenWaitMultiSignAsync(ownerAccount, multiSignatureAssociatedTokenAccount, lisiAssociatedTokenAccount, amount, multiSignature, signers);

                List<byte[]> signatures = new List<byte[]>();
                signatures.Add(ownerAccount.Sign(waitSign.Data));
                signatures.Add(multiSignature.Sign(waitSign.Data));
                signatures.Add(signer2.Sign(waitSign.Data));//获得各自的签名
                signatures.Add(signer3.Sign(waitSign.Data));

                var resut = await SPLTokenService.TransferSPLTokenByMultiSignAsync(waitSign.Data, signatures);
            }
            catch (Exception ex)
            {

                throw;
            }

        }
        #endregion



    }
}
