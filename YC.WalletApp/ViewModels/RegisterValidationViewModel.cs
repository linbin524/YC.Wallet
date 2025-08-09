using System;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using Prism.Mvvm;
using YC.WalletApp.Extension;

namespace YC.WalletApp.ViewModels
{
    public class RegisterValidationViewModel : BindableBase, IDataErrorInfo
    {
        private static readonly Regex PasswordRegex = new Regex(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,20}$", RegexOptions.Compiled);

        private string _account = string.Empty;
        private string _password = string.Empty;
        private string _confirmPassword = string.Empty;

        public string Account
        {
            get => _account;
            set
            {
                SetProperty(ref _account, value);
                RaisePropertyChanged(nameof(IsValid));
                // 通知验证错误变化以更新UI
                RaisePropertyChanged("Item[]");
            }
        }

        public string Password
        {
            get => _password;
            set
            {
                SetProperty(ref _password, value);
                // 当密码变化时，也要重新验证确认密码
                RaisePropertyChanged(nameof(ConfirmPassword));
                RaisePropertyChanged(nameof(IsValid));
                // 通知验证错误变化以更新UI
                RaisePropertyChanged("Item[]");
            }
        }

        public string ConfirmPassword
        {
            get => _confirmPassword;
            set
            {
                SetProperty(ref _confirmPassword, value);
                RaisePropertyChanged(nameof(IsValid));
                // 通知验证错误变化以更新UI
                RaisePropertyChanged("Item[]");
            }
        }

        public bool IsValid
        {
            get
            {
                return !string.IsNullOrEmpty(Account) &&
                       !string.IsNullOrEmpty(Password) &&
                       !string.IsNullOrEmpty(ConfirmPassword) &&
                       string.IsNullOrEmpty(this[nameof(Account)]) &&
                       string.IsNullOrEmpty(this[nameof(Password)]) &&
                       string.IsNullOrEmpty(this[nameof(ConfirmPassword)]);
            }
        }

        public string Error => string.Empty;

        public string this[string columnName]
        {
            get
            {
                switch (columnName)
                {
                    case nameof(Account):
                        return ValidateAccount();
                    case nameof(Password):
                        return ValidatePassword();
                    case nameof(ConfirmPassword):
                        return ValidateConfirmPassword();
                    default:
                        return string.Empty;
                }
            }
        }

        private string ValidateAccount()
        {
            if (string.IsNullOrEmpty(Account))
                return LanguageManager.Instance["AccountEmpty"];
            
            if (Account.Length <= 6)
                return LanguageManager.Instance["AccountTooShort"];

            return string.Empty;
        }

        private string ValidatePassword()
        {
            if (string.IsNullOrEmpty(Password))
                return LanguageManager.Instance["PasswordEmpty"];

            if (Password.Length < 8 || Password.Length > 20)
                return LanguageManager.Instance["PasswordTooShort"];

            if (!HasLowercaseLetter(Password))
                return LanguageManager.Instance["PasswordNoLowercase"];

            if (!HasUppercaseLetter(Password))
                return LanguageManager.Instance["PasswordNoUppercase"];

            if (!HasDigit(Password))
                return LanguageManager.Instance["PasswordNoDigit"];

            if (!HasSpecialCharacter(Password))
                return LanguageManager.Instance["PasswordNoSpecial"];

            return string.Empty;
        }

        private string ValidateConfirmPassword()
        {
            if (string.IsNullOrEmpty(ConfirmPassword))
                return LanguageManager.Instance["ConfirmPasswordEmpty"];

            if (Password != ConfirmPassword)
                return LanguageManager.Instance["PasswordMismatch"];

            return string.Empty;
        }

        private static bool HasLowercaseLetter(string password)
        {
            return password.Any(char.IsLower);
        }

        private static bool HasUppercaseLetter(string password)
        {
            return password.Any(char.IsUpper);
        }

        private static bool HasDigit(string password)
        {
            return password.Any(char.IsDigit);
        }

        private static bool HasSpecialCharacter(string password)
        {
            return password.Any(ch => !char.IsLetterOrDigit(ch));
        }
    }
}