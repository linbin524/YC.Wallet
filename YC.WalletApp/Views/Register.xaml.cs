using Mapster;
using Prism.Ioc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Threading;
using YC.WalletApp.ViewModels;
using YC.Model;
using YC.ApplicationService.IService;
using YC.WalletApp.Extension;


namespace YC.WalletApp.Views
{
    /// <summary>
    /// Register.xaml 的交互逻辑
    /// </summary>
    public partial class Register : Window
    {

        private IContainerExtension _container;
        private DispatcherTimer _timer = new DispatcherTimer();
        private int _countdown = 3;
        private IUserService _userService;
        public RegisterValidationViewModel ValidationViewModel { get; set; }
        
        public Register(IContainerExtension container, IUserService userService)
        {
            InitializeComponent();
            _container = container;
            _userService = userService;
            ValidationViewModel = new RegisterValidationViewModel();
            this.DataContext = this;
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (_countdown > 0)
            {
                SecondsText.Inlines.Clear();
                SecondsText.Inlines.Add(new Run(_countdown.ToString()));
                _countdown--;
            }
            else
            {
                _timer.Stop();
                //ShowDialog();
                this.Hide();
                this.Close();
              var loginPage=  _container.Resolve<Login>();
                loginPage.Show();
               // 
            }
        }

        private void ShowDialog()
        {
            MessageBox.Show("Countdown ends！", "prompt", MessageBoxButton.OK);
        }


        private void textEmail_MouseDown(object sender, MouseButtonEventArgs e)
        {
            txtEmail.Focus();
        }

        private void txtEmail_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(txtEmail.Text) && txtEmail.Text.Length > 0)
            {
                textEmail.Visibility = Visibility.Collapsed;
            }
            else
            {
                textEmail.Visibility = Visibility.Visible;
            }
        }

        private void textPassword_MouseDown(object sender, MouseButtonEventArgs e)
        {
            txtPassword.Focus();
        }

        private void txtPassword_TextChanged(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(txtPassword.Password) && txtPassword.Password.Length > 0)
            {
                textPassword.Visibility = Visibility.Collapsed;
            }
            else
            {
                textPassword.Visibility = Visibility.Visible;
            }
        }

        private void textConfirmPassword_MouseDown(object sender, MouseButtonEventArgs e)
        {
            txtConfirmPassword.Focus();
        }

        private void txtConfirmPassword_TextChanged(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(txtConfirmPassword.Password) && txtConfirmPassword.Password.Length > 0)
            {
                textConfirmPassword.Visibility = Visibility.Collapsed;
            }
            else
            {
                textConfirmPassword.Visibility = Visibility.Visible;
            }
        }


        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }

        #region 正则密码校验
        private static readonly Regex PasswordRegex = new Regex(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,20}$", RegexOptions.Compiled);

        public static bool IsPasswordValid(string password)
        {
            return PasswordRegex.IsMatch(password);
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
        #endregion

        private void Register_Click(object sender, RoutedEventArgs e)
        {
            // 前端验证已通过，直接处理注册业务逻辑
            if (!ValidationViewModel.IsValid)
            {
                return; // 按钮应该已被禁用，但这里加一层保护
            }

            RegisterViewModel vm = new RegisterViewModel();
            vm.Password = ValidationViewModel.Password;
            vm.Account = ValidationViewModel.Account;
            vm.ConfirmPassword = ValidationViewModel.ConfirmPassword;
            var user = vm.Adapt<SysUser>();
            var result = _userService.RegisterUser(user);
            
            if (result.State)
            {
                CountdownText.Visibility = Visibility.Visible;
                _timer.Interval = TimeSpan.FromSeconds(1);
                _timer.Tick += Timer_Tick;
                _timer.Start();
            }
            else
            {
                CommonExtension.ShowDialog(result.Message, _container, 0, "RegisterDialog");
            }
        }
        private void ReturnLogin_Click(object sender, RoutedEventArgs e) {

            this.Hide();
            this.Close();
            var loginPage = _container.Resolve<Login>();
            loginPage.Show();

        }
        
        private void close_MouseUp(object sender, MouseButtonEventArgs e)
        {
            this.Close();
        }

    }
    }
