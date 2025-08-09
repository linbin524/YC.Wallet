using FreeSql.DataAnnotations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApplicationService.Model
{
    
    
    [Table(Name ="Combination")]
    public class CombinationDto
    {
       
        [Column(IsIdentity = true, IsPrimary = true)]
        public long Id { get; set; }

        /// <summary>
        /// 复式类别名，选一，......，选十等
        /// </summary>
        public string TypeName { get; set; }

        /// <summary>
        /// 选中选择随机数
        /// </summary>
        public string ChooseNumberAarrayString { get; set; }

        /// <summary>
        /// 选择数字有几个
        /// </summary>
        public int ChooseNumberCount { get; set; }
        /// <summary>
        /// 批次唯一号
        /// </summary>
        public string UUID { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedTime{get;set;}

        /// <summary>
        /// 排列组合共几组
        /// </summary>
        public int PermutationGroupCount { get; set; }
        /// <summary>
        /// 需要花费总金额，每一注2元
        /// </summary>
        public long Amount { get; set; }

        /// <summary>
        /// 排列组合类别，自选 0，随机 1
        /// </summary>
        public int PermutationType { get; set; }
    }
}
