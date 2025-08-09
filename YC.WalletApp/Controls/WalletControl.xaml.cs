using Mapster;
using MaterialDesignThemes.Wpf;
using Newtonsoft.Json;
using Prism.Ioc;
using Solnet.Wallet;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Media;
using YC.ApplicationService;
using YC.ApplicationService.IService;
using YC.Common.ShareUtils;
using YC.Model.Entity;
using YC.SolanaSdkService;
using YC.WalletApp.Extension;
using YC.WalletApp.ViewModels;
using YC.WalletApp.Views;
using YC.WalletApp.Domain.PartViewControl;
using System.Windows.Input;
using Solnet.Extensions.Models;
using YC.Model;
using System.Collections.ObjectModel;
using YC.SolanaSdkService.DTO;
using Prism.Events;
using ImTools;

namespace YC.WalletApp.Controls
{
    /// <summary>
    /// WalletControl.xaml 的交互逻辑
    /// </summary>
    public partial class WalletControl : UserControl
    {
       
        private IContainerExtension _container;
        private IWalletService _WalletDtoService;
        private EventSendExtension _eventSendExtension;
        private WalletManageViewModel _vm;
        private IEventAggregator _eventAggregator;
        public WalletControl(IContainerExtension container,
            IWalletService WalletDtoService, EventSendExtension eventSendExtension,
            IEventAggregator eventAggregator)
        {
            //DataContext = new ListsAndGridsViewModel();
            _container = container;
            Loaded += WalletAccountControl_Loaded;
            InitializeComponent();
            _vm = _container.Resolve<WalletManageViewModel>();
            _WalletDtoService = WalletDtoService;
            _eventSendExtension=eventSendExtension;
            this.DataContext = _vm;
             // 初始化钱包列表
            //WalletListBox.ItemsSource = vm.WalletDtos;
            ChangeDataBinding();
            _eventAggregator = eventAggregator;
            _eventAggregator.GetEvent<PrismMessageEvent>().Subscribe(HandleReceivedMessage);
            Init();
        }

        /// <summary>
        /// 接受信息，一旦网络变化，重新处理数据
        /// </summary>
        /// <param name="message"></param>
        private void HandleReceivedMessage(string message)
        {
            Init();
        }

        private void Init() {
            MaskLayer.Visibility = Visibility.Hidden;
            AccountListBox.Visibility = Visibility.Hidden;
            AccountDetailsPanel.Visibility = Visibility.Hidden;
        }
        /// <summary>
        /// 数据有变更，就要重新处理
        /// </summary>
        public void ChangeDataBinding() {
            _eventSendExtension.MessageToSend=DefaultConfig.LocalWalletNetwork.ToString();
            _eventSendExtension.SendMessage();
            var vm =this.DataContext as WalletManageViewModel;
            // 确保设计时可见性（可选）
            //if (DesignerProperties.GetIsInDesignMode(this))
                MaskLayer.Visibility = Visibility.Hidden;
            //AccountListBox.Visibility = Visibility.Hidden;
            //AccountDetailsPanel.Visibility = Visibility.Hidden;
        }

        private void WalletAccountControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.UpdateLayout();
            // CommonExtension.ShowDialog("我打开了");
        }

        /// <summary>
        /// 创建钱包
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void CreateWallet_Click(object sender, RoutedEventArgs e)
        {
           var localNetwork= DefaultConfig.LocalWalletNetwork;
            var res= await  _WalletDtoService.CreateNewWalletAsync();
            var vm = this.DataContext as WalletManageViewModel;
            vm.InitWalletData();
            ChangeDataBinding();
            // 实现创建钱包的逻辑
             CommonExtension.ShowDialog(res.Message);
        }

        /// <summary>
        /// 导入钱包
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ImportWallet_Click(object sender, RoutedEventArgs e)
        {
           var filePath= CommonExtension.ImportFile();
            if (FileUtils.IsExistFile(filePath))
            {
               var res=await _WalletDtoService.ImportBatchWalletAsync(filePath);
                if (res.State)
                {
                    ChangeDataBinding();
                   
                     CommonExtension.ShowDialog("导入钱包成功！");
                }
                else {
                     CommonExtension.ShowDialog(res.Message);
                }
            }
            else {
                 CommonExtension.ShowDialog("文件不存在！");
            }
           
        }

        /// <summary>
        /// 导出钱包
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ExportWallet_Click(object sender, RoutedEventArgs e)
        {
            ShowMaskLayer(() => {
                #region 处理数据
                //for (var i = 0; i < _vm.WalletDtos.Count; i++) {
                //   var res = Task.Run(async () => {
                //        var result=await _WalletDtoService.GetTokenAccountInfoAsync(_vm.WalletDtos[i].Id);
                //       return result; 
                //   }).GetAwaiter();

                //    var dataList = res.GetResult().Data.Adapt<List<WalletAccountDto>>();
                //    _vm.WalletDtos[i].Accounts = dataList;
                //} 
                #endregion
                // 检查命令是否可以执行
                if (_vm.RunDialogCommand.CanExecute(null))
                {
                    // 执行命令
                    _vm.RunDialogCommand.Execute(null);
                }
            });
            
            //CommonExtension.ShowDialog("你现在打开了导出钱包功能");
          
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="action"></param>
        public void ShowMaskLayer(Action action) {
            // 显示加载遮罩层
            MaskLayer.Visibility = Visibility.Visible;
            action();
            // 无论数据请求成功还是失败，都隐藏加载遮罩层
            MaskLayer.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// 钱包选中  (触发遮罩)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void WalletListBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            // 显示加载遮罩层
            MaskLayer.Visibility = Visibility.Visible;
            await Task.Delay(500); // 给遮罩弹出时间
            try
            {
 
                if (WalletListBox.SelectedItem is WalletDto selectedWalletDto)
                {
                    AccountDetailsPanel.Visibility = Visibility.Hidden;
                    var res = await _WalletDtoService.GetTokenAccountInfoAsync(selectedWalletDto.Id);
                    var tempData = res.Data.Adapt<List<WalletAccountDto>>();
                    tempData.ForEach(x => {
                        x.DoubleTypeLamports = (double)x.Lamports / 1000000000;
                    
                    });
                    _vm.SelectWalletAccountDtos = tempData;
                    var objList = _vm.SelectWalletAccountDtos.Adapt<List<WalletAccountEntity>>();
                                         
                    objList.ForEach(x => {
                        x.CreationTime = DateTime.Now;
                        x.CreatorUserId=DefaultConfig.CurrentLoginUser.Id;
                        x.IsActive= true;
                        x.BelongWalletId = selectedWalletDto.Id;
                    });
                    SQLiteUtils.ExecuteTransaction(() => {
                       var deleteCount= SQLiteUtils._freesql.Delete<WalletAccountEntity>().Where(x => x.BelongWalletId == selectedWalletDto.Id).ExecuteAffrows();
                        var count = SQLiteUtils._freesql.InsertOrUpdate<WalletAccountEntity>()
                           .SetSource(objList)
                           .IfExistsDoNothing().ExecuteAffrows();
                    });
                    
                    
                    // 显示钱包账户列表
                    AccountListBox.Visibility = Visibility.Visible;
                }
            }
            catch (Exception ex)
            {

                throw;
            }
            finally
            {
                // 无论数据请求成功还是失败，都隐藏加载遮罩层
                MaskLayer.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// 钱包账户选中触发
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AccountListBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (AccountListBox.SelectedItem is WalletAccountDto selectedWalletAccountDto)
            {
                AccountDetailsPanel.Visibility = Visibility.Visible;
                _vm.AccountDetail = selectedWalletAccountDto;
                if (!_vm.AccountDetail.IsAssociatedTokenAccount)
                {
                    _vm.AccountDetail.AccountType = DefaultConfig.ContorlLanguage("WalletAccount");
                }
                else {
                    _vm.AccountDetail.AccountType = DefaultConfig.ContorlLanguage("TokenAssociatedAccount");
                }
                _vm.AccountDetail.DoubleTypeLamports = (double)_vm.AccountDetail.Lamports / 1000000000;
            }
        }

        private void ShowCustomMessageBoxButton_Click(object sender, RoutedEventArgs e)
        {
            var customMessageBox = new CustomMessageBox("提示", "这是一条自定义消息！");
            customMessageBox.Owner = Window.GetWindow(this) as MainWindow; // 设置所有者窗口
            var result = customMessageBox.ShowDialog();
            if (result == true)
            {
                 CommonExtension.ShowDialog("你点击了确定");
            }
            else
            {
                 CommonExtension.ShowDialog("你点击了取消");
            }
        }

        /// <summary>
        /// 钱包复制
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void CopyButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.DataContext is not WalletDto dataItem) return;

            try
            {
                // 创建新画刷进行动画
                var newBrush = new SolidColorBrush(Colors.Gray);
                button.Foreground = newBrush;
                var icon = button.Content as PackIcon;

                // 正确执行颜色动画
                var animation = new ColorAnimation(Colors.Green, TimeSpan.FromSeconds(0.3));
                newBrush.BeginAnimation(SolidColorBrush.ColorProperty, animation);

                // 复制操作
                var json = JsonConvert.SerializeObject(dataItem);
                Clipboard.SetText(json);

                // 恢复颜色
                await Task.Delay(1000);
                animation.To = Colors.Gray;
                newBrush.BeginAnimation(SolidColorBrush.ColorProperty, animation);
            }
            catch (Exception ex)
            {
                 CommonExtension.ShowDialog($"复制失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 账户信息列表数据复制
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CopyAccountDetail_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.CommandParameter != null)
            {
                try
                {
                    Clipboard.SetText(button.CommandParameter.ToString());
                    // 可选：显示复制成功提示
                     CommonExtension.ShowDialog("已复制到剪贴板");
                }
                catch (Exception ex)
                {
                     CommonExtension.ShowDialog($"复制失败: {ex.Message}");
                }
            }
        }

        private async void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.DataContext is not WalletDto dataItem) return;
            var walletBalance=Task.Run(async() => {
                return await _container.Resolve<IWalletService>().GetWalletLamportsBalanceAsync(dataItem.Id);
            }).GetAwaiter().GetResult();
           
            if (walletBalance == null) return;
           var obj= SQLiteUtils._freesql.Select<WalletEntity>().Where(x => x.Id == dataItem.Id).First();
            if (obj.LamportsBalance != walletBalance.Data)
            {//如果不一致，才更新
                obj.LamportsBalance = walletBalance.Data;//
                obj.LastModificationTime = DateTime.Now;
                var updateCount = await SQLiteUtils._freesql.Update<WalletEntity>()
                    .SetSource(obj).ExecuteAffrowsAsync();
                var vm = this.DataContext as WalletManageViewModel;
                vm.InitWalletData();
            }
        }
    }

   

   

}
