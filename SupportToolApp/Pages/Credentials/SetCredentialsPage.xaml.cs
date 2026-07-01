using SupportToolApp.Services;

namespace SupportToolApp.Pages.Credentials;

public partial class SetCredentialsPage : ContentPage
{
    private readonly CredentialService _creds;

    public SetCredentialsPage(CredentialService creds)
    {
        InitializeComponent();
        _creds = creds;
        EntUserDomain.Text    = creds.UserDomain;
        EntUserUsername.Text  = creds.UserUsername;
        EntUserPassword.Text  = creds.UserPassword;
        EntO365Username.Text  = creds.O365Username;
        EntO365Password.Text  = creds.O365Password;
        EntGoogleUsername.Text = creds.GoogleUsername;
        EntGooglePassword.Text = creds.GooglePassword;
    }

    private void OnSave(object? sender, EventArgs e)
    {
        _creds.SetUserCred(
            string.IsNullOrWhiteSpace(EntUserDomain.Text) ? "." : EntUserDomain.Text.Trim(),
            EntUserUsername.Text?.Trim() ?? "",
            EntUserPassword.Text ?? "");
        _creds.SetO365Cred(
            EntO365Username.Text?.Trim() ?? "",
            EntO365Password.Text ?? "");
        _creds.SetGoogleCred(
            EntGoogleUsername.Text?.Trim() ?? "",
            EntGooglePassword.Text ?? "");

        LblStatus.Text = "Credentials saved. All tools will use these values.";
        LblStatus.IsVisible = true;
    }

    private void OnClear(object? sender, EventArgs e)
    {
        EntUserDomain.Text = "."; EntUserUsername.Text = ""; EntUserPassword.Text = "";
        EntO365Username.Text = ""; EntO365Password.Text = "";
        EntGoogleUsername.Text = ""; EntGooglePassword.Text = "";
        _creds.SetUserCred(".", "", "");
        _creds.SetO365Cred("", "");
        _creds.SetGoogleCred("", "");
        LblStatus.Text = "Credentials cleared.";
        LblStatus.IsVisible = true;
    }
}
