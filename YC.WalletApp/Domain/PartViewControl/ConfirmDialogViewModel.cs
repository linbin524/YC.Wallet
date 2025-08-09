using MaterialDesign3Demo.Domain;
using MaterialDesignThemes.Wpf;
using Prism.Ioc;
using Prism.Mvvm;
using System;
using System.Diagnostics;
using System.Windows.Input;
using YC.WalletApp.ViewModels;

namespace YC.WalletApp.Domain.PartViewControl
{
    public class ConfirmDialogViewModel : BindableBase
    {
        public ICommand RunDialogCommand => new AnotherCommandImplementation(ExecuteRunDialog);
        public ICommand ConfirmCommand => new AnotherCommandImplementation(ExecuteConfirm);
        public ICommand CancelCommand => new AnotherCommandImplementation(ExecuteCancel);
        
        private string _content;
        public string Content { get => _content; set => SetProperty(ref _content, value); }
        private string _title;
        public string Title { get => _title; set => SetProperty(ref _title, value); }
        
        private string _dialogIdentifier = "SecondDialog";
        public string DialogIdentifier { get => _dialogIdentifier; set => SetProperty(ref _dialogIdentifier, value); }
        
        public IContainerExtension _container;
        public Action<bool> Callback { get; set; }
        
        public ConfirmDialogViewModel() { }
        
        private async void ExecuteRunDialog(object? _)
        {
            ConfirmDialogContentModel vm;
            if (_container != null)
            {
                vm = _container.Resolve<ConfirmDialogContentModel>();
            }
            else
            {
                vm = new ConfirmDialogContentModel();
            }
            vm.Content = Content;
            vm.Callback = Callback;
            // 设置标题
            vm.Title = Title;
            
            //let's set up a little MVVM, cos that's what the cool kids are doing:
            var view = new ConfirmDialog
            {
                DataContext = vm
            };

            //show the dialog
            var result = await DialogHost.Show(view, DialogIdentifier, ClosingEventHandler);

            //check the result...
            Debug.WriteLine("Dialog was closed, the CommandParameter used to close it was: " + (result ?? "NULL"));
            
            // 根据对话框结果调用回调
            if (result is bool confirmed)
            {
                Callback?.Invoke(confirmed);
            }
        }

        private void ClosingEventHandler(object sender, DialogClosingEventArgs eventArgs)
            => Debug.WriteLine("You can intercept the closing event, and cancel here.");
            
        private void ExecuteConfirm(object? _)
        {
            Callback?.Invoke(true);
        }
        
        private void ExecuteCancel(object? _)
        {
            Callback?.Invoke(false);
        }
    }
} 