
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace YC.Common.ShareUtils
{
    /// <summary>
    /// DES加密、解密帮助类
    /// </summary>
    public static class EncryptUtils
    {
        private static string DESKey = "";

        #region ========加密========
        /// <summary>
        /// 加密
        /// </summary>
        /// <param name="Text"></param>
        /// <returns></returns>
        public static string Encrypt(string Text)
        {
            return Encrypt(Text, DESKey);
        }


        public static string MD5(string pwd)
        {
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] data = System.Text.Encoding.Default.GetBytes(pwd);
            byte[] md5data = md5.ComputeHash(data);
            md5.Clear();
            string str = "";
            for (int i = 0; i < md5data.Length; i++)
            {
                str += md5data[i].ToString("x").PadLeft(2, '0');

            }
            return str;
        }

        /// <summary> 
        /// 加密数据 
        /// </summary> 
        /// <param name="Text"></param> 
        /// <param name="sKey"></param> 
        /// <returns></returns> 
        public static string Encrypt(string Text, string sKey)
        {
            DESCryptoServiceProvider des = new DESCryptoServiceProvider();
            byte[] inputByteArray;
            inputByteArray = Encoding.Default.GetBytes(Text);
            des.Key = ASCIIEncoding.ASCII.GetBytes(MD5(sKey).Substring(0, 8));
            des.IV = ASCIIEncoding.ASCII.GetBytes(MD5(sKey).Substring(0, 8));
            System.IO.MemoryStream ms = new System.IO.MemoryStream();
            CryptoStream cs = new CryptoStream(ms, des.CreateEncryptor(), CryptoStreamMode.Write);
            cs.Write(inputByteArray, 0, inputByteArray.Length);
            cs.FlushFinalBlock();
            StringBuilder ret = new StringBuilder();
            foreach (byte b in ms.ToArray())
            {
                ret.AppendFormat("{0:X2}", b);
            }
            return ret.ToString();
        }

        #endregion

        #region ========解密========
        /// <summary>
        /// 解密
        /// </summary>
        /// <param name="Text"></param>
        /// <returns></returns>
        public static string Decrypt(string Text)
        {
            if (!string.IsNullOrEmpty(Text))
            {

                //wjl add 180125
                try {
                    return Decrypt(Text, DESKey);
                } catch {
                    return Text;
                }
            }
            else
            {
                return "";
            }
        }
        /// <summary> 
        /// 解密数据 
        /// </summary> 
        /// <param name="Text"></param> 
        /// <param name="sKey"></param> 
        /// <returns></returns> 
        public static string Decrypt(string Text, string sKey)
        {
            DESCryptoServiceProvider des = new DESCryptoServiceProvider();
            int len;
            len = Text.Length / 2;
            byte[] inputByteArray = new byte[len];
            int x, i;
            for (x = 0; x < len; x++)
            {
                i = Convert.ToInt32(Text.Substring(x * 2, 2), 16);
                inputByteArray[x] = (byte)i;
            }
            des.Key = ASCIIEncoding.ASCII.GetBytes(MD5(sKey).Substring(0, 8));
            des.IV = ASCIIEncoding.ASCII.GetBytes(MD5(sKey).Substring(0, 8));
            System.IO.MemoryStream ms = new System.IO.MemoryStream();
            CryptoStream cs = new CryptoStream(ms, des.CreateDecryptor(), CryptoStreamMode.Write);
            cs.Write(inputByteArray, 0, inputByteArray.Length);
            cs.FlushFinalBlock();
            return Encoding.Default.GetString(ms.ToArray());
        }

        #endregion

        #region 3des加密

        /// <summary>
        /// 3des ecb模式加密
        /// </summary>
        /// <param name="aStrString">待加密的字符串</param>
        /// <param name="aStrKey">密钥</param>
        /// <param name="iv">加密矢量：只有在CBC解密模式下才适用</param>
        /// <param name="mode">运算模式</param>
        /// <returns>加密后的字符串</returns>
        public static string Encrypt3Des(string aStrString, string aStrKey, CipherMode mode = CipherMode.ECB, string iv = "12345678")
        {
            byte[] keyBytes = Encoding.UTF8.GetBytes(aStrKey);
            if (keyBytes.Length != 16 && keyBytes.Length != 24)
            {
                throw new ArgumentException("Triple DES 密钥长度必须是 16 或 24 字节。");
            }
            try
            {
                var des = new TripleDESCryptoServiceProvider
                {
                    Key = Encoding.UTF8.GetBytes(aStrKey),
                    Mode = mode
                };
                if (mode == CipherMode.CBC)
                {
                    des.IV = Encoding.UTF8.GetBytes(iv);
                }  
                var desEncrypt = des.CreateEncryptor();
                byte[] buffer = Encoding.UTF8.GetBytes(aStrString);
                return Convert.ToBase64String(desEncrypt.TransformFinalBlock(buffer, 0, buffer.Length));
            }
            catch (Exception e)
            {
                return string.Empty;
            }
        }

        #endregion

        #region 3des解密

        /// <summary>
        /// des 解密
        /// </summary>
        /// <param name="aStrString">加密的字符串</param>
        /// <param name="aStrKey">密钥</param>
        /// <param name="iv">解密矢量：只有在CBC解密模式下才适用</param>
        /// <param name="mode">运算模式</param>
        /// <returns>解密的字符串</returns>
        public static string Decrypt3Des(string aStrString, string aStrKey, CipherMode mode = CipherMode.ECB, string iv = "12345678")
        {
            try
            {
                var des = new TripleDESCryptoServiceProvider
                {
                    Key = Encoding.UTF8.GetBytes(aStrKey),
                    Mode = mode,
                    Padding = PaddingMode.PKCS7
                };
                if (mode == CipherMode.CBC)
                {
                    des.IV = Encoding.UTF8.GetBytes(iv);
                }
                var desDecrypt = des.CreateDecryptor();
                var result = "";
                byte[] buffer = Convert.FromBase64String(aStrString);
                result = Encoding.UTF8.GetString(desDecrypt.TransformFinalBlock(buffer, 0, buffer.Length));
                return result;
            }
            catch (Exception e)
            {
                return string.Empty;
            }
        }
        #endregion

        #region AES
        /// <summary>
        /// AES加密，并且有向量
        /// </summary>
        /// <param name="encrypteStr">需要加密的明文</param>
        /// <param name="key">秘钥</param>
        /// <param name="vector">向量</param>
        /// <returns>密文</returns>
        public static string AESEncryptedString(this string encrypteStr, string key, string vector)
        {
            byte[] aesBytes = Encoding.UTF8.GetBytes(encrypteStr);

            byte[] aesKey = new byte[32];
            //直接转
            Array.Copy(Convert.FromBase64String(key), aesKey, aesKey.Length);
            byte[] aesVector = new byte[16];
            //直接转
            Array.Copy(Convert.FromBase64String(vector), aesVector, aesVector.Length);

            Rijndael Aes = Rijndael.Create();
            //或者采用下方生成Aes
            //RijndaelManaged Aes = new();

            // 开辟一块内存流  
            using MemoryStream memoryStream = new MemoryStream();
            // 把内存流对象包装成加密流对象  
            using CryptoStream cryptoStream = new(memoryStream, Aes.CreateEncryptor(aesKey, aesVector), CryptoStreamMode.Write);
            // 明文数据写入加密流  
            cryptoStream.Write(aesBytes, 0, aesBytes.Length);
            cryptoStream.FlushFinalBlock();

            string result = Convert.ToBase64String(memoryStream.ToArray());
            return result;
        }



        /// <summary>
        /// AES解密，并且有向量
        /// </summary>
        /// <param name="decryptStr">被加密的明文</param>
        /// <param name="key">秘钥</param>
        /// <param name="vector">向量</param>
        /// <returns>明文</returns>
        public static string AESDecryptString(this string decryptStr, string key, string vector)
        {
            byte[] aesBytes = Convert.FromBase64String(decryptStr);
            byte[] aesKey = new byte[32];
            //直接转，可采用不同的方法，但是需与加密方法一致
            Array.Copy(Convert.FromBase64String(key), aesKey, aesKey.Length);
            byte[] aesVector = new byte[16];
            //直接转，可采用不同的方法，但是需与加密方法一致
            Array.Copy(Convert.FromBase64String(vector), aesVector, aesVector.Length);
            Rijndael Aes = Rijndael.Create();
            //或者采用下方生成Aes
            //RijndaelManaged Aes = new();

            // 开辟一块内存流，存储密文  
            using MemoryStream memoryStream = new(aesBytes);
            // 把内存流对象包装成加密流对象  
            using CryptoStream Decryptor = new(memoryStream, Aes.CreateDecryptor(aesKey, aesVector), CryptoStreamMode.Read);
            // 明文存储区  
            using MemoryStream originalMemory = new();
            byte[] Buffer = new byte[1024];
            int readBytes = 0;
            while ((readBytes = Decryptor.Read(Buffer, 0, Buffer.Length)) > 0)
            {
                originalMemory.Write(Buffer, 0, readBytes);
            }

            byte[] original = originalMemory.ToArray();
            string result = Convert.ToBase64String(originalMemory.ToArray());
            return result;
        }

        /// <summary>  
        /// AES加密(无向量)  
        /// </summary>  
        /// <param name="encrypteStr">需要加密的明文</param>  
        /// <param name="key">密钥</param>  
        /// <returns>密文</returns>  
        public static string AESEncryptedString(this string encrypteStr, string key)
        {
            byte[] aesBytes = Encoding.UTF8.GetBytes(encrypteStr);
            byte[] aesKey = new byte[32];
            //直接转
            //Array.Copy(Convert.FromBase64String(key), aesKey, aesKey.Length);
            //当长度不够时，右侧添加空格
            Array.Copy(Encoding.UTF8.GetBytes(key.PadRight(aesKey.Length)), aesKey, aesKey.Length);

            using MemoryStream memoryStream = new();
            Rijndael Aes = Rijndael.Create();
            //或者采用下方生成Aes
            //RijndaelManaged Aes = new();

            Aes.Mode = CipherMode.ECB;
            Aes.Padding = PaddingMode.PKCS7;
            Aes.KeySize = 128;
            Aes.Key = aesKey;
            using CryptoStream cryptoStream = new(memoryStream, Aes.CreateEncryptor(), CryptoStreamMode.Write);
            cryptoStream.Write(aesBytes, 0, aesBytes.Length);
            cryptoStream.FlushFinalBlock();
            Aes.Clear();
            return Convert.ToBase64String(memoryStream.ToArray());
        }


        /// <summary>  
        /// AES解密(无向量)  
        /// </summary>  
        /// <param name="decryptStr">被加密的明文</param>  
        /// <param name="key">密钥</param>  
        /// <returns>明文</returns>  
        public static string AESDecryptString(this string decryptStr, string key)
        {
            byte[] aesBytes = Convert.FromBase64String(decryptStr);
            byte[] aesKey = new byte[32];
            //需要跟加密一致
            //直接转
            //Array.Copy(Convert.FromBase64String(key), aesKey, aesKey.Length);
            //当长度不够时，右侧添加空格
            Array.Copy(Encoding.UTF8.GetBytes(key.PadRight(aesKey.Length)), aesKey, aesKey.Length);

            using MemoryStream memoryStream = new(aesBytes);
            Rijndael Aes = Rijndael.Create();
            //或者采用下方生成Aes
            //RijndaelManaged Aes = new();

            Aes.Mode = CipherMode.ECB;//需与加密方法一致
            Aes.Padding = PaddingMode.PKCS7;//需与加密方法一致
            Aes.KeySize = 128;
            Aes.Key = aesKey;
            using CryptoStream cryptoStream = new(memoryStream, Aes.CreateDecryptor(), CryptoStreamMode.Read);

            byte[] temp = new byte[aesBytes.Length + 32];
            int len = cryptoStream.Read(temp, 0, aesBytes.Length + 32);
            byte[] ret = new byte[len];
            Array.Copy(temp, 0, ret, 0, len);
            Aes.Clear();
            string result = Encoding.UTF8.GetString(ret);
            return result;
        }
        #endregion

        #region RSA

        /// <summary>
        /// rsa encryption
        /// </summary>
        /// <param name="xmlPublicKey"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        public static string RSAEncrypt(string xmlPublicKey, string content)
        {
            string encryptedContent = string.Empty;
            using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
            {
               
                rsa.FromXmlString(xmlPublicKey);
                byte[] encryptedData = rsa.Encrypt(Encoding.Default.GetBytes(content), false);
                encryptedContent = Convert.ToBase64String(encryptedData);
            }
            return encryptedContent;
        }

        /// <summary>
        /// rsa decryption
        /// </summary>
        /// <param name="xmlPrivateKey"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        public static string RSADecrypt(string xmlPrivateKey, string content)

        {
            string decryptedContent = string.Empty;

            using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())

            {
                rsa.FromXmlString(xmlPrivateKey);

                byte[] decryptedData = rsa.Decrypt(Convert.FromBase64String(content), false);

                decryptedContent = Encoding.UTF8.GetString(decryptedData);
            }

            return decryptedContent;
        }

        #endregion
    }

    
}
