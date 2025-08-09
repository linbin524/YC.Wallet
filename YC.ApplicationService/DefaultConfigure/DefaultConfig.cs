using Google.Protobuf.WellKnownTypes;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Solnet.Rpc;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using YC.ApplicationService.DefaultConfigure;
using YC.ApplicationService.DefaultConfigure.Model;
using YC.Common.ShareUtils;
using YC.Model;
using YC.Model.Entity;
using YC.SolanaSdkService;


namespace YC.ApplicationService
{
    /// <summary>
    /// 默认配置
    /// </summary>
    public class DefaultConfig
    {
        /// <summary>
        /// 如果是应用，默认要在开启时候，做一次设置动作
        /// </summary>
        public static string dbConfigFilePath = System.Environment.CurrentDirectory + "//Config//DefaultConfig.json";

        /// <summary>
        /// json配置
        /// </summary>
        private static string  jsonConfig = "";

        public static string JsonConfig
        {
            get
            {
               
                if (string.IsNullOrWhiteSpace(jsonConfig))
                {
                    jsonConfig = DefaultConfig.GetConfigJson(DefaultConfig.dbConfigFilePath);
                }
                return jsonConfig;
            }
           
        }

        public static string VerificationPublicKey = "";

        /// <summary>
        /// 程序配置
        /// </summary>
        public static DefaultAppConfig AppConfig { get; set; }

        /// <summary>
        /// 默认语言
        /// </summary>
        public static string LocalLanguage { get; set; }
        /// <summary>
        /// 默认配置语言
        /// </summary>
        public static LanguageEntity LanguageConfig { get; set; }
        public static List<SupportedLanguage> SupportedLanguages { get; set; }
        /// <summary>
        /// 当前登录用户
        /// </summary>
        public static SysUser CurrentLoginUser { get; set; }

       /// <summary>
       /// 测试拓展需要用的TokenDef
       /// </summary>
        public static List<TokenDefEntity> TestExpansionTokenDefs { get; set; }

        /// <summary>
        /// 扩展代币配置文件路径
        /// </summary>
        public static string TokenDefExtensionFilePath = System.IO.Path.Combine(System.Environment.CurrentDirectory, "DefaultConfigure", "tokendefExtension.json");

        /// <summary>
        /// 扩展代币定义列表
        /// </summary>
        public static List<TokenDefEntity> ExtensionTokenDefs { get; set; }

        private static Cluster _localWalletNetwork;

        //全局配置，动态更新sdk 中的全局网络
        public static Cluster LocalWalletNetwork {

            get {
                return _localWalletNetwork;
            }
            set {

                _localWalletNetwork = value;
                BasicConfig.LocalNet = _localWalletNetwork;
            }
        }
        /// <summary>
        /// 返回对应的语言的文字
        /// </summary>
        /// <param name="key">文字的Key</param>
        /// <returns></returns>
        public static string ContorlLanguage(string key) {
            try
            {
                // 检查输入参数
                if (string.IsNullOrWhiteSpace(key))
                {
                    return string.Empty;
                }

                // 检查 LanguageConfig 是否已初始化
                if (LanguageConfig == null)
                {
                    return key; // 如果语言配置未初始化，返回原始key
                }

                // 设置默认语言
                if (string.IsNullOrWhiteSpace(LocalLanguage)) {
                    LocalLanguage = LanguageConfig.DefaultLanguage;
                }

                // 检查 SupportedLanguages 是否为空
                if (LanguageConfig.SupportedLanguages == null || !LanguageConfig.SupportedLanguages.Any())
                {
                    return key;
                }

                var lg = LanguageConfig.SupportedLanguages.Where(x => x.Name == LocalLanguage).FirstOrDefault();
                if (lg == null)
                {
                    return key; // 如果找不到匹配的语言，返回原始key
                }

                // 构建文件路径
                DirectoryInfo directoryInfo = new DirectoryInfo(System.Environment.CurrentDirectory);
                string path = directoryInfo.Parent.Parent.Parent.FullName + "\\Assets\\Languages\\" + lg.JsonPath;
                
                // 检查文件是否存在
                bool isExist = File.Exists(path);
                if (!isExist) {
                    return key; // 如果文件不存在，返回原始key
                }

                var languageContent = GetConfigJson(path);
                if (string.IsNullOrWhiteSpace(languageContent))
                {
                    return key; // 如果文件内容为空，返回原始key
                }
                else
                {
                    try
                    {
                        var value = languageContent.ToJObject().Property(key)?.Value;
                        if (value == null)
                        {
                            return key; // 没有对应的翻译就把Key作为默认返回
                        }
                        else
                        {
                            return value.ToString();
                        }
                    }
                    catch (Exception ex)
                    {
                        // 记录异常但不抛出，返回原始key
                        System.Diagnostics.Debug.WriteLine($"ContorlLanguage 解析异常: {ex.Message}");
                        return key;
                    }
                }
            }
            catch (Exception ex)
            {
                // 记录异常但不抛出，返回原始key
                System.Diagnostics.Debug.WriteLine($"ContorlLanguage 方法异常: {ex.Message}");
                return key;
            }
        }

        /// <summary>
        /// 获取指定的json对象
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public static T GetJsonList<T>(string json) where T : class
        {
            try
            {
                if (string.IsNullOrWhiteSpace(json))
                {
                    return null;
                }

                T tempJsonData = json.ToObject<T>();
                return tempJsonData;
            }
            catch (Exception ex)
            {
                // 记录异常但不抛出，返回null
                System.Diagnostics.Debug.WriteLine($"GetJsonList 方法异常: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 获取指定的json字符串
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string GetConfigJson(string path)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
                {
                    return string.Empty;
                }

                string jsonfile = path;

                using (System.IO.StreamReader file = System.IO.File.OpenText(jsonfile))
                {
                    using (JsonTextReader reader = new JsonTextReader(file))
                    {
                        var o = JToken.ReadFrom(reader);
                        return o.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                // 记录异常但不抛出，返回空字符串
                System.Diagnostics.Debug.WriteLine($"GetConfigJson 方法异常: {ex.Message}");
                return string.Empty;
            }
        }

        /// <summary>
        /// 修改json配置
        /// </summary>
        /// <param name="path"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        public static bool SetConfigJson(string path, string content)
        {
            string error = "";
            bool result = FileUtils.CoverWriteFile(path, content, out error);
            return result;
        }

        /// <summary>
        /// 加载扩展代币配置
        /// </summary>
        /// <returns>扩展代币定义列表</returns>
        public static List<TokenDefEntity> LoadExtensionTokenDefs()
        {
            try
            {
                // 检查文件是否存在
                if (!File.Exists(TokenDefExtensionFilePath))
                {
                    return new List<TokenDefEntity>();
                }

                // 读取配置文件
                string jsonContent = GetConfigJson(TokenDefExtensionFilePath);
                if (string.IsNullOrWhiteSpace(jsonContent))
                {
                    return new List<TokenDefEntity>();
                }

                // 解析JSON配置
                var configObject = JObject.Parse(jsonContent);
                var tokenDefsArray = configObject["tokenDefs"] as JArray;
                
                if (tokenDefsArray == null)
                {
                    return new List<TokenDefEntity>();
                }

                // 转换为TokenDefEntity列表
                var extensionTokens = new List<TokenDefEntity>();
                foreach (var tokenItem in tokenDefsArray)
                {
                    try
                    {
                        var tokenDef = tokenItem.ToObject<TokenDefEntity>();
                        if (tokenDef != null && !string.IsNullOrWhiteSpace(tokenDef.Mint))
                        {
                            extensionTokens.Add(tokenDef);
                        }
                    }
                    catch (Exception ex)
                    {
                        // 记录单个代币解析错误，但继续处理其他代币
                        Console.WriteLine($"解析扩展代币配置时出错: {ex.Message}");
                    }
                }

                return extensionTokens;
            }
            catch (Exception ex)
            {
                // 记录错误但不抛出异常，返回空列表
                Console.WriteLine($"加载扩展代币配置文件时出错: {ex.Message}");
                return new List<TokenDefEntity>();
            }
        }
    }
}