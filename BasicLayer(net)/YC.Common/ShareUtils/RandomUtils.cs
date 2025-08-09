using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace YC.Common.ShareUtils
{
  public  class RandomUtils
    {
        /// <summary>
        /// 描 述:创建加密随机数生成器 生成强随机种子
        /// </summary>
        /// <returns></returns>
      private  static int GetRandomSeed()
        {
            byte[] bytes = new byte[4];
            System.Security.Cryptography.RNGCryptoServiceProvider rng = new System.Security.Cryptography.RNGCryptoServiceProvider();
            rng.GetBytes(bytes);
            return BitConverter.ToInt32(bytes, 0);
        }

        /// <summary>
        /// 获取真正随机数
        /// </summary>
        /// <param name="numMin"></param>
        /// <param name="numMax"></param>
        /// <returns></returns>
        public static int GetRandom(int numMin=0,int numMax=10000) {

            int ranNumer = new Random(GetRandomSeed()).Next(numMin, numMax);

            return ranNumer;
        }

       public static string GenerateRandomNumber()
        {
            // 使用 RNGCryptoServiceProvider 生成真正的随机数
            using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
            {
                byte[] randomBytes = new byte[4];
                rng.GetBytes(randomBytes);

                // 将字节数组转换为整数
                int randomInt = BitConverter.ToInt32(randomBytes, 0);

                // 确保随机数为正数
                randomInt = Math.Abs(randomInt);

                // 将整数转换为字符串
                return randomInt.ToString();
            }
        }
    }
}
