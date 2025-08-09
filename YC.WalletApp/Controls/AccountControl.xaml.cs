using HandyControl.Tools.Extension;
using Prism.Ioc;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using YC.WalletApp.ViewModels;
using YC.WalletApp.Views;

namespace YC.WalletApp.Controls
{
    /// <summary>
    /// AccountControl.xaml 的交互逻辑
    /// </summary>
    public partial class AccountControl : UserControl
    {
        IContainerExtension _container;
        public AccountControl(IContainerExtension container)
        {
            DataContext = new ListsAndGridsViewModel();
            _container=container;
            Loaded += AccountControl_Loaded;
            InitializeComponent();
        }

        private void AccountControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.UpdateLayout();
            MessageBox.Show("我打开了");
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
           var addAccount= _container.Resolve<AddAccountPage>();
            addAccount.Owner = this.GetParentWindow();
            //addAccount.Show();
            if (addAccount.ShowDialog() == true)
            {
                MessageBox.Show("我触发了");
            }
        }

        private Window GetParentWindow()
        {
            DependencyObject parent = VisualTreeHelper.GetParent(this);
            while (parent != null && !(parent is Window))
            {
                parent = VisualTreeHelper.GetParent(parent);
            }
            return parent as Window;
        }
        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is ListsAndGridsViewModel viewModel)
            {
                viewModel.Password = PasswordBox.Password;
            }
        }

        private void Submit_Click(object sender, RoutedEventArgs e)
        {
            var viewModel = DataContext as ListsAndGridsViewModel;
            if (viewModel != null && viewModel.IsValid)
            {
                MessageBox.Show("表单验证通过，提交成功！");
            }
        }
    }
}
