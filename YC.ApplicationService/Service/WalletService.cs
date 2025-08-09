using Mapster;
using MySqlX.XDevAPI.Common;
using Solnet.Extensions.Models;
using Solnet.Wallet;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YC.ApplicationService.IService;
using YC.Common.ShareUtils;
using YC.Model;
using YC.Model.Entity;
using YC.SolanaSdkService;
using YC.SolanaSdkService.DTO;
using PublicKey = Solnet.Wallet.PublicKey;

namespace YC.ApplicationService.Service
{
    public class WalletService : IWalletService
    {
        public WalletService() { }

        /// <summary>
        /// 创建新的钱包
        /// </summary>
        /// <returns></returns>
        public async Task<IApiResult<WalletEntity>> CreateNewWalletAsync()
        {
            ApiResult<WalletEntity> result=new ApiResult<WalletEntity>();
            try
            {
                var res = BaseService.CreateNewWallet();
                var insertRes = await SaveWalletInfoAsync(res);
                if (insertRes.State)
                {
                    return result.Ok(insertRes.Data, "创建钱包成功！");
                }
                else {
                    return result.NotOk("创建失败！");
                }
               
            }
            catch (Exception ex)
            {
                return result.NotOk("创建钱包失败！错误信息："+ ex.Message);
            }
            
        }

        /// <summary>
        /// 导入钱包
        /// </summary>
        /// <param name="filePath">文件地址</param>
        /// <returns></returns>
        public async Task<IApiResult<WalletEntity>> ImportWalletAsync(string filePath)
        {
            ApiResult<WalletEntity> result = new ApiResult<WalletEntity>();
            var wallet = BaseService.InitWallet(filePath);
            var insertRes = await SaveWalletInfoAsync(wallet);
            if (insertRes.State)
            {
                return result.Ok(insertRes.Data, "创建钱包成功！");
            }
            else
            {
                return result.NotOk(insertRes.Message);
            }
        }

        /// <summary>
        /// 批量导入钱包
        /// </summary>
        /// <param name="filePath">文件地址</param>
        /// <returns></returns>
        public async Task<IApiResult<List<WalletEntity>>> ImportBatchWalletAsync(string filePath) {
            ApiResult<List<WalletEntity>> result = new ApiResult<List<WalletEntity>>();
            var wallet= BaseService.BatchInitWallet(filePath);
            var insertRes = await SaveBatchWalletInfoAsync(wallet);
            if (insertRes.State)
            {
                return result.Ok(insertRes.Data, "创建钱包成功！");
            }
            else
            {
                return result.NotOk(insertRes.Message);
            }
        }

        /// <summary>
        /// 保存钱包信息到数据库
        /// </summary>
        /// <param name="wallet"></param>
        /// <returns></returns>
        internal async Task<IApiResult<WalletEntity>> SaveWalletInfoAsync(Wallet wallet) {
            ApiResult<WalletEntity> result = new ApiResult<WalletEntity>();
              var obj = wallet.Adapt<WalletEntity>();
            obj.MasterAccountPublicKey = wallet.Account.PublicKey.Key;
            obj.MnemonicStr = string.Join(" ", wallet.Mnemonic.Words);
            obj.CreationTime = DateTime.Now;
            obj.NetWorkType = DefaultConfig.LocalWalletNetwork.ToString();
            obj.CreatorUserId = DefaultConfig.CurrentLoginUser.Id;
            obj.WalletContent = wallet.ToIndentedJson();
           var isExist= SQLiteUtils._freesql.Select<WalletEntity>().Where(x => x.MasterAccountPublicKey == obj.MasterAccountPublicKey &&
            x.MnemonicStr == obj.MnemonicStr&&x.NetWorkType== obj.NetWorkType).Any();//通过网络、钱包公钥、助记词区分钱包
            if (isExist)
            {
                return result.NotOk("该钱包已经存在！");
            }
           var insertRes = await SQLiteUtils._freesql.Insert<WalletEntity>(obj).ExecuteAffrowsAsync();
            if (insertRes > 0)
            {
                return result.Ok(obj);
            }
            else {
                return result.NotOk();
            }
           
        }

        /// <summary>
        /// 批量保存钱包信息到数据库
        /// </summary>
        /// <param name="wallet"></param>
        /// <returns></returns>
        public async Task<IApiResult<List<WalletEntity>>> SaveBatchWalletInfoAsync(List<Wallet> wallets)
        {
            ApiResult<List<WalletEntity>> result = new ApiResult<List<WalletEntity>>();
           var tempList=new List<WalletEntity>();
            var sb=new StringBuilder();
            bool state = true;
            wallets.ForEach(async x => {
                var obj = x.Adapt<WalletEntity>();
                obj.MasterAccountPublicKey = x.Account.PublicKey.Key;
                obj.MnemonicStr = string.Join(" ", x.Mnemonic.Words);
                obj.CreationTime = DateTime.Now;
                obj.NetWorkType = DefaultConfig.LocalWalletNetwork.ToString();
                obj.CreatorUserId = DefaultConfig.CurrentLoginUser.Id;
                obj.WalletContent = x.ToIndentedJson();
                var isExist = SQLiteUtils._freesql.Select<WalletEntity>().Where(x => x.MasterAccountPublicKey == obj.MasterAccountPublicKey &&
                 x.MnemonicStr == obj.MnemonicStr && x.NetWorkType == obj.NetWorkType).Any();//通过网络、钱包公钥、助记词区分钱包
                if (isExist)
                {
                    state = false;
                    sb.AppendLine($"主账户为：{obj.Account.PrivateKey}的钱包已经存在！");
                }
                else {
                    var insertRes = await SQLiteUtils._freesql.Insert<WalletEntity>(obj).ExecuteAffrowsAsync();
                    if (insertRes==0)
                    {
                        sb.AppendLine($"插入主账户为：{obj.Account}的钱包失败！");
                    }
                }
                tempList.Add(obj);
            });
            if (state)
            {
                return result.Ok(tempList);
            }
            else {
                return result.NotOk(sb.ToString());
            } 

        }

        /// <summary>
        /// 通过钱包获取内部Token账户信息
        /// </summary>
        /// <param name="id">钱包id</param>
        /// <returns></returns>
        public async Task<ApiResult<TokenWalletFilterList>> GetTokenAccountInfoAsync(long id) {
            var walletObj = SQLiteUtils._freesql.Select<WalletEntity>().Where(x => x.Id == id).First();
            var wallet = new Wallet(string.Join(" ", walletObj.MnemonicStr), Solnet.Wallet.Bip39.WordList.English);
            var tokenDefEntityList = SQLiteUtils._freesql.Select<TokenDefEntity>().ToList();
            //无法使用Adapt 映射只能手动处理
            List<Solnet.Extensions.TokenMint.TokenDef> tokenDefList = new List<Solnet.Extensions.TokenMint.TokenDef>();
            for (int i = 0; i < tokenDefEntityList.Count; i++)
            {
                var t = tokenDefEntityList[i];
                tokenDefList.Add(new Solnet.Extensions.TokenMint.TokenDef(t.Mint, t.Name, t.Symbol, t.DecimalPlaces));
            }
            var list =  BaseService.GetWalletTokenBalance(wallet, tokenDefList);
            return list;
        }
        /// <summary>
        /// 查询钱包主账户Lamport 的余额
        /// </summary>
        /// <param name="walletId">钱包Id</param>
        /// <returns></returns>
        public async Task<ApiResult<ulong>> GetWalletLamportsBalanceAsync(long walletId) {
            var walletObj = SQLiteUtils._freesql.Select<WalletEntity>().Where(x => x.Id == walletId).First();
            var wallet = BaseService.GetWalletByMnemonicWords(walletObj.MnemonicStr);
                var res = await BaseService.GetAccountLamportsBalanceAsync(wallet.Account);
            return res;
        }

        /// <summary>
        /// 创建token 关联账户
        /// </summary>
        /// <param name="ownerAuthority">所属钱包</param>
        /// <param name="tokenMint">对应代币的Mint</param>
        /// <param name="payAccount">支付账户</param>
        /// <returns></returns>
        public async Task<ApiResult<PublicKey>> CreateAssociatedTokenAccountAsync(WalletEntity ownerWallet, string tokenMint, WalletEntity payerWallet) {
            ApiResult<PublicKey> res = new ApiResult<PublicKey>();
            try
            {
               var tempObj= SQLiteUtils._freesql.Select<WalletAccountEntity>().
                    Where(x=>x.BelongWalletId==ownerWallet.Id
                    &&x.IsAssociatedTokenAccount&&x.TokenMint==tokenMint).First();
                if (tempObj != null)
                {
                    return res.NotOk("该钱包要创建的代币关联账户已经存在！\n账户为："+ tempObj.PublicKey);
                }
                var obj = ownerWallet.WalletContent.ToObject<WalletDto>();
                var owner = GetWalletByMnemonic(ownerWallet.MnemonicStr).Account;
                var mint = new Solnet.Wallet.PublicKey(tokenMint);
                var payer = GetWalletByMnemonic(payerWallet.MnemonicStr).Account;
                 res = await BaseService.CreateAssociatedTokenAccountAsync(owner, mint, payer);
               return res;
            }
            catch (Exception ex)
            {
                return res.NotOk(ex.Message);
            }
           
            

        }

        public Wallet GetWalletByMnemonic(string words) {

            var wallet = new Wallet(words, Solnet.Wallet.Bip39.WordList.English);
            return wallet;
        }
        public IApiResult<SysUser> Login(SysUser sysUser)
        {
            long id = 0;
            var res = new ApiResult<SysUser>();
            try
            {
                var user = SQLiteUtils._freesql.Select<SysUser>().Where(x => x.Account == sysUser.Account && x.Password == sysUser.Password).First();
                if (user != null)
                {
                    return res.Ok(user);
                }
                else
                {
                    return res.NotOk("用户名或密码错误！");
                }

            }
            catch (Exception ex)
            {

                return res.NotOk(ex.ToString());
            }

        }
    }
}
