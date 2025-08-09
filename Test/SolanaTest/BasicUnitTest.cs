using Solnet.Extensions;
using Solnet.Extensions.TokenMint;
using Solnet.KeyStore;
using Solnet.Programs;
using Solnet.Programs.Models;
using Solnet.Programs.TokenSwap.Models;
using Solnet.Programs.Utilities;
using Solnet.Rpc;
using Solnet.Rpc.Builders;
using Solnet.Rpc.Core.Http;
using Solnet.Rpc.Models;
using Solnet.Wallet;
using Solnet.Wallet.Utilities;
using System.Text;
using YC.ApplicationService.Utils;
using YC.Common.ShareUtils;
using YC.Model.Entity;
using YC.SolanaSdkService;
using YC.SolanaSdkService.DTO;

namespace SolanaTest
{
    public class BasicUnitTest
    {
        public Wallet _ownsWallet { get; set; }//测试使用的钱包

        public Wallet _secondWallet { get; set; }//测试钱包2
        public IRpcClient _rpcClient { get; set; }//测试请求的PRC
        public BasicUnitTest() {
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

            _rpcClient = ClientFactory.GetClient(Cluster.DevNet);//定义全局RP
        }

        #region 账户与钱包
        /// <summary>
        /// 创建账户
        /// </summary>
        [Fact]
        public void CreateAccountTest()
        {
            var account = new Solnet.Wallet.Account();
            var publicKey = account.PublicKey;
            var privateKey = account.PrivateKey;
            Assert.True(account != null);
        }


        /// <summary>
        /// 创建钱包
        /// </summary>
        [Fact]
        public void CreateWalletTest()
        {
            // Generate a new mnemonic
            var newMnemonic = new Solnet.Wallet.Bip39.Mnemonic(Solnet.Wallet.Bip39.WordList.English, Solnet.Wallet.Bip39.WordCount.Twelve);
            var wallet = new Wallet(newMnemonic);
            var info = wallet.ToIndentedJson();
            string directStr = AppDomain.CurrentDomain.BaseDirectory + "\\Resources\\";
           
            string filePath = directStr + "\\walletInfo3.txt";
            bool isTrue = FileUtils.AppendWriteFile(filePath, info);
            Assert.True(isTrue);

        }

        /// <summary>
        /// 从文本中导入钱包
        /// </summary>
        [Fact]
        public void InitWalletTest()
        {
            string filePath = AppDomain.CurrentDomain.BaseDirectory + "\\Resources\\walletInfo.txt";
            string walletInfo = "";
            FileUtils.ReadFile(filePath, out walletInfo);
            Assert.True(!string.IsNullOrEmpty(walletInfo));

            var tempWalletObj = walletInfo.ToObject<WalletDto>();//不能直接转为wallet 对象
                                                                 // To initialize a wallet and have access to the same keys generated in sollet (the default)
            var sollet = new Wallet(string.Join(" ", tempWalletObj.Mnemonic.Words), Solnet.Wallet.Bip39.WordList.English);
            string tempSolletInfo = sollet.ToIndentedJson();
            Assert.True(walletInfo == tempSolletInfo);

            // Retrieve accounts by derivation path index
            var account = sollet.GetAccount(10);
        }
        #endregion

        #region 构建RPC和交易

        /// <summary>
        /// 发送交易
        /// </summary>
        [Fact]
        public async void BuilRpcClientAndSendTranscationTest()
        {
            var rpcClient = ClientFactory.GetClient(Cluster.DevNet);

            #region 创建http 和Stream 模式的RPC 请求
            var c2 = ClientFactory.GetStreamingClient(Cluster.DevNet);
            /* string url3 = "https://testnet.solana.com";
             var c3 = ClientFactory.GetClient(url3, null);

             string url4 = "wss://api.testnet.solana.com";
             var c4 = ClientFactory.GetStreamingClient(url4);*/
            //Assert.Equal(c, typeof(SolanaRpcClient)); 
            #endregion

            // 1. 导入钱包（已经在开发网络上进行空投）
            string filePath = AppDomain.CurrentDomain.BaseDirectory + "\\Resources\\walletInfo.txt";
            string walletInfo = "";
            FileUtils.ReadFile(filePath, out walletInfo);
            Assert.True(!string.IsNullOrEmpty(walletInfo));

            var tempWalletObj = walletInfo.ToObject<WalletDto>();//不能直接转为wallet 对象

            // Get a certain account's info
            var accountInfo = rpcClient.GetAccountInfo(tempWalletObj.Account.PublicKey.Key);
            var wallet = new Wallet(string.Join(" ", tempWalletObj.Mnemonic.Words), Solnet.Wallet.Bip39.WordList.English);
            string tempSolletInfo = wallet.ToIndentedJson();
            var fromAccount = wallet.GetAccount(0);
           
            // Get the destination account
            var toAccount = _secondWallet.GetAccount(0);
            //查询各自的余额
            var fromAccountBalance = await rpcClient.GetBalanceAsync(fromAccount.PublicKey);
            var toAccountBalance = await rpcClient.GetBalanceAsync(toAccount.PublicKey);

            // Get a recent block hash to include in the transaction
            var blockHash = rpcClient.GetLatestBlockHash();
            //var t = new Solnet.Wallet.PublicKey("4EdWvya7oAvX9L2oCYBiofNggdg4g1ZAZXvkNSbVVTZS");
            // Initialize a transaction builder and chain as many instructions as you want before building the message
            var tx = new TransactionBuilder().
                    SetRecentBlockHash(blockHash.Result.Value.Blockhash).
                    SetFeePayer(fromAccount).//有收取手续费
                    AddInstruction(SetComputeUnitLimit(30000)).
                    AddInstruction(SetComputeUnitPrice(1000000)).
                    AddInstruction(MemoProgram.NewMemo(fromAccount, "Hello from Sol Dev Net by powerlin :)")).
                    AddInstruction(SystemProgram.Transfer(fromAccount, toAccount, 1000000)).//交易燃料费用要注意
                    Build(fromAccount);

            var firstSig = await rpcClient.SendTransactionAsync(tx);
           
            //result Signature=5ScCQtQvXqJbDod31zSKdyT15veVGP54qddHZ6DugaRjeVZxc9kiM1mDu2i6Zqwimcw4QqjNuatr2cTE6AJGu3i1

        }

        /// <summary>
        /// 通过交易签名，获取交易信息
        /// </summary>
        [Fact]
        public async void GetTranscationBySignatureTest() {
            var rpcClient = ClientFactory.GetClient(Cluster.DevNet);
            string _txSignature = "5ScCQtQvXqJbDod31zSKdyT15veVGP54qddHZ6DugaRjeVZxc9kiM1mDu2i6Zqwimcw4QqjNuatr2cTE6AJGu3i1";
            RequestResult<TransactionMetaSlotInfo> data = await  rpcClient.GetTransactionAsync(_txSignature);
            var msg=Encoders.Base58.DecodeData(data.Result.Transaction.Message.Instructions[2].Data);
            string result = Encoding.UTF8.GetString(msg);//获得交易备注
        }

        /// <summary>
        /// 查询通用sol 的余额
        /// </summary>
        [Fact]
        public async void SimpleGetAccountBalanceTest() {

           
            var fromAccountBalance = await _rpcClient.GetBalanceAsync(_ownsWallet.GetAccount(0).PublicKey);
            var toAccountBalance = await _rpcClient.GetBalanceAsync(_secondWallet.GetAccount(0).PublicKey);
            Assert.NotNull(fromAccountBalance);
        }
        /// <summary>
        /// 查询余额的不同方式，1SOL=10亿 lamport
        /// 并查询对应Token
        /// </summary>
        [Fact]
        public async void GetAccountTokenBalanceTest()
        {
            var rpcClient = ClientFactory.GetClient(Cluster.DevNet);

            var fromAccount = _ownsWallet.GetAccount(0);

            // Get the destination account
            var toAccount = _ownsWallet.GetAccount(23);

            //1. 使用指定账户去查询各自的余额,燃料费 1035000
            var fromAccountBalance = await rpcClient.GetBalanceAsync(fromAccount.PublicKey);
            var toAccountBalance = await rpcClient.GetBalanceAsync(toAccount.PublicKey);

            //2. 使用钱包去查询
            // load Solana token list and get RPC client
            var tokens = new TokenMintResolver();
            var client = ClientFactory.GetClient(Cluster.DevNet);

            // load snapshot of wallet and sub-accounts
            TokenWallet tokenWallet = TokenWallet.Load(client, tokens, _ownsWallet.Account.PublicKey);
            var balances = tokenWallet.Balances();
           
            Solnet.Wallet.Account mintAccount = _ownsWallet.GetAccount(30);//用于铸造币的用户
            var testToken = new TokenDef(mintAccount.PublicKey, "碳币", "CTK", 2);
            tokens.Add(testToken);
           
           var testTokenAccount = tokenWallet.TokenAccounts().ForToken(testToken).WithAtLeast(5M).First();

            #region 创建关联账户
            // 如果李四的关联代币账户不存在，创建它
            //var createAtaInstruction = AssociatedTokenAccountProgram.CreateAssociatedTokenAccount(
            //     _secondWallet.Account.PublicKey,
            //     _secondWallet.GetAccount(12),
            //    mintAccount.PublicKey
            //);

            //var createAtaTransaction = new TransactionBuilder()
            //   .AddInstruction(createAtaInstruction)
            //   .SetRecentBlockHash(_rpcClient.GetLatestBlockHash().Result.Value.Blockhash)
            //   .SetFeePayer(_secondWallet.Account.PublicKey)
            //   .Build(_secondWallet.Account);

            //var createAtaSignature = await _rpcClient.SendTransactionAsync(createAtaTransaction);


            #endregion

            try
            {
                var result = await tokenWallet.SendAsync(testTokenAccount, 100000, _secondWallet.GetAccount(12), _secondWallet.Account, builder => builder.Build(_secondWallet.Account));
                var newBalances = tokenWallet.Balances();
                //var maxsym = balances.Max(x => x.Symbol.Length);
                //var maxname = balances.Max(x => x.TokenName.Length);
                foreach (var account in tokenWallet.TokenAccounts())
                {
                    string t = "";
                    // Console.WriteLine($"{account.Symbol.PadRight(maxsym)} {account.Symbol,14} {account.TokenName.PadRight(maxname)} {account.PublicKey} {(account.IsAssociatedTokenAccount ? "[ATA]" : "")}");
                }
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        /// <summary>
        /// 查询账户信息，确认mint accout，token account
        /// （1）每一种代币 只有一个mint account 标识唯一
        /// （2）每个钱包中可以创建多个account，但是这些account 的data中只能标识一种代币，如果
        ///  账户的data中存在详细的数据：A 账户 已经有USDT，那么A账户存储BTC，只能创建B账户存储BTC
        /// 
        /// </summary>
        [Fact]
        public async void GetAccountInfoTest() {

            var rpcClient = ClientFactory.GetClient(Cluster.DevNet);
            var firstAccount = _ownsWallet.GetAccount(0);

            // Get the destination account
            var account_1 = _ownsWallet.GetAccount(30);
            var account_2 = _ownsWallet.GetAccount(31);
            var account_3 = _ownsWallet.GetAccount(27);
            var account_17 = _ownsWallet.GetAccount(28);

            var firstAccountInfo = await rpcClient.GetAccountInfoAsync(firstAccount.PublicKey);
            var account_1_Info = await rpcClient.GetAccountInfoAsync(account_1.PublicKey);
            var account_2_Info = await rpcClient.GetAccountInfoAsync(account_2.PublicKey);
            var account_3_Info = await rpcClient.GetAccountInfoAsync(account_3.PublicKey);
            var account_17_Info = await rpcClient.GetAccountInfoAsync(account_17.PublicKey);
           //var nameServiceClient= new NameServiceClient(rpcClient);
           //var account_1_t= await nameServiceClient.GetAllNamesByOwnerAsync(account_1.PublicKey);
            //var account_1_t2 = await nameServiceClient.GetTokenInfoFromMintAsync(account_1.PublicKey);
            var account_1_InfoData = TokenSwapAccount.Deserialize(Convert.FromBase64String(account_1_Info.Result.Value.Data[0]));
            var account_2_InfoData = NonceAccount.Deserialize(Convert.FromBase64String(account_2_Info.Result.Value.Data[0]));
            var account_3_InfoData = NonceAccount.Deserialize(Convert.FromBase64String(account_3_Info.Result.Value.Data[0]));

            for (int i = 0; i < 23; i++) {  
                var t = _ownsWallet.GetAccount(i).PublicKey.Key; 
                if (t.Contains("BfAMZ9qV3jbM3VkxErRHqeHTD44HFAgiQn96k9AertKx")){
                    Console.WriteLine("找到了：i=" + i);
                }
            }
        }
        #endregion

        /// <summary>
        /// 这个没办法查找具体token 数据，需要使用TokenWallet 操作
        /// 
        /// 具体参考：GetAccountTokenBalanceTest
        /// </summary>
        [Fact]
        public async void BatchGetWalletAccountBalanceTest() {

            var rpcClient = ClientFactory.GetClient(Cluster.DevNet);
            var firstAccount = _secondWallet.GetAccount(0);

            // Get the destination account
            var account_1 = _secondWallet.GetAccount(1);
            var account_2 = _secondWallet.GetAccount(2);
            var account_3 = _secondWallet.GetAccount(3);
            var account_17 = _secondWallet.GetAccount(4);

            Dictionary<string, AccountInfo> dic=new Dictionary<string, AccountInfo>();
            for (int i = 1; i < 10; i++) {
                var firstAccountInfo = await rpcClient.GetAccountInfoAsync(firstAccount.PublicKey);
                dic.Add(_secondWallet.GetAccount(i).PublicKey, firstAccountInfo.Result.Value);
            }
            var t = dic;

        }

        /// <summary>
        /// 获取关联账户
        /// </summary>
        [Fact]
        public async void GetAssociatedTokenAccountTest() {
           var wallet1= BaseService.CreateNewWallet();
            Solnet.Wallet.Account mintAccount = _ownsWallet.GetAccount(322);//用于铸造币的用户
           var res=await BaseService.GetAssociatedTokenAccountAsync(wallet1.Account, mintAccount, wallet1.Account);
        }

        #region 注册测试

        /// <summary>
        /// 软件注册测试
        /// </summary>
        [Fact]
        public void RegisterTest() {

            // 获取硬件信息
            #region 客户端生成
            string hardwareInfo = RegistrationVerificationUtils.GetHardwareInfo();
            string key = _ownsWallet.Account.PublicKey.Key.Substring(0, 24);
            var encryptData = EncryptUtils.Encrypt3Des(hardwareInfo, key);
            #endregion

            #region 服务端验证
            var decryptData = EncryptUtils.Decrypt3Des(encryptData, key);
            // 签名
            string signature = RegistrationVerificationUtils.SignData(hardwareInfo, _ownsWallet.Account.PrivateKey);

            #endregion
            // 客户端验签
            bool isValid = RegistrationVerificationUtils.VerifySignature(hardwareInfo, signature, _ownsWallet.Account.PublicKey);
           Assert.True(isValid);
        }
        #endregion

        #region 后续再测试
        /// <summary>
        /// SecretKeyStore
        /// </summary>
        [Fact]
        public void SecretKeyStoreTest()
        {


            string filePath = AppDomain.CurrentDomain.BaseDirectory + "\\Resources\\walletInfo.txt";

            string passphrase = "";
            // Initialize the KeyStore
            var secretKeyStoreService = new SolanaKeyStoreService();

            // Restore a wallet from the json file generated by solana-keygen,
            // with the same passphrase used when generating the keys
            var wallet = secretKeyStoreService.RestoreKeystore(filePath, passphrase);

        }
        #endregion

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
    }
}