namespace SupportToolApp.Pages;

public partial class SplashPage : ContentPage
{
    public SplashPage()
    {
        InitializeComponent();
    }

    private async void OnGetStarted(object? sender, EventArgs e)
    {
        Preferences.Set("show_splash", ChkShowAgain.IsChecked);
        await Navigation.PopModalAsync();
    }
}
