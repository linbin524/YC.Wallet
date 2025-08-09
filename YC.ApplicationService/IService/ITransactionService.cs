using ApplicationService.IService;
using Solnet.Extensions.Models;
using Solnet.Wallet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YC.ApplicationService.DTO;
using YC.Model;
using YC.Model.Entity;
using YC.SolanaSdkService;

namespace YC.ApplicationService.IService
{
    public interface ITransactionService : IDependencyInjectionSupport
    {
        /// <summary>
        /// 发送交易
        /// </summary>
        /// <returns></returns>
        Task<IApiResult<WalletAccountTransRecordEntity>> SendTransactionAsync(SendTransactionDto entity);
        /// <summary>
        /// 铸造Token
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        Task<IApiResult<WalletAccountTransRecordEntity>> MintTokenAsync(CreateTokenDto entity);
    }
}
