using MySqlX.XDevAPI.Common;
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
using YC.WalletApp.Extension;
using YC.WalletApp.ViewModels;


namespace YC.WalletApp.Controls
{
    /// <summary>
    /// TokenDefControl.xaml 的交互逻辑
    /// </summary>
    public partial class TokenDefControl : UserControl
    {
        public TokenDefControl()
        {
            InitializeComponent();
            DataContext = new TokenDefViewModel();
            SetChange();
        }

      

        private void CanGoPrevious_Click(object sender, RoutedEventArgs e)
        {
            var vm = this.DataContext as TokenDefViewModel;
            vm.InitSelectAllCurrentPage();
            vm.PreviousPage();
            SetChange();
        }

        private void CanGoNextPage_Click(object sender, RoutedEventArgs e)
        {
            var vm = this.DataContext as TokenDefViewModel;
            vm.InitSelectAllCurrentPage();
            vm.NextPage();
            SetChange();
        }

        public void SetChange() {
            var vm = this.DataContext as TokenDefViewModel;
            btn_GoNext.IsEnabled = vm.CanGoNext;
            btn_GoPrevious.IsEnabled = vm.CanGoPrevious;
            
        }

        private void CanGoLastPage_Click(object sender, RoutedEventArgs e)
        {
            var vm = this.DataContext as TokenDefViewModel;
            vm.InitSelectAllCurrentPage();
            vm.GoToLastPage();
            SetChange();
        }

        private void CanGoToPage_Click(object sender, RoutedEventArgs e)
        {
            var vm = this.DataContext as TokenDefViewModel;
            vm.InitSelectAllCurrentPage();
            vm.GoToPage();
            SetChange();
        }

        private void InitializeTokenDef_Click(object sender, RoutedEventArgs e)
        {
           var vm = this.DataContext as TokenDefViewModel;
            vm.InitializeTokenDef();
        }

        #region 全选复选框事件处理
        // 注意：全选复选框现在通过双向绑定到 IsAllItemsSelected 属性来工作
        // 不再需要手动事件处理器
        #endregion


    }
}
