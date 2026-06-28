using System.Collections.ObjectModel;
using System.Windows.Input;
using SupportToolApp.Config;
using SupportToolApp.Helpers;
using SupportToolApp.Services;

namespace SupportToolApp.ViewModels;

public class SettingsViewModel : BaseViewModel
{
    private readonly IAuthService _auth;
    private readonly CredentialService _creds;

    private string _domain, _tenantId, _clientId, _adminUpn, _teamViewerPath, _sccmConsolePath;
    private string _deployToolName, _deployToolUrl;
    private string _ql1Name, _ql1Url, _ql2Name, _ql2Url, _ql3Name, _ql3Url, _ql4Name, _ql4Url;
    private string _folderName, _localFolder, _libraryName, _libraryUrl;
    private string _logoPath, _logoInitials, _logoColor;
    private string _newServerName = "", _newServerHost = "";
    private bool _useMockData;
    private string _statusMessage = "";
    private bool _hasStatus;
    private bool _isSigningIn;
    private bool _restartRequired;

    public string Domain { get => _domain; set => SetProperty(ref _domain, value); }
    public string TenantId { get => _tenantId; set => SetProperty(ref _tenantId, value); }
    public string ClientId { get => _clientId; set => SetProperty(ref _clientId, value); }
    public string AdminUpn { get => _adminUpn; set => SetProperty(ref _adminUpn, value); }
    public string TeamViewerPath { get => _teamViewerPath; set => SetProperty(ref _teamViewerPath, value); }
    public string SccmConsolePath { get => _sccmConsolePath; set => SetProperty(ref _sccmConsolePath, value); }
    public string DeployToolName { get => _deployToolName; set => SetProperty(ref _deployToolName, value); }
    public string DeployToolUrl  { get => _deployToolUrl;  set => SetProperty(ref _deployToolUrl, value); }

    public string Ql1Name { get => _ql1Name; set => SetProperty(ref _ql1Name, value); }
    public string Ql1Url  { get => _ql1Url;  set => SetProperty(ref _ql1Url, value); }
    public string Ql2Name { get => _ql2Name; set => SetProperty(ref _ql2Name, value); }
    public string Ql2Url  { get => _ql2Url;  set => SetProperty(ref _ql2Url, value); }
    public string Ql3Name { get => _ql3Name; set => SetProperty(ref _ql3Name, value); }
    public string Ql3Url  { get => _ql3Url;  set => SetProperty(ref _ql3Url, value); }
    public string Ql4Name { get => _ql4Name; set => SetProperty(ref _ql4Name, value); }
    public string Ql4Url  { get => _ql4Url;  set => SetProperty(ref _ql4Url, value); }

    public string FolderName  { get => _folderName;  set => SetProperty(ref _folderName, value); }
    public string LocalFolder { get => _localFolder; set => SetProperty(ref _localFolder, value); }
    public string LibraryName { get => _libraryName; set => SetProperty(ref _libraryName, value); }
    public string LibraryUrl  { get => _libraryUrl;  set => SetProperty(ref _libraryUrl, value); }

    public string LogoPath     { get => _logoPath;     set => SetProperty(ref _logoPath, value); }
    public string LogoInitials { get => _logoInitials; set => SetProperty(ref _logoInitials, value); }
    public string LogoColor    { get => _logoColor;    set => SetProperty(ref _logoColor, value); }

    public string NewServerName { get => _newServerName; set => SetProperty(ref _newServerName, value); }
    public string NewServerHost { get => _newServerHost; set => SetProperty(ref _newServerHost, value); }
    public bool IsSigningIn { get => _isSigningIn; set => SetProperty(ref _isSigningIn, value); }
    public bool RestartRequired { get => _restartRequired; set => SetProperty(ref _restartRequired, value); }

    public bool UseMockData
    {
        get => _useMockData;
        set
        {
            if (SetProperty(ref _useMockData, value))
                RestartRequired = true;
        }
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set { SetProperty(ref _statusMessage, value); HasStatus = !string.IsNullOrEmpty(value); }
    }
    public bool HasStatus { get => _hasStatus; set => SetProperty(ref _hasStatus, value); }

    // Auth status — read from service
    public bool IsSignedIn => _auth.IsSignedIn;
    public bool IsNotSignedIn => !_auth.IsSignedIn;
    public string SignedInUser => _auth.SignedInUser ?? "";
    public string AuthStatusText => _auth.IsSignedIn
        ? $"Signed in as {_auth.SignedInUser}"
        : "Not signed in";
    public string AuthStatusColor => _auth.IsSignedIn ? "#228B22" : "#888888";

    // Can sign in only when Tenant ID + Client ID are filled
    public bool CanSignIn => !string.IsNullOrWhiteSpace(TenantId)
                          && !string.IsNullOrWhiteSpace(ClientId)
                          && !IsSigningIn
                          && !_auth.IsSignedIn;
    public bool CanSignOut => _auth.IsSignedIn && !IsSigningIn;

    public ObservableCollection<ServerEntry> Servers { get; } = [];

    public ICommand SaveCommand { get; }
    public ICommand AddServerCommand { get; }
    public ICommand RemoveServerCommand { get; }
    public ICommand SignInCommand { get; }
    public ICommand SignOutCommand { get; }

    public ICommand BrowseLogoCommand { get; }
    public ICommand BrowseFolderCommand { get; }

    public SettingsViewModel(IAuthService auth, CredentialService creds)
    {
        _auth = auth;
        _creds = creds;

        var cfg = AppConfig.Instance;
        _domain = cfg.Domain;
        _tenantId = cfg.TenantId;
        _clientId = cfg.ClientId;
        _adminUpn = cfg.AdminUpn;
        _teamViewerPath = cfg.TeamViewerPath;
        _sccmConsolePath = cfg.SccmConsolePath;
        _deployToolName = cfg.DeployToolName;
        _deployToolUrl  = cfg.DeployToolUrl;
        _useMockData = cfg.UseMockData;
        foreach (var s in cfg.Servers) Servers.Add(s);

        _ql1Name = creds.QuickLink1Name; _ql1Url = creds.QuickLink1Url;
        _ql2Name = creds.QuickLink2Name; _ql2Url = creds.QuickLink2Url;
        _ql3Name = creds.QuickLink3Name; _ql3Url = creds.QuickLink3Url;
        _ql4Name = creds.QuickLink4Name; _ql4Url = creds.QuickLink4Url;
        _folderName  = creds.FolderName;  _localFolder = creds.LocalFolder;
        _libraryName = creds.LibraryName; _libraryUrl  = creds.LibraryUrl;
        _logoPath = creds.LogoPath; _logoInitials = creds.LogoInitials; _logoColor = creds.LogoColor;

        BrowseLogoCommand = new Command(async () =>
        {
            var path = await AppFileDialog.OpenFileAsync("Select a logo image", ".png", ".jpg", ".jpeg", ".bmp", ".gif");
            if (!string.IsNullOrEmpty(path)) LogoPath = path;
        });

        BrowseFolderCommand = new Command(async () =>
        {
            var folder = await AppFileDialog.BrowseFolderAsync("Select local folder");
            if (!string.IsNullOrEmpty(folder)) LocalFolder = folder;
        });

        SaveCommand = new Command(() =>
        {
            var c = AppConfig.Instance;
            c.Domain = Domain;
            c.TenantId = TenantId;
            c.ClientId = ClientId;
            c.AdminUpn = AdminUpn;
            c.TeamViewerPath = TeamViewerPath;
            c.SccmConsolePath = SccmConsolePath;
            c.DeployToolName = string.IsNullOrWhiteSpace(DeployToolName) ? "Deployment Tool" : DeployToolName;
            c.DeployToolUrl  = DeployToolUrl ?? "";
            c.UseMockData = UseMockData;
            c.Servers = [.. Servers];
            c.Save();

            _creds.SetQuickLinks(Ql1Name, Ql1Url, Ql2Name, Ql2Url, Ql3Name, Ql3Url, Ql4Name, Ql4Url);
            _creds.FolderName   = FolderName;
            _creds.LocalFolder  = LocalFolder;
            _creds.LibraryName  = LibraryName;
            _creds.LibraryUrl   = string.IsNullOrWhiteSpace(LibraryUrl) ? "https://en.wikipedia.org/wiki/Main_Page" : LibraryUrl;
            _creds.LogoPath     = LogoPath;
            _creds.LogoInitials = string.IsNullOrWhiteSpace(LogoInitials) ? "IT" : LogoInitials;
            _creds.LogoColor    = string.IsNullOrWhiteSpace(LogoColor) ? "#0078D4" : LogoColor;

            StatusMessage = RestartRequired
                ? "Settings saved — restart the app to apply data source change."
                : "Settings saved.";
        });

        AddServerCommand = new Command(() =>
        {
            if (string.IsNullOrWhiteSpace(NewServerName) || string.IsNullOrWhiteSpace(NewServerHost)) return;
            Servers.Add(new ServerEntry { Name = NewServerName.Trim(), Host = NewServerHost.Trim() });
            NewServerName = "";
            NewServerHost = "";
        });

        RemoveServerCommand = new Command<ServerEntry>(s => Servers.Remove(s));

        SignInCommand = new Command(async () =>
        {
            if (!CanSignIn) return;

            // Save Tenant/Client IDs first so GraphAuthService picks them up
            AppConfig.Instance.TenantId = TenantId;
            AppConfig.Instance.ClientId = ClientId;
            AppConfig.Instance.Save();

            IsSigningIn = true;
            NotifyAuthChanged();

            var success = await _auth.SignInAsync();

            IsSigningIn = false;
            NotifyAuthChanged();

            StatusMessage = success
                ? $"Signed in as {_auth.SignedInUser}"
                : "Sign-in failed. Check your Tenant ID and Client ID, then try again.";
        });

        SignOutCommand = new Command(async () =>
        {
            if (!CanSignOut) return;
            IsSigningIn = true;
            NotifyAuthChanged();

            await _auth.SignOutAsync();

            IsSigningIn = false;
            NotifyAuthChanged();
            StatusMessage = "Signed out.";
        });
    }

    private void NotifyAuthChanged()
    {
        OnPropertyChanged(nameof(IsSignedIn));
        OnPropertyChanged(nameof(IsNotSignedIn));
        OnPropertyChanged(nameof(SignedInUser));
        OnPropertyChanged(nameof(AuthStatusText));
        OnPropertyChanged(nameof(AuthStatusColor));
        OnPropertyChanged(nameof(CanSignIn));
        OnPropertyChanged(nameof(CanSignOut));
    }
}
