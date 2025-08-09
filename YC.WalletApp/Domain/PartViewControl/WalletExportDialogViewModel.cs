using System.Collections.Generic;
using System.Collections.ObjectModel;
using YC.WalletApp;
using YC.WalletApp.ViewModels;

namespace MaterialDesign3Demo.Domain;

public class WalletExportDialogViewModel : ViewModelBase
{
    private string? _name;

    public string? Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }
    private ObservableCollection<WalletDto> _wallets;
    public ObservableCollection<WalletDto> Wallets
    {
        get => _wallets;
        set => SetProperty(ref _wallets, value);
    }
}
