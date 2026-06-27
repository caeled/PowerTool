namespace SupportToolApp.Services;

public interface IAuthService
{
    Task<bool> SignInAsync();
    Task SignOutAsync();
    Task<string?> GetAccessTokenAsync(IEnumerable<string> scopes);
    bool IsSignedIn { get; }
    string? SignedInUser { get; }
}
