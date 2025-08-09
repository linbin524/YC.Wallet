using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YC.Model.Entity
{
    /// <summary>
    /// TokenDef
    /// </summary>
    [Table("TokenDef")]
    public class TokenDefEntity: FullEntity<long>
    {
        [Display(Name = "代币唯一标识(铸币时候的账户)")]
        public string Mint { get; set; }
        [Display(Name = "代币名称")]
        public string Name { get; set; }
        [Display(Name = "标识符号")]
        public string Symbol { get; set; }
        [Display(Name = "小数位")]
        public int DecimalPlaces { get; set; }
        [Display(Name = "发行网络")]
        public string SupplyNetWork { get; set; }
        [Display(Name = "类别")]
        public string Type { get; set; }
        [Display(Name = "售卖手续费比例")]
        public string SellerFee { get; set; }

        //
        // 摘要:
        //     The Coingecko identifier as supplied by the standard Solana token list or null
        [Display(Name = "标准Solana令牌列表提供的Coingecko标识符")]
        public string CoinGeckoId { get; set; }

        //
        // 摘要:
        //     The token project / more info url as supplied by the standard Solana token list
        //     or null
        [Display(Name = "代币项目地址")]
        public string TokenProjectUrl { get; set; }

        //
        // 摘要:
        //     The token logo url as supplied by the standard Solana token list or null
        [Display(Name = "代币LOGO")]
        public string TokenLogoUrl { get; set; }
    }
}
