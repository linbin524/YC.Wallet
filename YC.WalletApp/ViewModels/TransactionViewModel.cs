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

public class TransactionViewModel : BindableBase, INotifyDataErrorInfo, IDisposable
{
    private readonly IDialogService _dialogService;
    // Properties
    private WalletEntity _selectedWallet;

    private ObservableCollection<WalletEntity> _wallets;
    public ObservableCollection<WalletEntity> Wallets
    {
        get => _wallets;
        set{
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
        get
        {
            if (_walletAccounts != null) {
                for(int i=0;i<_walletAccounts.Count;i++)
                {
                    if (!_walletAccounts[i].IsAssociatedTokenAccount)
                    {
                        _walletAccounts[i].AccountType = DefaultConfig.ContorlLanguage("WalletAccount");
                    }
                    else
                    {
                        _walletAccounts[i].AccountType = DefaultConfig.ContorlLanguage("TokenAssociatedAccount");
                    }
                }

            }

            return _walletAccounts;
        }
        
        set => SetProperty(ref _walletAccounts, value);
    }
    

    //private ObservableCollection<TokenDefEntity> _tokenDef;
    //public ObservableCollection<TokenDefEntity> TokenDefs
    //{
    //    get => _tokenDef;
    //    set => SetProperty(ref _tokenDef, value);
    //}



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
    public TransactionViewModel(IDialogService dialogService,
        IEventAggregator eventAggregator,
        IContainerExtension container, SnackbarMessageQueue snackbarMessageQueue)
    {
        _dialogService = dialogService;
        _container = container;
        ConfirmCommand = new DelegateCommand(ExecuteConfirm, CanConfirm)
              .ObservesProperty(() => IsFormValid)
              .ObservesProperty(() => IsProcessing);
        CopyCommand = new DelegateCommand<string>(ExecuteCopy);
        //ShowTokenSelectorCommand = new DelegateCommand(ExecuteShowTokenSelector);
        this.IsLamportsTransfer = true;
        _eventAggregator = eventAggregator;
        _eventAggregator.GetEvent<PrismMessageEvent>().Subscribe(HandleReceivedMessage);
        _snackbarMessageQueue = snackbarMessageQueue;
        
        // 订阅语言变化事件
        LanguageManager.Instance.PropertyChanged += OnLanguageChanged;
        
        LoadData();
        
        ValidateAll();
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
        string network = DefaultConfig.LocalWalletNetwork.ToString();
        var res = SQLiteUtils._freesql.Select<WalletEntity>().Where(x => x.NetWorkType == network)
            .ToList().Adapt<ObservableCollection<WalletEntity>>();
        this.Wallets = res;
       
    }

    /// <summary>
    /// 语言变化处理
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnLanguageChanged(object sender, PropertyChangedEventArgs e)
    {
        // LanguageManager 在 ChangeLanguage() 时会触发 PropertyChanged 事件
        // 事件参数可能是空字符串、Item[] 或 Item
        if (string.IsNullOrEmpty(e.PropertyName) || e.PropertyName == "Item[]" || e.PropertyName == "Item")
        {
            try
            {
                // 语言变化时重新验证所有字段，更新错误消息
                ValidateAll();
                
                // 更新钱包账户类型显示
                if (WalletAccounts != null)
                {
                    foreach (var account in WalletAccounts)
                    {
                        if (!account.IsAssociatedTokenAccount)
                        {
                            account.AccountType = DefaultConfig.ContorlLanguage("WalletAccount");
                        }
                        else
                        {
                            account.AccountType = DefaultConfig.ContorlLanguage("TokenAssociatedAccount");
                        }
                    }
                    RaisePropertyChanged(nameof(WalletAccounts));
                }
            }
            catch (Exception ex)
            {
                // 记录异常但不中断程序
                System.Diagnostics.Debug.WriteLine($"Language change validation error: {ex.Message}");
            }
        }
    }
    #endregion

    #region 验证机制
    /// <summary>
    /// 金额
    /// </summary>
    private string _amount;

    private WalletAccountEntity _selectedWalletAccount;
    private WalletAccountEntity _showNewWalletAccount;
    public WalletAccountEntity ShowNewWalletAccount {
        get => _selectedWalletAccount;
        set => SetProperty(ref _selectedWalletAccount, value);
    }

    [CustomValidation(typeof(TransactionViewModel), nameof(ValidateSelectedWalletAccount))]
    public WalletAccountEntity SelectedWalletAccount
    {
        get => _selectedWalletAccount;
        set {
            SetProperty(ref _selectedWalletAccount, value, () => ValidateProperty(value));
            var obj = SQLiteUtils._freesql.Select<WalletAccountEntity>().Where(x => x.Id == SelectedWalletAccount.Id).First();
            var res = Task.Run(async () => {
                var data = await BaseService.GetTokenBalanceAsync(new Solnet.Wallet.PublicKey(obj.PublicKey));
                return data;
            }).GetAwaiter().GetResult();
            if (res.Data?.Amount != obj.Balance)
            {
                obj.Balance = res.Data.Amount;
                obj.LastModifierUserId = DefaultConfig.CurrentLoginUser.Id;
                obj.LastModificationTime = DateTime.Now;
                var updateRes = SQLiteUtils._freesql.Update<WalletAccountEntity>().SetSource(obj).ExecuteAffrows();
               
                //重新更新数据
                //WalletAccounts= SQLiteUtils._freesql.Select<WalletAccountEntity>().ToList().Adapt<ObservableCollection<WalletAccountEntity>>();
            }
            ShowNewWalletAccount = obj;

        } 
    }

    [CustomValidation(typeof(TransactionViewModel), nameof(ValidateAmount))]
    public string Amount
    {
        get => _amount;
        set => SetProperty(ref _amount, value, () => ValidateProperty(value));
    }
    private string _tokenInput;
    public string TokenInput
    {
        get => _tokenInput;
        set => SetProperty(ref _tokenInput, value);
    }

    [CustomValidation(typeof(TransactionViewModel), nameof(ValidateSelectedWallet))]
    public WalletEntity SelectedWallet
    {
        get => _selectedWallet;
        set => SetProperty(ref _selectedWallet, value, () => {
            ValidateProperty(SelectedWallet, nameof(SelectedWallet));
            //更新WalletAccount
            var walletAccounts = SQLiteUtils._freesql.Select<WalletAccountEntity>()
            .Where(x => x.BelongWalletId == SelectedWallet.Id
            && !x.TokenName.Contains("UnKnown")).ToList();
            WalletAccounts= walletAccounts.Adapt<ObservableCollection<WalletAccountEntity>>();
            foreach (var item in WalletAccounts)
            {
                if (!item.IsAssociatedTokenAccount)
                {
                    item.AccountType = DefaultConfig.ContorlLanguage("WalletAccount");
                }
                else
                {
                    item.AccountType = DefaultConfig.ContorlLanguage("TokenAssociatedAccount");
                }
            }
        });
    }

    private string _receiverAddress;

    [CustomValidation(typeof(TransactionViewModel), nameof(ValidateReceiverAddress))]
    public string ReceiverAddress
    {
        get => _receiverAddress;
        set => SetProperty(ref _receiverAddress, value, () => ValidateProperty(value));
    }
    private bool _isLamportsTransfer = true;
    public bool IsLamportsTransfer
    {
        get => _isLamportsTransfer;
        set => SetProperty(ref _isLamportsTransfer, value, () =>
        {
            IsTokenTransfer = !value;
            ValidateAll();
        });
    }
    private bool _isTokenTransfer;
    public bool IsTokenTransfer
    {
        get => _isTokenTransfer;
        set => SetProperty(ref _isTokenTransfer, value, () =>
        {
            IsLamportsTransfer = !value;
            ValidateAll();
        });
    }

    // 添加地址验证方法
    private bool IsValidAddress(string address)
    {
        if (string.IsNullOrWhiteSpace(address)) return false;

        // Solana地址基础验证规则：
        // 1. 长度43-44个字符
        // 2. 使用Base58编码
        return address.Length is 43 or 44 &&
               address.All(c => Base58Alphabet.Contains(c));
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
        ValidateProperty(SelectedWallet, nameof(SelectedWallet));
        ValidateProperty(Amount, nameof(Amount));
        ValidateProperty(ReceiverAddress, nameof(ReceiverAddress));
        if (IsLamportsTransfer)//只有验证接受地址
        {
            ValidateProperty(TokenInput, nameof(TokenInput));
        }
        else//验证选择的钱包账户
        {
            ValidateProperty(SelectedWalletAccount, nameof(SelectedWalletAccount));
            //ValidateProperty(SelectedTokenDef, nameof(SelectedTokenDef));
        }

    }

    private void ValidateProperty(object value, [CallerMemberName] string propertyName = null)
    {
        var context = new ValidationContext(this) { MemberName = propertyName };
        var results = new List<ValidationResult>();

        Validator.TryValidateProperty(value, context, results);

        // 将验证消息转换为多语言
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
        ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        CanExecuteCreate();
    }
    /// <summary>
    /// 验证按钮打开关键
    /// </summary>
    /// <returns></returns>
    private bool CanExecuteCreate()
    {
        double result;
      var isOk= double.TryParse(Amount, out result);
        if (!isOk) return IsFormValid=false;//如果数字无法转换直接返回验证错误
        if (IsLamportsTransfer) {
            IsFormValid = !HasErrors &&
                   SelectedWallet != null &&!string.IsNullOrWhiteSpace(ReceiverAddress)
                   && result>0;
        }else
        {
            IsFormValid = !HasErrors &&
                   SelectedWallet != null && SelectedWalletAccount!=null && !string.IsNullOrWhiteSpace(ReceiverAddress)
                   && result > 0;
        }
        return IsFormValid;
    }

    public IEnumerable GetErrors(string propertyName) =>
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
        var instance = (TransactionViewModel)context.ObjectInstance;
       
            if (wallet==null)
                return new ValidationResult("PleaseSelectWallet");

            //if (!IsValidSolanaAddress(mint))
            //    return new ValidationResult("无效的Solana地址格式");
       
        return ValidationResult.Success;
    }

    public static ValidationResult ValidateSelectedTokenType(object tokenDef, ValidationContext context)
    {
        var instance = (TransactionViewModel)context.ObjectInstance;
        if (instance.IsTokenTransfer)
        {
            if (tokenDef == null)
                return new ValidationResult("PleaseSelectTokenType");

            //if (!IsValidSolanaAddress(mint))
            //    return new ValidationResult("无效的Solana地址格式");
        }
        return ValidationResult.Success;
    }

    public static ValidationResult ValidateReceiverAddress(string address, ValidationContext context)
    {
        var instance = (TransactionViewModel)context.ObjectInstance;
        if (string.IsNullOrWhiteSpace(address))
            return new ValidationResult("PleaseEnterRecipientAddress");
        if (address.Length < 25) return new ValidationResult("RecipientAddressFormatIncorrect");
        return ValidationResult.Success;
    }

    public static ValidationResult ValidateAmount(string value, ValidationContext context)
    {
        var instance = (TransactionViewModel)context.ObjectInstance;
        if (string.IsNullOrWhiteSpace(value))
        {
            return new ValidationResult("InputAmountCannotBeEmpty");
        }

        // 正则表达式用于匹配有效的数字（整数或包含小数点的数字）
        string pattern = @"^[+-]?\d+(\.\d+)?$";
        if (!Regex.IsMatch(value.ToString(), pattern))
        {
            return new ValidationResult("InputMustBeValidNumber");
        }
        if (double.Parse(value.ToString())<0)
        {
            return new ValidationResult("TransferAmountCannotBeLessThanZero");
        }

        return ValidationResult.Success;
    }

    public static ValidationResult ValidateSelectedWalletAccount(object account, ValidationContext context)
    {
        var instance = (TransactionViewModel)context.ObjectInstance;
        if (instance.IsTokenTransfer)
        {
            if (account == null)
                return new ValidationResult(DefaultConfig.ContorlLanguage("Please select a wallet account."));

            //if (!IsValidSolanaAddress(mint))
            //    return new ValidationResult("无效的Solana地址格式");
        }
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
            //TokenDefs = SQLiteUtils._freesql.Select<TokenDefEntity>().ToList()
            //    .Adapt<ObservableCollection<TokenDefEntity>>();
            UpdateTransactionRecords();
        });
    }

    public void UpdateTransactionRecords() {
        this.TransactionRecords = SQLiteUtils._freesql.Select<WalletAccountTransRecordEntity>()
                      .OrderByDescending(r => r.CreationTime).Where(x => x.TransactionType != "MintToken")
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
        if (IsTokenTransfer&&SelectedWalletAccount!=null) { //单独验证输入token 小数位
        
        }

        try
        {
            // 立即在 UI 线程更新状态
            IsProcessing = true;

            // 强制立即刷新 UI（可选）
            await Application.Current.Dispatcher.InvokeAsync(() => { }, DispatcherPriority.Render);

            // 将耗时操作放在后台线程执行
            var service = _container.Resolve<ITransactionService>();
            var res = await Task.Run(async () =>
            {
                SendTransactionDto obj = new SendTransactionDto
                {
                    TansType = IsLamportsTransfer ? 0 : 1,
                    PayAccount = SelectedWallet.MasterAccountPublicKey,
                    Amount = Amount,
                    NetWork = DefaultConfig.LocalWalletNetwork.ToString(),
                    Receiver = ReceiverAddress.ToString(),
                    TokenDef = SQLiteUtils._freesql.Select<TokenDefEntity>()
                        .Where(x => x.Mint == SelectedWalletAccount.TokenMint).First(),
                    WalletAccount = SelectedWalletAccount,
                    WalletId = SelectedWallet.Id
                };
                return await service.SendTransactionAsync(obj);
            });

            // 回到 UI 线程处理结果
            if (res.State)
            {
                UpdateTransactionRecords();
                CommonExtension.ShowDialog("交易成功！");
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
        }
    }

    private void ExecuteCopy(string content)
    {
        if (!string.IsNullOrEmpty(content))
        {
            Clipboard.SetText(content);
            // Show toast
        }
    }

    /// <summary>
    /// 释放资源
    /// </summary>
    public void Dispose()
    {
        // 取消订阅语言变化事件
        LanguageManager.Instance.PropertyChanged -= OnLanguageChanged;
        
        // 取消订阅消息事件
        if (_eventAggregator != null)
        {
            _eventAggregator.GetEvent<PrismMessageEvent>().Unsubscribe(HandleReceivedMessage);
        }
    }

    
}

// TransactionRecordViewModel.cs
public class TransactionRecordViewModel : BindableBase
{
    private string? _payAccount;
    [Display(Name = "付款账户")]
    public string? PayAccount
    {
        get => _payAccount;
        set => SetProperty(ref _payAccount, value);
    }

    private string? _tokenSymbol;
    [Display(Name = "代币符号")]
    public string? TokenSymbol
    {
        get => _tokenSymbol;
        set => SetProperty(ref _tokenSymbol, value);
    }

    private DateTime? _creationTime;
    [Display(Name = "创建时间")]
    public DateTime? CreationTime
    {
        get => _creationTime;
        set => SetProperty(ref _creationTime, value);
    }

    private string? _tokenMint;
    [Display(Name = "代币铸造地址")]
    public string? TokenMint
    {
        get => _tokenMint;
        set => SetProperty(ref _tokenMint, value);
    }

    private string? _jsonContent;
    [Display(Name = "JSON 内容")]
    public string? JsonContent
    {
        get => _jsonContent;
        set => SetProperty(ref _jsonContent, value);
    }

    private string? _netWork;
    [Display(Name = "适配网络")]
    public string? NetWork
    {
        get => _netWork;
        set => SetProperty(ref _netWork, value);
    }

    private string? _remark;
    [Display(Name = "交易备注")]
    public string? Remark
    {
        get => _remark;
        set => SetProperty(ref _remark, value);
    }

    private string? _transferor;
    [Display(Name = "转出方")]
    public string? Transferor
    {
        get => _transferor;
        set => SetProperty(ref _transferor, value);
    }

    private string? _receiver;
    [Display(Name = "收款方")]
    public string? Receiver
    {
        get => _receiver;
        set => SetProperty(ref _receiver, value);
    }

    private string? _transferQuantity;
    [Display(Name = "转账数量")]
    public string? TransferQuantity
    {
        get => _transferQuantity;
        set => SetProperty(ref _transferQuantity, value);
    }

    private int? _transferStatus;
    /// <summary>
    /// 1 成功，0 失败， 2 等待确认
    /// </summary>
    [Display(Name = "转账状态")]
    public int? TransferStatus
    {
        get => _transferStatus;
        set
        {
            SetProperty(ref _transferStatus, value, () =>
            {
                // 当 TransferStatus 改变时，通知依赖属性更新

            });
            
        }
    }

    private string? _recentBlockhash;
    [Display(Name = "区块号")]
    public string? RecentBlockhash
    {
        get => _recentBlockhash;
        set => SetProperty(ref _recentBlockhash, value);
    }

    private ulong _fee;
    [Display(Name = "手续费")]
    public ulong Fee
    {
        get => _fee;
        set => SetProperty(ref _fee, value);
    }

    private string _transactionHash;
    [Display(Name = "交易哈希")]
    public string TransactionHash
    {
        get => _transactionHash;
        set => SetProperty(ref _transactionHash, value);
    }

    private string _transactionType;
    /// <summary>
    /// lamports、nft、SPLToken
    /// </summary>
    [Display(Name = "交易类型")]
    public string TransactionType
    {
        get => _transactionType;
        set => SetProperty(ref _transactionType, value);
    }

    private long _walletId;
    [Display(Name = "钱包Id")]
    public long WalletId
    {
        get => _walletId;
        set => SetProperty(ref _walletId, value);
    }

    private bool _isAssociatedTokenAccount;

    [Display(Name = "关联账户标识")]
    public bool IsAssociatedTokenAccount
    {
        get => _isAssociatedTokenAccount;
        set => SetProperty(ref _isAssociatedTokenAccount, value);
    }

    private bool _isExpanded;
    public bool IsExpanded
    {
        get => _isExpanded;
        set => SetProperty(ref _isExpanded, value);
    }
    public int _sequenceNumber;
    public int SequenceNumber
    {
        get => _sequenceNumber;
        set => SetProperty(ref _sequenceNumber, value);
    }
    private PackIconKind _statusIcon;
    public PackIconKind StatusIcon
    {
        get
        {
             return TransferStatus switch
                {
                    1 => PackIconKind.CheckCircle,
                    0 => PackIconKind.CloseCircle,
                    _ => PackIconKind.Clock
                };
        }
        set => SetProperty(ref _statusIcon, value);
    }

    private Brush _statusColor;
    public Brush StatusColor
    {
        get {
         return   TransferStatus switch
            {
                1 => Brushes.LimeGreen,
                0 => Brushes.Red,
                _ => Brushes.Gold
            };
        }
        set => SetProperty(ref _statusColor, value);
    }
   
}