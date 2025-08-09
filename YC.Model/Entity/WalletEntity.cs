using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YC.Model.Entity
{
    [Table("Wallet")]
    public class WalletEntity : FullEntity<long>
    {
        [Display(Name = "钱包名称")]
        public string WalletName { get; set; }

        [Display(Name = "钱包主账户")]
        public string MasterAccountPublicKey { get; set; }
        public string MnemonicStr { get; set; }

        [Display(Name = "创建网络类别")]
        public string NetWorkType { get; set; }

        [Display(Name = "钱包完整内容")]
        public string WalletContent { get; set; }

        [Display(Name = "Mnemonic")]
        [NotMapped]
        public Mnemonic Mnemonic { get; set; }


        [NotMapped]
        public Account Account { get; set; }

        [Display(Name = "Lamports余额")]

        public ulong LamportsBalance { get; set; }
    }

     
    public class PrivateKey
    {
        /// <summary>
        /// 
        /// </summary>
        public string Key { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string KeyBytes { get; set; }
    }

    public class PublicKey
    {
        /// <summary>
        /// 
        /// </summary>
        public string Key { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string KeyBytes { get; set; }
    }

    public class Account
    {
        /// <summary>
        /// 
        /// </summary>
        public PrivateKey PrivateKey { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public PublicKey PublicKey { get; set; }
    }

    public class WordList
    {
        /// <summary>
        /// 
        /// </summary>
        public string Space { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int WordCount { get; set; }
    }

    public class Mnemonic
    {
        /// <summary>
        /// 
        /// </summary>
        public string IsValidChecksum { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public WordList WordList { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public List <int> Indices { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public List <string> Words { get; set; }
    }



}
