using CommunityToolkit.Mvvm.Input;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using YC.ApplicationService;
using YC.Model;
using YC.Model.Entity;
using YC.WalletApp.Extension;

namespace YC.WalletApp.ViewModels
{
    public class LoginViewModel: BindableBase
    {
        public ICommand GenreDropDownMenuItemCommand { get; set; }
        private SupportedLanguage _currentLanguage;
        public SupportedLanguage CurrentLanguage
        {
            get { return _currentLanguage; }
            set { SetProperty(ref _currentLanguage, value); }
        }
        private LoginPerson _person;

        public LoginPerson Person
        {
            get { return _person; }
            set { SetProperty(ref _person, value); }
        }

        public LoginViewModel() {
            Person = new LoginPerson();
            GenreDropDownMenuItemCommand = new RelayCommand<string>(OnGenreSelected);
        }

        public static void OnGenreSelected(string selectedLanguage)
        {
            LanguageService.SetLanguage(selectedLanguage);
        }
    }

    public class LoginPerson : INotifyPropertyChanged, IDataErrorInfo
    {
        private string _name;
        private string _password;

        public string Name
        {
            get { return _name; }
            set
            {
                if (_name != value)
                {
                    _name = value;
                    OnPropertyChanged(nameof(Name));
                }
            }
        }

        public string Password
        {
            get { return _password; }
            set
            {
                if (_password != value)
                {
                    _password = value;
                    OnPropertyChanged(nameof(Password));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public string Error => null;

        public string this[string columnName]
        {
            get
            {
                switch (columnName)
                {
                    case nameof(Name):
                       if (string.IsNullOrEmpty(Name))
                        {
                            return "用户名不能为空.";
                        }
                        break;
                    case nameof(Password):
                        if (string.IsNullOrEmpty(Password))
                        {
                            return "密码不能为空.";
                        }
                        else if (Password.Length < 8) {
                            return "密码字符串长度小于8位.";
                        }
                        break;
                }
                return null;
            }
        }
    }
}
