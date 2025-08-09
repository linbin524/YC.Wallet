using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YC.SolanaSdkService
{
    public class CreatedTokenResultDto 
    {
        /// <summary>
        /// 授权管理超级用户，负责授权和铸币花费
        /// </summary>
        public String PayAccountPublicKey { get; set; }
        /// <summary>
        /// 复制铸造的账户，只负责干活，不负责存储最后的代币
        /// </summary>
        public String MintAccountPublicKey { get; set; }

        /// <summary>
        /// token 名称
        /// </summary>
        public string TokenName { get; set; }

        /// <summary>
        /// token 字符标识
        /// </summary>
        public string TokenSymbol { get; set; }
        /// <summary>
        /// Token 图标，通常是ipfs等网上可以访问的图片logo
        /// </summary>
        public string Uri { get; set; }

        /// <summary>
        /// 发行量，要注意结合小数位，如果发行10000个，加上小数位2，
        ///所以实际TokenSupply 要1000000，才可以标识是一万
        /// </summary>
        public ulong TokenSupply { get; set; }

        /// <summary>
        /// 小数点
        /// </summary>
        public int Decimals { get; set; }
        /// <summary>
        /// 铸币备注信息
        /// </summary>
        public string MemoString { get; set; }

        /// <summary>
        /// 接收代币账户
        /// </summary>
        public string DestinationPublicKey { get; set; }
        /// <summary>
        /// 接收铸造代币账户类型
        /// 
        /// 0 普通账户，1 关联账户
        /// </summary>
        public int DestinationAccountType { get; set; }

        /// <summary>
        /// 铸造代币时间
        /// </summary>
        public DateTime CreatedTokenTime { get; set; }

        /// <summary>
        /// 交易哈希
        /// </summary>
        public string CreatedTokenTranscationTxHash { get; set; }

        /// <summary>
        /// 交易请求信息
        /// </summary>
        public string TransRequestInfo { get; set; }

        /// <summary>
        /// 交易回执信息
        /// </summary>
        public string TransResponseInfo { get; set; }

        /// <summary>
        /// 交易手续费
        /// </summary>
        public ulong Fee { get; set; }
    }
}
