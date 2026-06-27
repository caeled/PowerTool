using SupportToolApp.ViewModels;

namespace SupportToolApp.Pages.RemoteAccess;

public partial class RemoteAccessPage : ContentPage
{
    public RemoteAccessPage(RemoteAccessViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
