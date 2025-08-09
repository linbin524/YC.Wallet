using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YC.Model.Entity
{
    [Table("Sys_Config")]
    public class SysConfigEntity : BaseEntity<long>
    {
        [Display(Name = "当前语言")]
        [StringLength(32, ErrorMessage = "{0}不能超过32个字符！")]
        public string LocalLanguage { get; set; }

        [Display(Name = "当前钱包网络")]
        [StringLength(32, ErrorMessage = "{0}不能超过32个字符！")]
        public string LocalWalletNetwork { get; set; }
    }
}
