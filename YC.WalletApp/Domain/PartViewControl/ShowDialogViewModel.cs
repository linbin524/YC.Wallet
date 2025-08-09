using MaterialDesign3Demo.Domain;
using MaterialDesignThemes.Wpf;
using MySqlX.XDevAPI.Common;
using Prism.Ioc;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using YC.WalletApp.ViewModels;

namespace YC.WalletApp.Domain.PartViewControl
{
   public class ShowDialogViewModel:BindableBase
    {
        public ICommand RunDialogCommand => new AnotherCommandImplementation(ExecuteRunDialog);
        private string _content;
        public string Content { get => _content; set => SetProperty(ref _content, value); }
        private int _type;
        public int Type { get => _type; set => SetProperty(ref _type, value); }
        private string _dialogIdentifier = "SecondDialog";
        public string DialogIdentifier { get => _dialogIdentifier; set => SetProperty(ref _dialogIdentifier, value); }
        public IContainerExtension _container;
        public ShowDialogViewModel() {
            
        }
        private async void ExecuteRunDialog(object? _)
        {
            ShowDialogContentModel vm;
            if (_container != null)
            {
                vm = _container.Resolve<ShowDialogContentModel>();
            }
            else
            {
                vm = new ShowDialogContentModel();
            }
            vm.Content = Content;
            if(Type!=0)vm.SetType(Type);//不是默认才执行设置类型
            //let's set up a little MVVM, cos that's what the cool kids are doing:
            var view = new ShowDialog
            {
                DataContext = vm
            };

            //show the dialog
            try
            {
                var result = await DialogHost.Show(view, DialogIdentifier, ClosingEventHandler);

                //check the result...
                Debug.WriteLine("Dialog was closed, the CommandParameter used to close it was: " + (result ?? "NULL"));
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());

            }
        }

        private void ClosingEventHandler(object sender, DialogClosingEventArgs eventArgs)
            => Debug.WriteLine("You can intercept the closing event, and cancel here.");
    }
}
