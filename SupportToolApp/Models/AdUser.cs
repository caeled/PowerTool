namespace SupportToolApp.Models;

public class AdUser
{
    public string SamAccountName { get; set; } = "";
    public string DisplayName { get; set; } = "";
    public string UserPrincipalName { get; set; } = "";
    public string Email { get; set; } = "";
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string Department { get; set; } = "";
    public string Title { get; set; } = "";
    public string Manager { get; set; } = "";
    public string OfficePhone { get; set; } = "";
    public bool IsEnabled { get; set; }
    public bool IsLockedOut { get; set; }
    public bool PasswordExpired { get; set; }
    public DateTime? LastLogon { get; set; }
    public DateTime? PasswordLastSet { get; set; }
    public DateTime Created { get; set; }
    public List<string> MemberOf { get; set; } = [];
    public List<string> ProxyAddresses { get; set; } = [];
    public bool HasO365License { get; set; }
}

public class AdComputer
{
    public string Name { get; set; } = "";
    public string DnsHostName { get; set; } = "";
    public string OperatingSystem { get; set; } = "";
    public string Description { get; set; } = "";
    public DateTime? LastLogon { get; set; }
    public bool IsEnabled { get; set; }
}
