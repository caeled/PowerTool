using SupportToolApp.ViewModels;

namespace SupportToolApp.Pages.Spam;

public partial class SpamPage : ContentPage
{
    public SpamPage(SpamViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
