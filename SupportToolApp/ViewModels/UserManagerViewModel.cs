using System.Collections.ObjectModel;
using System.Windows.Input;
using SupportToolApp.Models;
using SupportToolApp.Services;

namespace SupportToolApp.ViewModels;

public class UserManagerViewModel : BaseViewModel
{
    private readonly IAdService _ad;
    private readonly IExchangeService _exchange;

    private ObservableCollection<AdUser> _users = [];
    private AdUser? _selectedUser;
    private string _statusMessage = "";
    private bool _hasStatus;
    private bool _outOfOfficeEnabled;
    private string _outOfOfficeMessage = "";

    public ObservableCollection<AdUser> Users { get => _users; set => SetProperty(ref _users, value); }

    public AdUser? SelectedUser
    {
        get => _selectedUser;
        set
        {
            SetProperty(ref _selectedUser, value);
            OnPropertyChanged(nameof(HasSelectedUser));
            OnPropertyChanged(nameof(NoUserSelected));
            OnPropertyChanged(nameof(EnabledBadgeText));
            OnPropertyChanged(nameof(EnabledBadgeColor));
            OnPropertyChanged(nameof(LockedBadgeText));
            OnPropertyChanged(nameof(LockedBadgeColor));
            OnPropertyChanged(nameof(LicenseBadgeText));
            OnPropertyChanged(nameof(LicenseBadgeColor));
            if (value != null) LoadMailboxInfo(value.Email);
        }
    }

    public bool HasSelectedUser => SelectedUser != null;
    public bool NoUserSelected => SelectedUser == null;

    // Flat badge properties — avoids nested-object converter issues
    public string EnabledBadgeText => SelectedUser?.IsEnabled == true ? "Enabled" : "Disabled";
    public Color EnabledBadgeColor => SelectedUser?.IsEnabled == true ? Color.FromArgb("#228B22") : Color.FromArgb("#CC4400");

    public string LockedBadgeText => SelectedUser?.IsLockedOut == true ? "Locked Out" : "Unlocked";
    public Color LockedBadgeColor => SelectedUser?.IsLockedOut == true ? Color.FromArgb("#CC4400") : Color.FromArgb("#1565C0");

    public string LicenseBadgeText => SelectedUser?.HasO365License == true ? "O365 Licensed" : "No License";
    public Color LicenseBadgeColor => SelectedUser?.HasO365License == true ? Color.FromArgb("#228B22") : Colors.Gray;

    public string StatusMessage { get => _statusMessage; set { SetProperty(ref _statusMessage, value); HasStatus = !string.IsNullOrEmpty(value); } }
    public bool HasStatus { get => _hasStatus; set => SetProperty(ref _hasStatus, value); }
    public bool OutOfOfficeEnabled { get => _outOfOfficeEnabled; set => SetProperty(ref _outOfOfficeEnabled, value); }
    public string OutOfOfficeMessage { get => _outOfOfficeMessage; set => SetProperty(ref _outOfOfficeMessage, value); }

    public ICommand SearchCommand { get; }
    public ICommand EnableCommand { get; }
    public ICommand DisableCommand { get; }
    public ICommand UnlockCommand { get; }
    public ICommand ResetPasswordCommand { get; }
    public ICommand RevokeSessionsCommand { get; }
    public ICommand AddAliasCommand { get; }
    public ICommand RemoveAliasCommand { get; }
    public ICommand SaveOutOfOfficeCommand { get; }

    public UserManagerViewModel(IAdService ad, IExchangeService exchange)
    {
        _ad = ad;
        _exchange = exchange;

        SearchCommand = new Command<string>(async q => await SearchAsync(q));
        EnableCommand = new Command(async () => await SetEnabledAsync(true));
        DisableCommand = new Command(async () => await SetEnabledAsync(false));
        UnlockCommand = new Command(async () => await UnlockAsync());
        ResetPasswordCommand = new Command(async () => await ResetPasswordAsync());
        RevokeSessionsCommand = new Command(async () => await RevokeSessionsAsync());
        AddAliasCommand = new Command<string>(async a => await AddAliasAsync(a));
        RemoveAliasCommand = new Command<string>(async a => await RemoveAliasAsync(a));
        SaveOutOfOfficeCommand = new Command(async () => await SaveOutOfOfficeAsync());

        _ = SearchAsync("");
    }

    private async Task SearchAsync(string query)
    {
        var results = await _ad.SearchUsersAsync(query ?? "");
        Users = new ObservableCollection<AdUser>(results);
    }

    private async Task SetEnabledAsync(bool enable)
    {
        if (SelectedUser == null) return;
        var ok = enable
            ? await _ad.EnableUserAsync(SelectedUser.SamAccountName)
            : await _ad.DisableUserAsync(SelectedUser.SamAccountName);
        if (ok)
        {
            SelectedUser.IsEnabled = enable;
            StatusMessage = $"Account {(enable ? "enabled" : "disabled")} for {SelectedUser.DisplayName}.";
            RefreshBadges();
        }
    }

    private async Task UnlockAsync()
    {
        if (SelectedUser == null) return;
        var ok = await _ad.UnlockUserAsync(SelectedUser.SamAccountName);
        if (ok)
        {
            SelectedUser.IsLockedOut = false;
            StatusMessage = $"Account unlocked for {SelectedUser.DisplayName}.";
            RefreshBadges();
        }
    }

    private async Task ResetPasswordAsync()
    {
        if (SelectedUser == null) return;
        var newPw = await Application.Current!.MainPage!.DisplayPromptAsync(
            "Reset Password",
            $"New password for {SelectedUser.DisplayName}:",
            "Reset", "Cancel", keyboard: Keyboard.Default);
        if (string.IsNullOrEmpty(newPw)) return;
        var ok = await _ad.ResetPasswordAsync(SelectedUser.SamAccountName, newPw);
        StatusMessage = ok
            ? $"Password reset for {SelectedUser.DisplayName}. User must change at next logon."
            : "Password reset failed.";
    }

    private async Task RevokeSessionsAsync()
    {
        if (SelectedUser == null) return;
        var confirm = await Application.Current!.MainPage!.DisplayAlert(
            "Revoke O365 Sessions",
            $"This will immediately sign {SelectedUser.DisplayName} out of all Office 365 sessions worldwide. Proceed?",
            "Yes, Revoke", "Cancel");
        if (!confirm) return;
        var ok = await _exchange.RevokeO365SessionsAsync(SelectedUser.UserPrincipalName);
        StatusMessage = ok
            ? $"All O365 sessions revoked for {SelectedUser.DisplayName}."
            : "Failed to revoke sessions.";
    }

    private async Task AddAliasAsync(string alias)
    {
        if (SelectedUser == null || string.IsNullOrWhiteSpace(alias)) return;
        var ok = await _ad.AddProxyAddressAsync(SelectedUser.SamAccountName, alias);
        if (ok)
        {
            StatusMessage = $"Alias smtp:{alias} added.";
            OnPropertyChanged(nameof(SelectedUser));
        }
    }

    private async Task RemoveAliasAsync(string alias)
    {
        if (SelectedUser == null || string.IsNullOrWhiteSpace(alias)) return;
        var ok = await _ad.RemoveProxyAddressAsync(SelectedUser.SamAccountName, alias);
        if (ok)
        {
            StatusMessage = $"Alias {alias} removed.";
            OnPropertyChanged(nameof(SelectedUser));
        }
    }

    private async Task SaveOutOfOfficeAsync()
    {
        if (SelectedUser == null) return;
        var ok = await _exchange.SetOutOfOfficeAsync(SelectedUser.Email, OutOfOfficeEnabled, OutOfOfficeMessage);
        StatusMessage = ok ? "Out of office settings saved." : "Failed to save out of office.";
    }

    private async void LoadMailboxInfo(string email)
    {
        StatusMessage = "";
        var mb = await _exchange.GetMailboxAsync(email);
        if (mb != null)
        {
            OutOfOfficeEnabled = mb.OutOfOfficeEnabled;
            OutOfOfficeMessage = mb.OutOfOfficeMessage;
        }
        else
        {
            OutOfOfficeEnabled = false;
            OutOfOfficeMessage = "";
        }
    }

    private void RefreshBadges()
    {
        OnPropertyChanged(nameof(EnabledBadgeText));
        OnPropertyChanged(nameof(EnabledBadgeColor));
        OnPropertyChanged(nameof(LockedBadgeText));
        OnPropertyChanged(nameof(LockedBadgeColor));
    }
}
