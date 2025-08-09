using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YC.Model.Entity
{
    [Table("TokenMints")]
    public class TokenMintEntity : FullEntity<long>  // 主键类型为long（MintId）
    {
        [Display(Name = "交易哈希")]
        public string TransactionHash { get; set; }

        [Display(Name = "代币合约地址")]
        public string TokenAddress { get; set; }

        [Display(Name = "接收地址")]
        public string MintToAddress { get; set; }

        [Display(Name = "数量")]
        public decimal Quantity { get; set; }

        [Display(Name = "元数据链接")]
        public string MetadataUri { get; set; }

        [Display(Name = "操作者地址")]
        public string CreatedBy { get; set; }

        [Display(Name = "铸币时间")]
        public DateTime CreatedAt { get; set; }
    }
}
