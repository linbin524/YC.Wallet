using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YC.SolanaSdkService.DTO
{

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
        public List<int> Indices { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string[] Words { get; set; }
    }

    public class WalletDto
    {
        /// <summary>
        /// 
        /// </summary>
        public Account Account { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public Mnemonic Mnemonic { get; set; }
    }
}
