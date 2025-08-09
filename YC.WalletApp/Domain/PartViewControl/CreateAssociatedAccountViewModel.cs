using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using YC.Model.Entity;
using YC.ApplicationService;

namespace YC.WalletApp.Domain.PartViewControl
{
    public class CreateAssociatedAccountViewModel : BindableBase, INotifyDataErrorInfo
    {
        private readonly Dictionary<string, List<string>> _errors = new();

        public CreateAssociatedAccountViewModel()
        {
            //CreateAssociatedAccountCommand = new DelegateCommand(ExecuteCreate, CanExecuteCreate)
            //    .ObservesProperty(() => SelectedWallet)
            //    .ObservesProperty(() => HasErrors);
             ValidateAll();
         
        }

        private List<TokenDefEntity> _tokenDefs;
        public List<TokenDefEntity> TokenDefs { get => _tokenDefs; set => SetProperty(ref _tokenDefs, value); }

        private List<WalletEntity> _wallets;
        public List<WalletEntity> Wallets { get => _wallets; set => SetProperty(ref _wallets, value); }

        // Commands,这里没有用了，在WalletManageViewModel 那边调用方处理调用
        public DelegateCommand CreateAssociatedAccountCommand { get; }

        // Properties
        private bool _isTokenDefSelected = true;
        public bool IsTokenDefSelected
        {
            get => _isTokenDefSelected;
            set => SetProperty(ref _isTokenDefSelected, value, () =>
            {
                IsCustomInputSelected = !value;
                ValidateAll();
            });
        }

        private bool _isCustomInputSelected;
        public bool IsCustomInputSelected
        {
            get => _isCustomInputSelected;
            set => SetProperty(ref _isCustomInputSelected, value, () =>
            {
                IsTokenDefSelected = !value;
                ValidateAll();
            });
        }

        private TokenDefEntity _selectedTokenDef;
        [Required(ErrorMessage = "TokenDefRequired")]
        public TokenDefEntity SelectedTokenDef
        {
            get => _selectedTokenDef;
            set => SetProperty(ref _selectedTokenDef, value, () => {
                this.CustomMint= _selectedTokenDef?.Mint;
                this.CustomDecimalPlaces= _selectedTokenDef?.DecimalPlaces.ToString();
                this.CustomSymbol= _selectedTokenDef?.Symbol;
                ValidateAll();
            });
        }

        private WalletEntity _selectedWallet;
        [Required(ErrorMessage = "WalletRequired")]
        public WalletEntity SelectedWallet { get => _selectedWallet; set => SetProperty(ref _selectedWallet, value,()=> ValidateProperty(value)); }


        private WalletEntity _selectedPayWallet;
        [Required(ErrorMessage = "PayWalletRequired")]
        public WalletEntity SelectedPayWallet { get => _selectedPayWallet; set => SetProperty(ref _selectedPayWallet, value, () => ValidateProperty(value)); }


        private string _customSymbol;

        [Required(ErrorMessage = "CustomSymbolRequired")]
        [MaxLength(50, ErrorMessage = "CustomSymbolTooLong")]
        public string CustomSymbol { get=> _customSymbol; set=>SetProperty(ref _customSymbol,value, () => ValidateProperty(value)); }
       
        private string _customDecimalPlaces;
        [Required(ErrorMessage = "CustomDecimalsRequired")]
        [RegularExpression(@"^-?\d+([\.,]\d+)?$",ErrorMessage = "CustomDecimalsInvalid")]
        [MaxLength(10, ErrorMessage = "CustomDecimalsTooLong")]
        //[Range(0, 18, ErrorMessage = "小数位必须在0-18之间")]
        public string CustomDecimalPlaces { get => _customDecimalPlaces; set => SetProperty(ref _customDecimalPlaces, value, () => ValidateProperty(value)); }


        public string _customMint;
        [CustomValidation(typeof(CreateAssociatedAccountViewModel), nameof(ValidateMint))]
        public string CustomMint
        {
            get => _customMint;
            set
            {
                SetProperty(ref _customMint, value);
                ValidateProperty(value, nameof(CustomMint));
            }
        }

        // Validation 多种方式使用，提供后续多语言改造时候的操作
        public static ValidationResult ValidateMint(string mint, ValidationContext context)
        {
            var instance = (CreateAssociatedAccountViewModel)context.ObjectInstance;
            if (instance.IsCustomInputSelected)
            {
                if (string.IsNullOrWhiteSpace(mint))
                    return new ValidationResult("MintAddressRequired");

                //if (!IsValidSolanaAddress(mint))
                //    return new ValidationResult("无效的Solana地址格式");
            }
            return ValidationResult.Success;
        }

        private static bool IsValidSolanaAddress(string address)
        {
            // 实现具体的地址验证逻辑
            return address?.Length == 44 && address.All(c => char.IsLetterOrDigit(c));
        }
        /// <summary>
        /// 全部验证
        /// </summary>
        private void ValidateAll()
        {
            _errors.Clear();
            ValidateProperty(SelectedWallet, nameof(SelectedWallet));
            ValidateProperty(SelectedPayWallet, nameof(SelectedPayWallet));
            
            if (IsTokenDefSelected) {
                ValidateProperty(SelectedTokenDef, nameof(SelectedTokenDef));
            }else {
                ValidateProperty(CustomMint, nameof(CustomMint));
                ValidateProperty(CustomSymbol, nameof(CustomSymbol));
                ValidateProperty(CustomDecimalPlaces, nameof(CustomDecimalPlaces));
            }
            //var data = CanExecuteCreate();
        }

        private void ValidateProperty(object value, [CallerMemberName] string propertyName = null)
        {
            var context = new ValidationContext(this) { MemberName = propertyName };
            var results = new List<ValidationResult>();

            Validator.TryValidateProperty(value, context, results);
             ///每次验证时候存放
            _errors[propertyName] = results.Select(r => DefaultConfig.ContorlLanguage(r.ErrorMessage)).ToList();
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
            CanExecuteCreate();
            //CreateAssociatedAccountCommand.RaiseCanExecuteChanged();
        }

        // INotifyDataErrorInfo
        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

        public IEnumerable GetErrors(string propertyName) =>
            _errors.TryGetValue(propertyName, out var errors) ? errors : Enumerable.Empty<string>();

        public bool HasErrors => _errors.Any(e => e.Value.Any());

        // 其他辅助属性
        public IEnumerable<string> CustomErrors =>
            new[] { nameof(CustomMint), nameof(CustomSymbol), nameof(CustomDecimalPlaces) }
                .SelectMany(n => _errors.TryGetValue(n, out var e) ? e : Enumerable.Empty<string>());

        private bool _isBusy;
        public bool IsBusy
        {
            get  {
                return _isBusy; 
            }
            set => SetProperty(ref _isBusy, value);
        }

        #region 废弃
        public Action RequestClose { get; set; }

        // 业务逻辑
        private async void ExecuteCreate()
        {
            try
            {
                IsBusy = true;

                //var request = new CreateRequest
                //{
                //    Wallet = SelectedWallet,
                //    Mint = IsTokenDefSelected ? SelectedTokenDef.Mint : CustomMint,
                //    Symbol = IsTokenDefSelected ? SelectedTokenDef.Symbol : CustomSymbol,
                //    Decimals = IsTokenDefSelected ? SelectedTokenDef.DecimalPlaces : int.Parse(CustomDecimalPlaces)
                //};

                //await _accountService.CreateAsync(request);
                RequestClose?.Invoke();
            }
            catch (Exception ex)
            {
                // 处理异常
            }
            finally
            {
                IsBusy = false;
            }
        } 
        #endregion

        private bool CanExecuteCreate()
        {
           IsBusy=  !HasErrors &&
                   SelectedWallet != null && SelectedPayWallet != null && !string.IsNullOrWhiteSpace(CustomMint);
            return IsBusy;
        }
    }
}
