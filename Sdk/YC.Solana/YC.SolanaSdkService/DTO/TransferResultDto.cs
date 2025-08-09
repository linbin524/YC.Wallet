using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YC.SolanaSdkService
{
    public class TransferResultDto
    {
        public ulong Fee { get; set; }

        /// <summary>
        /// 豁免租金
        /// </summary>
        public ulong RentExemption { get; set; }

        public string LatestBlockHash { get; set; }

        /// <summary>
        /// 交易签名，也就是交易成功后返回的，和其他区块链的交易hash一样的
        /// </summary>
        public string TxSignature { get; set; }
    }
}
