using System.Collections.ObjectModel;
using System.Windows.Input;
using SupportToolApp.Config;
using SupportToolApp.Services;

namespace SupportToolApp.ViewModels;

public class SettingsViewModel : BaseViewModel
{
    private readonly IAuthService _auth;

    private string _domain, _tenantId, _clientId, _adminUpn, _teamViewerPath, _sccmConsolePath;
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

    public SettingsViewModel(IAuthService auth)
    {
        _auth = auth;

        var cfg = AppConfig.Instance;
        _domain = cfg.Domain;
        _tenantId = cfg.TenantId;
        _clientId = cfg.ClientId;
        _adminUpn = cfg.AdminUpn;
        _teamViewerPath = cfg.TeamViewerPath;
        _sccmConsolePath = cfg.SccmConsolePath;
        _useMockData = cfg.UseMockData;
        foreach (var s in cfg.Servers) Servers.Add(s);

        SaveCommand = new Command(() =>
        {
            var c = AppConfig.Instance;
            c.Domain = Domain;
            c.TenantId = TenantId;
            c.ClientId = ClientId;
            c.AdminUpn = AdminUpn;
            c.TeamViewerPath = TeamViewerPath;
            c.SccmConsolePath = SccmConsolePath;
            c.UseMockData = UseMockData;
            c.Servers = [.. Servers];
            c.Save();
            StatusMessage = RestartRequired
                ? "Settings saved — restart the app to apply data source change."
                : "Settings saved.";
        });

        AddServerCommand = new Command(() =>
            Servers.Add(new ServerEntry { Name = "New Server", Host = "server.contoso.com" }));

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
