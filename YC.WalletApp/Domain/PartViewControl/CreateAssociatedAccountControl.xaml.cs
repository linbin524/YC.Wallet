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

namespace YC.WalletApp.Domain.PartViewControl
{
    /// <summary>
    /// UserControl1.xaml 的交互逻辑
    /// </summary>
    public partial class CreateAssociatedAccountControl : UserControl
    {
        public CreateAssociatedAccountControl()
        {
            InitializeComponent();
        }

        // 暴露的公共方法
        public void TriggerCloseCommand()
        {
            btn_Close.Command.Execute(btn_Close.CommandParameter);
            //if (btn_Close.Command != null && btn_Close.Command.CanExecute(btn_Close.CommandParameter))
            //{
                
            //}
        }
    }
}
