using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YC.ApplicationService.DefaultConfigure
{
    /// <summary>
    /// 默认应用配置
    /// </summary>
    public class DefaultAppConfig
    {
        /// <summary>
        /// 是否为调试模式
        /// </summary>
        public bool IsDebug { get; set; }
        /// <summary>
        /// 定时服务是否按照交易记录验证执行
        /// </summary>
        public bool ScheduleToTransRecordExecute { get; set; }
        //
        public bool ScheduleServiceIsEnabeld { get; set; }
        public int ScheduleInitialDelay { get; set; }
        public int[] ScheduleRetryIntervals { get; set; }
        public int ScheduleNormalInterval { get; set; }
        public int ScheduleMaxRetries { get; set; }
       
        public string RegistrationCode { get; set; }





    }

   


   
}