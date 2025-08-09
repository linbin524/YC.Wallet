using ApplicationService.IService;
using Solnet.Extensions.Models;
using Solnet.Wallet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YC.Model;
using YC.Model.Entity;

namespace YC.ApplicationService.IService
{
    public interface IWalletService : IDependencyInjectionSupport
    {
        /// <summary>
        /// 创建钱包
        /// </summary>
        /// <returns></returns>
        Task<IApiResult<WalletEntity>> CreateNewWalletAsync();

        /// <summary>
        /// 导入钱包
        /// </summary>
        /// <param name="filePath">文件地址</param>
        /// <returns></returns>
        Task<IApiResult<WalletEntity>> ImportWalletAsync(string filePath);
        /// <summary>
        /// 批量导入钱包
        /// </summary>
        /// <param name="filePath">文件地址</param>
        /// <returns></returns>
        Task<IApiResult<List<WalletEntity>>> ImportBatchWalletAsync(string filePath);

        /// <summary>
        /// 通过钱包获取内部Token账户信息
        /// </summary>
        /// <param name="id">钱包id</param>
        /// <returns></returns>
        Task<ApiResult<TokenWalletFilterList>> GetTokenAccountInfoAsync(long id);

        /// <summary>
        /// 查询钱包主账户Lamport 的余额
        /// </summary>
        /// <param name="walletId">钱包Id</param>
        /// <returns></returns>
        Task<ApiResult<ulong>> GetWalletLamportsBalanceAsync(long walletId);

        /// <summary>
        /// 批量保存钱包信息到数据库
        /// </summary>
        /// <param name="wallet"></param>
        /// <returns></returns>
        Task<IApiResult<List<WalletEntity>>> SaveBatchWalletInfoAsync(List<Wallet> wallets);

        /// <summary>
        /// 创建token 关联账户
        /// </summary>
        /// <param name="ownerAuthority">所属钱包</param>
        /// <param name="tokenMint">对应代币的Mint</param>
        /// <param name="payAccount">支付账户</param>
        /// <returns></returns>
        Task<ApiResult<Solnet.Wallet.PublicKey>> CreateAssociatedTokenAccountAsync(
            WalletEntity ownerWallet,
            string tokenMint, WalletEntity payerWallet);
    }
}
