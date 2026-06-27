using SupportToolApp.ViewModels;

namespace SupportToolApp.Pages.SystemInfo;

public partial class SystemInfoPage : ContentPage
{
    public SystemInfoPage(SystemInfoViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
