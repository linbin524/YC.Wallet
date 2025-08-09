using Solnet.Rpc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YC.ApplicationService.DefaultConfigure.Model
{
    public class WalletNetwork
    {
        public string? Name { get; set; }
        public Cluster Cluster { get; set; }

    }
}
