using Solnet.Wallet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

namespace YC.ApplicationService.Utils
{
    /// <summary>
    /// 注册工具
    /// </summary>
    public class RegistrationVerificationUtils
    {
        // 获取本机硬件唯一参数
        public static string GetHardwareInfo()
        {
            string cpuInfo = string.Empty;
            string driveInfo = string.Empty;

            // 获取 CPU ID
            using (ManagementClass mc = new ManagementClass("win32_processor"))
            {
                ManagementObjectCollection moc = mc.GetInstances();
                foreach (ManagementObject mo in moc)
                {
                    cpuInfo = mo.Properties["processorID"].Value.ToString();
                    break;
                }
            }

            // 获取硬盘序列号
            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PhysicalMedia"))
            {
                foreach (ManagementObject wmi_HD in searcher.Get())
                {
                    if (wmi_HD["SerialNumber"] != null)
                    {
                        driveInfo = wmi_HD["SerialNumber"].ToString().Trim();
                        break;
                    }
                }
            }

            return $"{cpuInfo}{driveInfo}";
        }

        // 签名方法
        public static string SignData(string data, PrivateKey privateKey)
        {
            byte[] dataBytes = Encoding.UTF8.GetBytes(data);
            byte[] signature = privateKey.Sign(dataBytes);
            return Convert.ToBase64String(signature);
        }

        // 验签方法
        public static bool VerifySignature(string data, string signature, PublicKey publicKey)
        {
            byte[] dataBytes = Encoding.UTF8.GetBytes(data);
            byte[] signatureBytes = Convert.FromBase64String(signature);
            return publicKey.Verify(dataBytes, signatureBytes);
        }
    }
}
