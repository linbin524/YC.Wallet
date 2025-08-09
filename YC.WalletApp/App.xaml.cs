using ApplicationService.IService;
using DryIoc;
using FreeSql;
using Prism.DryIoc;
using Prism.Ioc;
using YC.WalletApp.Views;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Security.AccessControl;
using System.Windows;
using YC.Model.Entity;
using YC.ApplicationService;
using System.IO;
using Solnet.Rpc;
using YC.ApplicationService.Utils;
using Mapster;
using Solnet.Extensions.TokenMint;
using YC.ApplicationService.DefaultConfigure;
using YC.Common.ShareUtils;
using YC.ApplicationService.DefaultConfigure.Model;
using Example;
using YC.WalletApp.Domain.Utils;
using System.Threading.Tasks;
using YC.Common;
using System.Windows.Media;
using YC.ApplicationService.IService;
using ImTools;
using YC.WalletApp.Domain.Service;
using System.Runtime.ConstrainedExecution;



namespace YC.WalletApp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : PrismApplication
    {
        protected override Window CreateShell()
        {
            string connectionString = ConfigurationManager.ConnectionStrings["SQLiteConnection"].ConnectionString;
            SQLiteUtils._freesql = new FreeSqlBuilder()
                       .UseConnectionString(DataType.Sqlite, connectionString)
                       .UseAutoSyncStructure(true) // 自动同步实体结构
                       .Build();
            SQLiteUtils.SqliteInit();
            ConfigInit();
            LanguageInit();
            MappingInit();
            if (DefaultConfig.AppConfig.ScheduleServiceIsEnabeld) {
                Container.Resolve<UpdateWalletService>().TimeWorkInit();///定时服务启动
            }
          
            return Container.Resolve<Login>();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            // 假设你有一个包含所有服务类型的列表或程序集
            var typesToRegister = GetTypesToRegister().Where(t => t.Name.EndsWith("Service")); // 获取需要注册的类型的列表
            //containerRegistry.RegisterSingleton(typeof(IDependencyInjectionSupport));
            var types = AppDomain.CurrentDomain.GetAssemblies().SelectMany(s => s.GetTypes())//找到service的类型
                      .Where(x => x.Name.EndsWith("ViewModel"));
            // 对于非接口类型，你可以直接注册它们（如果有需要）
            // 例如，如果你有一个无接口的简单服务类
            foreach (var t in types)
            {
                containerRegistry.Register(t);
            }
            foreach (var type in typesToRegister)
            {
                // 你可以添加额外的逻辑来确定哪些类型应该被注册为接口
                // 例如，检查类型是否实现了某个特定的接口
                var baseType = typeof(IDependencyInjectionSupport);//使用指定标记的接口，标记为需要注入
                if (type.GetInterfaces().Length > 0)
                {

                    //bool isAssignable1 = animalType.IsAssignableFrom(dogType); // true，因为 Dog 是 Animal 的子类
                    // 查找并注册所有实现了该类关联的接口类型
                    //var implementations= type.GetInterfaces();
                    var implementations = AppDomain.CurrentDomain.GetAssemblies().SelectMany(s => s.GetTypes())//找到service的类型
                        .Where(t => t.IsInterface && baseType.IsAssignableFrom(t) && baseType != t && t.IsAssignableFrom(type));
                    var implementations2 = AppDomain.CurrentDomain.GetAssemblies().SelectMany(s => s.GetTypes())//找到service的类型
                       .Where(t => t.IsInterface && baseType.IsAssignableFrom(t));//得到IDependencyInjectionSupport和ISqliteService
                    var implementations3 = AppDomain.CurrentDomain.GetAssemblies().SelectMany(s => s.GetTypes())//找到service的类型
                       .Where(t => t.IsInterface && t.IsAssignableFrom(baseType));//得到IDependencyInjectionSupport
                    var implementations4 = AppDomain.CurrentDomain.GetAssemblies().SelectMany(s => s.GetTypes())//找到service的类型
                       .Where(t => t.IsInterface && baseType.IsAssignableTo(t));//得到IDependencyInjectionSupport
                    var implementations5 = AppDomain.CurrentDomain.GetAssemblies().SelectMany(s => s.GetTypes())//找到service的类型
                       .Where(t => t.IsInterface && t.IsAssignableTo(baseType));//得到IDependencyInjectionSupport和ISqliteService
                    foreach (var implementation in implementations)//注册继承IDependencyInjectionSupport，且Service结尾
                    {
                        // 注册服务，这里假设接口和实现是一对一的关系
                        containerRegistry.Register(implementation, type);
                        // Directly register with DryIoc

                    }
                }

            }
        }
        private IEnumerable<Type> GetTypesToRegister()
        {
            // 这里你可以通过不同的方式获取需要注册的类型列表
            // 例如，从某个特定的程序集中获取所有类型
            // 或者从一个配置文件中读取类型名称列表，并使用Type.GetType来加载它们

            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            IEnumerable<Type> registerTypeList = new HashSet<Type>();

            foreach (var assembly in assemblies)
            {
                registerTypeList = assembly.GetTypes().Where(t => !t.IsAbstract && !t.IsInterface && t.IsPublic && t.IsClass);
            }

            return registerTypeList;
            // 示例：从当前程序集中获取所有以"Service"结尾的类型
            //return Assembly.GetExecutingAssembly().GetTypes()
            //    .Where(t => t.Name.EndsWith("Service") && !t.IsAbstract && !t.IsInterface&&t.IsClass);
            //Assembly.GetExecutingAssembly().GetTypes();
            //// 创建包含构造函数参数的数组
            //object[] constructorParams = new object[] { constructorString, constructorList };

            //// 使用Activator.CreateInstance和构造函数参数来创建实例
            //object instance = Activator.CreateInstance(type, constructorParams);

            //// 如果需要将实例转换为原始类型，可以使用as或直接转换
            //MyClass myClassInstance = instance as MyClass;
        }

        /// <summary>
        /// 配置初始化
        /// </summary>
        public void ConfigInit()
        {

            DirectoryInfo directoryInfo = new DirectoryInfo(System.Environment.CurrentDirectory);
            string defaultConfigPath = System.Environment.CurrentDirectory + "//Assets//Config//AppConfig.json";
            var config = GetEntity<DefaultAppConfig>(defaultConfigPath);
            DefaultConfig.AppConfig = config;
            if (DefaultConfig.AppConfig.IsDebug)
            {///开发模式状态下，加载测试TokenDef
                string tokenDefPath = System.Environment.CurrentDirectory + "//Assets//Config//TokenDef.json";
                var list = GetEntity<List<TokenDefEntity>>(tokenDefPath, "TokenDefs");
                DefaultConfig.TestExpansionTokenDefs = list;
            }

        }

        public T GetEntity<T>(string filePath, string jsonKey = "") where T : class, new()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(filePath))
                {
                    System.Diagnostics.Debug.WriteLine("文件路径为空");
                    return new T();
                }

                bool isExist = File.Exists(filePath);
                if (!isExist)
                {
                    System.Diagnostics.Debug.WriteLine($"文件不存在: {filePath}");
                    return new T();
                }

                var configJsonStr = DefaultConfig.GetConfigJson(filePath);
                if (string.IsNullOrWhiteSpace(configJsonStr))
                {
                    System.Diagnostics.Debug.WriteLine("配置文件内容为空");
                    return new T();
                }

                if (string.IsNullOrWhiteSpace(jsonKey))
                {
                    return DefaultConfig.GetJsonList<T>(configJsonStr);
                }
                else
                {
                    return configJsonStr.GetObjectByJsonKey<T>(jsonKey);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetEntity 方法异常: {ex.Message}");
                return new T();
            }
        }

        /// <summary>
        /// 多语言初始化
        /// </summary>
        public void LanguageInit()
        {
            try
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(System.Environment.CurrentDirectory);
                string defaultLanguagePath = System.Environment.CurrentDirectory + "//Assets//Languages//LanguageConfig.json";
                
                // 检查语言配置文件是否存在
                if (!File.Exists(defaultLanguagePath))
                {
                    System.Diagnostics.Debug.WriteLine("语言配置文件不存在: " + defaultLanguagePath);
                    return;
                }

                var languageEntity = GetEntity<LanguageEntity>(defaultLanguagePath);
                
                // 检查语言实体是否有效
                if (languageEntity == null)
                {
                    System.Diagnostics.Debug.WriteLine("无法加载语言配置实体");
                    return;
                }

                DefaultConfig.LanguageConfig = languageEntity;
                DefaultConfig.SupportedLanguages = languageEntity.SupportedLanguages;
                
                ///加载默认配置
                if (string.IsNullOrWhiteSpace(DefaultConfig.LocalLanguage))
                {
                    try
                    {
                        var config = SQLiteUtils._freesql.Select<SysConfigEntity>().First();
                        if (config == null)
                        {
                            DefaultConfig.LocalLanguage = languageEntity.DefaultLanguage;
                            DefaultConfig.LocalWalletNetwork = Cluster.DevNet;//默认提供开发网络
                        }
                        else
                        {
                            if (string.IsNullOrWhiteSpace(config.LocalLanguage))
                            {
                                DefaultConfig.LocalLanguage = languageEntity.DefaultLanguage;
                            }
                            else
                            {
                                DefaultConfig.LocalLanguage = config.LocalLanguage;//从数据读取最近一次的语言选择
                            }
                            //处理默认网络
                            if (string.IsNullOrWhiteSpace(config.LocalWalletNetwork))
                            {
                                DefaultConfig.LocalWalletNetwork = Cluster.DevNet;//默认提供开发网络
                            }
                            else
                            {
                                Enum.TryParse(config.LocalWalletNetwork, out Cluster localWalletNetwork);
                                DefaultConfig.LocalWalletNetwork = localWalletNetwork;//从数据读取最近一次的网络选择
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        // 如果数据库操作失败，使用默认配置
                        System.Diagnostics.Debug.WriteLine($"数据库配置读取失败: {ex.Message}");
                        DefaultConfig.LocalLanguage = languageEntity.DefaultLanguage;
                        DefaultConfig.LocalWalletNetwork = Cluster.DevNet;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"语言初始化失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 实体映射
        /// </summary>
        public void MappingInit()
        {

            MapsterUtils.InitConfig(new Action(() =>
            {
                // 全局配置
                TypeAdapterConfig<TokenDefEntity, YC.WalletApp.ViewModels.TokenDef>.NewConfig()
                    //.Map(dest => dest.Mint, src => src.Mint)  // 自定义映射 A→B
                    .IgnoreNullValues(true);            // 可选：忽略空值
                TypeAdapterConfig<TokenDef, TokenDefEntity>.NewConfig()
                    .Map(dest => dest.Mint, src => src.TokenMint)  // 自定义映射 A→B
                    .Map(dest => dest.Name, src => src.TokenName)  // 自定义映射 A→B
                    .IgnoreNullValues(true);            // 可选：忽略空值

                TypeAdapterConfig<WalletEntity, WalletDto>.NewConfig()
                    .Map(dest => dest.PublicKey, src => src.MasterAccountPublicKey)  // 自定义映射 A→B
                    .Map(dest => dest.WalletName, src => "Wallet" + src.Id)  // 自定义映射 A→B
                    .IgnoreNullValues(true);
                TypeAdapterConfig<WalletAccountEntity, WalletAccountDto>.NewConfig()
                    .Map(dest => dest.TokenName, src => src.AccountName)  // 自定义映射 A→B
                    .IgnoreNullValues(true);

                // 添加TokenWalletAccount到WalletAccountDto的映射配置
                TypeAdapterConfig<Solnet.Extensions.TokenWalletAccount, WalletAccountDto>.NewConfig()
                    .Map(dest => dest.PublicKey, src => src.PublicKey)
                    .Map(dest => dest.TokenMint, src => src.TokenMint)
                    .Map(dest => dest.Symbol, src => src.Symbol)
                    .Map(dest => dest.TokenName, src => src.TokenName)
                    .Map(dest => dest.DecimalPlaces, src => src.DecimalPlaces)
                    .Map(dest => dest.QuantityDecimal, src => (double)src.QuantityDecimal)  // 确保使用QuantityDecimal
                    .Map(dest => dest.QuantityRaw, src => src.QuantityRaw)
                    .Map(dest => dest.Lamports, src => src.Lamports)
                    .Map(dest => dest.Owner, src => src.Owner)
                    .Map(dest => dest.AccountType, src => src.IsAssociatedTokenAccount ? "TokenAssociatedAccount" : "WalletAccount")
                    .IgnoreNullValues(true);

                //无法自动映射，只能手动处理，Solnet.Extensions.TokenMint.TokenDef 程序有带参数构造函数
                //TypeAdapterConfig<TokenDefEntity, Solnet.Extensions.TokenMint.TokenDef>.NewConfig()
                //    .Map(dest => dest.TokenMint, src => src.Mint)  // 自定义映射 A→B
                //    .Map(dest => dest.TokenName, src => src.Name)  // 自定义映射 A→B
                //    .IgnoreNullValues(true);

            }));
        }

       
    }
}
