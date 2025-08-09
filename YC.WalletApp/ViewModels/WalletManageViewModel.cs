using HandyControl.Tools.Extension;
using Mapster;
using MaterialDesign3Demo.Domain;
using MaterialDesignThemes.Wpf;
using Prism.Events;
using Prism.Ioc;
using Prism.Mvvm;
using Solnet.Extensions.TokenMint;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Threading;
using YC.ApplicationService;
using YC.ApplicationService.IService;
using YC.ApplicationService.Service;
using YC.Common.ShareUtils;
using YC.Model;
using YC.Model.Entity;
using YC.WalletApp.Domain;
using YC.WalletApp.Domain.PartViewControl;
using YC.WalletApp.Extension;
using YC.ApplicationService;


namespace YC.WalletApp.ViewModels
{
    public class WalletManageViewModel : BindableBase
    {

        #region 消息接收机制
        private readonly IEventAggregator _eventAggregator;
        private string _receivedMessage;

        public string ReceivedMessage
        {
            get { return _receivedMessage; }
            set { SetProperty(ref _receivedMessage, value); }
        }

        /// <summary>
        /// 接受信息，一旦网络变化，重新处理数据
        /// </summary>
        /// <param name="message"></param>
        private void HandleReceivedMessage(string message)
        {
            ReceivedMessage = message;
            switch (message) {
                case "changeNet": InitWalletData(); break; //从新更新钱包数据
                case "updateWalletBalance": InitWalletData(); break; //从新更新钱包数据
            }

          


        } 
        #endregion

        private ObservableCollection<WalletDto> _walletDtos;
        public ObservableCollection<WalletDto> WalletDtos {
            get {
                //string network = DefaultConfig.LocalWalletNetwork.ToString();
                //var res =  SQLiteUtils._freesql.Select<WalletEntity>().Where(x => x.NetWorkType == network).ToList();
                //walletDtos = res.Adapt<List<WalletDto>>();
                return _walletDtos;
            } set {
                SetProperty(ref _walletDtos, value);
                //SelectWalletAccountDtos = null;
                //AccountDetail = null;
            }
        }
       

        private List<WalletAccountDto> _selectWalletAccountDtos;
        public List<WalletAccountDto> SelectWalletAccountDtos { get {
                _selectWalletAccountDtos?.ForEach(x => {
                    if (!x.IsAssociatedTokenAccount)
                    {
                        x.AccountType = DefaultConfig.ContorlLanguage("WalletAccount");
                    }
                    else
                    {
                        x.AccountType = DefaultConfig.ContorlLanguage("TokenAssociatedAccount");
                    }
                });

                return _selectWalletAccountDtos;
            } set => SetProperty(ref _selectWalletAccountDtos, value); }

        private WalletAccountDto _accountDetail;
        /// <summary>
        /// 选中账户信息处理
        /// </summary>
        public WalletAccountDto AccountDetail { get{

                if (_accountDetail != null) {
                    if (!_accountDetail.IsAssociatedTokenAccount)
                    {
                        _accountDetail.AccountType = DefaultConfig.ContorlLanguage("WalletAccount");
                    }
                    else
                    {
                        _accountDetail.AccountType = DefaultConfig.ContorlLanguage("TokenAssociatedAccount");
                    }
                }

                return _accountDetail;
            } set => SetProperty(ref _accountDetail, value); }
        private readonly SnackbarMessageQueue _messageQueue;
        IContainerExtension _container;
        public WalletManageViewModel(IEventAggregator eventAggregator, IContainerExtension container) {
            _eventAggregator = eventAggregator;
            _eventAggregator.GetEvent<PrismMessageEvent>().Subscribe(HandleReceivedMessage);
            _container = container;
            InitWalletData();//初始化查询获取钱包数据
            ///定时服务更新钱包数据
            new DispatcherTimer(TimeSpan.FromSeconds(20), DispatcherPriority.Normal, (s, e) =>
            {
                InitWalletData();//初始化查询获取钱包数据
                // 每5秒执行的代码
                //System.Diagnostics.Debug.WriteLine(DateTime.Now.ToString("HH:mm:ss"));
            }, Dispatcher.CurrentDispatcher).Start();
        }

        #region 钱包导出配套
        public ICommand RunDialogCommand => new AnotherCommandImplementation(ExecuteRunDialog);
        
        private async void ExecuteRunDialog(object? _)
        {
            var vm =_container.Resolve<WalletExportDialogViewModel>();
            var tempList=WalletDtos.ToList();
            ///1.找出钱包所属账户
           var walletAccountList=SQLiteUtils._freesql.Select<WalletAccountEntity>().Where(x => WalletDtos.Any(y => y.Id == x.BelongWalletId)).ToList();
            ///2. 将钱包和对应账户组合
            tempList.ForEach(x => { 
                var tempAccountList= walletAccountList.Where(y=>y.BelongWalletId==x.Id).ToList();
                x.Accounts = tempAccountList.Adapt<List<WalletAccountDto>>();
            });
            ///3. 将完整钱包数据发送给导出列表
            vm.Wallets = tempList.Adapt<ObservableCollection<WalletDto>>(); 
            //let's set up a little MVVM, cos that's what the cool kids are doing:
            var view = new WalletExportDialog
            {
                DataContext = vm,
            };

            //show the dialog
            var result = await DialogHost.Show(view, "RootDialog", ExtendedOpenedEventHandler, ExtendedClosingEventHandler);
        }
        private void ExtendedOpenedEventHandler(object sender, DialogOpenedEventArgs eventArgs)
       => Debug.WriteLine("You could intercept the open and affect the dialog using eventArgs.Session.");

        private void ExtendedClosingEventHandler(object sender, DialogClosingEventArgs eventArgs)
        {
            var dialogContent = eventArgs.Session.Content as WalletExportDialog;
            if (eventArgs.Parameter is bool parameter &&
            parameter == false) {
                if (dialogContent?.DataContext is WalletExportDialogViewModel vm) {
                    vm.Wallets.ToList().ForEach(w => w.IsSelected = false);//把选中内容重新归为未选中
                }
                return;
            }

            if (eventArgs.Parameter is bool parameter_yes &&
               parameter_yes == true)
            {
                //是导出操作才进行
                // 1. 获取对话框内容
                bool isExport = false;
                // 2. 访问 DataContext
                if (dialogContent?.DataContext is WalletExportDialogViewModel vm)
                {
                    // 3. 现在可以访问 viewModel 的数据
                    Debug.WriteLine($"获取到 ViewModel 数据: {vm.Name}");
                    var selectedItems = new List<WalletDto>();
                    foreach (var wallet in vm.Wallets)
                    {
                        if (wallet.IsSelected) selectedItems.Add(wallet);
                        //CollectSelectedItems(wallet.Accounts, selectedItems);
                    }
                    var list = SQLiteUtils._freesql.Select<WalletEntity>().Where(x => selectedItems.Any(y => y.Id == x.Id)).ToList();
                    
                    // 创建一条由 30 个 - 组成的分割线
                    //string dashLine = new string('-', 100);
                    string exportTitle = DefaultConfig.ContorlLanguage("WalletList") + DateTime.Now.ToString("yyyy-MM-dd-HHmmssfff") + RandomUtils.GenerateRandomNumber();
                    string exportData = list.Select(x => x.WalletContent.ToObject<YC.SolanaSdkService.DTO.WalletDto>()).ToIndentedJson();
                    // 调用导出逻辑，使用selectedItems
                    isExport = CommonExtension.ExportFile(exportData, "", "", exportTitle);
                    //...now, lets update the "session" with some new content!
                    //eventArgs.Session.UpdateContent(new SampleProgressDialog());
                    ////note, you can also grab the session when the dialog opens via the DialogOpenedEventHandler
                    ////lets run a fake operation for 3 seconds then close this baby.
                    //Task.Delay(TimeSpan.FromSeconds(1))
                    //    .ContinueWith((t, _) => eventArgs.Session.Close(false), null,
                    //        TaskScheduler.FromCurrentSynchronizationContext());

                    vm.Wallets.ToList().ForEach(w => w.IsSelected = false);//把选中内容重新归为未选中

                    //OK, lets cancel the close...
                    eventArgs.Cancel();
                }
                if (isExport)
                {  //如果已经有其他窗口打开，调用会有问题
                    CommonExtension.ShowDialog(DefaultConfig.ContorlLanguage("ExportWalletSuccess"));
                }
                else
                {
                    CommonExtension.ShowDialog(DefaultConfig.ContorlLanguage("ExportWalletFailed"));
                }
            }
        }

        private void ClosingEventHandler(object sender, DialogClosingEventArgs eventArgs)
            => Debug.WriteLine("You can intercept the closing event, and cancel here.");
        #endregion

        public async void InitWalletData() {

           string network = DefaultConfig.LocalWalletNetwork.ToString();
           var res= await SQLiteUtils._freesql.Select<WalletEntity>().Where(x=>x.NetWorkType== network).ToListAsync();
            var service = _container.Resolve<IWalletService>();
            //for (int i = 0; i < res.Count; i++) {//获取最新的钱包余额
            //    var walletBalance = await service.GetWalletLamportsBalanceAsync(res[i].Id);
            //    res[i].LamportsBalance = walletBalance.Data;
            //   var updateCount= await SQLiteUtils._freesql.Update<WalletEntity>().SetSource(res).ExecuteAffrowsAsync();
            //}
           // res = res.ToObject<List<WalletEntity>>();
           var entityList= res.Adapt<List<WalletDto>>();
            WalletDtos = new ObservableCollection<WalletDto>();
            WalletDtos.AddRange(entityList);
        }


        #region 创建关联账户业务逻辑
        public DialogEventViewModel dialogEventVM { get; set; }
        public ICommand RunCreateDialogCommand => new AnotherCommandImplementation(ExecuteRunCreateDialog);

        private async void ExecuteRunCreateDialog(object? _)
        {
            CreateAssociatedAccountControl view;
            var vm = new CreateAssociatedAccountViewModel();
            vm.TokenDefs = SQLiteUtils.Query<TokenDefEntity>();
            vm.Wallets = SQLiteUtils.Query<WalletEntity>().Where(x=>x.NetWorkType==DefaultConfig.LocalWalletNetwork.ToString()).ToList();
            view = new CreateAssociatedAccountControl
            {
                DataContext = vm,
            };
            
            //show the dialog
            var result = await DialogHost.Show(view, "RootDialog", CreateDialog_ExtendedOpenedEventHandler, CreateDialog_ExtendedClosingEventHandler);

            //check the result...
            Debug.WriteLine("Dialog was closed, the CommandParameter used to close it was: " + (result ?? "NULL"));
        }
        private void CreateDialog_ExtendedOpenedEventHandler(object sender, DialogOpenedEventArgs eventArgs)
       => Debug.WriteLine("You could intercept the open and affect the dialog using eventArgs.Session.");

        private void CreateDialog_ExtendedClosingEventHandler(object sender, DialogClosingEventArgs eventArgs)
        {
            var dialogContent = eventArgs.Session.Content as CreateAssociatedAccountControl;
            if (eventArgs.Parameter is bool parameter &&
            parameter == false)
            {
                if (dialogContent.DataContext is CreateAssociatedAccountViewModel vm)
                {
                    vm.SelectedTokenDef = null;//把选中内容重新归为未选中
                    vm.SelectedWallet = null;//把选中内容重新归为未选中
                    vm.SelectedPayWallet = null;//把选中内容重新归为未选中
                }
                return;
            }

            if (eventArgs.Parameter is bool parameter_yes &&
               parameter_yes == true)
            {
                if (dialogContent.DataContext is CreateAssociatedAccountViewModel vm)
                {
                    if (!vm.IsBusy)
                    {
                        CommonExtension.ShowDialog(DefaultConfig.ContorlLanguage("PleaseCompleteInfo"));
                        //return;
                    }
                    else {
                        var service = _container.Resolve<IWalletService>();
                        var res = Task.Run(() =>
                        {
                            return service.CreateAssociatedTokenAccountAsync(vm.SelectedWallet, vm.CustomMint, vm.SelectedPayWallet);

                        }).GetAwaiter().GetResult();
                        dialogContent.TriggerCloseCommand();
                        eventArgs.Cancel();
                        if (res.State)
                        {
                            CommonExtension.ShowDialog($"{DefaultConfig.ContorlLanguage("CreateAssociatedAccountSuccess")}{res.Data.Key}");
                        }
                        else
                        {
                            CommonExtension.ShowDialog(res.Message);
                        }

                    }


                }
                
            }
        }
       
        #endregion
    }

    
}
