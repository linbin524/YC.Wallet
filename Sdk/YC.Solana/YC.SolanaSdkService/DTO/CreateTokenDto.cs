using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YC.SolanaSdkService
{
    /// <summary>
    /// 创建代币的 DTO
    /// </summary>
    public class CreateTokenDto
    {
        /// <summary>
        /// 授权管理超级用户，负责授权和铸币花费
        /// </summary>
        public Solnet.Wallet.Account PayAccount { get; set; }
        /// <summary>
        /// 复制铸造的账户，只负责干活，不负责存储最后的代币
        /// </summary>
        public Solnet.Wallet.Account MintAccount { get; set; }
        
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
        /// 是否采用关联账户存储代币
        /// </summary>
        public bool IsStorageAssociatedAccount { get; set; }

        /// <summary>
        /// 存储代币的用户
        /// </summary>
        public Solnet.Wallet.Account StorageTokenAccount { get; set; }

        /// <summary>
        /// 网络
        /// </summary>
         public string? NetWork { get; set; }

    }

}
