using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YC.Model.Entity;

namespace YC.ApplicationService.DTO
{
    public class SendTransactionDto
    {
        public long WalletId { get; set; }

        public int TansType { get; set; }

        public string Amount { get; set; }

        public TokenDefEntity TokenDef { get; set; }
        public WalletAccountEntity WalletAccount{ get; set; }
        public string Receiver { get; set; }
        public string PayAccount { get; set; }
        public string? NetWork { get; set; }

        public string Remark { get; set; }

        public string Balance { get; set; }

    }
}
