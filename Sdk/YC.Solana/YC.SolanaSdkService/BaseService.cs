using Solnet.Extensions.TokenMint;
using Solnet.Extensions;
using Solnet.Rpc;
using Solnet.Rpc.Core.Http;
using Solnet.Rpc.Messages;
using Solnet.Rpc.Models;
using Solnet.Wallet;
using Solnet.Programs;
using YC.Model;
using Solnet.Extensions.Models;
using Solnet.Rpc.Builders;
using Solnet.Rpc.Types;
using Org.BouncyCastle.Asn1.Ocsp;
using Solnet.Programs.Models.NameService;
using System.Reflection;
using static Solnet.Programs.Models.TokenProgram.TokenAccount;
using Solnet.Programs.Utilities;
using Org.BouncyCastle.Asn1.Pkcs;
using Xunit;
using YC.Common.ShareUtils;
using Solnet.Programs.Models.TokenProgram;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using Mapster;
using Xunit.Sdk;
using SixLabors.ImageSharp.Drawing;
using System.Collections;
using System.Text;
using Solnet.Wallet.Utilities;
using Org.BouncyCastle.Crypto.Agreement.Srp;

namespace YC.SolanaSdkService
{
    public class BaseService
    {
        //internal static IRpcClient _rpcClient = BasicConfig.RpcClient;//这个方式第一次初始化就不会再变了
        internal static IRpcClient _rpcClient { get=>BasicConfig.RpcClient;}//这个 方式每次会从get中回去最新的内容

            


        #region 钱包类操作
        
        /// <summary>
        /// 创建一个新的钱包
        /// </summary>
        /// <returns></returns>
        public static Wallet CreateNewWallet()
        {
            var newMnemonic = new Solnet.Wallet.Bip39.Mnemonic(Solnet.Wallet.Bip39.WordList.English, Solnet.Wallet.Bip39.WordCount.Twelve);
            var wallet = new Wallet(newMnemonic);
            return wallet;
        }

        /// <summary>
        /// 从文本中导入钱包
        /// </summary>
        public static Wallet InitWallet(string filePath)
        {
            string walletInfo = "";
            FileUtils.ReadFile(filePath, out walletInfo);
            var tempWalletObj = walletInfo.ToObject<DTO.WalletDto>();//不能直接转为wallet 对象
            var wallet = new Wallet(string.Join(" ", tempWalletObj.Mnemonic.Words), Solnet.Wallet.Bip39.WordList.English);
            return wallet;
        }

        /// <summary>
        /// 从文本中导入钱包
        /// </summary>
        public static List<Wallet> BatchInitWallet(string filePath)
        {
            string walletInfo = "";
            FileUtils.ReadFile(filePath, out walletInfo);
            var tempWalletObjs = walletInfo.ToObject<List<DTO.WalletDto>>();//不能直接转为wallet 对象
            List<Wallet> wallets = new List<Wallet>();
            tempWalletObjs.ForEach(x => {
                var wallet = new Wallet(string.Join(" ", x.Mnemonic.Words), Solnet.Wallet.Bip39.WordList.English);
                wallets.Add(wallet);
            }
            );
            return wallets;
        }

        /// <summary>
        /// 通过助记词获取钱包
        /// </summary>
        /// <param name="MnemonicWords"></param>
        /// <returns></returns>
        public static Wallet GetWalletByMnemonicWords(string MnemonicWords) {

            var wallet = new Wallet(MnemonicWords, Solnet.Wallet.Bip39.WordList.English);
            return wallet;
        }
        #endregion

        #region 余额查询类操作

        /// <summary>
        /// 查询账户的Lamports余额
        /// </summary>
        /// <param name="account"></param>
        /// <returns></returns>
        public static async Task<ApiResult<ulong>> GetAccountLamportsBalanceAsync(Account account)
        {
            ApiResult<ulong> res = new ApiResult<ulong>();
            var t=_rpcClient;
            var result = await t.GetBalanceAsync(account.PublicKey);
            if (!result.WasSuccessful)
            {
                return res.NotOk($"查询失败，错误为：{result.RawRpcResponse}");
            }
            return res.Ok(result.Result.Value);
        }

        /// <summary>
        /// 异步：查询钱包所有各类代币 余额
        /// </summary>
        /// <param name="wallet">钱包账户</param>
        /// <param name="tokenDefList">特殊代币的标识</param>
        /// <returns></returns>
        public static async Task<ApiResult<TokenWalletFilterList>> GetWalletTokenBalanceAsync(Wallet wallet, List<TokenDef> tokenDefList = null)
        {
            var res = new ApiResult<TokenWalletFilterList>();
            var tokens = new TokenMintResolver();
            var wellKnownTokens = WellKnownTokens.All();//加载所有链上已知代币
            foreach (var tokenDef in wellKnownTokens)
            {
                tokens.Add(tokenDef);
            }
            if (tokenDefList != null)
            {
                //var testToken = new TokenDef(mintAccount.PublicKey, "CTK", "CTK", 2);
                foreach (var tokenDef in tokenDefList)
                {
                   bool isExist= wellKnownTokens.Any(x => x.TokenMint == tokenDef.TokenMint);//先排除其他的再加入
                    if (!isExist) {
                        tokens.Add(tokenDef);
                    } 
                }
            }

            TokenWallet tokenWallet = TokenWallet.Load(_rpcClient, tokens, wallet.Account.PublicKey);
            //var balances = tokenWallet.Balances();
            var info = tokenWallet.TokenAccounts().WithCustomFilter(x => tokens.KnownTokens.Keys.Contains(x.TokenMint));
            return res.Ok(info);

        }

        /// <summary>
        /// 同步情况下  查询钱包所有各类代币 余额
        /// </summary>
        /// <param name="wallet"></param>
        /// <param name="tokenDefList"></param>
        /// <returns></returns>
        public static ApiResult<TokenWalletFilterList> GetWalletTokenBalance(Wallet wallet, List<TokenDef> tokenDefList = null)
        {
            var res = new ApiResult<TokenWalletFilterList>();
            var tokens = new TokenMintResolver();
            var wellKnownTokens = WellKnownTokens.All();//加载所有链上已知代币
            foreach (var tokenDef in wellKnownTokens)
            {
                tokens.Add(tokenDef);
            }
            if (tokenDefList != null)
            {
                //var testToken = new TokenDef(mintAccount.PublicKey, "CTK", "CTK", 2);
                foreach (var tokenDef in tokenDefList)
                {
                    bool isExist = wellKnownTokens.Any(x => x.TokenMint == tokenDef.TokenMint);//先排除其他的再加入
                    if (!isExist)
                    {
                        tokens.Add(tokenDef);
                    }
                }
            }

            TokenWallet tokenWallet;
           var a= Task.Run(() => {
               tokenWallet = TokenWallet.Load(_rpcClient, tokens, wallet.Account.PublicKey);
               return tokenWallet;
           });
            a.GetAwaiter().GetResult();
           //var tokenKnownObj= tokens.KnownTokens.Values.Where(t=>t.Symbol=="CTK").FirstOrDefault();
           //var forToken= a.Result.TokenAccounts().ForToken(tokenKnownObj);
           var info = a.Result.TokenAccounts().WithCustomFilter(x=>tokens.KnownTokens.Keys.Contains(x.TokenMint));
            return res.Ok(info);

        }

        /// <summary>
        /// 通过钱包，关联账户查询SPL代币余额
        /// </summary>
        /// <param name="mintAccount">铸造币 铸造账户地址（唯一）</param>
        /// <param name="tokenAccount">查询用户的账户地址</param>
        /// <param name="wallet">钱包</param>
        /// <returns></returns>
        public static async Task<ApiResult<TokenBalance>> GetAssociatedTokenBalanceByWalletAsync(Account mintAccount,  Wallet wallet)
        {
            var res = new ApiResult<TokenBalance>();

            var associatedTokenAccount = AssociatedTokenAccountProgram.DeriveAssociatedTokenAccount(wallet.Account.PublicKey, mintAccount.PublicKey);
            var getAtaInfo = await _rpcClient.GetAccountInfoAsync(associatedTokenAccount);
            if (getAtaInfo.Result.Value == null)
            {
                return res.NotOk("该关联账户不存在！");
            }
            //查询创建的代币以及数量
            var balanceInfo = await _rpcClient.GetTokenAccountBalanceAsync(associatedTokenAccount);
            if (balanceInfo.WasSuccessful)
            {
                return res.Ok(balanceInfo.Result.Value);
            }
            else
            {
                return res.NotOk(balanceInfo.Reason);
            }
        }

        /// <summary>
        /// 指定的关联代币账户查询Token 余额
        /// </summary>
        /// <param name="associatedTokenAccount">关联代币账户</param>
        /// <returns></returns>
        public static async Task<ApiResult<TokenBalance>> GetTokenBalanceAsync(PublicKey tokenAccount)
        {
            var res = new ApiResult<TokenBalance>();
            
            //查询创建的代币以及数量
            var balanceInfo = await _rpcClient.GetTokenAccountBalanceAsync(tokenAccount);
            if (balanceInfo.WasSuccessful)
            {
                return res.Ok(balanceInfo.Result.Value);
            }
            else
            {
                return res.NotOk(balanceInfo.Reason);
            }
        }

        #endregion

        #region 账户类操作

        /// <summary>
        /// 创建一个新的账户
        /// </summary>
        /// <returns></returns>
        public static Solnet.Wallet.Account CreateAccount()
        {
            var account = new Solnet.Wallet.Account();
            var publicKey = account.PublicKey;
            var privateKey = account.PrivateKey;
            return account;
        }
        
        /// <summary>
        /// 将账户注册到网络中
        /// </summary>
        /// <param name="payerAccount">支付交易手续费的账户</param>
        /// <param name="initialAccount">带注册账户</param>
        /// <param name="mintAccount">指定token铸造的账户</param>
        /// <returns></returns>
        public static async  Task<ApiResult<string>> CreateAccountByRegisteAsync(Account payerAccount, Account initialAccount,Account mintAccount) {
            ApiResult<string> res = new ApiResult<string>();
            // 获取创建账户所需的最低租金豁免余额
            ulong minBalanceForExemptionAcc = _rpcClient.GetMinimumBalanceForRentExemption(TokenProgram.TokenAccountDataSize).Result;
            var transactionBuilder = new TransactionBuilder();
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

            // 获取最近的区块哈希
            var recentBlockHash = await _rpcClient.GetLatestBlockHashAsync(Commitment.Finalized);
            transactionBuilder.SetRecentBlockHash(recentBlockHash.Result.Value.Blockhash);

            // 设置费用支付者和签名者
            transactionBuilder.SetFeePayer(payerAccount.PublicKey);
            //transactionBuilder.Sign(new List<Solnet.Wallet.Account>() { payerAccount, mintAccount });

            // 构建交易
            var transaction = transactionBuilder.Build(new List<Solnet.Wallet.Account>() { payerAccount, initialAccount });

            // 发送交易
            var sendResult = await _rpcClient.SendTransactionAsync(transaction);

            if (!sendResult.WasSuccessful) {
                return res.NotOk($"交易失败，错误信息：{sendResult.RawRpcResponse}");    
            }
            return res.Ok(sendResult.Result);
        }

       
        /// <summary>
        /// 获取关联账户
        /// </summary>
        /// <param name="ownerAuthority">通常是钱包对应主账户，一个钱包只能有一种对应token 的关联账户</param>
        /// <param name="tokenMint">铸造token 对应的铸造账户</param>
        /// <param name="payAccount">支付交易的账户，通常是钱包主账户</param>
        /// <returns></returns>
        public static async Task<ApiResult<PublicKey>> GetAssociatedTokenAccountAsync(Account ownerAuthority, PublicKey tokenMint, Account payAccount) {
            ApiResult<PublicKey> res = new ApiResult<PublicKey>();
            var payer = payAccount;
            // 获取关联代币账户
            var associatedTokenAccountAddress = AssociatedTokenAccountProgram.DeriveAssociatedTokenAccount(
                ownerAuthority,
                tokenMint
            );
            var getAtaInfoResult = await _rpcClient.GetAccountInfoAsync(associatedTokenAccountAddress);
            //if (!getAtaInfoResult.WasSuccessful)
            //{
            //    return res.NotOk("未能获取关联代币帐户信息。");
            //}

            TransactionInstruction createAssociatedTokenAccountInstruction = null;
            if (getAtaInfoResult.Result.Value == null)
            {
                // 如果关联代币账户不存在，创建它
                createAssociatedTokenAccountInstruction = AssociatedTokenAccountProgram.CreateAssociatedTokenAccount(
                    payer,
                    ownerAuthority,
                    tokenMint
                );
                var createAtaTransaction = new TransactionBuilder().AddInstruction(createAssociatedTokenAccountInstruction)
                      .SetRecentBlockHash(_rpcClient.GetLatestBlockHash().Result.Value.Blockhash);
                byte[] transactionData;
                transactionData = createAtaTransaction.SetFeePayer(payAccount.PublicKey)
                     .Build(new List<Account>() { payer });
                var createAtaSignature = await _rpcClient.SendTransactionAsync(transactionData);
                if (!createAtaSignature.WasSuccessful) {
                    return res.NotOk($"创建关联账户失败，详细信息：{createAtaSignature.RawRpcResponse}.");
                }
            }
            return res.Ok(associatedTokenAccountAddress);

        }

        /// <summary>
        /// 创建关联账户
        /// </summary>
        /// <param name="ownerAuthority">通常是钱包对应主账户，一个钱包只能有一种对应token 的关联账户</param>
        /// <param name="tokenMint">铸造token 对应的铸造账户</param>
        /// <param name="payAccount">支付交易的账户，通常是钱包主账户</param>
        /// <returns></returns>
        public static async Task<ApiResult<PublicKey>> CreateAssociatedTokenAccountAsync(Account ownerAuthority, PublicKey tokenMint, Account payAccount)
        {
            ApiResult<PublicKey> res = new ApiResult<PublicKey>();
            var payer = payAccount;
            // 获取关联代币账户
            var associatedTokenAccountAddress = AssociatedTokenAccountProgram.DeriveAssociatedTokenAccount(
                ownerAuthority,
                tokenMint
            );
            var getAtaInfoResult = await _rpcClient.GetAccountInfoAsync(associatedTokenAccountAddress);
            if (!getAtaInfoResult.WasSuccessful)
            {
                return res.NotOk("未能获取关联代币帐户信息。");
            }
            if (getAtaInfoResult.Result.Value != null)
            {
                return res.NotOk("代币关联账户已经存在!\n 一个钱包只能有一个对应Token的关联账户.\n 关联账户为：" + associatedTokenAccountAddress);
            }
            else {
                TransactionInstruction createAssociatedTokenAccountInstruction = null;
                // 如果关联代币账户不存在，创建它
                createAssociatedTokenAccountInstruction = AssociatedTokenAccountProgram.CreateAssociatedTokenAccount(
                    payer,
                    ownerAuthority,
                    tokenMint
                );
                var createAtaTransaction = new TransactionBuilder().AddInstruction(createAssociatedTokenAccountInstruction)
                      .SetRecentBlockHash(_rpcClient.GetLatestBlockHash().Result.Value.Blockhash);
                byte[] transactionData;
                transactionData = createAtaTransaction.SetFeePayer(payAccount.PublicKey)
                     .Build(new List<Account>() { payer });
                var createAtaSignature = await _rpcClient.SendTransactionAsync(transactionData);
                if (!createAtaSignature.WasSuccessful)
                {
                    return res.NotOk($"创建关联账户失败，详细信息：{createAtaSignature.RawRpcResponse}.");
                }
                return res.Ok(associatedTokenAccountAddress);
            }
        }


        /// <summary>
        /// 冻结账户
        /// </summary>
        /// <param name="mintAuthority">指定代币 拥有冻结权限用户，通常是mintTo 阶段铸币</param>
        /// <param name="tokenMint">指定代币</param>
        /// <param name="tokenAccountToFreeze">需要被冻结代币账户</param>
        /// <param name="tokenProgramId">代币账户程序id</param>
        /// <param name="signers">多方签名</param>
        /// <returns></returns>
        public static async Task<ApiResult<string>> FreezeTokenAccountAsync(Account mintAuthority, PublicKey tokenMint, PublicKey tokenAccountToFreeze, PublicKey tokenProgramId, IEnumerable<PublicKey> signers = null)
        {
            var res = new ApiResult<string>();
            try
            {
                // 创建冻结代币账户的指令
                var freezeInstruction = TokenProgram.FreezeAccount(
                    tokenAccountToFreeze,
                    tokenMint,
                    mintAuthority.PublicKey,
                    tokenProgramId,
                    signers);
                // 获取最近的区块哈希
                var recentBlockHash = await _rpcClient.GetLatestBlockHashAsync(Commitment.Finalized);
                // 创建交易
                var transaction = new TransactionBuilder()
                   .SetRecentBlockHash(recentBlockHash.Result.Value.Blockhash)
                   .SetFeePayer(mintAuthority.PublicKey)
                   .AddInstruction(freezeInstruction)
                   .Build(mintAuthority);
                // 发送交易
                var signature = await _rpcClient.SendTransactionAsync(transaction);
                if (signature.WasSuccessful)
                {
                    return res.Ok(signature.Result);

                }
                else
                {
                    return res.NotOk(signature.Reason);

                }
            }
            catch (Exception ex)
            {
                return res.NotOk(ex.Message);

            }

        }

        /// <summary>
        /// 解冻账户
        /// </summary>
        /// <param name="mintAuthority">指定代币 拥有冻结权限用户，通常是mintTo 阶段铸币超级管理账户</param>
        /// <param name="tokenMint">指定铸币用户</param>
        /// <param name="tokenAccountToUnFreeze">需要被解冻结代币账户</param>
        /// <param name="tokenProgramId">代币账户程序id</param>
        /// <param name="signers">多方签名</param>
        /// <returns></returns>
        public static async Task<ApiResult<string>> ThawTokenAccountAsync(Account mintAuthority, PublicKey tokenMint, PublicKey tokenAccountToThaw, PublicKey tokenProgramId, IEnumerable<PublicKey> signers = null)
        {
            var res = new ApiResult<string>();
            try
            {
                // 创建冻结代币账户的指令
                var freezeInstruction = TokenProgram.ThawAccount(
                    tokenAccountToThaw,
                    tokenMint,
                    mintAuthority.PublicKey,
                    tokenProgramId,
                    signers);
                // 获取最近的区块哈希
                var recentBlockHash = await _rpcClient.GetLatestBlockHashAsync(Commitment.Finalized);
                // 创建交易
                var transaction = new TransactionBuilder()
                   .SetRecentBlockHash(recentBlockHash.Result.Value.Blockhash)
                   .SetFeePayer(mintAuthority.PublicKey)
                   .AddInstruction(freezeInstruction)
                   .Build(mintAuthority);
                // 发送交易
                var signature = await _rpcClient.SendTransactionAsync(transaction);
                if (signature.WasSuccessful)
                {
                    return res.Ok(signature.Result);

                }
                else
                {
                    return res.NotOk(signature.Reason);

                }
            }
            catch (Exception ex)
            {
                return res.NotOk(ex.Message);

            }

        }

        #endregion

        /// <summary>
        /// 交易Lamports
        /// </summary>
        /// <param name="fromAccount">支出方</param>
        /// <param name="toAccount">接收方</param>
        /// <param name="lamports"></param>
        /// <param name="MemoString">备注</param>
        /// <returns></returns>
        public static async Task<ApiResult<string>> TransferLamportsAsync(Account fromAccount, Account toAccount, ulong lamports, string MemoString = null) {
            ApiResult<string> res = new ApiResult<string>();
            if (fromAccount == null || toAccount == null) { 
            
                return res.NotOk("转入方或者接收方需要不为空。");
            }
            if (lamports==0)
            {
                return res.NotOk("lamports 必须大于0.");
            }
            // 获取租金豁免金额
            var rentExemption = await _rpcClient.GetMinimumBalanceForRentExemptionAsync(0);

            if (lamports < rentExemption.Result)
            {
                return res.NotOk($"转账金额 {lamports}的 Lamports 低于租金豁免金额 {rentExemption.Result} Lamports，交易可能失败。");
            }

            //查询各自的余额
            var fromAccountBalance = await _rpcClient.GetBalanceAsync(fromAccount.PublicKey);
            var toAccountBalance = await _rpcClient.GetBalanceAsync(toAccount.PublicKey);


            // Get a recent block hash to include in the transaction
            var blockHash = _rpcClient.GetLatestBlockHash();

            // Initialize a transaction builder and chain as many instructions as you want before building the message
            var tx = new TransactionBuilder().
                    SetRecentBlockHash(blockHash.Result.Value.Blockhash).
                    SetFeePayer(fromAccount).//有收取手续费
                    AddInstruction(SetComputeUnitLimit(30000)).
                    AddInstruction(SetComputeUnitPrice(1000000)).
                    AddInstruction(MemoProgram.NewMemo(fromAccount, MemoString)).
                    AddInstruction(SystemProgram.Transfer(fromAccount, toAccount, lamports));//交易燃料费用要注意
                   
           var feeData= await SPLTokenService.CalculateTransactionFeeAsync(tx);

            if (fromAccountBalance.Result.Value < (lamports+ feeData.Data))
            {
                return res.NotOk("转出方的余额不足，不足以支付转账金额.");
            }

            var sendTransResult = await _rpcClient.SendTransactionAsync(tx.Build(fromAccount));
            if (!sendTransResult.WasSuccessful)
            {
                return res.NotOk($"交易失败，{sendTransResult.Result},详细信息：{sendTransResult.RawRpcResponse}");
            }
            else {
                return res.Ok(sendTransResult.Result);
            }

        }

        /// <summary>
        /// 交易Lamports
        /// </summary>
        /// <param name="fromAccount">支出方</param>
        /// <param name="toAccount">接收方</param>
        /// <param name="lamports"></param>
        /// <param name="MemoString">备注</param>
        /// <returns></returns>
        public static async Task<ApiResult<TransferResultDto>> TransferLamportsByPublicKeyAsync(Account fromAccount, PublicKey toAccount, ulong lamports, string MemoString = "")
        {
            ApiResult<TransferResultDto> res = new ApiResult<TransferResultDto>();
            if (fromAccount == null || toAccount == null)
            {

                return res.NotOk("转入方或者接收方需要不为空。");
            }
            if (lamports == 0)
            {
                return res.NotOk("lamports 必须大于0.");
            }

            //查询各自的余额
            var fromAccountBalance = await _rpcClient.GetBalanceAsync(fromAccount.PublicKey);
            var toAccountBalance = await _rpcClient.GetBalanceAsync(toAccount);

            // 获取租金豁免金额
            var rentExemption = await _rpcClient.GetMinimumBalanceForRentExemptionAsync(0);

            if (lamports < rentExemption.Result)
            {
                return res.NotOk($"转账金额 {lamports}的 Lamports 低于租金豁免金额 {rentExemption.Result} Lamports，交易可能失败。");
            }

            // Get a recent block hash to include in the transaction
            var blockHash = _rpcClient.GetLatestBlockHash();

            // Initialize a transaction builder and chain as many instructions as you want before building the message
            var tx = new TransactionBuilder().
                    SetRecentBlockHash(blockHash.Result.Value.Blockhash).
                    SetFeePayer(fromAccount).//有收取手续费
                    AddInstruction(SetComputeUnitLimit(30000)).
                    AddInstruction(SetComputeUnitPrice(1000000))
                    .AddInstruction(MemoProgram.NewMemo(fromAccount, MemoString))
                    .AddInstruction(SystemProgram.Transfer(fromAccount.PublicKey, toAccount, lamports));//交易燃料费用要注意

            var feeData = await SPLTokenService.CalculateTransactionFeeAsync(tx);

            if (fromAccountBalance.Result.Value <= (lamports + feeData.Data))
            {
                return res.NotOk("转出方的余额不足，不足以支付转账金额.");
            }
            var trans=tx.Build(fromAccount);
           
            var sendTransResult = await _rpcClient.SendTransactionAsync(trans);
            if (!sendTransResult.WasSuccessful)
            {
                return res.NotOk($"交易失败，{sendTransResult.Reason},详细信息：{sendTransResult.RawRpcResponse}");
            }
            else
            {
                return res.Ok(new TransferResultDto() { Fee= feeData.Data,
                    LatestBlockHash= blockHash.Result.Value.Blockhash,
                     RentExemption= rentExemption.Result,
                    TxSignature= sendTransResult.Result});
            }

        }

        public static async Task<RequestResult<TransactionMetaSlotInfo>> GetTransactionAsync(string txSignature) {

            RequestResult<TransactionMetaSlotInfo> data = await _rpcClient.GetTransactionAsync(txSignature);
            return data;
        }

        /// <summary>
        /// 获取最新区块
        /// </summary>
        /// <returns></returns>
        public static async Task<ApiResult<string>> GetLatestBlockHashAsync()
        {
            ApiResult<string> result = new ApiResult<string>();
            var blockHashResult =  await _rpcClient.GetLatestBlockHashAsync();
            if (!blockHashResult.WasSuccessful)
            {
                return result.NotOk("无法获取最新的区块哈希。");
            }
            return result.Ok(blockHashResult.Result.Value.Blockhash);
        }

        /// <summary>
        /// 检查 Solana 账户状态
        /// </summary>
        /// <param name="accountPublicKey">要检查的账户公钥</param>
        /// <returns>如果账户被冻结返回 true，否则返回 false</returns>
        public static async Task<ApiResult<AccountState>> CheckAccountStatusAsync(string accountPublicKey)
        {
            var res = new ApiResult<AccountState>();
            try
            {
                // 调用 RPC 方法获取账户信息
                var accountInfo = await _rpcClient.GetAccountInfoAsync(accountPublicKey, Commitment.Finalized);
                if (accountInfo.WasSuccessful && accountInfo.Result != null)
                {
                    // 从账户信息中获取账户数据
                    AccountInfo accountData = accountInfo.Result.Value;
                    if (accountData.Data != null)
                    {
                        var tokenAcc = Solnet.Programs.Models.TokenProgram.TokenAccount.Deserialize(Convert.FromBase64String(accountData.Data[0]));
                        return res.Ok(tokenAcc.State);

                    }
                    else {
                        return res.NotOk("用户信息不存在。");
                    }

                   
                }
                else
                {
                    return res.NotOk(accountInfo.Reason);
                }
            }
            catch (Exception ex)
            {
                return res.NotOk(ex.Message);
            }
        }

        #region 基础辅助方法

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

        /// <summary>
        /// 验证输入是否满足Token 标准
        /// </summary>
        /// <param name="InputToken"></param>
        /// <param name="decimalPlaces"></param>
        /// <returns></returns>
        public static ApiResult<ulong> ValidateInput(string InputToken, int decimalPlaces)
        {
            ApiResult<ulong> res=new ApiResult<ulong>();
            string errorMessage;
            if (double.TryParse(InputToken, out double tokenValue))
            {
                ulong smallestUnit = (ulong)(tokenValue * Math.Pow(10, decimalPlaces));
                if (smallestUnit < 0)
                {
                    return res.NotOk("输入的数值不能为负数。");
                }
                else if (smallestUnit % 1 != 0)
                {
                    return res.NotOk($"输入的数值转换为最小单位后必须为整数，该 token 小数位为 {decimalPlaces}。");
                }
                else
                {
                    return res.Ok(smallestUnit);
                }
            }
            else
            {
                return res.NotOk("输入的不是有效的数值。");
            }
            
           
        }

        #endregion
    }
    public static class EnumeratorExtensions
    {
        // 定义 ToEnumerable 扩展方法
        public static IEnumerable<T> ToEnumerable<T>(this IEnumerator<T> enumerator)
        {
            if (enumerator == null)
            {
                throw new ArgumentNullException(nameof(enumerator));
            }

            try
            {
                while (enumerator.MoveNext())
                {
                    yield return enumerator.Current;
                }
            }
            finally
            {
                // 释放资源
                enumerator.Dispose();
            }
        }
    }


}
