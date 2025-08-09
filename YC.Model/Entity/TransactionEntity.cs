using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YC.Model.Entity
{
    [Table("Transactions")]
    public class TransactionEntity : FullEntity<long> // 主键类型为string（TransactionId）
    {
        [Display(Name = "交易哈希")]
        public string TransactionHash { get; set; }
        [Display(Name = "交易类型")]
        public string TransactionType { get; set; }

        [Display(Name = "状态")]
        public string Status { get; set; }

        [Display(Name = "发起方地址")]
        public string FromAddress { get; set; }

        [Display(Name = "接收方地址")]
        public string ToAddress { get; set; }

        [Display(Name = "金额")]
        public decimal Amount { get; set; }

        [Display(Name = "手续费")]
        public decimal Fee { get; set; }

        [Display(Name = "代币合约地址")]
        public string TokenAddress { get; set; }

        [Display(Name = "提交时间")]
        public DateTime CreatedAt { get; set; }

        [Display(Name = "确认时间")]
        public DateTime? ConfirmedAt { get; set; }

        [Display(Name = "区块号")]
        public long? BlockNumber { get; set; }

        [Display(Name = "错误信息")]
        public string ErrorMessage { get; set; }
    }
}
