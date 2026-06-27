using SupportToolApp.ViewModels;

namespace SupportToolApp.Pages.UserManager;

public partial class UserManagerPage : ContentPage
{
    public UserManagerPage(UserManagerViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
