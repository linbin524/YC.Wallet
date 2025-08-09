using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using YC.WalletApp.Views;

namespace YC.WalletApp.ViewModels;

public class TabsViewModel : ViewModelBase
{

    private ObservableCollection<CustomTab> _customTabs;
    public ObservableCollection<CustomTab> CustomTabs
    {
        get { return _customTabs; }
        set { SetProperty(ref _customTabs, value); }
    }

    private CustomTab? _selectedTab;
    public CustomTab? SelectedTab {

        get { return _selectedTab; }
        set { SetProperty(ref _selectedTab, value); }
    }

    public string? VeryLongText { get; set; } = @"";

    public AnotherCommandImplementation closeCommand;

    public TabsViewModel()
    {
         closeCommand = new AnotherCommandImplementation(_ =>
        {
            if (SelectedTab is { } selectedTab)
                CustomTabs?.Remove(selectedTab);
        });

        CustomTabs = new ObservableCollection<CustomTab>();
       
    }

}

public partial class CustomTab : ObservableObject
{
    public ICommand CloseCommand { get; }

    public CustomTab(ICommand closeCommand) => CloseCommand = closeCommand;

    [ObservableProperty]
    private string? _customTabId;

    [ObservableProperty]
    private string? _customHeader;

    [ObservableProperty]
    private UserControl? _customContent;

}
