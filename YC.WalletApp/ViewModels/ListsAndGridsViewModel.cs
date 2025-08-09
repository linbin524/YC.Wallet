using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Data;
using YC.WalletApp.Domain;

namespace YC.WalletApp.ViewModels
{
   public class ListsAndGridsViewModel : ValidatableBindableBase
    {
        private string _name;
        private string _email;
        private string _password;
        private bool _isAgreed;
        private string _selectedItem;

        public ListsAndGridsViewModel()
        {
            // 首次进入界面触发验证
            ValidateProperty(nameof(Password), Password);
            RaisePropertyChanged(nameof(IsValid));
        }

        public string Name
        {
            get { return _name; }
            set { SetProperty(ref _name, value); }
        }

        public string Email
        {
            get { return _email; }
            set { SetProperty(ref _email, value); }
        }

        public string Password
        {
            get { return _password; }
            set { SetProperty(ref _password, value); }
        }

        public bool IsAgreed
        {
            get { return _isAgreed; }
            set { SetProperty(ref _isAgreed, value); }
        }

        public string SelectedItem
        {
            get { return _selectedItem; }
            set { SetProperty(ref _selectedItem, value); }
        }

        public bool IsValid => !HasErrors;

        protected override void ValidateProperty<T>(string propertyName, T value)
        {
            base.ValidateProperty(propertyName, value);

            switch (propertyName)
            {
                case nameof(Name):
                    if (string.IsNullOrEmpty(value as string))
                    {
                        SetError(propertyName, "姓名不能为空");
                    }
                    else if ((value as string).Length < 2)
                    {
                        SetError(propertyName, "姓名长度不能小于 2");
                    }
                    else
                    {
                        ClearErrors(propertyName);
                    }
                    break;
                case nameof(Email):
                    if (string.IsNullOrEmpty(value as string))
                    {
                        SetError(propertyName, "邮箱不能为空");
                    }
                    else if (!Regex.IsMatch(value as string, @"^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$"))
                    {
                        SetError(propertyName, "请输入有效的邮箱地址");
                    }
                    else
                    {
                        ClearErrors(propertyName);
                    }
                    break;
                case nameof(Password):
                    if (string.IsNullOrEmpty(value as string))
                    {
                        SetError(propertyName, "密码不能为空");
                    }
                    else if ((value as string).Length < 6)
                    {
                        SetError(propertyName, "密码长度不能小于 6");
                    }
                    else
                    {
                        ClearErrors(propertyName);
                    }
                    break;
                case nameof(IsAgreed):
                    if (!(bool)(object)value)
                    {
                        SetError(propertyName, "请同意协议");
                    }
                    else
                    {
                        ClearErrors(propertyName);
                    }
                    break;
                case nameof(SelectedItem):
                    if (string.IsNullOrEmpty(value as string))
                    {
                        SetError(propertyName, "请选择一个选项");
                    }
                    else
                    {
                        ClearErrors(propertyName);
                    }
                    break;
            }

            RaisePropertyChanged(nameof(IsValid));
        }
    }

   
}
