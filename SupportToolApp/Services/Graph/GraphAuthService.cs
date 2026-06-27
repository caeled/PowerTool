using Microsoft.Identity.Client;
using Microsoft.Identity.Client.Extensions.Msal;

namespace SupportToolApp.Services.Graph;

public class GraphAuthService : IAuthService
{
    private static readonly string[] DefaultScopes =
    [
        "User.ReadWrite.All",
        "Directory.ReadWrite.All",
        "Group.ReadWrite.All",
        "Mail.ReadWrite",
        "MailboxSettings.ReadWrite",
        "RoleManagement.ReadWrite.Directory"
    ];

    private IPublicClientApplication? _pca;
    private IAccount? _account;

    public bool IsSignedIn => _account != null;
    public string? SignedInUser => _account?.Username;

    private IPublicClientApplication GetApp()
    {
        if (_pca != null) return _pca;

        var config = Config.AppConfig.Instance;
        _pca = PublicClientApplicationBuilder
            .Create(config.ClientId)
            .WithAuthority($"https://login.microsoftonline.com/{config.TenantId}")
            .WithRedirectUri("http://localhost")
            .Build();

        _ = RegisterTokenCacheAsync(_pca.UserTokenCache);
        return _pca;
    }

    private static async Task RegisterTokenCacheAsync(ITokenCache tokenCache)
    {
        var cacheDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "SupportToolApp");
        Directory.CreateDirectory(cacheDir);

        var storageProperties = new StorageCreationPropertiesBuilder("msal.cache", cacheDir)
            .Build();

        var cacheHelper = await MsalCacheHelper.CreateAsync(storageProperties);
        cacheHelper.RegisterCache(tokenCache);
    }

    public async Task<bool> SignInAsync()
    {
        try
        {
            var app = GetApp();
            var accounts = await app.GetAccountsAsync();
            _account = accounts.FirstOrDefault();

            AuthenticationResult result;
            if (_account != null)
            {
                result = await app.AcquireTokenSilent(DefaultScopes, _account).ExecuteAsync();
            }
            else
            {
                result = await app.AcquireTokenInteractive(DefaultScopes).ExecuteAsync();
            }

            _account = result.Account;
            return true;
        }
        catch (MsalException)
        {
            _account = null;
            return false;
        }
    }

    public async Task SignOutAsync()
    {
        if (_account != null)
        {
            var app = GetApp();
            await app.RemoveAsync(_account);
            _account = null;
        }
    }

    public async Task<string?> GetAccessTokenAsync(IEnumerable<string> scopes)
    {
        try
        {
            var app = GetApp();
            var scopeList = scopes.ToList();

            if (_account == null)
            {
                var accounts = await app.GetAccountsAsync();
                _account = accounts.FirstOrDefault();
            }

            if (_account == null) return null;

            var result = await app.AcquireTokenSilent(scopeList, _account).ExecuteAsync();
            return result.AccessToken;
        }
        catch (MsalUiRequiredException)
        {
            await SignInAsync();
            return await GetAccessTokenAsync(scopes);
        }
        catch (MsalException)
        {
            return null;
        }
    }
}
