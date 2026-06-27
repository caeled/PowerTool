using SupportToolApp.Models;

namespace SupportToolApp.Services.Mock;

public class MockExchangeService : IExchangeService
{
    private readonly Dictionary<string, MailboxInfo> _mailboxes = new(StringComparer.OrdinalIgnoreCase)
    {
        ["jsmith@contoso.com"] = new MailboxInfo
        {
            DisplayName = "John Smith", PrimarySmtpAddress = "jsmith@contoso.com", MailboxType = "UserMailbox",
            TotalItemSizeBytes = 2_400_000_000L, ItemCount = 8432,
            EmailAddresses = ["SMTP:jsmith@contoso.com", "smtp:john.smith@contoso.com"],
            Permissions = [new() { User = "bwilliams", AccessRights = "FullAccess" }]
        },
        ["mmartinez@contoso.com"] = new MailboxInfo
        {
            DisplayName = "Maria Martinez", PrimarySmtpAddress = "mmartinez@contoso.com", MailboxType = "UserMailbox",
            TotalItemSizeBytes = 850_000_000L, ItemCount = 3100,
            EmailAddresses = ["SMTP:mmartinez@contoso.com"]
        },
        ["bwilliams@contoso.com"] = new MailboxInfo
        {
            DisplayName = "Bob Williams", PrimarySmtpAddress = "bwilliams@contoso.com", MailboxType = "UserMailbox",
            TotalItemSizeBytes = 5_200_000_000L, ItemCount = 22100,
            EmailAddresses = ["SMTP:bwilliams@contoso.com", "smtp:bob.williams@contoso.com"]
        },
        ["helpdesk@contoso.com"] = new MailboxInfo
        {
            DisplayName = "Help Desk", PrimarySmtpAddress = "helpdesk@contoso.com", MailboxType = "SharedMailbox",
            TotalItemSizeBytes = 350_000_000L, ItemCount = 1200,
            EmailAddresses = ["SMTP:helpdesk@contoso.com", "smtp:support@contoso.com"],
            Permissions = [new() { User = "jsmith", AccessRights = "FullAccess" }, new() { User = "bwilliams", AccessRights = "FullAccess" }]
        },
    };

    private readonly List<LicenseReport> _licenses =
    [
        new() { UserPrincipalName = "jsmith@contoso.com", DisplayName = "John Smith", IsLicensed = true, Licenses = ["Microsoft 365 E3"], UsageLocation = "US" },
        new() { UserPrincipalName = "mmartinez@contoso.com", DisplayName = "Maria Martinez", IsLicensed = true, Licenses = ["Microsoft 365 Business Standard"], UsageLocation = "US" },
        new() { UserPrincipalName = "tjohnson@contoso.com", DisplayName = "Tom Johnson", IsLicensed = false, Licenses = [], UsageLocation = "US" },
        new() { UserPrincipalName = "bwilliams@contoso.com", DisplayName = "Bob Williams", IsLicensed = true, Licenses = ["Microsoft 365 E3", "Power BI Pro"], UsageLocation = "US" },
        new() { UserPrincipalName = "slee@contoso.com", DisplayName = "Susan Lee", IsLicensed = true, Licenses = ["Microsoft 365 Business Standard"], UsageLocation = "US" },
    ];

    public Task<MailboxInfo?> GetMailboxAsync(string identifier)
    {
        _mailboxes.TryGetValue(identifier, out var mb);
        return Task.FromResult(mb);
    }

    public Task<bool> SetForwardingAsync(string mailbox, string forwardTo, bool keepCopy = true)
        => Task.FromResult(_mailboxes.ContainsKey(mailbox));

    public Task<bool> SetOutOfOfficeAsync(string mailbox, bool enabled, string message = "")
    {
        if (_mailboxes.TryGetValue(mailbox, out var mb))
        {
            mb.OutOfOfficeEnabled = enabled;
            mb.OutOfOfficeMessage = message;
        }
        return Task.FromResult(_mailboxes.ContainsKey(mailbox));
    }

    public Task<bool> GrantFullAccessAsync(string mailbox, string grantee)
    {
        if (_mailboxes.TryGetValue(mailbox, out var mb))
            mb.Permissions.Add(new MailboxPermission { User = grantee, AccessRights = "FullAccess" });
        return Task.FromResult(_mailboxes.ContainsKey(mailbox));
    }

    public Task<bool> RevokeFullAccessAsync(string mailbox, string grantee)
    {
        if (_mailboxes.TryGetValue(mailbox, out var mb))
            mb.Permissions.RemoveAll(p => p.User.Equals(grantee, StringComparison.OrdinalIgnoreCase));
        return Task.FromResult(_mailboxes.ContainsKey(mailbox));
    }

    public Task<bool> SetCalendarPermissionAsync(string mailbox, string user, string accessRights)
        => Task.FromResult(_mailboxes.ContainsKey(mailbox));

    public Task<bool> AddSendAsAsync(string mailbox, string trustee)
        => Task.FromResult(_mailboxes.ContainsKey(mailbox));

    public Task<bool> PurgeEmailsBySubjectAsync(string mailbox, string subject, string? sender = null)
        => Task.FromResult(true);

    public Task<bool> PurgeEmailsBySenderAsync(string mailbox, string sender)
        => Task.FromResult(true);

    public Task<bool> RevokeInboxRulesAsync(string mailbox)
        => Task.FromResult(_mailboxes.ContainsKey(mailbox));

    public Task<bool> RevokeO365SessionsAsync(string upn)
        => Task.FromResult(true);

    public Task<List<LicenseReport>> GetLicenseReportAsync()
        => Task.FromResult(_licenses.ToList());

    public Task<List<MailboxInfo>> GetSharedMailboxesAsync()
        => Task.FromResult(_mailboxes.Values.Where(m => m.MailboxType == "SharedMailbox").ToList());
}
