using SupportToolApp.Models;

namespace SupportToolApp.Services.Mock;

public class MockAdService : IAdService
{
    private readonly List<AdUser> _users =
    [
        new AdUser
        {
            SamAccountName = "jsmith", DisplayName = "John Smith", FirstName = "John", LastName = "Smith",
            UserPrincipalName = "jsmith@contoso.com", Email = "jsmith@contoso.com",
            Department = "IT", Title = "Systems Administrator", Manager = "bwilliams",
            OfficePhone = "ext. 1042",
            IsEnabled = true, IsLockedOut = false, HasO365License = true,
            LastLogon = DateTime.Now.AddHours(-2), PasswordLastSet = DateTime.Now.AddDays(-30),
            Created = DateTime.Now.AddYears(-3),
            MemberOf = ["Domain Users", "IT Staff", "VPN Users"],
            ProxyAddresses = ["SMTP:jsmith@contoso.com", "smtp:john.smith@contoso.com"]
        },
        new AdUser
        {
            SamAccountName = "mmartinez", DisplayName = "Maria Martinez", FirstName = "Maria", LastName = "Martinez",
            UserPrincipalName = "mmartinez@contoso.com", Email = "mmartinez@contoso.com",
            Department = "Accounting", Title = "Senior Accountant", Manager = "bwilliams",
            OfficePhone = "ext. 2201",
            IsEnabled = true, IsLockedOut = true, HasO365License = true,
            LastLogon = DateTime.Now.AddDays(-1), PasswordLastSet = DateTime.Now.AddDays(-91),
            PasswordExpired = true, Created = DateTime.Now.AddYears(-2),
            MemberOf = ["Domain Users", "Accounting"],
            ProxyAddresses = ["SMTP:mmartinez@contoso.com"]
        },
        new AdUser
        {
            SamAccountName = "tjohnson", DisplayName = "Tom Johnson", FirstName = "Tom", LastName = "Johnson",
            UserPrincipalName = "tjohnson@contoso.com", Email = "tjohnson@contoso.com",
            Department = "Sales", Title = "Sales Representative", Manager = "bwilliams",
            OfficePhone = "ext. 3310",
            IsEnabled = false, IsLockedOut = false, HasO365License = false,
            LastLogon = DateTime.Now.AddDays(-45), Created = DateTime.Now.AddYears(-1),
            MemberOf = ["Domain Users", "Sales"],
            ProxyAddresses = ["SMTP:tjohnson@contoso.com"]
        },
        new AdUser
        {
            SamAccountName = "bwilliams", DisplayName = "Bob Williams", FirstName = "Bob", LastName = "Williams",
            UserPrincipalName = "bwilliams@contoso.com", Email = "bwilliams@contoso.com",
            Department = "Management", Title = "IT Manager", Manager = "",
            OfficePhone = "ext. 1001",
            IsEnabled = true, IsLockedOut = false, HasO365License = true,
            LastLogon = DateTime.Now.AddMinutes(-15), PasswordLastSet = DateTime.Now.AddDays(-10),
            Created = DateTime.Now.AddYears(-5),
            MemberOf = ["Domain Users", "IT Staff", "Domain Admins", "VPN Users"],
            ProxyAddresses = ["SMTP:bwilliams@contoso.com", "smtp:bob.williams@contoso.com"]
        },
        new AdUser
        {
            SamAccountName = "slee", DisplayName = "Susan Lee", FirstName = "Susan", LastName = "Lee",
            UserPrincipalName = "slee@contoso.com", Email = "slee@contoso.com",
            Department = "HR", Title = "HR Coordinator", Manager = "bwilliams",
            OfficePhone = "ext. 4105",
            IsEnabled = true, IsLockedOut = false, HasO365License = true,
            LastLogon = DateTime.Now.AddHours(-4), PasswordLastSet = DateTime.Now.AddDays(-5),
            Created = DateTime.Now.AddYears(-2),
            MemberOf = ["Domain Users", "HR"],
            ProxyAddresses = ["SMTP:slee@contoso.com"]
        },
    ];

    private readonly List<AdComputer> _computers =
    [
        new() { Name = "DESKTOP-IT01", DnsHostName = "DESKTOP-IT01.contoso.com", OperatingSystem = "Windows 11 Pro", IsEnabled = true, LastLogon = DateTime.Now.AddHours(-1), Description = "IT Dept Workstation" },
        new() { Name = "DESKTOP-ACC01", DnsHostName = "DESKTOP-ACC01.contoso.com", OperatingSystem = "Windows 11 Pro", IsEnabled = true, LastLogon = DateTime.Now.AddHours(-3), Description = "Accounting Workstation" },
        new() { Name = "LAPTOP-SALES01", DnsHostName = "LAPTOP-SALES01.contoso.com", OperatingSystem = "Windows 11 Pro", IsEnabled = true, LastLogon = DateTime.Now.AddDays(-2), Description = "Sales Laptop" },
        new() { Name = "FS01", DnsHostName = "fs01.contoso.com", OperatingSystem = "Windows Server 2022", IsEnabled = true, LastLogon = DateTime.Now, Description = "File Server 1" },
        new() { Name = "DC01", DnsHostName = "dc01.contoso.com", OperatingSystem = "Windows Server 2022", IsEnabled = true, LastLogon = DateTime.Now, Description = "Primary Domain Controller" },
        new() { Name = "DESKTOP-OLD01", DnsHostName = "DESKTOP-OLD01.contoso.com", OperatingSystem = "Windows 10 Pro", IsEnabled = false, LastLogon = DateTime.Now.AddDays(-120), Description = "Decommissioned" },
    ];

    public Task<AdUser?> GetUserAsync(string identifier)
    {
        var user = _users.FirstOrDefault(u =>
            u.SamAccountName.Equals(identifier, StringComparison.OrdinalIgnoreCase) ||
            u.UserPrincipalName.Equals(identifier, StringComparison.OrdinalIgnoreCase) ||
            u.Email.Equals(identifier, StringComparison.OrdinalIgnoreCase));
        return Task.FromResult(user);
    }

    public Task<List<AdUser>> SearchUsersAsync(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return Task.FromResult(_users.ToList());

        var q = query.ToLower();
        var results = _users.Where(u =>
            u.SamAccountName.Contains(q, StringComparison.OrdinalIgnoreCase) ||
            u.DisplayName.Contains(q, StringComparison.OrdinalIgnoreCase) ||
            u.Email.Contains(q, StringComparison.OrdinalIgnoreCase) ||
            u.Department.Contains(q, StringComparison.OrdinalIgnoreCase)).ToList();
        return Task.FromResult(results);
    }

    public Task<List<AdComputer>> GetComputersAsync(string filter = "")
    {
        if (string.IsNullOrWhiteSpace(filter))
            return Task.FromResult(_computers.ToList());

        var results = _computers.Where(c =>
            c.Name.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
            c.Description.Contains(filter, StringComparison.OrdinalIgnoreCase)).ToList();
        return Task.FromResult(results);
    }

    public Task<bool> EnableUserAsync(string sam)
    {
        var user = _users.FirstOrDefault(u => u.SamAccountName == sam);
        if (user != null) user.IsEnabled = true;
        return Task.FromResult(user != null);
    }

    public Task<bool> DisableUserAsync(string sam)
    {
        var user = _users.FirstOrDefault(u => u.SamAccountName == sam);
        if (user != null) user.IsEnabled = false;
        return Task.FromResult(user != null);
    }

    public Task<bool> UnlockUserAsync(string sam)
    {
        var user = _users.FirstOrDefault(u => u.SamAccountName == sam);
        if (user != null) user.IsLockedOut = false;
        return Task.FromResult(user != null);
    }

    public Task<bool> ResetPasswordAsync(string sam, string newPassword, bool mustChange = true)
    {
        var user = _users.FirstOrDefault(u => u.SamAccountName == sam);
        if (user != null)
        {
            user.PasswordLastSet = DateTime.Now;
            user.PasswordExpired = mustChange;
        }
        return Task.FromResult(user != null);
    }

    public Task<bool> SetUPNAsync(string sam, string newUpn)
    {
        var user = _users.FirstOrDefault(u => u.SamAccountName == sam);
        if (user != null) user.UserPrincipalName = newUpn;
        return Task.FromResult(user != null);
    }

    public Task<bool> AddProxyAddressAsync(string sam, string smtpAddress)
    {
        var user = _users.FirstOrDefault(u => u.SamAccountName == sam);
        if (user != null && !user.ProxyAddresses.Contains(smtpAddress, StringComparer.OrdinalIgnoreCase))
            user.ProxyAddresses.Add($"smtp:{smtpAddress}");
        return Task.FromResult(user != null);
    }

    public Task<bool> RemoveProxyAddressAsync(string sam, string smtpAddress)
    {
        var user = _users.FirstOrDefault(u => u.SamAccountName == sam);
        user?.ProxyAddresses.RemoveAll(a => a.Contains(smtpAddress, StringComparison.OrdinalIgnoreCase));
        return Task.FromResult(user != null);
    }

    public Task<bool> CreateUserAsync(AdUser user, string password)
    {
        _users.Add(user);
        return Task.FromResult(true);
    }

    public Task<List<AdUser>> GetLockedOutUsersAsync()
    {
        return Task.FromResult(_users.Where(u => u.IsLockedOut).ToList());
    }
}
