using SupportToolApp.Services;

namespace SupportToolApp.Pages.Credentials;

public partial class SetCredentialsPage : ContentPage
{
    private readonly CredentialService _creds;

    public SetCredentialsPage(CredentialService creds)
    {
        InitializeComponent();
        _creds = creds;
        // Pre-fill from service
        EntUserDomain.Text   = creds.UserDomain;
        EntUserUsername.Text = creds.UserUsername;
        EntUserPassword.Text = creds.UserPassword;
        EntO365Username.Text    = creds.O365Username;
        EntO365Password.Text    = creds.O365Password;
        EntGoogleUsername.Text  = creds.GoogleUsername;
        EntGooglePassword.Text  = creds.GooglePassword;
        EntQl1Name.Text = creds.QuickLink1Name; EntQl1Url.Text = creds.QuickLink1Url;
        EntQl2Name.Text = creds.QuickLink2Name; EntQl2Url.Text = creds.QuickLink2Url;
        EntQl3Name.Text = creds.QuickLink3Name; EntQl3Url.Text = creds.QuickLink3Url;
        EntQl4Name.Text = creds.QuickLink4Name; EntQl4Url.Text = creds.QuickLink4Url;
        EntFolderName.Text  = creds.FolderName;
        EntLocalFolder.Text = creds.LocalFolder;
        EntLibraryName.Text = creds.LibraryName;
        EntLibraryUrl.Text  = creds.LibraryUrl;
        EntLogoPath.Text     = creds.LogoPath;
        EntLogoInitials.Text = creds.LogoInitials;
        EntLogoColor.Text    = creds.LogoColor;
    }

    private async void OnBrowseLogo(object sender, EventArgs e)
    {
        var result = await FilePicker.Default.PickAsync(new PickOptions
        {
            PickerTitle = "Select a logo image (PNG, JPG)",
            FileTypes   = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
            {
                { DevicePlatform.WinUI, new[] { ".png", ".jpg", ".jpeg", ".bmp", ".gif" } },
                { DevicePlatform.macOS, new[] { "png", "jpg", "jpeg", "bmp", "gif" } },
            })
        });
        if (result != null) EntLogoPath.Text = result.FullPath;
    }

    private async void OnBrowseFolder(object sender, EventArgs e)
    {
#if WINDOWS
        var picker = new Windows.Storage.Pickers.FolderPicker();
        picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.Desktop;
        picker.FileTypeFilter.Add("*");
        // Wire picker to the app window handle (required on Windows)
        var nativeWindow = Application.Current!.Windows[0].Handler?.PlatformView as MauiWinUIWindow;
        var hwnd = nativeWindow?.WindowHandle ?? IntPtr.Zero;
        if (hwnd != IntPtr.Zero) WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);
        var folder = await picker.PickSingleFolderAsync();
        if (folder != null)
            EntLocalFolder.Text = folder.Path;
#else
        // Fallback: pick any file, capture its directory
        var result = await FilePicker.Default.PickAsync(new PickOptions
        {
            PickerTitle = "Navigate to your folder and pick any file inside it"
        });
        if (result != null)
            EntLocalFolder.Text = Path.GetDirectoryName(result.FullPath) ?? result.FullPath;
#endif
    }

    private void OnSave(object sender, EventArgs e)
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
        _creds.SetQuickLinks(
            EntQl1Name.Text?.Trim() ?? "", EntQl1Url.Text?.Trim() ?? "",
            EntQl2Name.Text?.Trim() ?? "", EntQl2Url.Text?.Trim() ?? "",
            EntQl3Name.Text?.Trim() ?? "", EntQl3Url.Text?.Trim() ?? "",
            EntQl4Name.Text?.Trim() ?? "", EntQl4Url.Text?.Trim() ?? "");
        _creds.FolderName  = EntFolderName.Text?.Trim() ?? "";
        _creds.LocalFolder = EntLocalFolder.Text?.Trim() ?? "";
        _creds.LibraryName = EntLibraryName.Text?.Trim() ?? "";
        _creds.LibraryUrl  = string.IsNullOrWhiteSpace(EntLibraryUrl.Text)
            ? "https://en.wikipedia.org/wiki/Main_Page"
            : EntLibraryUrl.Text.Trim();
        _creds.LogoPath     = EntLogoPath.Text?.Trim() ?? "";
        _creds.LogoInitials = string.IsNullOrWhiteSpace(EntLogoInitials.Text) ? "IT" : EntLogoInitials.Text.Trim();
        _creds.LogoColor    = string.IsNullOrWhiteSpace(EntLogoColor.Text) ? "#0078D4" : EntLogoColor.Text.Trim();

        LblStatus.Text = "Credentials saved. All tools will use these values.";
        LblStatus.IsVisible = true;
    }

    private void OnClear(object sender, EventArgs e)
    {
        EntUserDomain.Text = "."; EntUserUsername.Text = ""; EntUserPassword.Text = "";
        EntO365Username.Text = ""; EntO365Password.Text = "";
        EntGoogleUsername.Text = ""; EntGooglePassword.Text = "";
        _creds.SetUserCred(".", "", "");
        _creds.SetO365Cred("", "");
        _creds.SetGoogleCred("", "");
        EntQl1Name.Text = ""; EntQl1Url.Text = "";
        EntQl2Name.Text = ""; EntQl2Url.Text = "";
        EntQl3Name.Text = ""; EntQl3Url.Text = "";
        EntQl4Name.Text = ""; EntQl4Url.Text = "";
        EntFolderName.Text = ""; EntLocalFolder.Text = "";
        EntLibraryName.Text = ""; EntLibraryUrl.Text = "";
        _creds.SetQuickLinks("", "", "", "", "", "", "", "");
        _creds.FolderName = ""; _creds.LocalFolder = "";
        _creds.LibraryName = ""; _creds.LibraryUrl = "https://en.wikipedia.org/wiki/Main_Page";
        EntLogoPath.Text = ""; EntLogoInitials.Text = "IT"; EntLogoColor.Text = "#0078D4";
        _creds.LogoPath = ""; _creds.LogoInitials = "IT"; _creds.LogoColor = "#0078D4";
        LblStatus.Text = "Credentials cleared.";
        LblStatus.IsVisible = true;
    }
}
