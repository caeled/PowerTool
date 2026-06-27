using Microsoft.Graph;
using Microsoft.Graph.Models;

using Microsoft.Kiota.Abstractions.Authentication;
using SupportToolApp.Models;

namespace SupportToolApp.Services.Graph;

public class GraphExchangeService : IExchangeService
{
    private readonly IAuthService _auth;
    private GraphServiceClient? _client;

    private static readonly string[] DefaultScopes =
    [
        "Mail.ReadWrite",
        "MailboxSettings.ReadWrite",
        "User.ReadWrite.All",
        "Directory.ReadWrite.All"
    ];

    public GraphExchangeService(IAuthService auth)
    {
        _auth = auth;
    }

    private GraphServiceClient GetClient()
    {
        if (_client != null) return _client;
        var provider = new ExchangeTokenProvider(_auth, DefaultScopes);
        _client = new GraphServiceClient(new BaseBearerTokenAuthenticationProvider(provider));
        return _client;
    }

    public async Task<MailboxInfo?> GetMailboxAsync(string identifier)
    {
        try
        {
            var client = GetClient();
            var user = await client.Users[identifier].GetAsync(req =>
                req.QueryParameters.Select = ["id", "displayName", "mail", "userPrincipalName", "mailboxSettings", "assignedLicenses"]);

            if (user == null) return null;

            var mailboxSettings = await client.Users[identifier].MailboxSettings.GetAsync();

            var info = new MailboxInfo
            {
                PrimarySmtpAddress = user.Mail ?? user.UserPrincipalName ?? "",
                DisplayName = user.DisplayName ?? "",
                MailboxType = "UserMailbox",
                OutOfOfficeEnabled = mailboxSettings?.AutomaticRepliesSetting?.Status == AutomaticRepliesStatus.AlwaysEnabled,
                OutOfOfficeMessage = mailboxSettings?.AutomaticRepliesSetting?.InternalReplyMessage ?? ""
            };

            // Get permissions (delegated access)
            try
            {
                var perms = await client.Users[identifier].MailFolders["inbox"].GetAsync();
                // Graph doesn't expose mailbox delegation directly via /users/{id}/mailboxPermissions
                // That requires EXO PowerShell or admin-consent for specific beta endpoints
                info.Permissions = [];
            }
            catch { info.Permissions = []; }

            return info;
        }
        catch { return null; }
    }

    public async Task<bool> SetForwardingAsync(string mailbox, string forwardTo, bool keepCopy = true)
    {
        // Graph doesn't support setting forwarding via MailboxSettings directly.
        // Use inbox rules as an approximation.
        try
        {
            var client = GetClient();
            var rule = new MessageRule
            {
                DisplayName = "Forward All (IT Support Tool)",
                Sequence = 1,
                IsEnabled = true,
                Conditions = new MessageRulePredicates
                {
                    // Matches all messages (no conditions = all)
                },
                Actions = new MessageRuleActions
                {
                    ForwardTo = [new Recipient { EmailAddress = new EmailAddress { Address = forwardTo } }],
                    StopProcessingRules = false
                }
            };

            await client.Users[mailbox].MailFolders["inbox"].MessageRules.PostAsync(rule);
            return true;
        }
        catch { return false; }
    }

    public async Task<bool> SetOutOfOfficeAsync(string mailbox, bool enabled, string message = "")
    {
        try
        {
            var client = GetClient();
            var settings = new MailboxSettings
            {
                AutomaticRepliesSetting = new AutomaticRepliesSetting
                {
                    Status = enabled
                        ? AutomaticRepliesStatus.AlwaysEnabled
                        : AutomaticRepliesStatus.Disabled,
                    InternalReplyMessage = message,
                    ExternalReplyMessage = message
                }
            };

            await client.Users[mailbox].MailboxSettings.PatchAsync(settings);
            return true;
        }
        catch { return false; }
    }

    public Task<bool> GrantFullAccessAsync(string mailbox, string grantee)
    {
        // Full mailbox delegation requires EXO PowerShell or Microsoft Graph beta (Add-MailboxPermission equivalent).
        // Not available in Graph v1.0; return false to indicate unsupported via this path.
        return Task.FromResult(false);
    }

    public Task<bool> RevokeFullAccessAsync(string mailbox, string grantee)
    {
        return Task.FromResult(false);
    }

    public Task<bool> SetCalendarPermissionAsync(string mailbox, string user, string accessRights)
    {
        // Calendar sharing permissions are partially available via /users/{id}/calendar/calendarPermissions
        // but changing sharee permissions requires more complex Graph calls — deferred for now.
        return Task.FromResult(false);
    }

    public Task<bool> AddSendAsAsync(string mailbox, string trustee)
    {
        // Send As delegation requires EXO PowerShell or Graph beta.
        return Task.FromResult(false);
    }

    public async Task<bool> PurgeEmailsBySubjectAsync(string mailbox, string subject, string? sender = null)
    {
        try
        {
            var client = GetClient();
            var filter = $"subject eq '{EscapeFilter(subject)}'";
            if (!string.IsNullOrWhiteSpace(sender))
                filter += $" and from/emailAddress/address eq '{EscapeFilter(sender)}'";

            var messages = await client.Users[mailbox].Messages.GetAsync(req =>
            {
                req.QueryParameters.Filter = filter;
                req.QueryParameters.Select = ["id"];
                req.QueryParameters.Top = 50;
            });

            if (messages?.Value == null) return true;

            foreach (var msg in messages.Value)
            {
                if (msg.Id != null)
                    await client.Users[mailbox].Messages[msg.Id].DeleteAsync();
            }
            return true;
        }
        catch { return false; }
    }

    public async Task<bool> PurgeEmailsBySenderAsync(string mailbox, string sender)
    {
        try
        {
            var client = GetClient();
            var messages = await client.Users[mailbox].Messages.GetAsync(req =>
            {
                req.QueryParameters.Filter = $"from/emailAddress/address eq '{EscapeFilter(sender)}'";
                req.QueryParameters.Select = ["id"];
                req.QueryParameters.Top = 50;
            });

            if (messages?.Value == null) return true;

            foreach (var msg in messages.Value)
            {
                if (msg.Id != null)
                    await client.Users[mailbox].Messages[msg.Id].DeleteAsync();
            }
            return true;
        }
        catch { return false; }
    }

    public async Task<bool> RevokeInboxRulesAsync(string mailbox)
    {
        try
        {
            var client = GetClient();
            var rules = await client.Users[mailbox].MailFolders["inbox"].MessageRules.GetAsync();
            if (rules?.Value == null) return true;

            foreach (var rule in rules.Value)
            {
                if (rule.Id != null)
                    await client.Users[mailbox].MailFolders["inbox"].MessageRules[rule.Id].DeleteAsync();
            }
            return true;
        }
        catch { return false; }
    }

    public async Task<bool> RevokeO365SessionsAsync(string upn)
    {
        try
        {
            var client = GetClient();
            await client.Users[upn].RevokeSignInSessions.PostAsync();
            return true;
        }
        catch { return false; }
    }

    public async Task<List<LicenseReport>> GetLicenseReportAsync()
    {
        try
        {
            var client = GetClient();
            var users = await client.Users.GetAsync(req =>
            {
                req.QueryParameters.Select = ["displayName", "userPrincipalName", "assignedLicenses", "assignedPlans", "accountEnabled"];
                req.QueryParameters.Top = 100;
            });

            var report = new List<LicenseReport>();
            if (users?.Value == null) return report;

            foreach (var u in users.Value)
            {
                report.Add(new LicenseReport
                {
                    DisplayName = u.DisplayName ?? "",
                    UserPrincipalName = u.UserPrincipalName ?? "",
                    Licenses = u.AssignedLicenses?.Count > 0
                        ? u.AssignedLicenses.Select(l => l.SkuId?.ToString() ?? "Unknown").ToList()
                        : [],
                    IsLicensed = u.AssignedLicenses?.Count > 0
                });
            }
            return report;
        }
        catch { return []; }
    }

    public async Task<List<MailboxInfo>> GetSharedMailboxesAsync()
    {
        // Graph doesn't have a direct "shared mailbox" filter on v1.0.
        // Return empty — shared mailboxes require EXO or beta endpoint with mailboxType filter.
        return [];
    }

    private static string EscapeFilter(string s) => s.Replace("'", "''");
}

internal class ExchangeTokenProvider : IAccessTokenProvider
{
    private readonly IAuthService _auth;
    private readonly string[] _scopes;

    public ExchangeTokenProvider(IAuthService auth, string[] scopes)
    {
        _auth = auth;
        _scopes = scopes;
    }

    public async Task<string> GetAuthorizationTokenAsync(Uri uri,
        Dictionary<string, object>? additionalAuthenticationContext = null,
        CancellationToken cancellationToken = default)
    {
        return await _auth.GetAccessTokenAsync(_scopes) ?? "";
    }

    public AllowedHostsValidator AllowedHostsValidator { get; } =
        new AllowedHostsValidator(["graph.microsoft.com"]);
}
