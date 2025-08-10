// TransactionViewModel.cs
using MaterialDesignThemes.Wpf;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System;
using YC.Model.Entity;
using Mapster;
using System.Windows;
using System.Drawing;
using System.Linq;
using YC.ApplicationService;
using YC.WalletApp.Extension;
using Prism.Events;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Collections;
using YC.WalletApp.Domain.PartViewControl;
using Google.Protobuf.WellKnownTypes;
using System.Text.RegularExpressions;
using YC.ApplicationService.DTO;
using YC.ApplicationService.Service;
using Prism.Ioc;
using YC.ApplicationService.IService;
using ImTools;
using MySqlX.XDevAPI.Common;
using ControlzEx.Standard;
using YC.SolanaSdkService;
using System.Windows.Threading;
using YC.Common.ShareUtils;
using YC.ApplicationService;

public class MintTokenViewModel : BindableBase, INotifyDataErrorInfo, IDisposable
{

    #region 属性
    // Properties
    private WalletEntity _selectedWallet;

    private ObservableCollection<WalletEntity> _wallets;
    public ObservableCollection<WalletEntity> Wallets
    {
        get => _wallets;
        set
        {
            SetProperty(ref _wallets, value);
        }
    }

    private ObservableCollection<TransactionRecordViewModel> _transactionRecords;
    public ObservableCollection<TransactionRecordViewModel> TransactionRecords
    {
        get => _transactionRecords;
        set => SetProperty(ref _transactionRecords, value);
    }

    private ObservableCollection<WalletAccountEntity> _walletAccounts;
    public ObservableCollection<WalletAccountEntity> WalletAccounts
    {
        get => _walletAccounts;
        set => SetProperty(ref _walletAccounts, value);
    }

    private bool _isGeneralAccount;
    public bool IsGeneralAccount
    {
        get => _isGeneralAccount;
        set {
            SetProperty(ref _isGeneralAccount, value, () => {
                IsAssociationedAccount = !value;
                ValidateAll();
            });
           
        } 
    }

    private bool _isAssociationedAccount;
    public bool IsAssociationedAccount
    {
        get => _isAssociationedAccount;
        set {
            SetProperty(ref _isAssociationedAccount, value, () => {

                IsGeneralAccount = !value;
                ValidateAll();
            });
            
        } 
            
    }

    private string _tokenName;
    /// <summary>
    /// token 名称
    /// </summary>
    [Required(ErrorMessage = "TokenNameRequired")]
    public string TokenName
    {
        get => _tokenName;
        set => SetProperty(ref _tokenName, value, () => ValidateAll());
    }

    private string _tokenSymbol;

    [Required(ErrorMessage = "TokenSymbolRequired")]
    /// <summary>
    /// token 字符标识
    /// </summary>
    public string TokenSymbol
    {
        get => _tokenSymbol;
        set => SetProperty(ref _tokenSymbol, value, () => ValidateAll());
    }

    private string _uri;

    /// <summary>
    /// Token 图标，通常是ipfs等网上可以访问的图片logo
    /// </summary>
    [Required(ErrorMessage = "UriRequired")]
    public string Uri
    {
        get => _uri;
        set => SetProperty(ref _uri, value, () => ValidateAll());
    }

    private string _tokenSupply;
    [Required(ErrorMessage = "TokenSupplyRequired")]
    [RegularExpression(@"^[1-9]\d*$", ErrorMessage = "TokenSupplyInvalid")]
    public string TokenSupply
    {
        get => _tokenSupply;
        set => SetProperty(ref _tokenSupply, value, () => ValidateAll());
    }

    private string _decimals;

    [Required(ErrorMessage = "DecimalsRequired")]
    [RegularExpression(@"^[1-9]\d*$", ErrorMessage = "DecimalsInvalid")]
    /// <summary>
    /// 小数点
    /// </summary>
    public string Decimals
    {
        get => _decimals; set =>
            SetProperty(ref _decimals, value, () => ValidateAll());
    }
    private string _memoString;

    /// <summary>
    /// 铸币备注信息
    /// </summary>
    [Required(ErrorMessage = "MemoRequired")]
    public string MemoString
    {
        get => _memoString; set =>
            SetProperty(ref _memoString, value, () => ValidateAll());
    } 
    #endregion

    // Commands
    public DelegateCommand ConfirmCommand { get; }
    public DelegateCommand<string> CopyCommand { get; }
    public DelegateCommand ShowTokenSelectorCommand { get; }
    private readonly SnackbarMessageQueue _messageQueue;
    private IContainerExtension _container;
    private SnackbarMessageQueue _snackbarMessageQueue;
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="dialogService"></param>
    public MintTokenViewModel(IEventAggregator eventAggregator,
        IContainerExtension container, SnackbarMessageQueue snackbarMessageQueue)
    {
        _container = container;
        ConfirmCommand = new DelegateCommand(ExecuteConfirm, CanConfirm)
              .ObservesProperty(() => IsFormValid)
              .ObservesProperty(() => IsProcessing);
        CopyCommand = new DelegateCommand<string>(ExecuteCopy);
        _eventAggregator = eventAggregator;
        _eventAggregator.GetEvent<PrismMessageEvent>().Subscribe(HandleReceivedMessage);
        _snackbarMessageQueue = snackbarMessageQueue;
        this.IsAssociationedAccount = true;
        

        
        LoadData();
       
        ValidateAll();
        
        // 订阅语言切换事件
        LanguageManager.Instance.PropertyChanged += OnLanguageChanged;
    }

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
        if (ReceivedMessage == "changeLanguage")
        {
            ValidateAll();
        }
        else {
            string network = DefaultConfig.LocalWalletNetwork.ToString();
            var res = SQLiteUtils._freesql.Select<WalletEntity>().Where(x => x.NetWorkType == network)
                .ToList().Adapt<ObservableCollection<WalletEntity>>();
            this.Wallets = res;
        }

      
    }
    
    /// <summary>
    /// 语言切换事件处理
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnLanguageChanged(object sender, PropertyChangedEventArgs e)
    {
        try
        {
            // 重新验证所有属性，更新验证消息的语言
            ValidateAll();
        }
        catch (Exception ex)
        {
            // 记录异常但不中断程序
            System.Diagnostics.Debug.WriteLine($"Language change validation error: {ex.Message}");
        }
    }
    #endregion

    #region 验证机制
    /// <summary>

    [CustomValidation(typeof(MintTokenViewModel), nameof(ValidateSelectedWallet))]
    public WalletEntity SelectedWallet
    {
        get => _selectedWallet;
        set => SetProperty(ref _selectedWallet, value, () => {
            ValidateProperty(SelectedWallet, nameof(SelectedWallet));
            //更新WalletAccount
            //var walletAccounts = SQLiteUtils._freesql.Select<WalletAccountEntity>()
            //.Where(x => x.BelongWalletId == SelectedWallet.Id
            //&& !x.TokenName.Contains("UnKnown")).ToList();
            //WalletAccounts= walletAccounts.Adapt<ObservableCollection<WalletAccountEntity>>();
           
        });
    }


    // Base58字符集
    private const string Base58Alphabet = "123456789ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz";
    private readonly Dictionary<string, List<string>> _errors = new();
    // INotifyDataErrorInfo
    public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;
    /// <summary>
    /// 验证所有
    /// </summary>
    private void ValidateAll()
    {
        _errors.Clear();
        //CanExecuteCreate();
        ValidateProperty(SelectedWallet, nameof(SelectedWallet));
        ValidateProperty(TokenName, nameof(TokenName));
        ValidateProperty(TokenSymbol, nameof(TokenSymbol));
        ValidateProperty(TokenSupply, nameof(TokenSupply));
        ValidateProperty(Decimals, nameof(Decimals));
        ValidateProperty(MemoString, nameof(MemoString));
        ValidateProperty(Uri, nameof(Uri));
    }

    private void ValidateProperty(object value, [CallerMemberName] string propertyName = null)
    {
        // 防止propertyName为null
        if (string.IsNullOrEmpty(propertyName))
        {
            return;
        }

        try
        {
            var context = new ValidationContext(this) { MemberName = propertyName };
            var results = new List<ValidationResult>();

            Validator.TryValidateProperty(value, context, results);

            // 将验证消息转换为多语言，添加异常处理
            var translatedErrors = new List<string>();
            foreach (var result in results)
            {
                try
                {
                    var translatedError = DefaultConfig.ContorlLanguage(result.ErrorMessage);
                    translatedErrors.Add(translatedError);
                }
                catch (Exception ex)
                {
                    // 如果翻译失败，使用原始错误消息
                    System.Diagnostics.Debug.WriteLine($"翻译错误消息失败: {ex.Message}");
                    translatedErrors.Add(result.ErrorMessage);
                }
            }

            _errors[propertyName] = translatedErrors;
            
            // 添加空引用检查，确保ErrorsChanged事件安全触发
            try
            {
                ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ErrorsChanged事件触发异常: {ex.Message}");
            }

            CanExecuteCreate();
        }
        catch (Exception ex)
        {
            // 记录异常但不抛出，确保UI不会崩溃
            System.Diagnostics.Debug.WriteLine($"ValidateProperty 方法异常: {ex.Message}");
        }
    }
    /// <summary>
    /// 验证按钮打开关键
    /// </summary>
    /// <returns></returns>
    private bool CanExecuteCreate()
    {
        double.TryParse(Decimals, out double tempDecimals);
        double.TryParse(Decimals, out double tempSupply);
            IsFormValid = !HasErrors &&
                   SelectedWallet != null &&!string.IsNullOrWhiteSpace(TokenName)
                   && !string.IsNullOrWhiteSpace(TokenSymbol)
                   && !string.IsNullOrWhiteSpace(TokenSupply)
                   && !string.IsNullOrWhiteSpace(MemoString)
                   && tempDecimals > 0
                   && tempSupply > 0;
        
        return IsFormValid;
    }

    public IEnumerable GetErrors(string propertyName) =>
        string.IsNullOrEmpty(propertyName) ? Enumerable.Empty<string>() : 
        _errors.TryGetValue(propertyName, out var errors) ? errors : Enumerable.Empty<string>();

    public bool HasErrors => _errors.Any(e => e.Value.Any());
    /// <summary>
    /// 全局结果验证
    /// </summary>
    /// 
    private bool _isFormValid;
    public bool IsFormValid
    {
        get => _isFormValid;
        set => SetProperty(ref _isFormValid, value);
    }

    private bool _isProcessing;


    public bool IsProcessing
    {
        get => _isProcessing;
        set {
            SetProperty(ref _isProcessing, value);
            //RaisePropertyChanged(nameof(IsProcessing));
            
        }
    }
    // Validation 多种方式使用，提供后续多语言改造时候的操作
    public static ValidationResult ValidateSelectedWallet(object wallet, ValidationContext context)
    {
        var instance = (MintTokenViewModel)context.ObjectInstance;
       
            if (wallet==null)
                return new ValidationResult("PleaseSelectWallet");

            //if (!IsValidSolanaAddress(mint))
            //    return new ValidationResult("无效的Solana地址格式");
       
        return ValidationResult.Success;
    }

    

    #endregion

    private async void LoadData()
    {
        await Task.Run(() =>
        {
            string network = DefaultConfig.LocalWalletNetwork.ToString();
            var res = SQLiteUtils._freesql.Select<WalletEntity>().Where(x => x.NetWorkType == network)
                .ToList().Adapt<ObservableCollection<WalletEntity>>();
            this.Wallets = res;
           
            UpdateTransactionRecords();
        });
    }

    public void UpdateTransactionRecords() {
        this.TransactionRecords = SQLiteUtils._freesql.Select<WalletAccountTransRecordEntity>()
                      .OrderByDescending(r => r.CreationTime).Where(x=>x.TransactionType== "MintToken")
                      .Take(50).ToList()
                      .Adapt<ObservableCollection<TransactionRecordViewModel>>();
        for(int i = 0; i < TransactionRecords.Count; i++)
        {          
            TransactionRecords[i].SequenceNumber = i + 1; // 从1开始编号
        }
    }

    private bool CanConfirm() => IsFormValid && !IsProcessing;

    private async void ExecuteConfirm()
    {
        try
        {
            // 立即在 UI 线程更新状态
            IsProcessing = true;

            // 强制立即刷新 UI（可选）
            await Application.Current.Dispatcher.InvokeAsync(() => { }, DispatcherPriority.Render);

           var tempWallet= BaseService.GetWalletByMnemonicWords(SelectedWallet.MnemonicStr);
            // 将耗时操作放在后台线程执行
            var service = _container.Resolve<ITransactionService>();
            CreateTokenDto obj = new();
            obj = this.Adapt<CreateTokenDto>();
            obj.NetWork = DefaultConfig.LocalWalletNetwork.ToString();
            if (IsAssociationedAccount)
            {
                obj.IsStorageAssociatedAccount = true;
            }
            else { 
            obj.StorageTokenAccount= tempWallet.GetAccount(RandomUtils.GetRandom(100000, 10000000));//随机生成一个普通账户存储代币
            }
            
            obj.MintAccount = tempWallet.GetAccount(RandomUtils.GetRandom(100000,10000000));//随机生成一个铸币账户
            obj.PayAccount= tempWallet.Account;
            obj.Uri = Uri;
             var res = await Task.Run(async () =>
            {
                return await service.MintTokenAsync(obj);
            });

            // 回到 UI 线程处理结果
            if (res.State)
            {
                UpdateTransactionRecords();
                CommonExtension.ShowDialog(string.Format(LanguageManager.Instance["MintTokenSuccess"], obj.TokenSymbol));
            }
            else
            {
                CommonExtension.ShowDialog(res.Message);
            }
        }
        finally
        {
            // 确保最终状态更新
            IsProcessing = false;
            // 强制立即刷新 UI（可选）
            await Application.Current.Dispatcher.InvokeAsync(() => { }, DispatcherPriority.Render);
        }
    }

    private void ExecuteCopy(string content)
    {
        try
        {
            Clipboard.SetText(content);
            _snackbarMessageQueue.Enqueue(LanguageManager.Instance["CopySuccess"]);
        }
        catch (Exception ex)
        {
            _snackbarMessageQueue.Enqueue(LanguageManager.Instance["CopyFailed"]);
        }
    }

    #region IDisposable
    private bool _disposed = false;

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            // 取消订阅语言切换事件
            if (LanguageManager.Instance != null)
            {
                LanguageManager.Instance.PropertyChanged -= OnLanguageChanged;
            }

            // 取消订阅消息事件
            if (_eventAggregator != null)
            {
                _eventAggregator.GetEvent<PrismMessageEvent>().Unsubscribe(HandleReceivedMessage);
            }

            _disposed = true;
        }
    }
    #endregion
}

