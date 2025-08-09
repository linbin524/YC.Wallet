using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace YC.Common.ShareUtils
{
   public class IPUtils
    {
        /// <summary>
        /// 是否为ip
        /// </summary>
        /// <param name="ip"></param>
        /// <returns></returns>
        public static bool IsIP(string ip)
        {
            return Regex.IsMatch(ip, @"^((2[0-4]\d|25[0-5]|[01]?\d\d?)\.){3}(2[0-4]\d|25[0-5]|[01]?\d\d?)$");
        }

        /// <summary>
        /// 获得IP地址
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static string GetIP(HttpRequest request)
        {
            if (request == null)
            {
                return "";
            }

            string ip = request.Headers["X-Real-IP"].FirstOrDefault();
            if (ip.IsNull())
            {
                ip = request.Headers["X-Forwarded-For"].FirstOrDefault();
            }
            if (ip.IsNull())
            {
                ip = request.HttpContext?.Connection?.RemoteIpAddress.MapToIPv4().ToString() + ":" + request.HttpContext?.Connection?.RemotePort;
                //ip = request.HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString()
            }
            if (ip.IsNull())
            {
                ip = "127.0.0.1";
            }

            return ip;
        }

        /// <summary>
        /// 获得IP地址
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static string GetIP(HttpContext context,bool incloudPort=true)
        {
            if (context.Request == null)
            {
                return "";
            }

            if (context.Request.Path.Value.Contains("/api/ChatGPTMember/PostChatGPT"))
            {
                string result = context.Request.Headers["HTTP_X_FORWARDED_FOR"];
                if (string.IsNullOrEmpty(result))
                {
                    result = context.Request.Headers["REMOTE_ADDR"];
                }
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("connection:" + context.Request.HttpContext?.Connection?.RemoteIpAddress+":"+ context.Request.HttpContext?.Connection?.RemotePort);
                sb.AppendLine("Headers【X-Forwarded-For】:" + context.Request.Headers["X-Forwarded-For"].ToJson());
                sb.AppendLine("Headers【HTTP_X_FORWARDED_FOR】:" + context.Request.Headers["HTTP_X_FORWARDED_FOR"].ToJson());
                //sb.AppendLine("Headers【REMOTE_ADDR】:" + context.Request.Headers["REMOTE_ADDR"].ToJson());
            
                sb.AppendLine("Headers" + context.Request.Headers.ToJson());
                sb.AppendLine("ip:" + context.Request.HttpContext?.Connection?.RemoteIpAddress?.ToString());
                LogUtils.WriteLog(new LogDto() { TypeName = "chatGPT 拦截IP请求日志", Message = sb.ToString() });
            }

            string ip = context.Request.Headers["X-Real-IP"].FirstOrDefault();
            if (ip.IsNull())
            {
                ip = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            }
            if (ip.IsNull())
            {
                if (!incloudPort) {//不包括端口
                    ip = context.Request.HttpContext?.Connection?.RemoteIpAddress.MapToIPv4().ToString();
                }
                else {
                    ip = context.Request.HttpContext?.Connection?.RemoteIpAddress.MapToIPv4().ToString() + ":" + context.Request.HttpContext?.Connection?.RemotePort;

                }
              
            }
            if (ip.IsNull())
            {
                ip = "127.0.0.1:1234";
            }

            return ip;
        }

        /// <summary>
        /// 获得MAC地址
        /// </summary>
        /// <returns></returns>
        public static string GetMACIp()
        {
            //本地计算机网络连接信息
            //IPGlobalProperties computerProperties = IPGlobalProperties.GetIPGlobalProperties();
            //获取本机电脑名
            //var HostName = computerProperties.HostName;
            //获取域名
            //var DomainName = computerProperties.DomainName;

            //获取本机所有网络连接
            NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();

            if (nics == null || nics.Length < 1)
            {
                return "";
            }

            var MACIp = "";
            foreach (NetworkInterface adapter in nics)
            {
                var adapterName = adapter.Name;

                var adapterDescription = adapter.Description;
                var NetworkInterfaceType = adapter.NetworkInterfaceType;
                if (adapterName == "本地连接" || adapterName == "WLAN")
                {
                    PhysicalAddress address = adapter.GetPhysicalAddress();
                    byte[] bytes = address.GetAddressBytes();

                    for (int i = 0; i < bytes.Length; i++)
                    {
                        MACIp += bytes[i].ToString("X2");

                        if (i != bytes.Length - 1)
                        {
                            MACIp += "-";
                        }
                    }
                }
            }

            return MACIp;
        }
    }

  
}
