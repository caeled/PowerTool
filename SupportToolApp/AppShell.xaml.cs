using System.Text.Json;
using SupportToolApp.Config;
using SupportToolApp.Helpers;
using SupportToolApp.Pages.Credentials;
using SupportToolApp.Pages.Settings;
using SupportToolApp.Services;

namespace SupportToolApp;

public partial class AppShell : Shell
{
    private readonly CredentialService _creds;

    public AppShell(CredentialService creds)
    {
        InitializeComponent();
        _creds = creds;

        // Register pages removed from flyout so GoToAsync still works
        Routing.RegisterRoute("Settings",       typeof(SettingsPage));
        Routing.RegisterRoute("SetCredentials", typeof(SetCredentialsPage));
        Routing.RegisterRoute("Help",           typeof(Pages.Help.HelpPage));
        Routing.RegisterRoute("NetworkMonitor", typeof(Pages.Monitor.NetworkMonitorPage));
        Routing.RegisterRoute("Search",         typeof(Pages.Search.SearchPage));
        Routing.RegisterRoute("Compare",        typeof(Pages.Compare.ComparePage));
        Routing.RegisterRoute("SoundCombine",   typeof(Pages.SoundCombine.SoundCombinePage));
        creds.LoadQuickLinks();
        creds.Changed += () => MainThread.BeginInvokeOnMainThread(RefreshQuickLinks);
        RefreshQuickLinks();
        Navigated += (_, _) => FlyoutIsPresented = false;
        Loaded += async (_, _) =>
        {
            await GoToAsync("//Tools");
            if (Preferences.Get("show_splash", true))
                await Navigation.PushModalAsync(new Pages.SplashPage());
        };
    }

    private void RefreshQuickLinks()
    {
        BtnFolder.Text      = string.IsNullOrWhiteSpace(_creds.FolderName) ? "Folder" : _creds.FolderName;
        BtnFolder.IsVisible = !string.IsNullOrWhiteSpace(_creds.LocalFolder);
        BtnLibrary.Text     = string.IsNullOrWhiteSpace(_creds.LibraryName) ? "Library" : _creds.LibraryName;
        SetQl(BtnQuickLink1, _creds.QuickLink1Name, _creds.QuickLink1Url);
        SetQl(BtnQuickLink2, _creds.QuickLink2Name, _creds.QuickLink2Url);
        SetQl(BtnQuickLink3, _creds.QuickLink3Name, _creds.QuickLink3Url);
        SetQl(BtnQuickLink4, _creds.QuickLink4Name, _creds.QuickLink4Url);
        RefreshLogo();
    }

    private void RefreshLogo()
    {
        if (!string.IsNullOrWhiteSpace(_creds.LogoPath) && File.Exists(_creds.LogoPath))
        {
            LogoImage.Source    = ImageSource.FromFile(_creds.LogoPath);
            LogoImage.IsVisible = true;
            LogoInitialsLabel.IsVisible = false;
        }
        else
        {
            LogoImage.IsVisible = false;
            LogoInitialsLabel.IsVisible = true;
            LogoInitialsLabel.Text = string.IsNullOrWhiteSpace(_creds.LogoInitials) ? "IT" : _creds.LogoInitials;
        }
        if (Color.TryParse(_creds.LogoColor, out var color))
            LogoBadge.BackgroundColor = color;
    }

    private static void SetQl(Button btn, string name, string url)
    {
        btn.IsVisible = !string.IsNullOrWhiteSpace(url);
        btn.Text      = string.IsNullOrWhiteSpace(name) ? url : name;
    }

    private async void OnFolderClicked(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_creds.LocalFolder)) return;
        try { await Launcher.OpenAsync(new Uri($"file:///{_creds.LocalFolder.Replace('\\', '/')}")); }
        catch { System.Diagnostics.Process.Start("explorer.exe", _creds.LocalFolder); }
    }

    private async void OnLibraryClicked(object? sender, EventArgs e)
        => await Launcher.OpenAsync(new Uri(string.IsNullOrWhiteSpace(_creds.LibraryUrl)
            ? "https://en.wikipedia.org/wiki/Main_Page" : _creds.LibraryUrl));

    private async void OnO365Clicked(object? sender, EventArgs e)
        => await Launcher.OpenAsync(new Uri("https://m365.cloud.microsoft/chat/"));

    private async void OnGoogleClicked(object? sender, EventArgs e)
        => await Launcher.OpenAsync(new Uri("https://docs.google.com/document/u/0/"));

    private async void OnQuickLink1Clicked(object? sender, EventArgs e)
    {
        if (!string.IsNullOrWhiteSpace(_creds.QuickLink1Url))
            await Launcher.OpenAsync(new Uri(_creds.QuickLink1Url));
    }

    private async void OnQuickLink2Clicked(object? sender, EventArgs e)
    {
        if (!string.IsNullOrWhiteSpace(_creds.QuickLink2Url))
            await Launcher.OpenAsync(new Uri(_creds.QuickLink2Url));
    }

    private async void OnQuickLink3Clicked(object? sender, EventArgs e)
    {
        if (!string.IsNullOrWhiteSpace(_creds.QuickLink3Url))
            await Launcher.OpenAsync(new Uri(_creds.QuickLink3Url));
    }

    private async void OnQuickLink4Clicked(object? sender, EventArgs e)
    {
        if (!string.IsNullOrWhiteSpace(_creds.QuickLink4Url))
            await Launcher.OpenAsync(new Uri(_creds.QuickLink4Url));
    }

    private async void OnSettingsClicked(object? sender, EventArgs e)
        => await Shell.Current.GoToAsync("Settings");

    private async void OnCredentialsClicked(object? sender, EventArgs e)
        => await Shell.Current.GoToAsync("SetCredentials");

    private async void OnHelpClicked(object? sender, EventArgs e)
        => await Shell.Current.GoToAsync("Help");

    private void OnExitClicked(object? sender, EventArgs e)
        => Application.Current?.Quit();

    private async void OnSaveConfig(object? sender, EventArgs e)
    {
        try
        {
            var json = ConfigExport.Capture(_creds).ToJson();
            var defaultName = $"SupportTool-Config-{DateTime.Now:yyyy-MM-dd}.json";

            var path = await AppFileDialog.SaveFileAsync("Save Configuration", defaultName, ".json");
            if (string.IsNullOrEmpty(path)) return;
            await File.WriteAllTextAsync(path, json);
            await DisplayAlert("Config Saved", $"Configuration saved to:\n{path}\n\nCredentials were not included.", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Save Failed", ex.Message, "OK");
        }
    }

    private async void OnLoadConfig(object? sender, EventArgs e)
    {
        try
        {
            var loadPath = await AppFileDialog.OpenFileAsync("Select a SupportTool config file", ".json");
            if (string.IsNullOrEmpty(loadPath)) return;
            var json = await File.ReadAllTextAsync(loadPath);
            var export = ConfigExport.FromJson(json);
            if (export == null)
            {
                await DisplayAlert("Load Failed", "The selected file is not a valid SupportTool config.", "OK");
                return;
            }

            var confirm = await DisplayAlert("Load Config",
                "This will overwrite your current settings (not credentials). Continue?", "Yes", "Cancel");
            if (!confirm) return;

            export.Apply(_creds);
            RefreshQuickLinks();

            await DisplayAlert("Config Loaded",
                "Settings loaded. Open the Settings page to review, then save to persist.", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Load Failed", ex.Message, "OK");
        }
    }
}
