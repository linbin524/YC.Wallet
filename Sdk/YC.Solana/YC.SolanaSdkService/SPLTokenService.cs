using Mapster;
using Org.BouncyCastle.Asn1.Ocsp;
using Renci.SshNet.Messages;
using Solnet.Metaplex.NFT.Library;
using Solnet.Programs;
using Solnet.Programs.Models.TokenProgram;
using Solnet.Rpc;
using Solnet.Rpc.Builders;
using Solnet.Rpc.Models;
using Solnet.Rpc.Types;
using Solnet.Wallet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ubiety.Dns.Core.Records;
using YC.Model;



namespace YC.SolanaSdkService
{
    public class SPLTokenService: BaseService
    {
        internal static IRpcClient _rpcClient { get => BasicConfig.RpcClient; }

        /// <summary>
        /// 使用关联账户铸造代币
        /// </summary>
        /// <param name="createTokenDto"></param>
        /// <returns></returns>
        public static async Task<ApiResult<CreatedTokenResultDto>> CreateTokenAsync(CreateTokenDto createTokenDto)
        {
            ApiResult<CreatedTokenResultDto> res = new ApiResult<CreatedTokenResultDto>();
            // 构建交易
            var transactionBuilder = new TransactionBuilder();
            ///普通账户存储
            PublicKey storageTokenAccount;

            TransactionInstruction createAssociatedTokenAccountInstruction = null;
            try
            {
                #region 铸造前，账户余额等准备的配套验证
                // 检查 payerAccount 余额
                var payBalanceResult = await _rpcClient.GetBalanceAsync(createTokenDto.PayAccount.PublicKey);
                if (!payBalanceResult.WasSuccessful)
                {
                    return res.NotOk("获取支付账户余额失败。");
                }
               
                    Console.WriteLine($"Payer account balance: {payBalanceResult.Result} Lamports");

                /// 对铸造账户进行Token 余额校验，保证该账户没有使用过的唯一性。（有使用过就不能再用）
                var mintAtaInfo = await _rpcClient.GetTokenAccountBalanceAsync(createTokenDto.MintAccount.PublicKey);
                if (mintAtaInfo.WasSuccessful)
                {
                    return res.NotOk("铸造前，基础信息校验时候，确认铸造账户已经存在，请更换其他映射账户！");
                }

                // 获取创建账户所需的最低租金豁免余额
                var minBalanceForExemptionAccResult = await _rpcClient.GetMinimumBalanceForRentExemptionAsync(TokenProgram.TokenAccountDataSize);
                if (!minBalanceForExemptionAccResult.WasSuccessful)
                {
                    return res.NotOk("获取创建账户所需的最低租金豁免余额失败.");
                }
                ulong minBalanceForExemptionAcc = minBalanceForExemptionAccResult.Result;
                Console.WriteLine($"MinBalanceForRentExemption Account >> {minBalanceForExemptionAcc}");

                var minBalanceForExemptionMintResult = await _rpcClient.GetMinimumBalanceForRentExemptionAsync(TokenProgram.MintAccountDataSize);
                if (!minBalanceForExemptionMintResult.WasSuccessful)
                {
                    return res.NotOk("未能获得铸造账户租金豁免的最低余额。");
                }
                ulong minBalanceForExemptionMint = minBalanceForExemptionMintResult.Result;
                Console.WriteLine($"MinBalanceForRentExemption Mint Account >> {minBalanceForExemptionMint}");
                var mintAccount = createTokenDto.MintAccount.PublicKey;
                var ownerAuthority = createTokenDto.PayAccount.PublicKey;
                var payerAuthority = createTokenDto.PayAccount.PublicKey;
                if (payBalanceResult.Result.Value < minBalanceForExemptionMint)//
                {
                    return res.NotOk("支付账户的余额低于铸造账户租金豁免的最低余额，请先充值。");
                }

                
                #endregion
                
                 ///使用关联账户模式铸造
                if (createTokenDto.IsStorageAssociatedAccount) {
                    #region 铸造前， 创建关联账户
                    // 创建关联代币账户
                    var associatedTokenAccountAddress = AssociatedTokenAccountProgram.DeriveAssociatedTokenAccount(
                        ownerAuthority,
                        mintAccount
                    );
                    storageTokenAccount= associatedTokenAccountAddress;
                    var getAtaInfoResult = await _rpcClient.GetAccountInfoAsync(associatedTokenAccountAddress);
                    if (!getAtaInfoResult.WasSuccessful)
                    {
                        return res.NotOk("创建关联代币账户后,未能获取关联代币帐户信息。");
                    }

                    if (getAtaInfoResult.Result.Value == null)
                    {
                        // 如果关联代币账户不存在，创建它
                        createAssociatedTokenAccountInstruction = AssociatedTokenAccountProgram.CreateAssociatedTokenAccount(
                            payerAuthority,
                            ownerAuthority,
                            mintAccount
                        );
                    }
                    
                    #endregion
                }
                else {

                    storageTokenAccount = createTokenDto.StorageTokenAccount.PublicKey;
                }

                #region 铸造前，构建铸造代币交易准备
                // 创建Mint代币账户指令
                var createMintAccountInstruction = SystemProgram.CreateAccount(
                    payerAuthority,
                    mintAccount,
                    minBalanceForExemptionMint,
                    TokenProgram.MintAccountDataSize,
                    TokenProgram.ProgramIdKey);

                // 初始化代币铸造账户指令 1
                var initializeMintInstruction = TokenProgram.InitializeMint(
                    mintAccount,
                    createTokenDto.Decimals, // 铸造币小数点
                    ownerAuthority, // 管理mint账户后续发币等权限的账户
                    ownerAuthority // 冻结权的账户
                );

                // 铸造代币指令 ，将铸造代币转移到指定的账户
                var mintToInstruction = TokenProgram.MintTo(
                    mint: mintAccount,
                    destination: storageTokenAccount,
                    amount: createTokenDto.TokenSupply,
                    mintAuthority: ownerAuthority
                );

                // 创建元数据账户地址
                var metadataProgramId = new PublicKey("metaqbxxUerdq28cj1RbAWkYQm3ybzjb6a8bt518x1s");
                PublicKey metadataAddress;
                PublicKey.TryFindProgramAddress(
                    new List<byte[]>
                    {
                    System.Text.Encoding.UTF8.GetBytes("metadata"),
                    metadataProgramId.KeyBytes,
                    mintAccount.KeyBytes
                    },
                    metadataProgramId, out metadataAddress, out _
                );

                // 创建元数据账户指令
                var createMetadataInstruction = MetadataProgram.CreateMetadataAccount(
                    metadataKey: metadataAddress,
                    mintKey: mintAccount,
                    authorityKey: ownerAuthority,
                    payerKey: payerAuthority,
                    updateAuthority: ownerAuthority,
                    new Metadata()
                    {
                        name = createTokenDto.TokenName,
                        symbol = createTokenDto.TokenSymbol,
                        uri = createTokenDto.Uri,
                        ///二次销售的卖方费用手续费。
                        sellerFeeBasisPoints = 0,
                        creators = new List<Creator>() {
                        new Creator(ownerAuthority, 100, true)
                        }
                    },
                    TokenStandard.Fungible,
                    isMutable: true,
                    true
                );

                
                transactionBuilder.AddInstruction(createMintAccountInstruction);
                transactionBuilder.AddInstruction(initializeMintInstruction);
                if (createAssociatedTokenAccountInstruction != null)//使用关联账户存储代币
                {
                    transactionBuilder.AddInstruction(createAssociatedTokenAccountInstruction);
                }
                if (!createTokenDto.IsStorageAssociatedAccount) {///使用普通账户存储代币
                    // 添加创建初始账户的指令
                    transactionBuilder.AddInstruction(SystemProgram.CreateAccount(
                        payerAuthority,
                        storageTokenAccount,
                        minBalanceForExemptionAcc,
                        TokenProgram.TokenAccountDataSize,
                        TokenProgram.ProgramIdKey));

                    // 添加初始化初始账户的指令
                    transactionBuilder.AddInstruction(TokenProgram.InitializeAccount(
                        storageTokenAccount,
                        mintAccount,
                        payerAuthority));
                }
                
                transactionBuilder.AddInstruction(mintToInstruction);
                transactionBuilder.AddInstruction(createMetadataInstruction);
                transactionBuilder.AddInstruction(MemoProgram.NewMemo(createTokenDto.PayAccount, createTokenDto.MemoString));

                // 获取最近的区块哈希
                var recentBlockHashResult = await _rpcClient.GetLatestBlockHashAsync(Commitment.Finalized);
                if (!recentBlockHashResult.WasSuccessful)
                {
                    return res.NotOk("未能获取最终交易的最新区块哈希。");
                }
                transactionBuilder.SetRecentBlockHash(recentBlockHashResult.Result.Value.Blockhash);

                // 设置费用支付者和签名者
                transactionBuilder.SetFeePayer(payerAuthority);
                byte[] transaction;
                if (createTokenDto.IsStorageAssociatedAccount)
                {
                    // 构建交易
                     transaction = transactionBuilder.Build(new List<Account>() { createTokenDto.PayAccount, createTokenDto.MintAccount });
                }
                else {
                    // 构建交易
                     transaction = transactionBuilder.Build(new List<Account>() { createTokenDto.PayAccount, createTokenDto.MintAccount,
                         createTokenDto.StorageTokenAccount });
                }

                #endregion

              var fee = await CalculateTransactionFeeAsync(transactionBuilder);
              // 发送交易
              var sendResult = await _rpcClient.SendTransactionAsync(transaction);
                if (!sendResult.WasSuccessful)
                {
                    return res.NotOk("铸造代币时候，发送最终交易失败。" + sendResult.RawRpcResponse);
                }
                if (sendResult.Reason!="OK")
                {
                    return res.NotOk($"铸造代币时候，发送最终交易失败：{sendResult.Result}，详细信息：{sendResult.RawRpcResponse}" );
                }
                Console.WriteLine($"Transaction sent: {sendResult.Result}");
                var resData = createTokenDto.Adapt<CreatedTokenResultDto>();
                resData.CreatedTokenTime = DateTime.Now;
                resData.CreatedTokenTranscationTxHash = sendResult.Result;
                resData.DestinationAccountType = createTokenDto.IsStorageAssociatedAccount?1:0; // 默认使用关联账户
                resData.DestinationPublicKey = storageTokenAccount;
                resData.TransRequestInfo = sendResult.RawRpcRequest;
                resData.TransResponseInfo = sendResult.RawRpcResponse;
                resData.PayAccountPublicKey = createTokenDto.PayAccount.PublicKey;
                resData.MintAccountPublicKey = createTokenDto.MintAccount.PublicKey;
                resData.Fee = fee.Data;
                return res.Ok(resData, $"铸造代币:'{createTokenDto.TokenName}',代币符号:'{createTokenDto.TokenSymbol}',创建交易已成功发送");
            }
            catch (Exception ex)
            {
                return res.NotOk($"发生意外错误: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 转移自定义铸造的 Token 到目标地址
        /// </summary>
        /// <param name="mintAccount">铸造 Token 的账户</param>
        /// <param name="ownerAccount">Token 所有者账户</param>
        /// <param name="sourceAssociatedTokenAccount">源关联代币账户</param>
        /// <param name="destinationAssociatedTokenAccount">目标关联代币账户</param>
        /// <param name="amount">转移的 Token 数量</param>
        /// <returns>交易签名</returns>
        public static async Task<ApiResult<string>> TransferSPLTokenAsync(
            Account mintAccount,
            Account payAccount,
            PublicKey sourceAssociatedTokenAccount,
            PublicKey destinationAssociatedTokenAccount,
            ulong amount,  IEnumerable<PublicKey> signers = null)
        {
            ApiResult<string> res = new ApiResult<string>();
           var result=await BuildTransDataAsync(mintAccount,
               payAccount,
               sourceAssociatedTokenAccount,
               destinationAssociatedTokenAccount, 
               amount, 
               signers);
            if (!result.State) {
                return res.NotOk(result.Message);
            }
            byte[] signData = result.Data.Build(new List<Account>() { payAccount } );

            // 发送转移交易
            var transferSignature = _rpcClient.SendTransaction(signData);
            if (!transferSignature.WasSuccessful)
            {
                return res.NotOk($"交易发送失败: {transferSignature.Reason}");
            }
            return res.Ok($"{transferSignature.Result},详细信息：{transferSignature.RawRpcResponse}");

           
        }

        public static async Task<ApiResult<TransferResultDto>> TransferSPLTokenAsync(
            Account payAccount,
            PublicKey sourceAccount,
            PublicKey destinationAccount,
            ulong amount, IEnumerable<PublicKey> signers = null)
        {
            ApiResult<TransferResultDto> res = new ApiResult<TransferResultDto>();
            var result = await BuildTransDataAsync(
                payAccount,
                sourceAccount,
                destinationAccount,
                amount,
                signers);
            if (!result.State)
            {
                return res.NotOk(result.Message);
            }
            // 获取租金豁免金额
            var rentExemption = await _rpcClient.GetMinimumBalanceForRentExemptionAsync(0);
            var feeData = await SPLTokenService.CalculateTransactionFeeAsync(result.Data);
            var fromAccountTokenBalance= await  BaseService.GetTokenBalanceAsync(sourceAccount);
            if (ulong.Parse(fromAccountTokenBalance.Data.Amount) < (amount + feeData.Data))
            {
                return res.NotOk("转出方的余额不足，不足以支付转账金额.");
            }
            byte[] signData = result.Data.Build(new List<Account>() { payAccount });

            // 发送转移交易
            var transferSignature =await _rpcClient.SendTransactionAsync(signData);
            if (!transferSignature.WasSuccessful)
            {
                return res.NotOk($"交易发送失败: {transferSignature.Reason}");
            }
            return res.Ok(new TransferResultDto()
            {
                Fee = feeData.Data,
                LatestBlockHash = "",
                RentExemption = rentExemption.Result,
                TxSignature = transferSignature.Result
            });


        }

        /// <summary>
        /// 多方签名交易
        /// </summary>
        /// <param name="msgData">交易信息的字节数组</param>
        /// <param name="signatures">各方对交易信息签名后拼接起来的组合</param>
        /// <returns></returns>
        public static async Task<ApiResult<string>> TransferSPLTokenByMultiSignAsync(byte[] msgData,List<byte[]> signatures,Account payerAccount=null)
        {
            ApiResult<string> res = new ApiResult<string>();
            Solnet.Rpc.Models.Message msg = Solnet.Rpc.Models.Message.Deserialize(Convert.ToBase64String(msgData));

            var tx = Transaction.Populate(msg, signatures);
           
            //var signData = tx.Build(payerAccount);
            var signData = tx.Serialize();

            var test= _rpcClient.SimulateTransaction(signData);
            // 发送转移交易
            var transferSignature = _rpcClient.SendTransaction(signData);
            if (!transferSignature.WasSuccessful)
            {
                return res.NotOk($"交易发送失败: {transferSignature.Reason}");
            }
            return res.Ok($"{transferSignature.Result},详细信息：{transferSignature.RawRpcResponse}");


        }


        /// <summary>
        /// 构建交易信息
        /// </summary>
        /// <param name="mintAccount"></param>
        /// <param name="ownerAccount"></param>
        /// <param name="sourceAssociatedTokenAccount"></param>
        /// <param name="destinationAssociatedTokenAccount"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        public static async Task<ApiResult<TransactionBuilder>> BuildTransDataAsync(Account mintAccount,
            Account payAccount,
            PublicKey sourceAssociatedTokenAccount,
            PublicKey destinationAssociatedTokenAccount,
            ulong amount, IEnumerable<PublicKey> signers=null) {

            ApiResult<TransactionBuilder> result = new ApiResult<TransactionBuilder>();
            // 基本校验
            if (mintAccount == null || payAccount == null || sourceAssociatedTokenAccount == null 
                || destinationAssociatedTokenAccount == null || amount <= 0)
            {
                return result.NotOk("输入参数无效，请检查铸造账户、所有者账户、源关联账户、目标地址和转移数量。");
            }
            // 检查源关联代币账户余额

            var sourceBalanceResult = await _rpcClient.GetTokenAccountBalanceAsync(sourceAssociatedTokenAccount);
            if (!sourceBalanceResult.WasSuccessful)
            {
                return result.NotOk("无法获取源关联代币账户余额。");
            }
            ulong sourceBalance = ulong.Parse(sourceBalanceResult.Result.Value.Amount);
            if (sourceBalance < amount)
            {
                return result.NotOk("源关联代币账户余额不足，无法完成转账。");
            }

            // 检查目标关联代币账户是否存在，不存在则创建
            var accountInfo = await _rpcClient.GetAccountInfoAsync(destinationAssociatedTokenAccount);
            if (accountInfo.Result.Value == null)
            {
                return result.NotOk("目标代币接受地址不存在。");
            }

            // 获取最新的区块哈希
            var getLastestBlockHash = await GetLatestBlockHashAsync();
            if (!getLastestBlockHash.State)
            {
                return result.NotOk(getLastestBlockHash.Message);
            }
            
            // 创建转移 Token 的指令
            var transferInstruction = TokenProgram.Transfer(
                sourceAssociatedTokenAccount,
                destinationAssociatedTokenAccount,
                amount,
                payAccount, signers);

            // 构建转移交易
            var msgData = new TransactionBuilder()
               .AddInstruction(transferInstruction)
               .SetRecentBlockHash(getLastestBlockHash.Data)
               .SetFeePayer(payAccount.PublicKey);

            return result.Ok(msgData);
        }
        public static async Task<ApiResult<TransactionBuilder>> BuildTransDataAsync(
            Account payAccount,
            PublicKey sourceAccount,
            PublicKey destinationAccount,
            ulong amount, IEnumerable<PublicKey> signers = null)
        {

            ApiResult<TransactionBuilder> result = new ApiResult<TransactionBuilder>();
            // 基本校验
            if ( payAccount == null || sourceAccount == null
                || destinationAccount == null || amount <= 0)
            {
                return result.NotOk("输入参数无效，请检查支付手续费账户、转出方账户、收款方账户、转移数量。");
            }
            // 检查源关联代币账户余额
            var sourceBalanceResult = await _rpcClient.GetTokenAccountBalanceAsync(sourceAccount);
            if (!sourceBalanceResult.WasSuccessful)
            {
                return result.NotOk("无法获取转出账户余额。");
            }
            ulong sourceBalance = ulong.Parse(sourceBalanceResult?.Result?.Value?.Amount);
            if (sourceBalance < amount)
            {
                return result.NotOk("转出账户余额不足，无法完成转账。");
            }

            // 检查目标关联代币账户是否存在，不存在则创建
            var accountInfo = await _rpcClient.GetAccountInfoAsync(destinationAccount);
            if (accountInfo.Result.Value == null)
            {
                return result.NotOk("目标代币接受地址不存在。");
            }

            // 获取最新的区块哈希
            var getLastestBlockHash = await GetLatestBlockHashAsync();
            if (!getLastestBlockHash.State)
            {
                return result.NotOk(getLastestBlockHash.Message);
            }

            // 创建转移 Token 的指令
            var transferInstruction = TokenProgram.Transfer(
                sourceAccount,
                destinationAccount,
                amount,
                payAccount, signers);

            // 构建转移交易
            var msgData = new TransactionBuilder()
               .AddInstruction(transferInstruction)
               .SetRecentBlockHash(getLastestBlockHash.Data)
               .SetFeePayer(payAccount.PublicKey);

            return result.Ok(msgData);
        }

        /// <summary>
        /// 多方签名交易前构建交易信息
        /// </summary>
        /// <param name="mintAccount">token 铸币者（只负责铸造币，完成标识全网唯一代币标识作用）</param>
        /// <param name="ownerAccount">钱包所有者，
        /// 所以交易Fee需要由钱包主账户支付</param>
        /// <param name="sourceAssociatedTokenAccount">发送Token的关联代币账户</param>
        /// <param name="destinationAssociatedTokenAccount">接受方Token的关联代币账户</param>
        /// <param name="amount">Token 数量</param>
        /// <param name="multiSignAccount">多方签名主签名对象</param>
        /// <param name="signers">多方签名参与者的公钥，通常用于后续签名的验签</param>
        /// <returns></returns>
        public static async Task<ApiResult<byte[]>> BuildTransferSPLTokenWaitMultiSignAsync(
           //Account mintAccount,
           Account payerAccount,
           PublicKey sourceAssociatedTokenAccount,
           PublicKey destinationAssociatedTokenAccount,
           ulong amount, Account multiSignAccount, IEnumerable<PublicKey> signers = null)
        {

            ApiResult<byte[]> result = new ApiResult<byte[]>();
            // 基本校验
            if ( payerAccount == null || sourceAssociatedTokenAccount == null || destinationAssociatedTokenAccount == null || amount <= 0)
            {
                return result.NotOk("输入参数无效，请检查铸造账户、所有者账户、源关联账户、目标地址和转移数量。");
            }
            // 检查源关联代币账户余额

            var sourceBalanceResult = await _rpcClient.GetTokenAccountBalanceAsync(sourceAssociatedTokenAccount);
            if (!sourceBalanceResult.WasSuccessful)
            {
                return result.NotOk("无法获取源关联代币账户余额。");
            }
            ulong sourceBalance = ulong.Parse(sourceBalanceResult.Result.Value.Amount);
            if (sourceBalance < amount)
            {
                return result.NotOk("源关联代币账户余额不足，无法完成转账。");
            }

            // 检查目标关联代币账户是否存在，不存在则创建
            var accountInfo = await _rpcClient.GetAccountInfoAsync(destinationAssociatedTokenAccount);
            if (accountInfo.Result.Value == null)
            {
                return result.NotOk("目标代币接受地址不存在。");
            }

            // 获取最新的区块哈希
            var getLastestBlockHash = await GetLatestBlockHashAsync();
            if (!getLastestBlockHash.State)
            {
                return result.NotOk(getLastestBlockHash.Message);
            }

            // 创建转移 Token 的指令
            var transferInstruction = TokenProgram.Transfer(
                sourceAssociatedTokenAccount,
                destinationAssociatedTokenAccount,
                amount,
                multiSignAccount, signers);

            // 构建转移交易
            var msgData = new TransactionBuilder()
               .AddInstruction(transferInstruction)
               .SetRecentBlockHash(getLastestBlockHash.Data)
               .SetFeePayer(payerAccount.PublicKey);

           
            return result.Ok(msgData.CompileMessage());
        }

        /// <summary>
        /// 计算交易燃料费用的公共方法
        /// </summary>
        /// <param name="rpcClient">Solana RPC 客户端</param>
        /// <param name="payerAccount">支付费用的账户</param>
        /// <param name="instructions">交易指令列表</param>
        /// <returns>包含计算结果的对象</returns>
        public static async Task<ApiResult<ulong>> CalculateTransactionFeeAsync(TransactionBuilder transactionBuilder)
        {
            var result = new ApiResult<ulong>();
            try
            {
                // 获取交易消息
                var message = transactionBuilder.CompileMessage();
                var mesData = Convert.ToBase64String(message);
                Solnet.Rpc.Models.Message msg = Solnet.Rpc.Models.Message.Deserialize(mesData);
                // 估算交易费用
                var feeResult = await _rpcClient.GetFeeForMessageAsync(mesData);
                if (!feeResult.WasSuccessful)
                {
                    return result.NotOk($"Failed to estimate transaction fee: {feeResult.Reason}");
                }
                return result.Ok(feeResult.Result.Value);
            }
            catch (Exception ex)
            {

                result.NotOk($"An unexpected error occurred while calculating fuel fee: {ex.Message}");
                return result;
            }
        }
    }
}
