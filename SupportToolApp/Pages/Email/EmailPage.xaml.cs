using SupportToolApp.ViewModels;

namespace SupportToolApp.Pages.Email;

public partial class EmailPage : ContentPage
{
    public EmailPage(EmailViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
