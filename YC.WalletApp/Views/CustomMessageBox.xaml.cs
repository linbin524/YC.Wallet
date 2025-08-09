using CommunityToolkit.Mvvm.Input;
using Prism.Mvvm;
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

namespace YC.WalletApp.Views
{
    /// <summary>
    /// UserControl1.xaml 的交互逻辑
    /// </summary>
    public partial class CustomMessageBox : Window
    {
        public CustomMessageBox(string title, string message)
        {
            InitializeComponent();
            DataContext = new CustomMessageBoxViewModel(title, message, this);
        }
    }

    public class CustomMessageBoxViewModel: BindableBase
    {
        public string Title { get; set; }
        public string Message { get; set; }
        public ICommand OkCommand { get; set; }
        public ICommand CancelCommand { get; set; }

        public CustomMessageBoxViewModel(string title, string message, Window window)
        {
            Title = title;
            Message = message;
            OkCommand = new RelayCommand(() =>
            {
                window.DialogResult = true;
                window.Close();
            });
            CancelCommand = new RelayCommand(() =>
            {
                window.DialogResult = false;
                window.Close();
            });
        }
    }

   
}
