using SupportToolApp.Models;

namespace SupportToolApp.Services;

public interface IExchangeService
{
    Task<MailboxInfo?> GetMailboxAsync(string identifier);
    Task<bool> SetForwardingAsync(string mailbox, string forwardTo, bool keepCopy = true);
    Task<bool> SetOutOfOfficeAsync(string mailbox, bool enabled, string message = "");
    Task<bool> GrantFullAccessAsync(string mailbox, string grantee);
    Task<bool> RevokeFullAccessAsync(string mailbox, string grantee);
    Task<bool> SetCalendarPermissionAsync(string mailbox, string user, string accessRights);
    Task<bool> AddSendAsAsync(string mailbox, string trustee);

    Task<bool> PurgeEmailsBySubjectAsync(string mailbox, string subject, string? sender = null);
    Task<bool> PurgeEmailsBySenderAsync(string mailbox, string sender);
    Task<bool> RevokeInboxRulesAsync(string mailbox);
    Task<bool> RevokeO365SessionsAsync(string upn);

    Task<List<LicenseReport>> GetLicenseReportAsync();
    Task<List<MailboxInfo>> GetSharedMailboxesAsync();
}
