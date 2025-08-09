using Mapster;
using Solnet.Rpc.Core.Http;
using Solnet.Rpc.Models;
using Solnet.Wallet.Utilities;
using System.IO.Pipelines;
using System.Text;
using System.Threading.Tasks;
using YC.ApplicationService.DTO;
using YC.ApplicationService.IService;
using YC.Common.ShareUtils;
using YC.Model;
using YC.Model.Entity;
using YC.SolanaSdkService;
using YC.SolanaSdkService.DTO;
using PublicKey = Solnet.Wallet.PublicKey;

namespace YC.ApplicationService.Service
{
    public class TransactionService : ITransactionService
    {
        public TransactionService() { 
        
        }

        /// <summary>
        /// 交易
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<IApiResult<WalletAccountTransRecordEntity>> SendTransactionAsync(SendTransactionDto entity)
        {
            ApiResult<WalletAccountTransRecordEntity> res = new();
            WalletAccountTransRecordEntity obj = new WalletAccountTransRecordEntity();
            obj = entity.Adapt<WalletAccountTransRecordEntity>();

            var walletEntity= SQLiteUtils.Query<WalletEntity>().Where(x => x.Id == entity.WalletId).First();
            var tempWalletObj = walletEntity.WalletContent.ToObject<WalletDto>();//不能直接转为wallet 对象
            var tempAccount = new Solnet.Wallet.Account(tempWalletObj.Account.PrivateKey.Key, tempWalletObj.Account.PublicKey.Key);
            var wallet=BaseService.GetWalletByMnemonicWords(walletEntity.MnemonicStr);
            
            var toAccount = new PublicKey(entity.Receiver);
            if (entity.TansType == 0)
            { //是Lamport
               var validateRes= BaseService.ValidateInput(entity.Amount, 9);
                if (!validateRes.State) {
                   return res.NotOk(validateRes.Message);//不满足数据要求，返回
                }
                var lamportsBalance = await BaseService.GetAccountLamportsBalanceAsync(wallet.Account);
                if (lamportsBalance.Data <= validateRes.Data)
                { //不够支付
                    return res.NotOk("余额不足！");
                }

                var transRes=await BaseService.TransferLamportsByPublicKeyAsync(wallet.Account, toAccount, validateRes.Data);
                if (!transRes.State)
                {
                    return res.NotOk(transRes.Message);
                }
                obj.TransactionType = "Lamports";
                obj.Fee = transRes.Data.Fee;
                obj.Transferor = walletEntity.MasterAccountPublicKey;
                obj.TransactionHash = transRes.Data.TxSignature;
                obj.TransferQuantity = validateRes.Data.ToString();
                obj.TransferQuantity = (validateRes.Data / Math.Pow(10, 9)).ToString();
            }
            else { //token 处理
               //var associatedTokenAccount= await BaseService.GetAssociatedTokenAccountAsync(tempAccount, new Solnet.Wallet.PublicKey(entity.TokenDef.Mint), tempAccount);
               // if (!associatedTokenAccount.State) {
               //     return res.NotOk(associatedTokenAccount.Message);
               // }
                obj.Transferor = entity.WalletAccount.PublicKey;
                var validateRes = BaseService.ValidateInput(entity.Amount, entity.TokenDef.DecimalPlaces);
                if (!validateRes.State)
                {
                    return res.NotOk(validateRes.Message);//不满足数据要求，返回
                }
                var lamportsBalance = await BaseService.GetTokenBalanceAsync(wallet.Account);
                if (lamportsBalance.Data != null) {
                    if (ulong.Parse(lamportsBalance.Data.Amount) <= validateRes.Data)
                    { //不够支付
                        return res.NotOk("余额不足！");
                    }
                }

                var transRes = await SPLTokenService.TransferSPLTokenAsync(tempAccount, new Solnet.Wallet.PublicKey(obj.Transferor), toAccount,validateRes.Data);
                if (!transRes.State)
                {
                    return res.NotOk(transRes.Message);
                }
                obj.TransactionType = "SPLToken";
                //获取交易hash
                obj.Fee = transRes.Data.Fee;
                obj.TransactionHash = transRes.Data.TxSignature;
                obj.TokenMint=entity.TokenDef.Mint;
                //显示为UI 可看的
                obj.TransferQuantity = (validateRes.Data/ Math.Pow(10, entity.TokenDef.DecimalPlaces)).ToString();
            }

            #region 数据库存储
            //插入数据库
            RequestResult<TransactionMetaSlotInfo> data = await BaseService.GetTransactionAsync(obj.TransactionHash);
            obj.RecentBlockhash = data.Result?.Transaction?.Message?.RecentBlockhash;
            obj.BlockTime = data.Result?.BlockTime;
            obj.CreationTime = DateTime.Now;
            if (data.Result?.Meta?.Fee != null) { 
             obj.Fee= data.Result.Meta.Fee;//使用链上返回的手续费最准确
            }
            obj.JsonContent = data.Result?.ToIndentedJson();
            obj.CreatorUserId = DefaultConfig.CurrentLoginUser.Id;
            obj.TokenSymbol = entity.TansType == 0 ? "SOL" : entity.TokenDef.Symbol;
            obj.IsAssociatedTokenAccount = entity.TansType == 0 ? false : true;
            obj.TransferStatus = 1;
           var insertCount= SQLiteUtils._freesql.Insert<WalletAccountTransRecordEntity>(obj).ExecuteAffrows();
            return insertCount>0?res.Ok(obj) :res.NotOk("交易成功，但插入数据库失败.");

            #endregion

            throw new NotImplementedException();
        }

        /// <summary>
        /// 铸造Token
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public async Task<IApiResult<WalletAccountTransRecordEntity>> MintTokenAsync(CreateTokenDto entity) {
            ApiResult<WalletAccountTransRecordEntity> res = new();
            WalletAccountTransRecordEntity obj = new();

            var validateRes = BaseService.ValidateInput(entity.TokenSupply.ToString(), entity.Decimals);
            obj.TransactionType = "MintToken";
            obj.Transferor = entity.MintAccount.PublicKey;
            obj.TransferQuantity = entity.TokenSupply.ToString();
            obj.TokenMint = entity.MintAccount.PublicKey;
            obj.PayAccount = entity.PayAccount.PublicKey;
            obj.CreationTime = DateTime.Now;
            obj.IsAssociatedTokenAccount = entity.IsStorageAssociatedAccount; // 默认使用关联账户
            obj.CreatorUserId = DefaultConfig.CurrentLoginUser.Id;
            obj.TokenSymbol = entity.TokenSymbol;
            obj.Remark = entity.MemoString;
            obj.NetWork = DefaultConfig.LocalWalletNetwork.ToString();
            //显示为UI 可看的
            obj.TransferQuantity = (validateRes.Data / Math.Pow(10, entity.Decimals)).ToString();

            var result = await SPLTokenService.CreateTokenAsync(entity);
            if (result.State)///发行成功
            {
                obj.TransferStatus = 1;

            }
            else { ///发行失败
                obj.TransferStatus = 0;

            }
            if(result.Data!=null) { obj.Fee = result.Data.Fee; }
            obj.TransactionHash = result?.Data?.CreatedTokenTranscationTxHash;
            obj.Receiver = result?.Data?.DestinationPublicKey;
            obj.JsonContent = result?.Data?.TransResponseInfo;
            try
            {
                SQLiteUtils.ExecuteTransaction(() =>
                {
                    var insertMintTokenTransRecordCount =SQLiteUtils._freesql.Insert<WalletAccountTransRecordEntity>(obj).ExecuteAffrows();
                    if (result.State)//更新tokenDef
                    {
                        var tempObj = new TokenDefEntity();
                        tempObj.CreationTime = DateTime.Now;
                        tempObj.Mint = entity.MintAccount.PublicKey;
                        tempObj.Name = entity.TokenName;
                        tempObj.Symbol = entity.TokenSymbol;
                        tempObj.SupplyNetWork = entity.NetWork;
                        tempObj.DecimalPlaces = entity.Decimals;
                        tempObj.CreatorUserId = DefaultConfig.CurrentLoginUser.Id;
                        tempObj.TokenProjectUrl = entity.Uri;
                        tempObj.Type = "SPLToken";
                        var insertTokenDefCount =  SQLiteUtils._freesql.Insert<TokenDefEntity>(tempObj).ExecuteAffrows();
                    }
                });
            }
            catch (Exception ex)
            {
                if(res.State) return res.Ok(obj, "铸造成功！但插入数据库失败！" + ex.Message);
                else return res.NotOk(res.Message+"\n"+ex.Message);

            }
            return result.State?res.Ok(obj) :res.NotOk(result.Message);
        }
    }
}
