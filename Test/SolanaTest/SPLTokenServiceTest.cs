using Solnet.Extensions.TokenMint;
using Solnet.Programs;
using Solnet.Rpc;
using Solnet.Rpc.Builders;
using Solnet.Wallet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YC.Common.ShareUtils;
using YC.SolanaSdkService;
using YC.SolanaSdkService.DTO;

namespace SolanaTest
{

    public class SPLTokenServiceTest
    {
        public Wallet _ownsWallet { get; set; }//测试使用的钱包

        public Wallet _secondWallet { get; set; }//测试钱包2
        public IRpcClient _rpcClient { get; set; }//测试请求的PRC

        public SPLTokenServiceTest()
        {

            string filePath = AppDomain.CurrentDomain.BaseDirectory + "\\Resources\\walletInfo.txt";
            string walletInfo = "";
            FileUtils.ReadFile(filePath, out walletInfo);
            Assert.True(!string.IsNullOrEmpty(walletInfo));

            string filePath2 = AppDomain.CurrentDomain.BaseDirectory + "\\Resources\\walletInfo2.txt";
            string walletInfo2 = "";
            FileUtils.ReadFile(filePath2, out walletInfo2);
            Assert.True(!string.IsNullOrEmpty(walletInfo2));


            var tempWalletObj = walletInfo.ToObject<WalletDto>();//不能直接转为wallet 对象
            var tempWalletObj2 = walletInfo2.ToObject<WalletDto>();//不能直接转为wallet 对象
                                                                   // To initialize a wallet and have access to the same keys generated in sollet (the default)
            _ownsWallet = new Wallet(string.Join(" ", tempWalletObj.Mnemonic.Words), Solnet.Wallet.Bip39.WordList.English);
            _secondWallet = new Wallet(string.Join(" ", tempWalletObj2.Mnemonic.Words), Solnet.Wallet.Bip39.WordList.English);

            _rpcClient = ClientFactory.GetClient(Cluster.DevNet);//定义全局RPC

        }
        /// <summary>
        /// 铸造代币
        /// </summary>
        [Fact]
        public async Task CreateTokenTest()
        {
            CreateTokenDto createTokenDto = new CreateTokenDto();
            createTokenDto.Decimals=5;
            createTokenDto.Uri = "";
            createTokenDto.TokenSupply = 20000000000000;
            createTokenDto.TokenName = "ADC1的个人MeMe代币";
            createTokenDto.TokenSymbol = "ADC-MeMe";
            createTokenDto.MemoString = $"铸造名称为:'{createTokenDto.TokenName}'，" +
                    $"代币标识为:'{createTokenDto.TokenSymbol}'的SPLToken。";
            createTokenDto.PayAccount = _ownsWallet.Account;
            
            createTokenDto.MintAccount = _ownsWallet.GetAccount(212121211);
            createTokenDto.IsStorageAssociatedAccount = true;
            createTokenDto.StorageTokenAccount = _ownsWallet.GetAccount(6612);
           var result=await SPLTokenService.CreateTokenAsync(createTokenDto);
            Assert.True(result.State);
        }


        /// <summary>
        /// 交易测试
        /// </summary>
        [Fact]
        public async Task TransferTokenTest()
        {
            // 张三账户
            Solnet.Wallet.Account mintAccount = _ownsWallet.GetAccount(322);//用于铸造币的用户
            Console.WriteLine($"张三 铸造账户MintAccount: {mintAccount}");
            Solnet.Wallet.Account ownerAccount = _ownsWallet.GetAccount(0);//管理员用户
            Console.WriteLine($"张三 管理账户OwnerAccount: {ownerAccount}");
            var zhangsanAssociatedTokenAccount = AssociatedTokenAccountProgram.DeriveAssociatedTokenAccount(_ownsWallet.Account.PublicKey, mintAccount.PublicKey);
            Solnet.Wallet.Account initalAccount = _ownsWallet.GetAccount(321);//代币用户
            Console.WriteLine($"张三 关联自定义Token的InitialAccount: {initalAccount}");

            // 获取李四的关联代币账户
            var lisiAssociatedTokenAccount = AssociatedTokenAccountProgram.DeriveAssociatedTokenAccount(_secondWallet.Account.PublicKey, mintAccount.PublicKey);

            var getLisiAtaInfo = await _rpcClient.GetAccountInfoAsync(lisiAssociatedTokenAccount);
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
            ulong amount = 1000000;
            try
            {
                var resut = await SPLTokenService.TransferSPLTokenAsync(mintAccount, ownerAccount, initalAccount, lisiAssociatedTokenAccount, amount);

            }
            catch (Exception ex)
            {

                throw;
            }
        }


        /// <summary>
        /// 将自定义的代币转移给其他用户,测算交易费用
        /// </summary>
        [Fact]
        public async void CaculationTransFeeTest()
        {
            // 张三账户
            Solnet.Wallet.Account mintAccount = _ownsWallet.GetAccount(322);//用于铸造币的用户
            Console.WriteLine($"张三 铸造账户MintAccount: {mintAccount}");
            Solnet.Wallet.Account ownerAccount = _ownsWallet.GetAccount(0);//管理员用户
            Console.WriteLine($"张三 管理账户OwnerAccount: {ownerAccount}");
            var zhangsanAssociatedTokenAccount = AssociatedTokenAccountProgram.DeriveAssociatedTokenAccount(_ownsWallet.Account.PublicKey, mintAccount.PublicKey);
            Solnet.Wallet.Account initalAccount = _ownsWallet.GetAccount(321);//代币用户
            Console.WriteLine($"张三 关联自定义Token的InitialAccount: {initalAccount}");

            // 获取李四的关联代币账户
            var lisiAssociatedTokenAccount = AssociatedTokenAccountProgram.DeriveAssociatedTokenAccount(_secondWallet.Account.PublicKey, mintAccount.PublicKey);

            var getLisiAtaInfo = await _rpcClient.GetAccountInfoAsync(lisiAssociatedTokenAccount);
            #region 处理关联账户
            if (getLisiAtaInfo.Result.Value == null)
            {
                // 如果李四的关联代币账户不存在，创建它
                var createAtaInstruction = AssociatedTokenAccountProgram.CreateAssociatedTokenAccount(
                     _secondWallet.Account.PublicKey,
                     _secondWallet.Account.PublicKey,
                    mintAccount.PublicKey
                );

                var createAtaTransaction = new TransactionBuilder()
                   .AddInstruction(createAtaInstruction)
                   .SetRecentBlockHash(_rpcClient.GetLatestBlockHash().Result.Value.Blockhash)
                   .SetFeePayer(_secondWallet.Account.PublicKey)
                   .Build(_secondWallet.Account);

                var createAtaSignature = await _rpcClient.SendTransactionAsync(createAtaTransaction);
                Console.WriteLine($"创建李四关联代币账户交易签名: {createAtaSignature.Result}");
                await Task.Delay(5000);
            }
            #endregion

            // 从张三的关联代币账户转移 1000 万个代币到李四的关联代币账户
            var transferInstruction = TokenProgram.Transfer(
                initalAccount,
                lisiAssociatedTokenAccount,
                200000,
                ownerAccount
            );
            var transferTransaction = new TransactionBuilder()
               .AddInstruction(transferInstruction)
               .SetRecentBlockHash(_rpcClient.GetLatestBlockHash().Result.Value.Blockhash)
               .SetFeePayer(ownerAccount);

            var feeData = await SPLTokenService.CalculateTransactionFeeAsync(transferTransaction);
           

        }
    }
}
