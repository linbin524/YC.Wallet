using ApplicationService.IService;
using Prism.Ioc;
using YC.WalletApp.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using YC.ApplicationService.IService;
using YC.ApplicationService.Service;
using YC.Model;
using YC.WalletApp.Domain;
using System.Windows.Interop;
using YC.WalletApp.ViewModels;
using YC.ApplicationService;
using CommunityToolkit.Mvvm.Input;
using YC.WalletApp.Extension;
using System.Globalization;
using YC.Model.Entity;
using Newtonsoft.Json;
using System.Net.Http;
using MySqlX.XDevAPI.Common;
using ControlzEx.Standard;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth;
using System.Net;

namespace YC.WalletApp.Views
{
    /// <summary>
    /// Login.xaml 的交互逻辑
    /// </summary>
    public partial class Login : Window
    {
        private IContainerExtension _container;
        private IUserService _userService;
        private LoginViewModel _dataContext;
        private GoogleOAuthExtension _googleOAuthService;

        public Login(IContainerExtension container, IUserService userService)
        {
            InitializeComponent();
            _container = container;
            _userService= userService;
            _googleOAuthService= _container.Resolve<GoogleOAuthExtension>();    
            txtEmail.Text = "admin123";
            txtPassword.Password = "123456";
            _userService.InitDefalutLoginUser(new SysUser() { Account= txtEmail.Text, Password= txtPassword.Password });

            _dataContext = container.Resolve<LoginViewModel>();
            this.DataContext = _dataContext;
           
            //cbb_changeLanguage.ItemsSource = DefaultConfig.SupportedLanguages;
            sbtn_1.Content = DefaultConfig.LocalLanguage;
            //dataContext.Language = DefaultConfig.LanguageConfig;
        }


        private void lb_changeLanguage_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListBox listBox = sender as ListBox;
            if (listBox == null) return;

            var language = listBox.SelectedItem as SupportedLanguage;
            sbtn_1.Content = language.Name;
            if (language == null) return;
            LoginViewModel.OnGenreSelected(language.Name);
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

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }

        private void login_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(txtEmail.Text) && !string.IsNullOrEmpty(txtPassword.Password))
            {
                SysUser sysUser= new SysUser();
                sysUser.Account = txtEmail.Text.Trim();
                sysUser.Password = txtPassword.Password.Trim();
              var result=  _userService.Login(sysUser);
                if (result.State)
                {
                    DefaultConfig.CurrentLoginUser = result.Data;
                    var mainPage = _container.Resolve<MainWindow>();
                    mainPage.Show();
                    this.Hide();
                    this.Close();
                }
                else {
                    CommonExtension.ShowDialog(result.Message);
                    //HandyControl.Controls.MessageBox.Error();
                }

            }
        }

        private void close_MouseUp(object sender, MouseButtonEventArgs e)
        {
            this.Close();
        }

        private void Register_Click(object sender, RoutedEventArgs e)
        {

            this.Hide();
            //this.Close();
           var registerPage= _container.Resolve<Register>();
            registerPage.Show();
        }

        private  void GoogleOAuthLogin_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _googleOAuthService._authService.StartLogin();
                UserCredential userInfo;
                _googleOAuthService._authService.LoginSuccess += tempUser => Dispatcher.Invoke(() => {
                    // 登录成功，处理用户信息
                    var token = _googleOAuthService._authService.CurrentTokens.access_token;
                    var userInfo = tempUser;
                    SysUser sysUser = new SysUser();
                    sysUser.Account = userInfo.email;
                    sysUser.Password = "";
                    var user = SQLiteUtils._freesql.Select<SysUser>().Where(x => x.GoogleEmail == userInfo.email && x.GoogleName == userInfo.name).First();
                    if (user != null)
                    {
                        user.GoogleToken = token;
                        var updateCount = SQLiteUtils._freesql.Update<SysUser>(user).ExecuteAffrows();
                        DefaultConfig.CurrentLoginUser = user;

                    }
                    else
                    {
                        var tempUser = new SysUser()
                        {
                            Name = userInfo.name,
                            GoogleName = userInfo.name,
                            IsSupportGoogleLogin = true,
                            Account = userInfo.email,
                            GoogleEmail = userInfo.email,
                            Avatar = userInfo.picture,
                            CreationTime = DateTime.Now,
                            GoogleToken = token
                        };
                        int createdCount = SQLiteUtils._freesql.Insert<SysUser>(tempUser).ExecuteAffrows();
                        if (createdCount > 0)
                        {
                            user = SQLiteUtils._freesql.Select<SysUser>().Where(x => x.GoogleEmail == userInfo.email && x.GoogleName == userInfo.name).First();
                        }
                        else
                        {
                            CommonExtension.ShowDialog("Save Google Info Failed");
                            return;
                        }
                        DefaultConfig.CurrentLoginUser = user;
                    }
                    var mainPage = _container.Resolve<MainWindow>();
                    mainPage.Show();
                    this.Hide();
                    this.Close();
                }
           );

                _googleOAuthService._authService.LoginFailed += ex => Dispatcher.Invoke(() =>
                   CommonExtension.ShowDialog($"Error: {ex.Message}"));

                _googleOAuthService._authService.LoginCanceled += () => Dispatcher.Invoke(() =>
                   CommonExtension.ShowDialog("Google Login canceled"));
               

                //UserInfoText.Text = $"Logged in as:\n{userInfo.Email}\n{userInfo.Name}";
                //UserAvatar.Source = new BitmapImage(new Uri(userInfo.Picture));
            }
            catch (Exception ex)
            {
                CommonExtension.ShowDialog($"Error: {ex.Message}");
            }
        }

        

        private async void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await _googleOAuthService._authService.RevokeTokenAsync();
                MessageBox.Show("Logged out successfully");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Logout failed: {ex.Message}");
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            _googleOAuthService._authService.Dispose();
            base.OnClosed(e);
        }

    }
    }
