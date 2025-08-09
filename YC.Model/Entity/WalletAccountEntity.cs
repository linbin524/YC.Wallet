using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YC.Model.Entity
{
    [Table("WalletAccount")]
    public class WalletAccountEntity: FullEntity<long>
    {
        [Display(Name = "账户名称")]
        public string AccountName { get; set; }

        [Display(Name = "余额")]
        public string Balance { get; set; }

        [Display(Name = "所属钱包")]
        public long BelongWalletId { get; set; }
       
        /// <summary>
        /// 0 正常，1 冻结，2 销毁
        /// </summary>
        [Display(Name = "状态")]
        public int Status { get; set; }

        [NotMapped]
        [Display(Name = "交易记录")]
        public List<TransactionEntity> Transactions { get; set; }
        
        [Display(Name = "代币详情")]
        public string TokenDetails { get; set; }

        [Display(Name = "公钥")]
        public string PublicKey { get; set; }

        [Display(Name = "所有者")]
        public string Owner { get; set; }

        [Display(Name = "是否为关联代币账户")]
        public bool IsAssociatedTokenAccount { get; set; }

        [Display(Name = "账户类型")]
        public string AccountType { get; set; }

        [Display(Name = "兰波特数量（整数）")]
        public int Lamports { get; set; }

        [NotMapped]
        [Display(Name = "兰波特数量（双精度）")]
        public double DoubleTypeLamports { get; set; }

        [Display(Name = "账户数量")]
        public int AccountCount { get; set; }

        [Display(Name = "代币铸造地址")]
        public string TokenMint { get; set; }

        [Display(Name = "代币符号")]
        public string Symbol { get; set; }

        [Display(Name = "代币名称")]
        public string TokenName { get; set; }

        [Display(Name = "小数位数")]
        public int DecimalPlaces { get; set; }

        [Display(Name = "带小数的数量")]
        public Double QuantityDecimal { get; set; }

        [Display(Name = "原始数量")]
        public ulong QuantityRaw { get; set; }
    }
}
