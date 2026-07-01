#pragma warning disable CS0618 // DisplayAlert obsolete until MAUI async variant resolves from ViewModel context
using System.Windows.Input;
using SupportToolApp.Services;

namespace SupportToolApp.ViewModels;

public class SpamViewModel : BaseViewModel
{
    private readonly IExchangeService _exchange;

    private string _purgeMailbox = "", _purgeSender = "", _purgeSubject = "";
    private string _rulesMailbox = "", _sessionUpn = "", _breachUpn = "";
    private string _statusMessage = "", _errorMessage = "";
    private bool _hasStatus, _hasError;

    public string PurgeMailbox { get => _purgeMailbox; set => SetProperty(ref _purgeMailbox, value); }
    public string PurgeSender { get => _purgeSender; set => SetProperty(ref _purgeSender, value); }
    public string PurgeSubject { get => _purgeSubject; set => SetProperty(ref _purgeSubject, value); }
    public string RulesMailbox { get => _rulesMailbox; set => SetProperty(ref _rulesMailbox, value); }
    public string SessionUpn { get => _sessionUpn; set => SetProperty(ref _sessionUpn, value); }
    public string BreachUpn { get => _breachUpn; set => SetProperty(ref _breachUpn, value); }

    public string StatusMessage { get => _statusMessage; set { SetProperty(ref _statusMessage, value); HasStatus = !string.IsNullOrEmpty(value); HasError = false; } }
    public string ErrorMessage { get => _errorMessage; set { SetProperty(ref _errorMessage, value); HasError = !string.IsNullOrEmpty(value); HasStatus = false; } }
    public bool HasStatus { get => _hasStatus; set => SetProperty(ref _hasStatus, value); }
    public bool HasError { get => _hasError; set => SetProperty(ref _hasError, value); }

    public ICommand PurgeCommand { get; }
    public ICommand RevokeRulesCommand { get; }
    public ICommand RevokeSessionsCommand { get; }
    public ICommand NuclearCommand { get; }

    public SpamViewModel(IExchangeService exchange)
    {
        _exchange = exchange;

        PurgeCommand = new Command(async () =>
        {
            if (string.IsNullOrWhiteSpace(PurgeMailbox) && string.IsNullOrWhiteSpace(PurgeSender) && string.IsNullOrWhiteSpace(PurgeSubject))
            {
                ErrorMessage = "Enter at least a mailbox, sender, or subject to purge.";
                return;
            }
            var confirm = await ((Page)Application.Current!.Windows[0].Page!).DisplayAlertAsync(
                "Confirm Email Purge",
                $"Permanently delete emails matching:\n" +
                $"  Mailbox: {(string.IsNullOrWhiteSpace(PurgeMailbox) ? "ALL" : PurgeMailbox)}\n" +
                $"  From: {PurgeSender}\n  Subject: {PurgeSubject}\n\nThis cannot be undone.",
                "Yes, Purge", "Cancel");
            if (!confirm) return;

            bool ok;
            if (!string.IsNullOrWhiteSpace(PurgeSender))
                ok = await _exchange.PurgeEmailsBySenderAsync(PurgeMailbox, PurgeSender);
            else
                ok = await _exchange.PurgeEmailsBySubjectAsync(PurgeMailbox, PurgeSubject, PurgeSender);

            StatusMessage = ok
                ? $"Purge complete. (Mock — no emails were deleted in this demo)"
                : "Purge failed.";
        });

        RevokeRulesCommand = new Command(async () =>
        {
            if (string.IsNullOrWhiteSpace(RulesMailbox)) { ErrorMessage = "Enter a mailbox address."; return; }
            var confirm = await ((Page)Application.Current!.Windows[0].Page!).DisplayAlertAsync(
                "Revoke Inbox Rules",
                $"Remove ALL inbox rules for {RulesMailbox}?\nAttackers use inbox rules to hide phishing confirmations.",
                "Yes, Remove All Rules", "Cancel");
            if (!confirm) return;
            var ok = await _exchange.RevokeInboxRulesAsync(RulesMailbox);
            StatusMessage = ok ? $"All inbox rules removed for {RulesMailbox}. (Mock)" : "Failed to revoke inbox rules.";
        });

        RevokeSessionsCommand = new Command(async () =>
        {
            if (string.IsNullOrWhiteSpace(SessionUpn)) { ErrorMessage = "Enter a UPN."; return; }
            var confirm = await ((Page)Application.Current!.Windows[0].Page!).DisplayAlertAsync(
                "Revoke O365 Sessions",
                $"Immediately sign {SessionUpn} out of ALL Office 365 sessions?",
                "Yes, Revoke", "Cancel");
            if (!confirm) return;
            var ok = await _exchange.RevokeO365SessionsAsync(SessionUpn);
            StatusMessage = ok ? $"All sessions revoked for {SessionUpn}. (Mock)" : "Session revocation failed.";
        });

        NuclearCommand = new Command(async () =>
        {
            if (string.IsNullOrWhiteSpace(BreachUpn)) { ErrorMessage = "Enter the compromised user's UPN."; return; }
            var confirm = await ((Page)Application.Current!.Windows[0].Page!).DisplayAlertAsync(
                "Full Breach Remediation",
                $"For {BreachUpn}, this will:\n\n" +
                "  1. Purge known phishing emails\n" +
                "  2. Remove all inbox rules\n" +
                "  3. Revoke all O365 sessions\n\n" +
                "Proceed with full remediation?",
                "Yes — Full Remediation", "Cancel");
            if (!confirm) return;

            await _exchange.PurgeEmailsBySenderAsync(BreachUpn, "");
            await _exchange.RevokeInboxRulesAsync(BreachUpn);
            await _exchange.RevokeO365SessionsAsync(BreachUpn);

            StatusMessage = $"Full breach remediation complete for {BreachUpn}.\nEmails purged • Rules revoked • Sessions terminated. (Mock)";
        });
    }
}
