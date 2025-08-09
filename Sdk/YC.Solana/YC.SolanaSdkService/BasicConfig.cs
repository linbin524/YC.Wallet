using Solnet.Rpc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace YC.SolanaSdkService
{
    public class BasicConfig
    {
        public BasicConfig() {
           


        }

       
        /// <summary>
        /// 设置当前全局网络
        /// </summary>
        public static Cluster LocalNet { get; set; }

        public static IRpcClient RpcClient { get {
               
                return ClientFactory.GetClient(BasicConfig.LocalNet);// 定义全局RPC
            } }
    }
}
