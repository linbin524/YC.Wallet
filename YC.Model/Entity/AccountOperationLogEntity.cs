using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YC.Model.Entity
{
    // ================== 账户操作日志表 ==================
    [Table("account_operation_logs")]
    public class AccountOperationLogEntity : FullEntity<long>
    {
        [Display(Name = "账户地址")]
        public string AccountAddress { get; set; }

        [Display(Name = "操作类型")]
        public string OperationType { get; set; }

        [Display(Name = "操作前状态")]
        public string PreviousStatus { get; set; }

        [Display(Name = "操作后状态")]
        public string CurrentStatus { get; set; }

        [Display(Name = "操作发起方")]
        public string InitiatedBy { get; set; }

        [Display(Name = "关联交易哈希")]
        public string RelatedTxId { get; set; }

        [Display(Name = "操作备注")]
        public string AuditComment { get; set; }

        [Display(Name = "操作设备指纹")]
        public string OperatorDevice { get; set; }

        [Display(Name = "记录时间")]
        public DateTime CreatedAt { get; set; }
    }
}
