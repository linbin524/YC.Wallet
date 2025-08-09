using YC.WalletApp.ViewModels;

namespace MaterialDesign3Demo.Domain;

public class ShowDialogContentModel : ViewModelBase
{
    public ShowDialogContentModel() {

        SetType(0);
    }

    private string? _content;

    public string? Content
    {
        get => _content;
        set => SetProperty(ref _content, value);
    }

    private int _width;

    public int Width
    {
        get => _width;
        set => SetProperty(ref _width, value);
    }

    private int _height;

    public int Height
    {
        get => _height;
        set => SetProperty(ref _height, value);
    }

    public void SetType(int type) {
        switch (type) { 
            case 0: Width = 300; Height = 200; break;
            case 1: Width = 200; Height = 150; break;
            case 2: Width = 500; Height = 300; break;
            default: Width = 150; Height = 50; break;
        }
    }
}
