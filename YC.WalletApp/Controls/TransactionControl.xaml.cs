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
using YC.WalletApp.Extension;

namespace YC.WalletApp.Controls
{
    /// <summary>
    /// TransacationControl.xaml 的交互逻辑
    /// </summary>
    public partial class TransactionControl : UserControl
    {
        private IContainerExtension _container;
        public TransactionControl(IContainerExtension container)
        {
            _container= container;
           var vm= _container.Resolve<TransactionViewModel>();
            this.DataContext = vm;
            InitializeComponent();
        }

        private void OnCopyText(object sender, MouseButtonEventArgs e)
        {
            if (sender is TextBlock tb)
            {
                // 仅获取值部分（最后一个Run的内容）
                var value = tb.Inlines.OfType<Run>().LastOrDefault()?.Text;

                if (!string.IsNullOrEmpty(value))
                {
                    try
                    {
                        Clipboard.SetText(value);
                        NotificationBar.MessageQueue.Enqueue(
                            $"{LanguageManager.Instance["CopySuccess"]}{value}",
                            LanguageManager.Instance["Close"],
                            () => { /* 可选回调 */ });
                    }
                    catch
                    {
                        NotificationBar.MessageQueue.Enqueue(LanguageManager.Instance["CopyFailed"]);
                    }
                }
            }
        }

       
    }
}
