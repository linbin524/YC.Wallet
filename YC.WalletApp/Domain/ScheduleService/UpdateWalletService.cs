using Solnet.Rpc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using YC.ApplicationService.IService;
using YC.ApplicationService;
using YC.Common.ShareUtils;
using YC.Common;
using YC.Model.Entity;
using YC.WalletApp.Domain.Utils;
using Prism.Ioc;
using YC.WalletApp.Extension;
using Mapster;
using Solnet.Wallet;
using ImTools;

namespace YC.WalletApp.Domain.Service
{
    /// <summary>
    /// 更新钱包余额
    /// </summary>
    public class UpdateWalletService
    {
        private IContainerExtension _container;
        private TimerServiceUtils _timerService;
        private EventSendExtension _eventSendExtension;
        public  UpdateWalletService(IContainerExtension container, EventSendExtension eventSendExtension) {

            _container = container;
            _timerService = _container.Resolve<TimerServiceUtils>();
            _eventSendExtension = eventSendExtension;
            _eventSendExtension.MessageToSend = "updateWalletBalance";

        }

        
        /// <summary>
        /// 定时服务开启
        /// </summary>
        public void TimeWorkInit()
        {
             
            // 注册服务A（单次执行）
            _timerService.ScheduleTask(new TimerServiceUtils.TimerConfig
            {
                TaskId = "App_GetAllWalletBalanceService",
                InitialDelay = 0,       // 立即执行
                NormalInterval = 1,     // 任意有效值（实际不会使用）
                RetryIntervals = new int[0], // 禁用重试
                MaxRetries = 0,         // 禁用重试
                TaskAction = App_GetAllWalletBalanceService,
                ErrorHandler = ex => LogError("StartError_App_GetAllWalletBalanceService", ex),

            });

        }
        // 注册服务B（自适应重试）
        private void Schedule_UpdateAllWalletBalanceService()
        {
            _timerService.ScheduleTask(new TimerServiceUtils.TimerConfig
            {
                TaskId = "Schedule_UpdateAllWalletBalanceService",
                InitialDelay = DefaultConfig.AppConfig.ScheduleInitialDelay,    // 测试用60秒（生产环境改为300秒）
                NormalInterval = DefaultConfig.AppConfig.ScheduleNormalInterval, // 正常间隔10秒
                RetryIntervals = DefaultConfig.AppConfig.ScheduleRetryIntervals, // 重试策略
                MaxRetries = DefaultConfig.AppConfig.ScheduleMaxRetries,      // 最大重试3次
                TaskAction = Update_AllWalletBalanceService,
                ErrorHandler = ex => LogError("StartError_Schedule_UpdateAllWalletBalanceService", ex),

            });
        }
        // 服务A实现（单次执行）内部调用，执行服务B
        private async Task App_GetAllWalletBalanceService()
        {
            try
            {
                try
                {
                    // 安全遍历方式所有网络
                    foreach (Cluster c in Enum.GetValues(typeof(Cluster)))
                    {
                        if (c is Cluster.MainNet) {
                            string t = "";
                        }
                        string network = c.ToString();
                        var wallets = await SQLiteUtils._freesql.Select<WalletEntity>().Where(x => x.NetWorkType == network).ToListAsync();
                        if (wallets.Count() > 0)//有钱包数据情况下才更新
                        {
                            for (int i = 0; i < wallets.Count; i++)
                            {//获取最新的钱包余额
                                var walletBalance = await _container.Resolve<IWalletService>().GetWalletLamportsBalanceAsync(wallets[i].Id);
                                if (wallets[i].LamportsBalance != walletBalance.Data)
                                {//如果不一致，才更新
                                    wallets[i].LamportsBalance = walletBalance.Data;//
                                    wallets[i].LastModificationTime = DateTime.Now;
                                    var updateCount = await SQLiteUtils._freesql.Update<WalletEntity>()
                                        .SetSource(wallets[i]).ExecuteAffrowsAsync();
                                    LogInfo("App_GetAllWalletBalance", "更新钱包余额成功，执行成功：" + wallets[i].ToMsJson());
                                    _eventSendExtension.SendMessage();//要求更新钱包数据
                                    await Task.Delay(1000);//请求停一下，不要那么频繁请求
                                }
                             await  UpdateWalletAccountDataAsync(wallets[i].Id);
                            }

                        }
                    }
                }
                catch (Exception ex)
                {

                    LogError("执行钱包余额更新服务异常", ex);
                }


                // 注册服务B
                Schedule_UpdateAllWalletBalanceService();
            }
            finally
            {
                // 确保停止服务A
                _timerService.StopTask("App_GetAllWalletBalanceService");
            }
        }

        /// <summary>
        /// 更新钱包对应账户的数据
        /// </summary>
        /// <param name="walletId"></param>
        /// <returns></returns>
        public async Task UpdateWalletAccountDataAsync(long walletId) {

            try
            {
                var res = await _container.Resolve<IWalletService>().GetTokenAccountInfoAsync(walletId);
                var objList = res.Data.Adapt<List<WalletAccountDto>>().Adapt<List<WalletAccountEntity>>();

                objList.ForEach(x =>
                {
                    x.CreationTime = DateTime.Now;
                    x.CreatorUserId =99999999;//标识后台服务更新的
                    x.IsActive = true;
                    x.BelongWalletId = walletId;
                });
               
                SQLiteUtils.ExecuteTransaction(() =>
                {
                    var deleteCount = SQLiteUtils._freesql.Delete<WalletAccountEntity>().Where(x => x.BelongWalletId == walletId).ExecuteAffrows();
                    var count = SQLiteUtils._freesql.InsertOrUpdate<WalletAccountEntity>()
                       .SetSource(objList)
                       .IfExistsDoNothing().ExecuteAffrows();
                });
                LogInfo("App_UpdateWalletAccountData", "获取钱包最新账户数据，执行成功：" + objList.ToMsJson());
            }
            catch (Exception ex)
            {
                LogError("StartError_App_UpdateWalletAccountData", ex);


            }
        }

        /// <summary>
        /// 服务B 实现
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private async Task Update_AllWalletBalanceService()
        {
            // 安全遍历方式所有网络
            foreach (Cluster c in Enum.GetValues(typeof(Cluster)))
            {
                string network = c.ToString();
                var wallets = await SQLiteUtils._freesql.Select<WalletEntity>().Where(x => x.NetWorkType == network).ToListAsync();
                if (wallets.Count() > 0)//有钱包数据情况下才更新
                {
                    if (DefaultConfig.AppConfig.ScheduleToTransRecordExecute)
                    { //如果开启交易记录模式验证
                        var toDayTrans = await SQLiteUtils._freesql.Select<WalletAccountTransRecordEntity>()
                                                .Where(x => x.CreationTime == DateTime.Today).ToListAsync();//获取当天交易记录
                        if (toDayTrans.Count > 0)
                        { //今天有交易，才进行拉取数据更新
                            ///从wallet 钱包里面查找创建时间比交易时间 小的数据
                            var existCreatedData = wallets.Where(x => toDayTrans.Any(y => y.CreationTime >= x.CreationTime)).ToList();
                            if (existCreatedData.Count > 0)
                            {
                                foreach (var e in existCreatedData)
                                {
                                    if (e.LastModificationTime != null)
                                    { //如果修改时间存在,使用修改时间
                                        if (toDayTrans.Any(x => x.CreationTime <= e.LastModificationTime))
                                        {
                                            var walletBalance = await _container.Resolve<IWalletService>().GetWalletLamportsBalanceAsync(e.Id);
                                            if (e.LamportsBalance != walletBalance.Data)
                                            {//如果不一致，才更新
                                                e.LamportsBalance = walletBalance.Data;//
                                                e.LastModificationTime = DateTime.Now;
                                                var updateCount = await SQLiteUtils._freesql.Update<WalletEntity>()
                                                    .SetSource(e).ExecuteAffrowsAsync();
                                                LogInfo("Schedule_UpdateAllWalletBalance_LastModificationTime", "更新钱包余额成功，执行成功：" + e.ToMsJson());
                                                _eventSendExtension.SendMessage();//要求更新钱包数据
                                                await Task.Delay(1000);//请求停一下，不要那么频繁请求
                                            }
                                        }

                                    }
                                    else
                                    {//LastModificationTime 不存在说明是新的钱包，要尝试更新数据
                                        var walletBalance = await _container.Resolve<IWalletService>().GetWalletLamportsBalanceAsync(e.Id);
                                        if (e.LamportsBalance != walletBalance.Data)
                                        {//如果不一致，才更新
                                            e.LamportsBalance = walletBalance.Data;//
                                            e.LastModificationTime = DateTime.Now;
                                            var updateCount = await SQLiteUtils._freesql.Update<WalletEntity>()
                                                .SetSource(e).ExecuteAffrowsAsync();
                                            LogInfo("Schedule_UpdateAllWalletBalance_CreationTime", "更新钱包余额成功，执行成功：" + e.ToMsJson());
                                            _eventSendExtension.SendMessage();//要求更新钱包数据
                                            await Task.Delay(1000);//请求停一下，不要那么频繁请求
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else
                    {//全量更新
                        for (int i = 0; i < wallets.Count; i++)
                        {//获取最新的钱包余额
                            var walletBalance = await _container.Resolve<IWalletService>().GetWalletLamportsBalanceAsync(wallets[i].Id);
                            if (wallets[i].LamportsBalance != walletBalance.Data)
                            {//如果不一致，才更新
                                wallets[i].LamportsBalance = walletBalance.Data;//
                                wallets[i].LastModificationTime = DateTime.Now;
                                var updateCount = await SQLiteUtils._freesql.Update<WalletEntity>()
                                    .SetSource(wallets).ExecuteAffrowsAsync();
                                LogInfo("Schedule_UpdateAllWalletBalance_All", "更新钱包余额成功，执行成功：" + wallets[i].ToMsJson());
                                _eventSendExtension.SendMessage();//要求更新钱包数据
                                await Task.Delay(1000);//请求停一下，不要那么频繁请求
                            }
                        }
                    }



                }
            }
            //// 30%概率失败
            //if (_random.NextDouble() < 0.3)
            //{
            //    throw new Exception("模拟随机失败");
            //}

            LogInfo("AppUpdate_AllWalletBalance", "服务B执行成功");
        }

        // 服务B实现（模拟随机失败）
        private readonly Random _random = new();

        #region 日志工具
        private void LogInfo(string typeName, string message)
        {
            LogUtils.WriteLog(new LogDto()
            {
                CreateDate = DateTime.Now,
                Message = message,
                TypeName = typeName

            });
            //Dispatcher.Invoke(() =>
            //    Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] {message}"));
        }

        private void LogError(string typeName, Exception ex)
        {
            LogUtils.WriteLog(new LogDto()
            {
                CreateDate = DateTime.Now,
                Message = ex.ToMsJson(),
                TypeName = typeName

            });
            //Dispatcher.Invoke(() =>
            //    Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] {service}错误: {ex.Message}"));
        }
        #endregion 日志工具 
    }
}
