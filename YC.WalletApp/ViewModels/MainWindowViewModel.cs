
using ApplicationService.IService;
using Prism.Mvvm;
using YC.WalletApp.Controls;
using YC.WalletApp.ViewModels;
using YC.WalletApp.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YC.WalletApp.Domain.Entity;
using YC.Model;
using Prism.Ioc;
using YC.Model.Entity;
using YC.ApplicationService;
using YC.WalletApp.Extension;
using System.Runtime.CompilerServices;
using System.ComponentModel;
using Solnet.Rpc;
using Prism.Interactivity;
using YC.ApplicationService.DefaultConfigure.Model;
using Prism.Commands;
using Prism.Events;


namespace YC.WalletApp.ViewModels
{
    public class MainWindowViewModel: ViewModelBase
    {

        #region 消息传递机制
        private readonly IEventAggregator _eventAggregator;
        private string _messageToSend;

        public string MessageToSend
        {
            get { return _messageToSend; }
            set { SetProperty(ref _messageToSend, value); }
        }

        public DelegateCommand SendMessageCommand { get; }

        public void SendMessage()
        {
            if (!string.IsNullOrEmpty(MessageToSend))
            {
                _eventAggregator.GetEvent<PrismMessageEvent>().Publish(MessageToSend);
                MessageToSend = string.Empty;
            }
        } 
        #endregion

        public readonly ISqliteService _sqliteService;
       public IContainerExtension _container;
        public MainWindowViewModel(ISqliteService sqliteService,
            IContainerExtension container)
        {
            _sqliteService= sqliteService;
            _sqliteService.QueryAll();
            _container = container;
            this.Tabs = new TabsViewModel();
            //_eventAggregator = eventAggregator;//消息传递
            SendMessageCommand = new DelegateCommand(SendMessage);

            Init();//这个在多语言切换时候，要重新调用，如果把Tabs 放到里面，会清空，界面的Tabs 会被清空
        }

        private SupportedLanguage _currentLanguage;
        public SupportedLanguage CurrentLanguage
        {
            get { return _currentLanguage; }
            set { SetProperty(ref _currentLanguage, value); }
        }

        private string _currentWalletNetWork;
        public string  CurrentWalletNetWork
        {
            get { return _currentWalletNetWork; }
            set { SetProperty(ref _currentWalletNetWork, value); }
        }

        private WalletNetwork _walletNetWork;
        public WalletNetwork WalletNetWork
        {
            get { return _walletNetWork; }
            set { SetProperty(ref _walletNetWork, value); }
        }

        /// <summary>
        /// 网络配置
        /// </summary>
        private List<WalletNetwork> _walletNetworks;
        public List<WalletNetwork> WalletNetworks
        {
            get
            {
                return _walletNetworks;
            }
            set
            {
                SetProperty(ref _walletNetworks, value);
            }
        }

        private SysUser _user;
        public SysUser User
        {
            get { return _user; }
            set { SetProperty(ref _user, value); }
        }

        private TabsViewModel _tabs;
        public TabsViewModel Tabs {
            get { return _tabs; }
            set { SetProperty(ref _tabs, value); }
        }
        private ObservableCollection<ModuleInfo> _modules;
        public ObservableCollection<ModuleInfo> Modules
        {
            get { return _modules; }
            set { SetProperty(ref _modules, value); }
        }

        public static void OnGenreSelected(string selectedLanguage)
        {
            LanguageService.SetLanguage(selectedLanguage);
        }

        public  void Init()
        {
            //Tabs= new TabsViewModel();
            Modules = new ObservableCollection<ModuleInfo>();
            Modules.Add(new ModuleInfo() { ModuleId = "WalletManage", IconFont = "AccountBoxMultipleOutline", Title = DefaultConfig.ContorlLanguage("WalletManage"), UserControl = _container.Resolve<WalletControl>() }); 
            Modules.Add(new ModuleInfo() { ModuleId = "TransferManage", IconFont = "BankTransfer", Title = DefaultConfig.ContorlLanguage("TransferManage"), UserControl = _container.Resolve<TransactionControl>() }); 
            //Modules.Add(new ModuleInfo() { ModuleId = "TransferRecord", IconFont = "ClipboardTextOutline", Title = DefaultConfig.ContorlLanguage("TransferRecord"), UserControl = _container.Resolve<TransactionControl>() }); 
            Modules.Add(new ModuleInfo()
            {
                ModuleId = "DevManage",
                IconFont = "DeveloperBoard",
                Title = DefaultConfig.ContorlLanguage("DevManage"),
                Items = new List<ModuleInfo>() {
            new ModuleInfo() {ModuleId="TokenDefManage", IconFont = "PlusCircleMultiple",
                Title = DefaultConfig.ContorlLanguage("TokenDefManage"),
                UserControl = new TokenDefControl() },
            //new ModuleInfo()
            //{
            //    ModuleId = "LogManage",
            //    IconFont = "AccountBoxMultipleOutline",
            //    Title = DefaultConfig.ContorlLanguage("LogManage"),
            //    UserControl = _container.Resolve<TestUserControl>()
            //},
            new ModuleInfo()
            {
                ModuleId = "MintTokenManage",
                IconFont = "AccountBoxMultipleOutline",
                Title = DefaultConfig.ContorlLanguage("MintTokenManage"),
                UserControl = _container.Resolve<MintTokenControl>()
            },


                }
            });
            
            
            
            this.WalletNetworks = new List<WalletNetwork>() {
        new WalletNetwork(){Name=DefaultConfig.ContorlLanguage("MainNet"), Cluster=Cluster.MainNet },
        new WalletNetwork(){Name=DefaultConfig.ContorlLanguage("DevNet"), Cluster=Cluster.DevNet },
        new WalletNetwork(){Name=DefaultConfig.ContorlLanguage("TestNet"), Cluster=Cluster.TestNet },
        };
            
            this.CurrentWalletNetWork = DefaultConfig.LocalWalletNetwork.ToString();//初始化加载默认网络
            #region 测试代码
            //Modules.Add(new ModuleInfo() { ModuleId = "AccountManage", IconFont = "AccountBoxMultipleOutline", Title = DefaultConfig.ContorlLanguage("AccountManage"), UserControl = _container.Resolve<AccountControl>() }); ;
            //Modules.Add(new ModuleInfo() { ModuleId = "WebBrowser", IconFont = "\xe77a", Title = DefaultConfig.ContorlLanguage("Brower"), UserControl = new UserControl1() });
            //Modules.Add(new ModuleInfo() { ModuleId = "Automation", IconFont = "\xe50a", Title = DefaultConfig.ContorlLanguage("Automation"), UserControl = new UIAutomationControl() });
            //Modules.Add(new ModuleInfo() { ModuleId = "表单", IconFont = "\xe669", Title = "表单", UserControl = new UserControl1() });
            //Modules.Add(new ModuleInfo() { ModuleId = "cefSharp", IconFont = "\xe502", Title = "cefSharp", UserControl = new UserControl1() });
            //Modules.Add(new ModuleInfo()
            //{
            //    ModuleId = "TasksMenu",
            //    IconFont = "\xe77a",
            //    Title = "算法",
            //    Items = new List<ModuleInfo>() {
            //new ModuleInfo() {ModuleId="Tasks", IconFont = "\xe50a", Title = DefaultConfig.ContorlLanguage("Brower"),UserControl = new UserControl1() },
            //new ModuleInfo() {ModuleId="Program", IconFont = "\xe669", Title = DefaultConfig.ContorlLanguage("Brower"),UserControl = new UserControl1() }
            //}
            //});
            //Modules.Add(new ModuleInfo()
            //{
            //    ModuleId = "test",
            //    IconFont = "\xe77a",
            //    Title = "测试",
            //    Items = new List<ModuleInfo>() {
            //new ModuleInfo() {ModuleId="Tasks", IconFont = "\xe50a", Title = "Tasks",UserControl = new UserControl1() },
            //new ModuleInfo() {ModuleId="Program", IconFont = "\xe669", Title = "Program",UserControl = new UserControl1() }
            //}
            //}); 
            #endregion

        }

    }

   
}
