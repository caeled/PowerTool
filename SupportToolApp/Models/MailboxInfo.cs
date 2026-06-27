namespace SupportToolApp.Models;

public class MailboxInfo
{
    public string DisplayName { get; set; } = "";
    public string PrimarySmtpAddress { get; set; } = "";
    public string MailboxType { get; set; } = "";
    public long TotalItemSizeBytes { get; set; }
    public int ItemCount { get; set; }
    public List<string> EmailAddresses { get; set; } = [];
    public List<MailboxPermission> Permissions { get; set; } = [];
    public bool LitigationHoldEnabled { get; set; }
    public bool OutOfOfficeEnabled { get; set; }
    public string OutOfOfficeMessage { get; set; } = "";
}

public class MailboxPermission
{
    public string User { get; set; } = "";
    public string AccessRights { get; set; } = "";
}

public class LicenseReport
{
    public string UserPrincipalName { get; set; } = "";
    public string DisplayName { get; set; } = "";
    public List<string> Licenses { get; set; } = [];
    public bool IsLicensed { get; set; }
    public string UsageLocation { get; set; } = "";
}
