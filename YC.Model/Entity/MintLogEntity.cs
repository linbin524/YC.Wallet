using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YC.Model.Entity
{
    // ================== 铸币操作日志表 ==================
    [Table("mint_logs")]
    public class MintLogEntity : FullEntity<long>
    {
        [Display(Name = "铸币记录ID")]
        public long MintId { get; set; }

        [Display(Name = "事件类型")]
        public string EventType { get; set; }

        [Display(Name = "代币合约地址")]
        public string TokenAddress { get; set; }

        [Display(Name = "接收地址")]
        public string MintToAddress { get; set; }

        [Display(Name = "铸造数量")]
        public decimal Quantity { get; set; }

        [Display(Name = "变更前元数据")]
        public string MetadataBefore { get; set; }  // JSON存储

        [Display(Name = "变更后元数据")]
        public string MetadataAfter { get; set; }    // JSON存储

        [Display(Name = "操作者ID")]
        public string OperatorId { get; set; }

        [Display(Name = "失败原因")]
        public string ErrorReason { get; set; }

        [Display(Name = "记录时间")]
        public DateTime CreatedAt { get; set; }
    }
}
