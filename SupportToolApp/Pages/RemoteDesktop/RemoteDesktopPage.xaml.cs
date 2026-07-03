using SupportToolApp.Services;

namespace SupportToolApp.Pages.RemoteDesktop;

public partial class RemoteDesktopPage : ContentPage
{
    private readonly CredentialService _creds;

    public RemoteDesktopPage(CredentialService creds)
    {
        InitializeComponent();
        _creds = creds;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        // Prefill and auto-load the saved Guacamole URL if we have one
        if (_creds.HasRemoteDesktop)
        {
            UrlEntry.Text = _creds.RemoteDesktopUrl;
            Navigate(_creds.RemoteDesktopUrl);
        }
    }

    private void OnGo(object? sender, EventArgs e) => Navigate(UrlEntry.Text);

    private async void OnSave(object? sender, EventArgs e)
    {
        var url = Normalize(UrlEntry.Text);
        if (url == null) { await DisplayAlert("Invalid URL", "Enter a valid http(s) URL first.", "OK"); return; }
        _creds.RemoteDesktopUrl = url;
        UrlEntry.Text = url;
        Navigate(url);
        await DisplayAlert("Saved", "Remote desktop URL saved.", "OK");
    }

    private async void Navigate(string? raw)
    {
        var url = Normalize(raw);
        if (url == null) { await DisplayAlert("Invalid URL", "Enter a valid http(s) URL.", "OK"); return; }
        EmptyState.IsVisible = false;
        Browser.Source = url;
    }

    // Returns a normalized absolute http(s) URL, or null if it can't be made valid.
    private static string? Normalize(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return null;
        raw = raw.Trim();
        if (!raw.Contains("://")) raw = "http://" + raw;   // Guacamole is commonly plain http on LAN
        return Uri.TryCreate(raw, UriKind.Absolute, out var uri)
            && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps)
            ? uri.ToString()
            : null;
    }
}
