using YC.WalletApp.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YC.WalletApp.Domain;
using System.ComponentModel;
using System.Xml.Linq;

namespace YC.WalletApp.ViewModels
{
    public class RegisterViewModel: ValidateModelBase, IDataErrorInfo
    {
        
       
        private string _Account;
        /// <summary>
        /// 账号
        /// </summary>
        [Required(ErrorMessage = "账号不允许为空")]
        [MinLength(6, ErrorMessage = "账号不能少于六个字符")]
        [MaxLength(36, ErrorMessage = "账号不能大于36字符")]
        public string Account
        {
            get { return _Account; }
            set { _Account = value; RaisePropertyChanged(); }
        }


        private string _Password;
        /// <summary>
        /// 密码
        /// </summary>
        [Required(ErrorMessage = "密码不允许为空")]
        [MinLength(6, ErrorMessage = "密码不能少于8个字符")]
        public string Password
        {
            get { return _Password; }
            set { _Password = value; RaisePropertyChanged(); }
        }


        private string _ConfirmPassword;

        /// <summary>
        /// 确认密码
        /// </summary>
        [Required]
        [Compare("Password", ErrorMessage = "密码和确认密码不一致")]
        public string ConfirmPassword
        {
            get { return _ConfirmPassword; }
            set { _ConfirmPassword = value; RaisePropertyChanged(); }
        }

        public string Error => null;

        public string this[string columnName]
        {
            get
            {
                switch (columnName)
                {
                    case nameof(Account):
                        if (string.IsNullOrEmpty(Account))
                        {
                            return "用户名不能为空.";
                        }
                        break;
                    case nameof(Password):
                        if (string.IsNullOrEmpty(Password))
                        {
                            return "密码不能为空.";
                        }
                        else if (Password.Length < 8)
                        {
                            return "密码字符串长度小于8位.";
                        }
                        break;
                }
                return null;
            }
        }
    }
}
