using Org.BouncyCastle.Crypto.Agreement.Srp;
using SharpCompress.Common;
using Solnet.Examples;
using Solnet.Extensions;
using Solnet.Extensions.TokenMint;
using Solnet.KeyStore;
using Solnet.KeyStore.Model;
using Solnet.Metaplex.NFT.Library;
using Solnet.Programs;
using Solnet.Programs.Models.TokenProgram;
using Solnet.Programs.Utilities;
using Solnet.Rpc;
using Solnet.Rpc.Builders;
using Solnet.Rpc.Core.Http;
using Solnet.Rpc.Messages;
using Solnet.Rpc.Models;
using Solnet.Wallet;
using Solnet.Wallet.Bip39;
using Solnet.Wallet.Utilities;
using System.Security.Policy;
using System.Security.Principal;
using System.Text;
using System.Transactions;
using System.Xml.Linq;
using YC.Common.ShareUtils;
using YC.SolanaSdkService.DTO;

namespace SolanaTest
{
    namespace SolanaTest
    {
        public class SPLTokenUnitTest
        {

            /// <summary>
            /// The public key of the ComputeBudget Program.
            /// </summary>
            public static readonly Solnet.Wallet.PublicKey ProgramIdKey = new("ComputeBudget111111111111111111111111111111");

            /// <summary>
            /// Set Compute Unit Limit Instruction for Priority Fees
            /// </summary>
            /// <param name="units"></param>
            /// <returns></returns>
            public static TransactionInstruction SetComputeUnitLimit(uint units)
            {
                List<AccountMeta> keys = new();

                byte[] instructionBytes = new byte[9];
                instructionBytes.WriteU8(2, 0);
                instructionBytes.WriteU64(units, 1);

                return new TransactionInstruction
                {
                    ProgramId = ProgramIdKey.KeyBytes,
                    Keys = keys,
                    Data = instructionBytes
                };
            }
            /// <summary>
            /// Set Compute Unit Price Instruction for Priority Fees
            /// </summary>
            /// <param name="priority_rate"></param>
            /// <returns></returns>
            public static TransactionInstruction SetComputeUnitPrice(ulong priority_rate)
            {
                List<AccountMeta> keys = new();

                byte[] instructionBytes = new byte[9];
                instructionBytes.WriteU8(3, 0);
                instructionBytes.WriteU64(priority_rate, 1);

                return new TransactionInstruction
                {
                    ProgramId = ProgramIdKey.KeyBytes,
                    Keys = keys,
                    Data = instructionBytes
                };
            }


            public Wallet _ownsWallet { get; set; }//测试使用的钱包

            public Wallet _secondWallet { get; set; }//测试钱包2
            public IRpcClient _rpcClient { get; set; }//测试请求的PRC

            public SPLTokenUnitTest()
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
            private const string MnemonicWords =
           "route clerk disease box emerge airport loud waste attitude film army tray " +
           "forward deal onion eight catalog surface unit card window walnut wealth medal";
            /// <summary>
            /// 创建、初始化和铸造，发现有些钱包索引不会出现签名错误，比如17，18
            /// </summary>
            [Fact]
            public async void PublishTokenTest()
            {
                //var newMnemonic = new Solnet.Wallet.Bip39.Mnemonic(Solnet.Wallet.Bip39.WordList.English, Solnet.Wallet.Bip39.WordCount.Twelve);
                //Wallet wallet = new Wallet(newMnemonic);
                var wallet = _ownsWallet;
                // 获取最新的区块哈希
                RequestResult<ResponseValue<LatestBlockHash>> blockHash = _rpcClient.GetLatestBlockHash();
                if (!blockHash.WasSuccessful)
                {
                    Console.WriteLine("Failed to get latest block hash.");
                    return;
                }
                // 获取创建账户所需的最低租金豁免余额
                ulong minBalanceForExemptionAcc = _rpcClient.GetMinimumBalanceForRentExemption(TokenProgram.TokenAccountDataSize).Result;
                Console.WriteLine($"MinBalanceForRentExemption Account >> {minBalanceForExemptionAcc}");
                ulong minBalanceForExemptionMint = _rpcClient.GetMinimumBalanceForRentExemption(TokenProgram.MintAccountDataSize).Result;
                Console.WriteLine($"MinBalanceForRentExemption Mint Account >> {minBalanceForExemptionMint}");
                // 获取所需的账户
                Solnet.Wallet.Account mintAccount = wallet.GetAccount(27);
                Console.WriteLine($"MintAccount: {mintAccount}");
                Solnet.Wallet.Account ownerAccount = wallet.GetAccount(0);
                Console.WriteLine($"OwnerAccount: {ownerAccount}");
                Solnet.Wallet.Account initialAccount = wallet.GetAccount(28);
                Console.WriteLine($"InitialAccount: {initialAccount}");
                #region 构建交易
                // 构建交易
                var transactionBuilder = new TransactionBuilder()
                   .SetRecentBlockHash(blockHash.Result.Value.Blockhash)
                   .SetFeePayer(ownerAccount.PublicKey);
                // 添加创建 Mint 账户的指令
                transactionBuilder.AddInstruction(SystemProgram.CreateAccount(
                    ownerAccount.PublicKey,
                    mintAccount.PublicKey,
                    minBalanceForExemptionMint,
                    TokenProgram.MintAccountDataSize,
                    TokenProgram.ProgramIdKey));//ProgramIdKey 是否需要修改
                // 添加初始化 Mint 账户的指令
                transactionBuilder.AddInstruction(TokenProgram.InitializeMint(
                    mintAccount.PublicKey,
                    2,
                    ownerAccount.PublicKey,//管理mint账户后续发币等权限的账户
                    ownerAccount.PublicKey));//冻结权的账户
                // 添加创建初始账户的指令
                transactionBuilder.AddInstruction(SystemProgram.CreateAccount(
                    ownerAccount.PublicKey,
                    initialAccount.PublicKey,
                    minBalanceForExemptionAcc,
                    TokenProgram.TokenAccountDataSize,
                    TokenProgram.ProgramIdKey));
                // 添加初始化初始账户的指令
                transactionBuilder.AddInstruction(TokenProgram.InitializeAccount(
                    initialAccount.PublicKey,
                    mintAccount.PublicKey,
                    ownerAccount.PublicKey));
                // 添加铸造代币的指令
                transactionBuilder.AddInstruction(TokenProgram.MintTo(
                    mintAccount.PublicKey,
                    initialAccount.PublicKey,
                    25000,
                    ownerAccount.PublicKey));
                #endregion
                // 添加备忘录指令
                transactionBuilder.AddInstruction(MemoProgram.NewMemo(initialAccount.PublicKey, "Hello from Sol.Net"));
                // 构建并签署交易
                var signers = new List<Solnet.Wallet.Account> { ownerAccount, mintAccount, initialAccount, };//签名有顺序要求，并且除了ownerAccount，其他两个没有使用过的,同时需要修改官方源码，
                                                                                                             //把内部foreach改为for，需要严格的顺序限制
                var transaction = transactionBuilder.Build(signers);
                // 发送交易
                RequestResult<string> txReq = await _rpcClient.SendTransactionAsync(transaction);
                if (txReq.WasSuccessful)
                {
                    Console.WriteLine($"Transaction sent successfully. Signature: {txReq.Result}");
                }
                else
                {
                    Console.WriteLine($"Transaction failed: {txReq.Reason}");
                }
                /*
                                RequestResult<ResponseValue<SimulationLogs>> txSim = await _rpcClient.SimulateTransactionAsync(tx);
                                string logs = Examples.PrettyPrintTransactionSimulationLogs(txSim.Result.Value.Logs);
                                Console.WriteLine($"Transaction Simulation:\n\tError: {txSim.Result.Value.Error}\n\tLogs: \n" + logs);

                                RequestResult<string> txReq = await _rpcClient.SendTransactionAsync(tx);
                                Console.WriteLine($"Tx Signature: {txReq.Result}");

                                var tx2 = Transaction.Deserialize(tx);
                                var msg = tx2.CompileMessage();

                                Assert.True(tx2.Signatures[0].PublicKey.Verify(msg, tx2.Signatures[0].Signature));

                               */
            }
            /// <summary>
            /// Transfer a Token to a new Token Account
            /// </summary>
            [Fact]
            public async void TransferTokenTest()
            {

                // Initialize the rpc client and a wallet
                var rpcClient = _rpcClient;
                var wallet = _ownsWallet;

                var blockHash = rpcClient.GetLatestBlockHash();
                var minBalanceForExemptionAcc =
                    rpcClient.GetMinimumBalanceForRentExemption(TokenProgram.TokenAccountDataSize).Result;

                var mintAccount = wallet.GetAccount(17);
                var ownerAccount = wallet.GetAccount(0);
                var initialAccount = wallet.GetAccount(18);
                var newAccount = wallet.GetAccount(24);//

                var tx = new TransactionBuilder().
                    SetRecentBlockHash(blockHash.Result.Value.Blockhash).
                    SetFeePayer(ownerAccount).
                    AddInstruction(SetComputeUnitLimit(30000)).
                    AddInstruction(SetComputeUnitPrice(1000000)).
                    AddInstruction(SystemProgram.CreateAccount(
                        ownerAccount,
                        newAccount,
                        minBalanceForExemptionAcc,
                        TokenProgram.TokenAccountDataSize,
                        TokenProgram.ProgramIdKey)).
                    AddInstruction(TokenProgram.InitializeAccount(
                        newAccount.PublicKey,
                        mintAccount.PublicKey,
                        ownerAccount.PublicKey)).
                    AddInstruction(TokenProgram.Transfer(
                        initialAccount.PublicKey,
                        newAccount.PublicKey,
                        25000,
                        ownerAccount)).
                    AddInstruction(MemoProgram.NewMemo(initialAccount, "Hello from Sol.Net")).
                    Build(new List<Solnet.Wallet.Account> { ownerAccount, newAccount, initialAccount });//加上它就可以了，initialAccount
                // 发送交易
                RequestResult<string> txReq = await _rpcClient.SendTransactionAsync(tx);
                if (txReq.WasSuccessful)
                {
                    Console.WriteLine($"Transaction sent successfully. Signature: {txReq.Result}");
                }
                else
                {
                    Console.WriteLine($"Transaction failed: {txReq.Reason}");
                }
            }


            [Fact]
            public async void CreateTokenTest()
            {
                #region 初始铸造
                var wallet = _ownsWallet;
                var payer = _ownsWallet.Account.PublicKey;
                // 获取最新的区块哈希
                RequestResult<ResponseValue<LatestBlockHash>> blockHash = _rpcClient.GetLatestBlockHash();
                if (!blockHash.WasSuccessful)
                {
                    Console.WriteLine("Failed to get latest block hash.");
                    return;
                }
                // 1. 获取创建账户所需的最低租金豁免余额
                ulong minBalanceForExemptionAcc = _rpcClient.GetMinimumBalanceForRentExemption(TokenProgram.TokenAccountDataSize).Result;
                Console.WriteLine($"MinBalanceForRentExemption Account >> {minBalanceForExemptionAcc}");
                ulong minBalanceForExemptionMint = _rpcClient.GetMinimumBalanceForRentExemption(TokenProgram.MintAccountDataSize).Result;
                Console.WriteLine($"MinBalanceForRentExemption Mint Account >> {minBalanceForExemptionMint}");
                // 获取所需的账户
                Solnet.Wallet.Account mintAccount = wallet.GetAccount(36);//用于铸造币的用户
                Console.WriteLine($"MintAccount: {mintAccount}");
                Solnet.Wallet.Account ownerAccount = wallet.GetAccount(0);//管理员用户
                Console.WriteLine($"OwnerAccount: {ownerAccount}");
                Solnet.Wallet.Account initialAccount = wallet.GetAccount(37);//接受代币用户
                Console.WriteLine($"InitialAccount: {initialAccount}");

                #region 添加元数据
                //var metadata = new Metadata()
                //{
                //    name = "碳币",
                //    symbol = "CTK",
                //    uri = "https://example.com/token-metadata.json",
                //    sellerFeeBasisPoints = 500, // 5% 的费用
                //    creators = new List<Creator>() {
                //    new Creator(wallet.Account.PublicKey,100,true)
                //    }
                //};

                //// 创建元数据账户指令
                //var createMetadataInstruction = MetadataProgram.CreateMetadataAccount(
                //    payer, mintAccount,
                //    payer,
                //    payer,
                //    payer,
                //    metadata,
                //    TokenStandard.FungibleAsset,
                //    true,
                //    true
                //    );
                #endregion

                #region 2. 构建交易
                // 构建交易
                var transactionBuilder = new TransactionBuilder()
                   .SetRecentBlockHash(blockHash.Result.Value.Blockhash)
                   .SetFeePayer(ownerAccount.PublicKey);
                // 添加创建 Mint 账户的指令
                transactionBuilder.AddInstruction(SystemProgram.CreateAccount(
                    ownerAccount.PublicKey,
                    mintAccount.PublicKey,
                    minBalanceForExemptionMint,
                    TokenProgram.MintAccountDataSize,
                    TokenProgram.ProgramIdKey));//ProgramIdKey 可能不需要更改，猜测可能是引用指定的智能合约程序的id

                // 添加初始化 Mint 账户的指令
                transactionBuilder.AddInstruction(TokenProgram.InitializeMint(
                    mintAccount.PublicKey,
                    2,
                    ownerAccount.PublicKey,//管理mint账户后续发币等权限的账户
                    ownerAccount.PublicKey));//冻结权的账户
                // 添加创建初始账户的指令
                transactionBuilder.AddInstruction(SystemProgram.CreateAccount(
                    ownerAccount.PublicKey,
                    initialAccount.PublicKey,
                    minBalanceForExemptionAcc,
                    TokenProgram.TokenAccountDataSize,
                    TokenProgram.ProgramIdKey));
                // 添加初始化初始账户的指令
                transactionBuilder.AddInstruction(TokenProgram.InitializeAccount(
                    initialAccount.PublicKey,
                    mintAccount.PublicKey,
                    ownerAccount.PublicKey));

                //transactionBuilder.AddInstruction(createMetadataInstruction);//添加元数据

                // 添加铸造代币的指令
                transactionBuilder.AddInstruction(TokenProgram.MintTo(
                    mintAccount.PublicKey,
                    initialAccount.PublicKey,
                    1000000000,
                    ownerAccount.PublicKey));
              
                #endregion
                // 添加备忘录指令
                transactionBuilder.AddInstruction(MemoProgram.NewMemo(initialAccount.PublicKey, "Hello from Sol.Net"));
                // 构建并签署交易
                var signers = new List<Solnet.Wallet.Account> { ownerAccount, mintAccount, initialAccount, };//签名有顺序要求，并且除了ownerAccount，其他两个没有使用过的,同时需要修改官方源码，
                                                                                                             //把内部foreach改为for，需要严格的顺序限制
                var transaction = transactionBuilder.Build(signers);
                // 发送交易
                RequestResult<string> txReq = await _rpcClient.SendTransactionAsync(transaction);
               
                if (txReq.WasSuccessful)
                {
                    Console.WriteLine($"Transaction sent successfully. Signature: {txReq.Result}");
                }
                else
                {
                    Console.WriteLine($"Transaction failed: {txReq.Reason}");
                }
                #endregion
            }


            [Fact]
            public async void CreateToken2Test()
            {
                #region 初始铸造
                var wallet = _ownsWallet;
                var payer = _ownsWallet.Account.PublicKey;
                // 获取最新的区块哈希
                RequestResult<ResponseValue<LatestBlockHash>> blockHash = _rpcClient.GetLatestBlockHash();
                if (!blockHash.WasSuccessful)
                {
                    Console.WriteLine("Failed to get latest block hash.");
                    return;
                }
                // 1. 获取创建账户所需的最低租金豁免余额
                ulong minBalanceForExemptionAcc = _rpcClient.GetMinimumBalanceForRentExemption(TokenProgram.TokenAccountDataSize).Result;
                Console.WriteLine($"MinBalanceForRentExemption Account >> {minBalanceForExemptionAcc}");
                ulong minBalanceForExemptionMint = _rpcClient.GetMinimumBalanceForRentExemption(TokenProgram.MintAccountDataSize).Result;
                Console.WriteLine($"MinBalanceForRentExemption Mint Account >> {minBalanceForExemptionMint}");
                // 获取所需的账户
                Solnet.Wallet.Account mintAccount = wallet.GetAccount(36);//用于铸造币的用户
                Console.WriteLine($"MintAccount: {mintAccount}");
                Solnet.Wallet.Account ownerAccount = wallet.GetAccount(0);//管理员用户
                Console.WriteLine($"OwnerAccount: {ownerAccount}");
                Solnet.Wallet.Account initialAccount = wallet.GetAccount(37);//接受代币用户
                Console.WriteLine($"InitialAccount: {initialAccount}");

                #region 添加元数据
                var metadata = new Metadata()
                {
                    name = "碳币",
                    symbol = "CTK",
                    uri = "https://example.com/token-metadata.json",
                    sellerFeeBasisPoints = 500, // 5% 的费用
                    creators = new List<Creator>() {
                    new Creator(wallet.Account.PublicKey,100,true)
                    }
                };

                // 创建元数据账户指令
                var createMetadataInstruction = MetadataProgram.CreateMetadataAccount(
                    initialAccount, mintAccount,
                    payer,
                    payer,
                    payer,
                    metadata,
                    TokenStandard.FungibleAsset,
                    true,
                    true
                    );
                #endregion

                #region 2. 构建交易
                // 构建交易
                var transactionBuilder = new TransactionBuilder()
                   .SetRecentBlockHash(blockHash.Result.Value.Blockhash)
                   .SetFeePayer(ownerAccount.PublicKey);
                // 添加创建 Mint 账户的指令
                transactionBuilder.AddInstruction(SystemProgram.CreateAccount(
                    ownerAccount.PublicKey,
                    mintAccount.PublicKey,
                    minBalanceForExemptionMint,
                    TokenProgram.MintAccountDataSize,
                    TokenProgram.ProgramIdKey));//ProgramIdKey 可能不需要更改，猜测可能是引用指定的智能合约程序的id

                // 添加初始化 Mint 账户的指令
                transactionBuilder.AddInstruction(TokenProgram.InitializeMint(
                    mintAccount.PublicKey,
                    2,
                    ownerAccount.PublicKey,//管理mint账户后续发币等权限的账户
                    ownerAccount.PublicKey));//冻结权的账户
                // 添加创建初始账户的指令
                transactionBuilder.AddInstruction(SystemProgram.CreateAccount(
                    ownerAccount.PublicKey,
                    initialAccount.PublicKey,
                    minBalanceForExemptionAcc,
                    TokenProgram.TokenAccountDataSize,
                    TokenProgram.ProgramIdKey));

                // 添加初始化初始账户的指令
                transactionBuilder.AddInstruction(TokenProgram.InitializeAccount(
                    initialAccount.PublicKey,
                    mintAccount.PublicKey,
                    ownerAccount.PublicKey));

                // 添加铸造代币的指令
                transactionBuilder.AddInstruction(TokenProgram.MintTo(
                    mintAccount.PublicKey,
                    initialAccount.PublicKey,
                    1000000000,
                    ownerAccount.PublicKey));

                #endregion
                // 添加备忘录指令
                transactionBuilder.AddInstruction(MemoProgram.NewMemo(initialAccount.PublicKey, "Hello from Sol.Net"));
                // 构建并签署交易
                var signers = new List<Solnet.Wallet.Account> { ownerAccount, mintAccount, initialAccount, };//签名有顺序要求，并且除了ownerAccount，其他两个没有使用过的,同时需要修改官方源码，
                                                                                                             //把内部foreach改为for，需要严格的顺序限制
                var transaction = transactionBuilder.Build(signers);
                // 发送交易
                RequestResult<string> txReq = await _rpcClient.SendTransactionAsync(transaction);

                if (txReq.WasSuccessful)
                {
                    Console.WriteLine($"Transaction sent successfully. Signature: {txReq.Result}");
                }
                else
                {
                    Console.WriteLine($"Transaction failed: {txReq.Reason}");
                }
                #endregion

            }



            /// <summary>
            /// 指定钱包账户查看指定代币的关联账户，如果不存在，就自动创建
            /// </summary>
            [Fact]
            public async void GetAssociatedTokenAccountTest()
            {
                // 获取所需的账户
                Solnet.Wallet.Account mintAccount = _ownsWallet.GetAccount(30);//用于铸造币的用户
                Console.WriteLine($"MintAccount: {mintAccount}");
                Solnet.Wallet.Account ownerAccount = _ownsWallet.GetAccount(0);//管理员用户

                // 获取李四的关联代币账户 
                var lisiAssociatedTokenAccount = AssociatedTokenAccountProgram.DeriveAssociatedTokenAccount(_secondWallet.Account.PublicKey, mintAccount.PublicKey);
                var getLisiAtaInfo = await _rpcClient.GetAccountInfoAsync(lisiAssociatedTokenAccount);
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


            }


            /// <summary>
            /// 将自定义的代币转移给其他用户
            /// </summary>
            [Fact]
            public async void TransferSPLTokenTest() {

                // 张三账户
                Solnet.Wallet.Account mintAccount = _ownsWallet.GetAccount(30);//用于铸造币的用户
                Console.WriteLine($"张三 铸造账户MintAccount: {mintAccount}");
                Solnet.Wallet.Account ownerAccount = _ownsWallet.GetAccount(0);//管理员用户
                Console.WriteLine($"张三 管理账户OwnerAccount: {ownerAccount}");
                Solnet.Wallet.Account initialAccount = _ownsWallet.GetAccount(31);//张三代币用户
                Console.WriteLine($"张三 关联自定义Token的InitialAccount: {initialAccount}");

                // 获取李四的关联代币账户
                var lisiAssociatedTokenAccount = AssociatedTokenAccountProgram.DeriveAssociatedTokenAccount(_secondWallet.Account.PublicKey, mintAccount.PublicKey);

                // 从张三的关联代币账户转移 1000 万个代币到李四的关联代币账户
                var transferInstruction = TokenProgram.Transfer(
                    initialAccount,
                    lisiAssociatedTokenAccount,
                    200000, 
                    ownerAccount
                );

               
                var transferTransaction = new TransactionBuilder()
                   .AddInstruction(transferInstruction)
                   .SetRecentBlockHash(_rpcClient.GetLatestBlockHash().Result.Value.Blockhash)
                   .SetFeePayer(ownerAccount)
                   .Build(ownerAccount);

                var transferSignature = await _rpcClient.SendTransactionAsync(transferTransaction);
                Console.WriteLine($"转移代币到李四关联代币账户交易签名: {transferSignature.Result}");
                await Task.Delay(5000);

               
                // 李四查询代币是否到账
                var lisiAtaInfo = await _rpcClient.GetTokenAccountBalanceAsync(lisiAssociatedTokenAccount);
                if (lisiAtaInfo.WasSuccessful)
                {
                    Console.WriteLine($"李四的 CTK 代币余额: {lisiAtaInfo.Result.Value.Amount}");
                }
                else
                {
                    Console.WriteLine("查询余额失败。");
                }
            }

            /// <summary>
            /// 查询自定义代币的余额
            /// </summary>
            [Fact]
            public async void GetTokenBalanceTest() {
                // 张三账户
                Solnet.Wallet.Account mintAccount = _ownsWallet.GetAccount(30);//用于铸造币的用户
                Console.WriteLine($"张三 铸造账户MintAccount: {mintAccount}");
                //查询创建的代币以及数量
                var zhangsanAtaInfo = await _rpcClient.GetTokenAccountBalanceAsync(_ownsWallet.GetAccount(31).PublicKey);
                // 获取李四的关联代币账户
                var lisiAssociatedTokenAccount = AssociatedTokenAccountProgram.DeriveAssociatedTokenAccount(_secondWallet.Account.PublicKey, mintAccount.PublicKey);

                //查询创建的代币以及数量
                var lisiAtaInfo = await _rpcClient.GetTokenAccountBalanceAsync(lisiAssociatedTokenAccount);
            }


          
            /// <summary>
            /// 为自定义的代币添加元数据，比如名称、代号等
            /// </summary>
            [Fact]
            public async void SetTokenMetaDataTest() {
                // 张三账户
                Solnet.Wallet.Account mintAccount = _ownsWallet.GetAccount(29);//用于铸造币的用户
                var payer = _ownsWallet.Account.PublicKey;
                //// 添加代币元数据，固定的
                var metadataProgramId = new Solnet.Wallet.PublicKey("metaqbxxUerdq28cj1RbAWkYQm3ybzjb6a8bt518x1s");

                //// add TokenDef for a TestNet minted token created by Solnet examples
                var tokens = new TokenMintResolver();
                tokens.Add(new TokenDef(mintAccount.PublicKey, "碳币", "CTK", 2));
              
                // load snapshot of wallet and sub-accounts
                TokenWallet tokenWallet = TokenWallet.Load(_rpcClient, tokens, _ownsWallet.Account);
                var balances = tokenWallet.Balances();

                var metadata = new Metadata()
                {
                    name = "碳币",
                    symbol = "CTK",
                    uri = "https://example.com/token-metadata.json",
                    sellerFeeBasisPoints = 500, // 5% 的费用
                    creators = new List<Creator>() {
                    new Creator(_ownsWallet.Account.PublicKey,100,true)
                    }
                };
                var initAccount=TokenProgram.InitializeAccount(_ownsWallet.GetAccount(31),mintAccount, payer);
                // 创建元数据账户指令
                var createMetadataInstruction = MetadataProgram.CreateMetadataAccount(
                    _ownsWallet.GetAccount(31), mintAccount,
                    payer,
                    payer,
                    payer,
                    metadata,
                    TokenStandard.FungibleAsset,
                    true,
                    true
                    );


                var metadataTransaction = new TransactionBuilder()
                   .AddInstruction(createMetadataInstruction)
                   .SetRecentBlockHash(_rpcClient.GetLatestBlockHash().Result.Value.Blockhash)
                   .SetFeePayer(_ownsWallet.Account)
                   .Build(_ownsWallet.Account);
                var metadataSignature = await _rpcClient.SendTransactionAsync(metadataTransaction);
                Console.WriteLine($"创建代币元数据账户交易签名: {metadataSignature.Result}");
                #region 查询元数据，待完善
                /*await Task.Delay(5000);
                // 查询代币元数据
                var metadataInfo = await _rpcClient.GetAccountInfoAsync(mintAccount.PublicKey);
                if (metadataInfo.WasSuccessful && metadataInfo.Result.Value != null)
                {
                    // 解析元数据账户的数据
                    byte[] metadataData = Convert.FromBase64String(metadataInfo.Result.Value.Data[0]);
                    using (MemoryStream ms = new MemoryStream(metadataData))
                    using (BinaryReader br = new BinaryReader(ms))
                    {
                        // 跳过头部字节（例如，结构体版本等）
                        br.ReadByte(); // 版本
                                       // 读取更新权限公钥
                        byte[] updateAuthorityBytes = br.ReadBytes(32);
                        Solnet.Wallet.PublicKey updateAuthority = new Solnet.Wallet.PublicKey(updateAuthorityBytes);
                        // 读取铸造账户公钥
                        byte[] mintBytes = br.ReadBytes(32);
                        Solnet.Wallet.PublicKey mint = new Solnet.Wallet.PublicKey(mintBytes);
                        // 读取元数据数据
                        byte dataLen = br.ReadByte();
                        string name = Encoding.UTF8.GetString(br.ReadBytes(dataLen));
                        dataLen = br.ReadByte();
                        string symbol = Encoding.UTF8.GetString(br.ReadBytes(dataLen));
                        dataLen = br.ReadByte();
                        string uri = Encoding.UTF8.GetString(br.ReadBytes(dataLen));
                        ushort sellerFeeBasisPoints = br.ReadUInt16();
                        // 读取创作者信息
                        byte creatorsLen = br.ReadByte();
                        List<(Solnet.Wallet.PublicKey, byte, bool)> creators = new List<(Solnet.Wallet.PublicKey, byte, bool)>();
                        for (int i = 0; i < creatorsLen; i++)
                        {
                            byte[] creatorBytes = br.ReadBytes(32);
                            Solnet.Wallet.PublicKey creator = new Solnet.Wallet.PublicKey(creatorBytes);
                            byte share = br.ReadByte();
                            bool verified = br.ReadBoolean();
                            creators.Add((creator, share, verified));
                        }
                        bool primarySaleHappened = br.ReadBoolean();
                        bool isMutable = br.ReadBoolean();
                        Console.WriteLine($"代币名称: {name}");
                        Console.WriteLine($"代币符号: {symbol}");
                        Console.WriteLine($"代币 URI: {uri}");
                        Console.WriteLine($"卖家费用基点: {sellerFeeBasisPoints}");
                        Console.WriteLine($"创作者数量: {creators.Count}");
                        Console.WriteLine($"首次销售是否发生: {primarySaleHappened}");
                        Console.WriteLine($"是否可修改: {isMutable}");
                    }
                }
                else
                {
                    Console.WriteLine("查询代币元数据失败。");
                } */
                #endregion
            }
          

        }
    }

    public static class StringExtensions
    {
        public static byte[] ToBytesLengthPrefixed(this string str)
        {
            // 将字符串转换为 UTF-8 编码的字节数组
            byte[] strBytes = Encoding.UTF8.GetBytes(str);
            // 创建一个新的字节数组，长度为字符串字节数组长度加上 4 字节（用于存储长度信息）
            byte[] result = new byte[4 + strBytes.Length];
            // 将字符串长度以 32 位整数形式写入结果数组的前 4 个字节
            BitConverter.GetBytes(strBytes.Length).CopyTo(result, 0);
            // 将字符串字节数组复制到结果数组中
            strBytes.CopyTo(result, 4);
            return result;
        }
    }

}