
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace YC.Model.Entity
{
    [Table("account_operations")]
    public class AccountOperationEntity : FullEntity<long>  // 主键类型为long（OperationId）
    {
        [Display(Name = "账户地址")]
        public string AccountAddress { get; set; }

        [Display(Name = "操作类型")]
        public string OperationType { get; set; }

        [Display(Name = "操作者地址")]
        public string InitiatedBy { get; set; }

        [Display(Name = "关联交易哈希")]
        public string TransactionId { get; set; }

        [Display(Name = "操作原因")]
        public string Reason { get; set; }

        [Display(Name = "操作时间")]
        public DateTime CreatedAt { get; set; }
    }
}
