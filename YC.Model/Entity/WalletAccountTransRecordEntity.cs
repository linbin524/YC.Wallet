using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YC.Model.Entity
{
    [Table("WalletAccountTransRecord")]
    public class WalletAccountTransRecordEntity : FullEntity<long>
    {
        [Display(Name = "付款账户")]
        public string? PayAccount { get; set; }

        [Display(Name = "代币符号")]
        public string? TokenSymbol { get; set; }

        [Display(Name = "创建时间")]
        public DateTime? CreationTime { get; set; }

        [Display(Name = "代币铸造地址")]
        public string? TokenMint { get; set; }

        [Display(Name = "JSON 内容")]
        public string? JsonContent { get; set; }

        [Display(Name = "适配网络")]
        public string? NetWork { get; set; }

        [Display(Name = "交易备注")]
        public string? Remark { get; set; }
        [Display(Name = "转出方")]
        public string? Transferor { get; set; }

        [Display(Name = "转出方")]
        public string? Receiver { get; set; }
        [Display(Name = "转账数量")]
        public string? TransferQuantity { get; set; }

        /// <summary>
        /// 1 成功，0 失败， 2 等待确认
        /// </summary>
        [Display(Name = "转账状态")]
        public int? TransferStatus{ get; set; }

        [Display(Name = "区块号")]
        public string? RecentBlockhash { get; set; }

        [Display(Name = "手续费")]
        public ulong Fee { get; set; }

        [Display(Name = "交易哈希")]
        public string TransactionHash { get; set; }

        /// <summary>
        /// lamports、nft、SPLToken
        /// </summary>
        [Display(Name = "交易类型")]
        public string TransactionType { get; set; }

        public long WalletId { get; set; }

        public bool IsAssociatedTokenAccount { get; set; }

        public  long? BlockTime { get; set; }

    }
}
