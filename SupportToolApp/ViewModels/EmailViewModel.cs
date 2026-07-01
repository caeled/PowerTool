#pragma warning disable CS0618 // DisplayAlert obsolete until MAUI async variant resolves from ViewModel context
using System.Collections.ObjectModel;
using System.Windows.Input;
using SupportToolApp.Helpers;
using SupportToolApp.Models;
using SupportToolApp.Services;

namespace SupportToolApp.ViewModels;

public class EmailViewModel : BaseViewModel
{
    private readonly IExchangeService _exchange;

    private MailboxInfo? _mailbox;
    private bool _hasMailbox;
    private string _statusMessage = "";
    private bool _hasStatus;
    private string _errorMessage = "";
    private bool _hasError;
    private ObservableCollection<MailboxInfo> _sharedMailboxes = [];

    // Form fields
    private string _lookupAddress = "";
    private string _forwardFrom = "";
    private string _forwardTo = "";
    private bool _keepForwardCopy = true;
    private string _delegateMailbox = "";
    private string _delegateUser = "";

    public MailboxInfo? Mailbox { get => _mailbox; set { SetProperty(ref _mailbox, value); HasMailbox = value != null; OnPropertyChanged(nameof(MailboxSizeDisplay)); OnPropertyChanged(nameof(PermissionsDisplay)); } }
    public bool HasMailbox { get => _hasMailbox; set => SetProperty(ref _hasMailbox, value); }
    public string StatusMessage { get => _statusMessage; set { SetProperty(ref _statusMessage, value); HasStatus = !string.IsNullOrEmpty(value); HasError = false; } }
    public string ErrorMessage { get => _errorMessage; set { SetProperty(ref _errorMessage, value); HasError = !string.IsNullOrEmpty(value); HasStatus = false; } }
    public bool HasStatus { get => _hasStatus; set => SetProperty(ref _hasStatus, value); }
    public bool HasError { get => _hasError; set => SetProperty(ref _hasError, value); }
    public ObservableCollection<MailboxInfo> SharedMailboxes { get => _sharedMailboxes; set => SetProperty(ref _sharedMailboxes, value); }

    public string LookupAddress { get => _lookupAddress; set => SetProperty(ref _lookupAddress, value); }
    public string ForwardFrom { get => _forwardFrom; set => SetProperty(ref _forwardFrom, value); }
    public string ForwardTo { get => _forwardTo; set => SetProperty(ref _forwardTo, value); }
    public bool KeepForwardCopy { get => _keepForwardCopy; set => SetProperty(ref _keepForwardCopy, value); }
    public string DelegateMailbox { get => _delegateMailbox; set => SetProperty(ref _delegateMailbox, value); }
    public string DelegateUser { get => _delegateUser; set => SetProperty(ref _delegateUser, value); }

    public string MailboxSizeDisplay => Mailbox == null ? "" :
        $"{Mailbox.TotalItemSizeBytes / 1_073_741_824.0:F2} GB  •  {Mailbox.ItemCount:N0} items  •  {Mailbox.MailboxType}";

    public string PermissionsDisplay => Mailbox == null || Mailbox.Permissions.Count == 0
        ? "No delegated permissions"
        : string.Join("\n", Mailbox.Permissions.Select(p => $"  {p.User}  →  {p.AccessRights}"));

    // Parse Header
    private string _rawHeader = "", _headerResult = "";
    private bool _hasHeaderResult;
    public string RawHeader      { get => _rawHeader;       set => SetProperty(ref _rawHeader, value); }
    public string HeaderResult   { get => _headerResult;    set { SetProperty(ref _headerResult, value); HasHeaderResult = !string.IsNullOrEmpty(value); } }
    public bool   HasHeaderResult { get => _hasHeaderResult; set => SetProperty(ref _hasHeaderResult, value); }
    public ICommand ParseHeaderCommand { get; }
    public ICommand ClearHeaderCommand { get; }

    public ICommand LookupCommand { get; }
    public ICommand SetForwardingCommand { get; }
    public ICommand GrantAccessCommand { get; }
    public ICommand RevokeAccessCommand { get; }
    public ICommand AddSendAsCommand { get; }
    public ICommand LoadSharedMailboxesCommand { get; }
    public ICommand ClearForwardingCommand { get; }

    public EmailViewModel(IExchangeService exchange)
    {
        _exchange = exchange;

        ParseHeaderCommand = new Command(() =>
        {
            if (string.IsNullOrWhiteSpace(RawHeader)) return;
            HeaderResult = EmailHeaderParser.Parse(RawHeader);
        });

        ClearHeaderCommand = new Command(() => { RawHeader = ""; HeaderResult = ""; });

        LookupCommand = new Command(async () =>
        {
            if (string.IsNullOrWhiteSpace(LookupAddress)) return;
            Mailbox = await _exchange.GetMailboxAsync(LookupAddress.Trim());
            if (Mailbox == null)
                ErrorMessage = $"No mailbox found for: {LookupAddress}";
            else
                StatusMessage = $"Loaded mailbox for {Mailbox.DisplayName}.";
        });

        SetForwardingCommand = new Command(async () =>
        {
            if (string.IsNullOrWhiteSpace(ForwardFrom) || string.IsNullOrWhiteSpace(ForwardTo))
            {
                ErrorMessage = "Both From and Forward To addresses are required.";
                return;
            }
            var ok = await _exchange.SetForwardingAsync(ForwardFrom.Trim(), ForwardTo.Trim(), KeepForwardCopy);
            if (ok)
                StatusMessage = $"Forwarding set: {ForwardFrom} → {ForwardTo}{(KeepForwardCopy ? " (copy kept)" : "")}";
            else
                ErrorMessage = $"Could not set forwarding — mailbox not found: {ForwardFrom}";
        });

        ClearForwardingCommand = new Command(async () =>
        {
            if (string.IsNullOrWhiteSpace(ForwardFrom)) { ErrorMessage = "Enter the mailbox address first."; return; }
            var ok = await _exchange.SetForwardingAsync(ForwardFrom.Trim(), "", false);
            StatusMessage = ok ? $"Forwarding cleared for {ForwardFrom}." : "Failed to clear forwarding.";
        });

        GrantAccessCommand = new Command(async () =>
        {
            if (string.IsNullOrWhiteSpace(DelegateMailbox) || string.IsNullOrWhiteSpace(DelegateUser)) { ErrorMessage = "Both fields required."; return; }
            var ok = await _exchange.GrantFullAccessAsync(DelegateMailbox.Trim(), DelegateUser.Trim());
            StatusMessage = ok ? $"Full Access granted: {DelegateUser} → {DelegateMailbox}" : "Failed to grant access.";
        });

        RevokeAccessCommand = new Command(async () =>
        {
            if (string.IsNullOrWhiteSpace(DelegateMailbox) || string.IsNullOrWhiteSpace(DelegateUser)) { ErrorMessage = "Both fields required."; return; }
            var confirm = await ((Page)Application.Current!.Windows[0].Page!).DisplayAlert("Confirm", $"Revoke {DelegateUser}'s access to {DelegateMailbox}?", "Yes", "Cancel");
            if (!confirm) return;
            var ok = await _exchange.RevokeFullAccessAsync(DelegateMailbox.Trim(), DelegateUser.Trim());
            StatusMessage = ok ? $"Access revoked for {DelegateUser} on {DelegateMailbox}." : "Failed to revoke access.";
        });

        AddSendAsCommand = new Command(async () =>
        {
            if (string.IsNullOrWhiteSpace(DelegateMailbox) || string.IsNullOrWhiteSpace(DelegateUser)) { ErrorMessage = "Both fields required."; return; }
            var ok = await _exchange.AddSendAsAsync(DelegateMailbox.Trim(), DelegateUser.Trim());
            StatusMessage = ok ? $"Send-As added: {DelegateUser} can send as {DelegateMailbox}." : "Failed to add Send-As.";
        });

        LoadSharedMailboxesCommand = new Command(async () =>
        {
            var list = await _exchange.GetSharedMailboxesAsync();
            SharedMailboxes = new ObservableCollection<MailboxInfo>(list);
            StatusMessage = $"Loaded {list.Count} shared mailbox(es).";
        });
    }
}
