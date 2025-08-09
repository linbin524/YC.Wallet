using Newtonsoft.Json.Linq;
using Solnet.Extensions;
using Solnet.Extensions.TokenMint;
using Solnet.Metaplex.Candymachine.Core.Types;
using Solnet.Metaplex.NFT;
using Solnet.Metaplex.NFT.Library;
using Solnet.Programs;
using Solnet.Programs.Models;
using Solnet.Programs.Models.NameService;
using Solnet.Programs.Utilities;
using Solnet.Rpc;
using Solnet.Rpc.Builders;
using Solnet.Rpc.Core.Http;
using Solnet.Rpc.Messages;
using Solnet.Rpc.Models;
using Solnet.Rpc.Types;
using Solnet.Wallet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using YC.Common.ShareUtils;
using YC.SolanaSdkService;
using YC.SolanaSdkService.DTO;
using Creator = Solnet.Metaplex.NFT.Library.Creator;

namespace SolanaTest
{
    public class TokenTest
    {
        public Wallet _ownsWallet { get; set; }//测试使用的钱包

        public Wallet _secondWallet { get; set; }//测试钱包2
        public IRpcClient _rpcClient { get; set; }//测试请求的PRC

        public TokenTest()
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
        /// 创建Token 代币,发布成功，使用mint的PublicKey 作为address 到explorer.solana.com 去查看
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task CreateTokenTest()
        {
            #region 基础配置
            // 连接到 Solana Devnet
            var rpcClient = ClientFactory.GetClient(Cluster.DevNet);
            // 用于支付交易费用和操作代币的账户
            var payerAccount = _ownsWallet.Account;

            // 检查 payerAccount 余额
            var balance = await rpcClient.GetBalanceAsync(payerAccount.PublicKey);
            Console.WriteLine($"Payer account balance: {balance.Result} Lamports");

            // 获取创建账户所需的最低租金豁免余额
            ulong minBalanceForExemptionAcc = rpcClient.GetMinimumBalanceForRentExemption(TokenProgram.TokenAccountDataSize).Result;
            Console.WriteLine($"MinBalanceForRentExemption Account >> {minBalanceForExemptionAcc}");
            ulong minBalanceForExemptionMint = rpcClient.GetMinimumBalanceForRentExemption(TokenProgram.MintAccountDataSize).Result;
            Console.WriteLine($"MinBalanceForRentExemption Mint Account >> {minBalanceForExemptionMint}");
            // 代币相关信息
            string tokenName = "CTK";
            string tokenSymbol = "CTK";
            string uri = "https://example.com/token_metadata.json"; // 这里需要替换为实际的元数据 JSON 文件链接
            ulong tokenSupply = 10000000000; // 代币总量为 1 亿

            // 创建代币铸造账户
            var mintAccount = _ownsWallet.GetAccount(322);
            var initialAccount = _ownsWallet.GetAccount(321);

            var mintBalance = await rpcClient.GetBalanceAsync(mintAccount.PublicKey);
            if (mintBalance.Result.Value < minBalanceForExemptionMint)
            {
                var blockHash = rpcClient.GetLatestBlockHash();
                var tx = new TransactionBuilder().
                       SetRecentBlockHash(blockHash.Result.Value.Blockhash).
                       SetFeePayer(payerAccount).//有收取手续费
                       AddInstruction(SetComputeUnitLimit(30000)).
                       AddInstruction(SetComputeUnitPrice(1000000)).
                       AddInstruction(MemoProgram.NewMemo(payerAccount, "Hello from Sol Dev Net by powerlin :)")).
                       AddInstruction(SystemProgram.Transfer(payerAccount, mintAccount, 500000000)).//交易燃料费用要注意
                       Build(payerAccount);

                var firstSig = await rpcClient.SendTransactionAsync(tx);
            } 
            #endregion
            #region 构建交易
            mintBalance = await rpcClient.GetBalanceAsync(mintAccount.PublicKey);
            var mintAuthority = payerAccount.PublicKey;
            var freezeAuthority = payerAccount.PublicKey;


            // 创建Mint代币账户指令
            var rentExemption = await rpcClient.GetMinimumBalanceForRentExemptionAsync(TokenProgram.MintAccountDataSize);
            var createMintAccountInstruction = SystemProgram.CreateAccount(
                    payerAccount.PublicKey,
                    mintAccount.PublicKey,
                    minBalanceForExemptionMint,
                    TokenProgram.MintAccountDataSize,
                    TokenProgram.ProgramIdKey);//ProgramIdKey 可能不需要更改，猜测可能是引用指定的智能合约程序的id

            // 初始化代币铸造账户指令 1
            var initializeMintInstruction = TokenProgram.InitializeMint(
               mintAccount.PublicKey,
                    2,
                    payerAccount.PublicKey,//管理mint账户后续发币等权限的账户
                    payerAccount.PublicKey);//冻结权的账户;


            // 创建关联代币账户
            var associatedTokenAccountAddress = AssociatedTokenAccountProgram.DeriveAssociatedTokenAccount(
                payerAccount.PublicKey,
                mintAccount.PublicKey
            );
            var getAtaInfo = await _rpcClient.GetAccountInfoAsync(associatedTokenAccountAddress);

            // 铸造代币指令 
            var mintToInstruction = TokenProgram.MintTo(
                mint: mintAccount,
                destination: initialAccount,
                amount: tokenSupply,
                mintAuthority: mintAuthority
            );

            // 创建元数据账户地址
            var metadataProgramId = new Solnet.Wallet.PublicKey("metaqbxxUerdq28cj1RbAWkYQm3ybzjb6a8bt518x1s");
            Solnet.Wallet.PublicKey metadataAddress;
            Solnet.Wallet.PublicKey.TryFindProgramAddress(
                new List<byte[]>
                {
                    System.Text.Encoding.UTF8.GetBytes("metadata"),
                    metadataProgramId.KeyBytes,
                    mintAccount.PublicKey.KeyBytes
                },
                metadataProgramId, out metadataAddress, out _
            );

            // 创建元数据账户指令
            var createMetadataInstruction = MetadataProgram.CreateMetadataAccount(
                metadataKey: metadataAddress,
                mintKey: mintAccount.PublicKey,
                authorityKey: mintAuthority,
                payerKey: payerAccount.PublicKey,
                updateAuthority: mintAuthority,
                new Metadata()
                {
                    name = tokenName,
                    symbol = tokenSymbol,
                    uri = uri,
                    sellerFeeBasisPoints = 0,
                     creators = new List<Creator>() {
                    new Creator(payerAccount.PublicKey,100,true)
                    }
                },
                TokenStandard.Fungible,
                isMutable: true,
                true
            );

            // 构建交易
            var transactionBuilder = new TransactionBuilder();
            transactionBuilder.AddInstruction(createMintAccountInstruction);
            transactionBuilder.AddInstruction(initializeMintInstruction);
            // 添加创建初始账户的指令
            transactionBuilder.AddInstruction(SystemProgram.CreateAccount(
                payerAccount.PublicKey,
                initialAccount.PublicKey,
                minBalanceForExemptionAcc,
                TokenProgram.TokenAccountDataSize,
                TokenProgram.ProgramIdKey));

            // 添加初始化初始账户的指令
            transactionBuilder.AddInstruction(TokenProgram.InitializeAccount(
                initialAccount.PublicKey,
                mintAccount.PublicKey,
                payerAccount.PublicKey));

            //if (getAtaInfo.Result.Value == null)
            //{
            //    var createAssociatedTokenAccountInstruction = AssociatedTokenAccountProgram.CreateAssociatedTokenAccount(
            //       payerAccount.PublicKey,
            //       payerAccount.PublicKey,
            //       mintAccount.PublicKey
            //   );
            //    transactionBuilder.AddInstruction(createAssociatedTokenAccountInstruction);
            //}

            transactionBuilder.AddInstruction(mintToInstruction);
            // 添加备忘录指令
            transactionBuilder.AddInstruction(MemoProgram.NewMemo(initialAccount.PublicKey, "创建token BCRT"));
            transactionBuilder.AddInstruction(createMetadataInstruction);

            #endregion
            // 获取最近的区块哈希
            var recentBlockHash = await rpcClient.GetLatestBlockHashAsync(Commitment.Finalized);
            transactionBuilder.SetRecentBlockHash(recentBlockHash.Result.Value.Blockhash);

            // 设置费用支付者和签名者
            transactionBuilder.SetFeePayer(payerAccount.PublicKey);
            //transactionBuilder.Sign(new List<Solnet.Wallet.Account>() { payerAccount, mintAccount });

            // 构建交易
            var transaction = transactionBuilder.Build(new List<Solnet.Wallet.Account>() { payerAccount, mintAccount,initialAccount });

            // 发送交易
            var sendResult = await rpcClient.SendTransactionAsync(transaction);
            Console.WriteLine($"Transaction sent: {sendResult.Result}");
        }


        /// <summary>
        /// 创建Token 代币,使用关联账户存储代币
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task CreateToken2Test()
        {
            #region 基础配置
            // 连接到 Solana Devnet
            var rpcClient = ClientFactory.GetClient(Cluster.DevNet);
            // 用于支付交易费用和操作代币的账户
            var payerAccount = _ownsWallet.Account;

            // 检查 payerAccount 余额
            var balance = await rpcClient.GetBalanceAsync(payerAccount.PublicKey);
            Console.WriteLine($"Payer account balance: {balance.Result} Lamports");

            // 获取创建账户所需的最低租金豁免余额
            ulong minBalanceForExemptionAcc = rpcClient.GetMinimumBalanceForRentExemption(TokenProgram.TokenAccountDataSize).Result;
            Console.WriteLine($"MinBalanceForRentExemption Account >> {minBalanceForExemptionAcc}");
            ulong minBalanceForExemptionMint = rpcClient.GetMinimumBalanceForRentExemption(TokenProgram.MintAccountDataSize).Result;
            Console.WriteLine($"MinBalanceForRentExemption Mint Account >> {minBalanceForExemptionMint}");
            // 代币相关信息
            string tokenName = "A-DDD";
            string tokenSymbol = "A-DDD Coin";
            string uri = "https://ipfs.io/ipfs/Qmag7FFbmwjCrmpEtwTpCPL7SmgpbXTXG4Cx94g3wLiupe"; // 这里需要替换为实际的元数据 JSON 文件链接
            ulong tokenSupply = 10000000000; // 代币总量为 1 亿

            // 创建代币铸造账户
            var mintAccount = _ownsWallet.GetAccount(924);

            var mintBalance = await rpcClient.GetBalanceAsync(mintAccount.PublicKey);
            if (mintBalance.Result.Value < minBalanceForExemptionMint)
            {
                var blockHash = rpcClient.GetLatestBlockHash();
                var tx = new TransactionBuilder().
                       SetRecentBlockHash(blockHash.Result.Value.Blockhash).
                       SetFeePayer(payerAccount).//有收取手续费
                       AddInstruction(SetComputeUnitLimit(30000)).
                       AddInstruction(SetComputeUnitPrice(1000000)).
                       AddInstruction(MemoProgram.NewMemo(payerAccount, "Hello from Sol Dev Net by powerlin :)")).
                       AddInstruction(SystemProgram.Transfer(payerAccount, mintAccount, 500000000)).//交易燃料费用要注意
                       Build(payerAccount);

                var firstSig = await rpcClient.SendTransactionAsync(tx);
            }
            #endregion
            #region 构建交易
            mintBalance = await rpcClient.GetBalanceAsync(mintAccount.PublicKey);
            var mintAuthority = payerAccount.PublicKey;
            var freezeAuthority = payerAccount.PublicKey;


            // 创建Mint代币账户指令
            var rentExemption = await rpcClient.GetMinimumBalanceForRentExemptionAsync(TokenProgram.MintAccountDataSize);
            var createMintAccountInstruction = SystemProgram.CreateAccount(
                    payerAccount.PublicKey,
                    mintAccount.PublicKey,
                    minBalanceForExemptionMint, 
                    TokenProgram.MintAccountDataSize,
                    TokenProgram.ProgramIdKey);//ProgramIdKey 可能不需要更改，猜测可能是引用指定的智能合约程序的id

            // 初始化代币铸造账户指令 1
            var initializeMintInstruction = TokenProgram.InitializeMint(
               mintAccount.PublicKey,
                    2,
                    payerAccount.PublicKey,//管理mint账户后续发币等权限的账户
                   payerAccount.PublicKey);//冻结权的账户;


            // 创建关联代币账户
            var associatedTokenAccountAddress = AssociatedTokenAccountProgram.DeriveAssociatedTokenAccount(
                payerAccount.PublicKey,
                mintAccount.PublicKey
            );
            var getAtaInfo = await _rpcClient.GetAccountInfoAsync(associatedTokenAccountAddress);

            // 铸造代币指令 
            var mintToInstruction = TokenProgram.MintTo(
                mint: mintAccount,
                destination: associatedTokenAccountAddress,
                amount: tokenSupply,
                mintAuthority: mintAuthority
            );

            // 创建元数据账户地址
            var metadataProgramId = new Solnet.Wallet.PublicKey("metaqbxxUerdq28cj1RbAWkYQm3ybzjb6a8bt518x1s");
            Solnet.Wallet.PublicKey metadataAddress;
            Solnet.Wallet.PublicKey.TryFindProgramAddress(
                new List<byte[]>
                {
                    System.Text.Encoding.UTF8.GetBytes("metadata"),
                    metadataProgramId.KeyBytes,
                    mintAccount.PublicKey.KeyBytes
                },
                metadataProgramId, out metadataAddress, out _
            );

            // 创建元数据账户指令
            var createMetadataInstruction = MetadataProgram.CreateMetadataAccount(
                metadataKey: metadataAddress,
                mintKey: mintAccount.PublicKey,
                authorityKey: mintAuthority,
                payerKey: payerAccount.PublicKey,
                updateAuthority: mintAuthority,
                new Metadata()
                {
                    name = tokenName,
                    symbol = tokenSymbol,
                    uri = uri,
                    sellerFeeBasisPoints = 0,
                    creators = new List<Creator>() {
                    new Creator(payerAccount.PublicKey,100,true)
                    }
                },
                TokenStandard.Fungible,
                isMutable: true,
                true
            );

            // 构建交易
            var transactionBuilder = new TransactionBuilder();
            transactionBuilder.AddInstruction(createMintAccountInstruction);
            transactionBuilder.AddInstruction(initializeMintInstruction);


            if (getAtaInfo.Result.Value == null)
            {
                var createAssociatedTokenAccountInstruction = AssociatedTokenAccountProgram.CreateAssociatedTokenAccount(
                   payerAccount.PublicKey,
                   payerAccount.PublicKey,
                   mintAccount.PublicKey
               );
                transactionBuilder.AddInstruction(createAssociatedTokenAccountInstruction);
            }

            transactionBuilder.AddInstruction(mintToInstruction);
            
            transactionBuilder.AddInstruction(createMetadataInstruction);
            transactionBuilder.AddInstruction(MemoProgram.NewMemo(payerAccount, "Hello from Sol Dev Net Created ACKT by powerlin :)"));
            #endregion
            // 获取最近的区块哈希
            var recentBlockHash = await rpcClient.GetLatestBlockHashAsync(Commitment.Finalized);
            transactionBuilder.SetRecentBlockHash(recentBlockHash.Result.Value.Blockhash);

            // 设置费用支付者和签名者
            transactionBuilder.SetFeePayer(payerAccount.PublicKey);
           
            // 构建交易
            var transaction = transactionBuilder.Build(new List<Solnet.Wallet.Account>() { payerAccount, mintAccount });

            // 发送交易
            var sendResult = await rpcClient.SendTransactionAsync(transaction);
            Console.WriteLine($"Transaction sent: {sendResult.Result}");
        }
        /// <summary>
        /// 增发对应的币
        /// </summary>
        [Fact]
        public async Task AddMintTokenTest() {

            // 连接到 Solana Devnet
            var rpcClient = _rpcClient;

            // 张三账户
            Solnet.Wallet.Account mintAccount = _ownsWallet.GetAccount(322);//用于铸造币的用户
            Console.WriteLine($"张三 铸造账户MintAccount: {mintAccount}");
            Solnet.Wallet.Account ownerAccount = _ownsWallet.GetAccount(0);//管理员用户
            Console.WriteLine($"张三 管理账户OwnerAccount: {ownerAccount}");
            var zhangsanAssociatedTokenAccount = AssociatedTokenAccountProgram.DeriveAssociatedTokenAccount(_ownsWallet.Account.PublicKey, mintAccount.PublicKey);
            Solnet.Wallet.Account initalAccount = _ownsWallet.GetAccount(321);//代币用户
            Console.WriteLine($"张三 关联自定义Token的InitialAccount: {initalAccount}");


            // 代币铸造账户【权限掌控】
            Solnet.Wallet.PublicKey mintAuthority = ownerAccount.PublicKey;
            // 目标接收账户
            Solnet.Wallet.PublicKey destinationAccount = initalAccount;
            // 要铸造的代币数量
            ulong amountToMint = 98766000;
            // 铸造代币指令
            var mintToInstruction = TokenProgram.MintTo(
                mint: mintAccount.PublicKey,
                destination: destinationAccount,
                amount: amountToMint,
                mintAuthority: mintAuthority
            );
            // 构建交易
            var transactionBuilder = new TransactionBuilder();
            transactionBuilder.AddInstruction(mintToInstruction);
            // 获取最近的区块哈希
            var recentBlockHash = await rpcClient.GetLatestBlockHashAsync(Commitment.Finalized);
            transactionBuilder.SetRecentBlockHash(recentBlockHash.Result.Value.Blockhash);
            // 设置费用支付者和签名者
            var feePayer = ownerAccount;
            transactionBuilder.SetFeePayer(feePayer.PublicKey);
            // 构建交易
            var transaction = transactionBuilder.Build(new List<Solnet.Wallet.Account>() { ownerAccount });
            var testTrans = await rpcClient.SimulateTransactionAsync(transaction);
            // 发送交易
            var sendResult = await rpcClient.SendTransactionAsync(transaction);
            Console.WriteLine($"Transaction sent: {sendResult.Result}");
        
    }

        /// <summary>
        /// 销毁对应的币,主钱包 销毁second 钱包的代币，
        /// 只能使用自己钱包所有者才可以销毁
        /// 
        /// </summary>
        [Fact]
        public async Task BurnTokenTest()
        {
           
            IRpcClient rpcClient = _rpcClient;
            Solnet.Wallet.PublicKey owner = _ownsWallet.Account.PublicKey;
            // 代币账户地址
            Solnet.Wallet.PublicKey tokenAccount = new Solnet.Wallet.PublicKey("3DoNg4xg4TU6qisYvMysDCzCt8JH4AdnfMxfaqbg5h7e");
            // 代币的 mint 地址
            Solnet.Wallet.PublicKey mint = new Solnet.Wallet.PublicKey("AhxWF63HAXZbFaCP6HAEVE8X5EYCBhbQVovBFtVqiQBi");
            ulong amount = 200000;
            #region 另一个钱包
            // 获取李四的关联代币账户
            //var lisiAssociatedTokenAccount = AssociatedTokenAccountProgram.DeriveAssociatedTokenAccount(_secondWallet.Account.PublicKey, mint);

            //Assert.Equal(tokenAccount, lisiAssociatedTokenAccount);

            //// 要销毁的代币数量
            //ulong amount = 200000;

            //// 检查代币账户余额
            //var balanceInfo = await rpcClient.GetTokenAccountBalanceAsync(lisiAssociatedTokenAccount);
            //if (balanceInfo.WasSuccessful)
            //{
            //    var balance = ulong.Parse(balanceInfo.Result.Value.Amount);
            //    if (amount > balance)
            //    {
            //        Console.WriteLine("代币账户余额不足，无法销毁指定数量的代币。");
            //        return;
            //    }
            //}
            //else
            //{
            //    Console.WriteLine($"查询代币账户余额时出错: {balanceInfo.Reason}");
            //    return;
            //}

            //// 检查代币账户是否初始化
            //var accountInfo = await rpcClient.GetAccountInfoAsync(lisiAssociatedTokenAccount);
            //byte[] accountDataBytes = Convert.FromBase64String(accountInfo.Result.Value.Data[0]);
            //var accountData = RecordHeader.Deserialize(accountDataBytes);
            //if (accountInfo.Result.Value == null || accountInfo.Result.Value.Data.Count == 0)
            //{
            //    Console.WriteLine("代币账户未初始化，请先初始化账户。");
            //    return;
            //} 
            #endregion


            // 创建销毁代币的指令
            TransactionInstruction destroyInstruction = TokenProgram.Burn(
                tokenAccount,
                mint,
                amount,
                _secondWallet.Account.PublicKey
                );

            // 获取最近的区块哈希
            var recentBlockHash = await rpcClient.GetLatestBlockHashAsync(Commitment.Finalized);

            // 创建交易
            TransactionBuilder transactionBuilder = new TransactionBuilder()
               .SetRecentBlockHash(recentBlockHash.Result.Value.Blockhash)
               .SetFeePayer(_secondWallet.Account)
               .AddInstruction(destroyInstruction);

            // 对交易进行签名
            var transaction = transactionBuilder.Build(new List<Solnet.Wallet.Account>() { _secondWallet.Account });
            var test=rpcClient.SimulateTransaction(transaction);
            // 发送交易
            RequestResult<string> sendTransactionResult = await rpcClient.SendTransactionAsync(transaction);
            if (sendTransactionResult.WasSuccessful)
            {
                Console.WriteLine($"代币销毁交易已发送，交易 ID: {sendTransactionResult.Result}");
            }
            else
            {
                Console.WriteLine($"发送交易时出错: {sendTransactionResult.Reason}");
            }
        }
        
        
        /// <summary>
        /// 更新metadata 元数据,有问题，测试底层返回
        /// [
        //  "Program metaqbxxUerdq28cj1RbAWkYQm3ybzjb6a8bt518x1s invoke [1]",
        //  "Program log: This instruction was deprecated in a previous release and is now removed",
        //  "Program metaqbxxUerdq28cj1RbAWkYQm3ybzjb6a8bt518x1s consumed 2371 of 200000 compute units",
        //  "Program metaqbxxUerdq28cj1RbAWkYQm3ybzjb6a8bt518x1s failed: custom program error: 0x4b"
        //]
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task UpdateTokenMetadataTest()
        {
            // 用于支付交易费用和操作代币的账户
            var payerAccount = _ownsWallet.Account;
            // 创建代币铸造账户
            var mintAccount = _ownsWallet.GetAccount(422);
            Console.WriteLine($"Mint Account Public Key: {mintAccount.PublicKey}");

            // 创建元数据账户地址
            var metadataProgramId = new Solnet.Wallet.PublicKey("metaqbxxUerdq28cj1RbAWkYQm3ybzjb6a8bt518x1s");
            Solnet.Wallet.PublicKey metadataAddress;
            Solnet.Wallet.PublicKey.TryFindProgramAddress(
                new List<byte[]>
                {
            System.Text.Encoding.UTF8.GetBytes("metadata"),
            metadataProgramId.KeyBytes,
            mintAccount.PublicKey.KeyBytes
                },
                metadataProgramId, out metadataAddress, out _
            );
            Console.WriteLine($"Metadata Account Address: {metadataAddress}");

            // 连接到 Solana Devnet
            var rpcClient = _rpcClient;
            // 元数据账户地址
            Solnet.Wallet.PublicKey metadataAccount = metadataAddress;
            // 更新权限账户
            Solnet.Wallet.Account updateAuthority = payerAccount;

            string tokenName = "ACKT";
            string tokenSymbol = "ACKT";
            string uri = "https://ipfs.io/ipfs/Qmag7FFbmwjCrmpEtwTpCPL7SmgpbXTXG4Cx94g3wLiupe"; // 这里需要替换为实际的元数据 JSON 文件链接
            ulong tokenSupply = 10000000000; // 代币总量为 1 亿

            // 新的元数据信息
            var newMetadata = new Metadata()
            {
                name = tokenName,
                symbol = tokenSymbol,
                uri = uri,
                sellerFeeBasisPoints = 0,
                creators = new List<Creator>() { new Creator(payerAccount.PublicKey, 100, true) }
            };

            // 更新元数据指令
            var updateMetadataInstruction = MetadataProgram.UpdateMetadataAccount(
                metadataKey: metadataAccount,
                updateAuthority: updateAuthority.PublicKey,
                newUpdateAuthority: updateAuthority.PublicKey,
                data: newMetadata,
                primarySaleHappend: true
            );

            // 构建交易
            var transactionBuilder = new TransactionBuilder();
            transactionBuilder.AddInstruction(updateMetadataInstruction);

            // 获取最近的区块哈希
            var recentBlockHash = await rpcClient.GetLatestBlockHashAsync(Commitment.Finalized);
            Console.WriteLine($"Recent Block Hash: {recentBlockHash.Result.Value.Blockhash}");
            transactionBuilder.SetRecentBlockHash(recentBlockHash.Result.Value.Blockhash);

            // 设置费用支付者和签名者
            var feePayer = payerAccount;
            transactionBuilder.SetFeePayer(feePayer.PublicKey);

            // 构建交易
            var transaction = transactionBuilder.Build(new List<Solnet.Wallet.Account>() { payerAccount });
           var testTx= await rpcClient.SimulateTransactionAsync(transaction);
            // 发送交易
            var sendResult = await rpcClient.SendTransactionAsync(transaction);
            Console.WriteLine($"Transaction sent: {sendResult.Result}");
            if (!sendResult.WasSuccessful)
            {
                Console.WriteLine($"Transaction failed. Reason: {sendResult.Reason}");
            }
        }


        /// <summary>
        /// 将自定义的代币转移给其他用户(关联账户)
        /// </summary>
        [Fact]
        public async void TransferSPLTokenbyAssociatedTest()
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
               .SetFeePayer(ownerAccount)
               .Build(ownerAccount);

           var transferSignature = await _rpcClient.SendTransactionAsync(transferTransaction);
            Console.WriteLine($"转移代币到李四关联代币账户交易签名: {transferSignature.Result}");
            await Task.Delay(5000);


            // 李四查询代币是否到账
            var lisiAtaInfo = await _rpcClient.GetTokenAccountBalanceAsync(lisiAssociatedTokenAccount);
            if (lisiAtaInfo.WasSuccessful)
            {
                Console.WriteLine($"李四的 BCRT 代币余额: {lisiAtaInfo.Result.Value.Amount}");
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
        public async void GetTokenBalanceTest()
        {
            // 张三账户
            Solnet.Wallet.Account mintAccount = _ownsWallet.GetAccount(322);//用于铸造币的用户
            Console.WriteLine($"张三 铸造账户MintAccount: {mintAccount}");
            //查询创建的代币以及数量
            Solnet.Wallet.Account initalAccount = _ownsWallet.GetAccount(321);//代币用户
            var zhangsanAtaInfo = await _rpcClient.GetTokenAccountBalanceAsync(initalAccount.PublicKey);
            // 获取李四的关联代币账户
            var lisiAssociatedTokenAccount = AssociatedTokenAccountProgram.DeriveAssociatedTokenAccount(_secondWallet.Account.PublicKey, mintAccount.PublicKey);

            //查询创建的代币以及数量
            var lisiAtaInfo = await _rpcClient.GetTokenAccountBalanceAsync(lisiAssociatedTokenAccount);
            // load snapshot of wallet and sub-accounts

            var tokens = new TokenMintResolver();
            var testToken = new TokenDef(mintAccount.PublicKey, "CTK", "CTK", 2);
            tokens.Add(testToken);
            TokenWallet tokenWallet = TokenWallet.Load(_rpcClient, tokens, _secondWallet.Account.PublicKey);
            var balances = tokenWallet.Balances();
            var info = tokenWallet.TokenAccounts();
        }

       


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
