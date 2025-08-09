using ApplicationService.IService;
using DryIoc;
using Prism.Ioc;
using YC.WalletApp.ViewModels;
using System;
using System.Windows;
using YC.WalletApp.Domain;
using System.Windows.Controls;
using YC.Model.Entity;
using YC.ApplicationService;
using System.ComponentModel;
using YC.ApplicationService.DefaultConfigure.Model;
using Solnet.Rpc;
using System.Linq;
using YC.WalletApp.Controls;
using MaterialDesignThemes.Wpf;
using YC.WalletApp.Extension;
using static YC.WalletApp.ViewModels.WalletManageViewModel;
using System.Drawing;
using YC.ApplicationService.Utils;
using System.Windows.Threading;
using YC.Common;
using FreeSql.Internal;
using System.Threading.Tasks;

namespace YC.WalletApp.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ISqliteService _sqliteService;
        private MainWindowViewModel _dataContext;
        private IContainerExtension _container;
        private EventSendExtension _eventSendExtension;
        public MainWindow(IContainerExtension container, 
            ISqliteService sqliteService, EventSendExtension eventSendExtension)
        {
           
            InitializeComponent();
            _sqliteService = sqliteService;
            _container = container;
          
            _dataContext = container.Resolve<MainWindowViewModel>();
            _eventSendExtension= eventSendExtension;
            _dataContext.User = DefaultConfig.CurrentLoginUser;
            sbtn_1.Content = DefaultConfig.LocalLanguage;
            //cbb_changeNet.ItemsSource = _dataContext.WalletNetworks;//初始化加载显示网络集合
          
            this.DataContext = _dataContext;
            //var service= container.Resolve<ISqliteService>();
            //this.Tabs.DataContext = dataContext.Tabs;
            var selectNet = _dataContext.WalletNetworks.Where(x => x.Cluster == DefaultConfig.LocalWalletNetwork).FirstOrDefault();//初始化加载显示网络
            cbb_changeNet.SelectedItem= selectNet;
            Loaded += Mainwindow_Loaded;
           

        }

        private void Mainwindow_Loaded(object sender, RoutedEventArgs e) {
           
        }
      
        /// <summary>
        /// 切换网络
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void cbb_changeNetWork_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox combobox = sender as ComboBox;
            if (combobox == null) return;
          
            var walletNetwork = combobox.SelectedItem as WalletNetwork;
            if (walletNetwork != null) {
                DefaultConfig.LocalWalletNetwork = walletNetwork.Cluster;//当前网络进行切换
                _dataContext.CurrentWalletNetWork = walletNetwork.Cluster.ToString();//界面网络内容进行切换
                ///将变更的数据存入数据
                var config = SQLiteUtils._freesql.Select<SysConfigEntity>().First();
                if (config != null)
                {
                    config.LocalWalletNetwork = walletNetwork.Cluster.ToString();
                    var result = SQLiteUtils.Update<SysConfigEntity>(config);
                }
                else
                {
                    config = new SysConfigEntity();
                    config.LocalWalletNetwork = walletNetwork.Cluster.ToString();
                    var result = SQLiteUtils.Insert<SysConfigEntity>(config);
                }
            }
            var vm = this.DataContext as MainWindowViewModel;
            _eventSendExtension.MessageToSend = "changeNet";
            _eventSendExtension.SendMessage();
        }

        /// <summary>
        /// 切换语言
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lb_changeLanguage_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var test = cbb_changeNet.SelectedItem;

            ListBox listBox = sender as ListBox;
            if (listBox == null) return;

            var language = listBox.SelectedItem as SupportedLanguage;
            sbtn_1.Content = language.Name;
            if (language == null) return;
            //DefaultConfig.LocalLanguage = language.Name;
            MainWindowViewModel.OnGenreSelected(language.Name);
            //cbb_changeNet.ItemsSource = _dataContext.WalletNetworks;//初始化加载显示网络集合
            //var selectNet = _dataContext.WalletNetworks.Where(x => x.Cluster == DefaultConfig.LocalWalletNetwork).FirstOrDefault();//初始化加载显示网络
            //cbb_changeNet.SelectedItem = selectNet;
            ////重新刷新数据源，实现界面更新
            _dataContext.Init();
            var selectNet = _dataContext.WalletNetworks.Where(x => x.Cluster == DefaultConfig.LocalWalletNetwork).FirstOrDefault();//初始化加载显示网络
            cbb_changeNet.SelectedItem = selectNet;//多语言切换要重新绑定，不然前端控件数据显示不会更新

            for (int i = 0; i < _dataContext.Tabs.CustomTabs.Count; i++) {
                //更新数据让菜单等变化
              _dataContext.Tabs.CustomTabs[i].CustomHeader =DefaultConfig.ContorlLanguage(_dataContext.Tabs.CustomTabs[i].CustomTabId);
                
            }

            _eventSendExtension.MessageToSend = "changeLanguage";
            _eventSendExtension.SendMessage();
            //this.DataContext = _dataContext;
            LoginViewModel.OnGenreSelected(language.Name);
        }
        private void ShowCustomMessageBoxButton_Click(object sender, RoutedEventArgs e)
        {
            var customMessageBox = new CustomMessageBox("提示", "这是一条自定义消息！");
            customMessageBox.Owner = this; // 设置所有者窗口
            var result = customMessageBox.ShowDialog();
            if (result == true)
            {
                MessageBox.Show("你点击了确定");
            }
            else
            {
                MessageBox.Show("你点击了取消");
            }
        }

       
    }
}
