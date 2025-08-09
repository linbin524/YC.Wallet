using FreeSql.DataAnnotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationService.Model
{
    [Table(Name = "CombinationDetail")]
    public class CombinationDetailDto
    {
        [Column(IsIdentity = true, IsPrimary = true)]
        public long Id { get; set; }

        /// <summary>
        /// 排序事件Id
        /// </summary>
        public long CombinationId { get; set; }

        /// <summary>
        /// 排序所属顺序
        /// </summary>

        public long IndexId { get; set; }

        /// <summary>
        /// 单个排序组合字符串
        /// </summary>
        public string CombinationString { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedTime { get; set; }
    }
}
