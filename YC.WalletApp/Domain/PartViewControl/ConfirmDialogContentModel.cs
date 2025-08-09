using YC.WalletApp.ViewModels;
using YC.ApplicationService;
using System;

namespace MaterialDesign3Demo.Domain
{
    public class ConfirmDialogContentModel : ShowDialogContentModel
    {
        public ConfirmDialogContentModel() : base()
        {
            SetType(0); // 使用默认尺寸
        }

        public Action<bool> Callback { get; set; }
        
        private string _title;
        public string Title { get => _title; set => SetProperty(ref _title, value); }
        
        public string ConfirmText => DefaultConfig.ContorlLanguage("Confirm");
        public string CancelText => DefaultConfig.ContorlLanguage("Cancel");

        public void Confirm()
        {
            Callback?.Invoke(true);
        }

        public void Cancel()
        {
            Callback?.Invoke(false);
        }
    }
} 