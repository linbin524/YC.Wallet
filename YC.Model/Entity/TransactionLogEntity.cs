using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace YC.Model.Entity
{
    // ================== 交易操作日志表 ==================
    [Table("transaction_logs")]
    public class TransactionLogEntity : FullEntity<long>
    {
        [Display(Name = "交易哈希")]
        public string TransactionId { get; set; }

        [Display(Name = "事件类型")]
        public string EventType { get; set; }

        [Display(Name = "状态")]
        public string Status { get; set; }

        [Display(Name = "发起方地址")]
        public string FromAddress { get; set; }

        [Display(Name = "接收方地址")]
        public string ToAddress { get; set; }

        [Display(Name = "金额")]
        public decimal Amount { get; set; }

        [Display(Name = "手续费")]
        public decimal FeeCharged { get; set; }

        [Display(Name = "区块哈希")]
        public string BlockHash { get; set; }

        [Display(Name = "操作者IP")]
        public string OperatorIp { get; set; }

        [Display(Name = "错误代码")]
        public string ErrorCode { get; set; }

        [Display(Name = "错误详情")]
        public string ErrorDetails { get; set; }

        [Display(Name = "记录时间")]
        public DateTime CreatedAt { get; set; }
    }

}