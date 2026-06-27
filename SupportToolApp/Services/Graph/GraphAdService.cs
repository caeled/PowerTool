using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Kiota.Abstractions.Authentication;
using SupportToolApp.Models;

namespace SupportToolApp.Services.Graph;

public class GraphAdService : IAdService
{
    private readonly IAuthService _auth;
    private GraphServiceClient? _client;

    private static readonly string[] DefaultScopes =
    [
        "User.ReadWrite.All",
        "Directory.ReadWrite.All",
        "Group.Read.All"
    ];

    public GraphAdService(IAuthService auth)
    {
        _auth = auth;
    }

    private GraphServiceClient GetClient()
    {
        if (_client != null) return _client;
        var provider = new TokenProvider(_auth, DefaultScopes);
        _client = new GraphServiceClient(new BaseBearerTokenAuthenticationProvider(provider));
        return _client;
    }

    public async Task<AdUser?> GetUserAsync(string identifier)
    {
        try
        {
            var client = GetClient();
            var user = await client.Users[identifier].GetAsync(req =>
            {
                req.QueryParameters.Select = UserSelectFields();
                req.QueryParameters.Expand = ["manager"];
            });

            return user == null ? null : MapUser(user);
        }
        catch { return null; }
    }

    public async Task<List<AdUser>> SearchUsersAsync(string query)
    {
        try
        {
            var client = GetClient();
            var filter = string.IsNullOrWhiteSpace(query)
                ? null
                : $"startsWith(displayName,'{EscapeFilter(query)}') or startsWith(userPrincipalName,'{EscapeFilter(query)}') or startsWith(mail,'{EscapeFilter(query)}')";

            var response = await client.Users.GetAsync(req =>
            {
                req.QueryParameters.Select = UserSelectFields();
                req.QueryParameters.Top = 50;
                if (filter != null) req.QueryParameters.Filter = filter;
            });

            var users = new List<AdUser>();
            if (response?.Value == null) return users;

            foreach (var u in response.Value)
                users.Add(MapUser(u));

            return users;
        }
        catch { return []; }
    }

    public async Task<List<AdComputer>> GetComputersAsync(string filter = "")
    {
        // Entra ID doesn't have computer objects in the same way on-prem AD does.
        // Devices are available via /devices endpoint.
        try
        {
            var client = GetClient();
            var f = string.IsNullOrWhiteSpace(filter)
                ? null
                : $"startsWith(displayName,'{EscapeFilter(filter)}')";

            var response = await client.Devices.GetAsync(req =>
            {
                req.QueryParameters.Select = ["displayName", "operatingSystem", "operatingSystemVersion", "accountEnabled", "approximateLastSignInDateTime", "deviceId"];
                req.QueryParameters.Top = 50;
                if (f != null) req.QueryParameters.Filter = f;
            });

            var computers = new List<AdComputer>();
            if (response?.Value == null) return computers;

            foreach (var d in response.Value)
            {
                computers.Add(new AdComputer
                {
                    Name = d.DisplayName ?? "",
                    DnsHostName = d.DisplayName ?? "",
                    OperatingSystem = $"{d.OperatingSystem} {d.OperatingSystemVersion}".Trim(),
                    IsEnabled = d.AccountEnabled ?? false,
                    LastLogon = d.ApproximateLastSignInDateTime?.DateTime,
                    Description = d.DeviceId ?? ""
                });
            }

            return computers;
        }
        catch { return []; }
    }

    public async Task<bool> EnableUserAsync(string sam)
    {
        try
        {
            var client = GetClient();
            var userId = await ResolveUserIdAsync(sam);
            if (userId == null) return false;

            await client.Users[userId].PatchAsync(new Microsoft.Graph.Models.User
            {
                AccountEnabled = true
            });
            return true;
        }
        catch { return false; }
    }

    public async Task<bool> DisableUserAsync(string sam)
    {
        try
        {
            var client = GetClient();
            var userId = await ResolveUserIdAsync(sam);
            if (userId == null) return false;

            await client.Users[userId].PatchAsync(new Microsoft.Graph.Models.User
            {
                AccountEnabled = false
            });
            return true;
        }
        catch { return false; }
    }

    public Task<bool> UnlockUserAsync(string sam)
    {
        // Entra ID doesn't have explicit unlock; accounts auto-unlock or require password reset.
        // We reset the sign-in block flag — same effect for cloud-only accounts.
        return EnableUserAsync(sam);
    }

    public async Task<bool> ResetPasswordAsync(string sam, string newPassword, bool mustChange = true)
    {
        try
        {
            var client = GetClient();
            var userId = await ResolveUserIdAsync(sam);
            if (userId == null) return false;

            await client.Users[userId].PatchAsync(new Microsoft.Graph.Models.User
            {
                PasswordProfile = new PasswordProfile
                {
                    Password = newPassword,
                    ForceChangePasswordNextSignIn = mustChange
                }
            });
            return true;
        }
        catch { return false; }
    }

    public async Task<bool> SetUPNAsync(string sam, string newUpn)
    {
        try
        {
            var client = GetClient();
            var userId = await ResolveUserIdAsync(sam);
            if (userId == null) return false;

            await client.Users[userId].PatchAsync(new Microsoft.Graph.Models.User
            {
                UserPrincipalName = newUpn
            });
            return true;
        }
        catch { return false; }
    }

    public async Task<bool> AddProxyAddressAsync(string sam, string smtpAddress)
    {
        try
        {
            var client = GetClient();
            var userId = await ResolveUserIdAsync(sam);
            if (userId == null) return false;

            var user = await client.Users[userId].GetAsync(req =>
                req.QueryParameters.Select = ["proxyAddresses"]);

            var addresses = user?.ProxyAddresses?.ToList() ?? [];
            var normalized = smtpAddress.StartsWith("smtp:", StringComparison.OrdinalIgnoreCase)
                ? smtpAddress
                : $"smtp:{smtpAddress}";

            if (!addresses.Any(a => a.Equals(normalized, StringComparison.OrdinalIgnoreCase)))
                addresses.Add(normalized);

            await client.Users[userId].PatchAsync(new Microsoft.Graph.Models.User
            {
                ProxyAddresses = addresses
            });
            return true;
        }
        catch { return false; }
    }

    public async Task<bool> RemoveProxyAddressAsync(string sam, string smtpAddress)
    {
        try
        {
            var client = GetClient();
            var userId = await ResolveUserIdAsync(sam);
            if (userId == null) return false;

            var user = await client.Users[userId].GetAsync(req =>
                req.QueryParameters.Select = ["proxyAddresses"]);

            var addresses = user?.ProxyAddresses?.ToList() ?? [];
            addresses.RemoveAll(a => a.Contains(smtpAddress, StringComparison.OrdinalIgnoreCase));

            await client.Users[userId].PatchAsync(new Microsoft.Graph.Models.User
            {
                ProxyAddresses = addresses
            });
            return true;
        }
        catch { return false; }
    }

    public async Task<bool> CreateUserAsync(AdUser user, string password)
    {
        try
        {
            var client = GetClient();
            var config = Config.AppConfig.Instance;

            await client.Users.PostAsync(new Microsoft.Graph.Models.User
            {
                AccountEnabled = true,
                DisplayName = user.DisplayName,
                GivenName = user.FirstName,
                Surname = user.LastName,
                UserPrincipalName = user.UserPrincipalName,
                MailNickname = user.SamAccountName,
                Department = user.Department,
                JobTitle = user.Title,
                BusinessPhones = [user.OfficePhone],
                PasswordProfile = new PasswordProfile
                {
                    Password = password,
                    ForceChangePasswordNextSignIn = true
                }
            });
            return true;
        }
        catch { return false; }
    }

    public async Task<List<AdUser>> GetLockedOutUsersAsync()
    {
        // Entra ID doesn't expose "locked out" the same way.
        // Return users who are disabled as the closest equivalent.
        try
        {
            var client = GetClient();
            var response = await client.Users.GetAsync(req =>
            {
                req.QueryParameters.Select = UserSelectFields();
                req.QueryParameters.Filter = "accountEnabled eq false";
                req.QueryParameters.Top = 50;
            });

            var users = new List<AdUser>();
            if (response?.Value == null) return users;
            foreach (var u in response.Value)
                users.Add(MapUser(u));
            return users;
        }
        catch { return []; }
    }

    // --- Helpers ---

    private async Task<string?> ResolveUserIdAsync(string identifier)
    {
        try
        {
            var client = GetClient();
            // identifier may be UPN (contains @) or onPremisesSamAccountName
            if (identifier.Contains('@'))
                return identifier; // Graph accepts UPN directly as the ID segment

            var response = await client.Users.GetAsync(req =>
            {
                req.QueryParameters.Select = ["id"];
                req.QueryParameters.Filter = $"onPremisesSamAccountName eq '{EscapeFilter(identifier)}'";
                req.QueryParameters.Top = 1;
            });
            return response?.Value?.FirstOrDefault()?.Id;
        }
        catch { return null; }
    }

    private static AdUser MapUser(Microsoft.Graph.Models.User u)
    {
        var managerName = "";
        if (u.Manager is Microsoft.Graph.Models.User mgr)
            managerName = mgr.DisplayName ?? mgr.UserPrincipalName ?? "";

        return new AdUser
        {
            SamAccountName = u.OnPremisesSamAccountName ?? u.MailNickname ?? "",
            DisplayName = u.DisplayName ?? "",
            UserPrincipalName = u.UserPrincipalName ?? "",
            Email = u.Mail ?? u.UserPrincipalName ?? "",
            FirstName = u.GivenName ?? "",
            LastName = u.Surname ?? "",
            Department = u.Department ?? "",
            Title = u.JobTitle ?? "",
            Manager = managerName,
            OfficePhone = u.BusinessPhones?.FirstOrDefault() ?? u.MobilePhone ?? "",
            IsEnabled = u.AccountEnabled ?? false,
            IsLockedOut = false, // Not directly available in Entra ID for cloud-only
            HasO365License = u.AssignedLicenses?.Count > 0,
            LastLogon = u.SignInActivity?.LastSignInDateTime?.DateTime,
            PasswordLastSet = u.LastPasswordChangeDateTime?.DateTime,
            Created = u.CreatedDateTime?.DateTime ?? DateTime.MinValue,
            MemberOf = [],
            ProxyAddresses = u.ProxyAddresses?.ToList() ?? []
        };
    }

    private static string[] UserSelectFields() =>
    [
        "id", "displayName", "givenName", "surname", "userPrincipalName",
        "mail", "mailNickname", "onPremisesSamAccountName",
        "department", "jobTitle", "businessPhones", "mobilePhone",
        "accountEnabled", "assignedLicenses", "proxyAddresses",
        "createdDateTime", "lastPasswordChangeDateTime", "signInActivity"
    ];

    private static string EscapeFilter(string s) => s.Replace("'", "''");
}

internal class TokenProvider : IAccessTokenProvider
{
    private readonly IAuthService _auth;
    private readonly string[] _scopes;

    public TokenProvider(IAuthService auth, string[] scopes)
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
