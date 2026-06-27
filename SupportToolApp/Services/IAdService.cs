using SupportToolApp.Models;

namespace SupportToolApp.Services;

public interface IAdService
{
    Task<AdUser?> GetUserAsync(string identifier);
    Task<List<AdUser>> SearchUsersAsync(string query);
    Task<List<AdComputer>> GetComputersAsync(string filter = "");

    Task<bool> EnableUserAsync(string samAccountName);
    Task<bool> DisableUserAsync(string samAccountName);
    Task<bool> UnlockUserAsync(string samAccountName);
    Task<bool> ResetPasswordAsync(string samAccountName, string newPassword, bool mustChange = true);
    Task<bool> SetUPNAsync(string samAccountName, string newUpn);

    Task<bool> AddProxyAddressAsync(string samAccountName, string smtpAddress);
    Task<bool> RemoveProxyAddressAsync(string samAccountName, string smtpAddress);

    Task<bool> CreateUserAsync(AdUser user, string password);
    Task<List<AdUser>> GetLockedOutUsersAsync();
}
