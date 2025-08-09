using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YC.Model.Entity
{
    // ================== 错误聚合日志表 ==================
    [Table("error_aggregation_logs")]
    public class ErrorAggregationLogEntity : FullEntity<long>
    {
        [Display(Name = "错误代码")]
        public string ErrorCode { get; set; }

        [Display(Name = "错误来源")]
        public string ErrorSource { get; set; }

        [Display(Name = "关联ID")]
        public string RelatedId { get; set; }

        [Display(Name = "错误计数")]
        public int ErrorCount { get; set; }

        [Display(Name = "首次发生时间")]
        public DateTime FirstOccurrence { get; set; }

        [Display(Name = "最后发生时间")]
        public DateTime LastOccurrence { get; set; }

        [Display(Name = "错误示例")]
        public string SampleMessage { get; set; }

        [Display(Name = "告警状态")]
        public string AlertStatus { get; set; }

        [Display(Name = "记录时间")]
        public DateTime CreatedAt { get; set; }
    }
}
