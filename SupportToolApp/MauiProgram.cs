using Microsoft.Extensions.Logging;
using SupportToolApp.Pages.Email;
using SupportToolApp.Pages.RemoteAccess;
using SupportToolApp.Pages.Reporting;
using SupportToolApp.Pages.Search;
using SupportToolApp.Pages.Settings;
using SupportToolApp.Pages.Spam;
using SupportToolApp.Pages.SystemInfo;
using SupportToolApp.Pages.UserManager;
using SupportToolApp.Services;
using SupportToolApp.Pages.Credentials;
using SupportToolApp.Pages.Tools;
using SupportToolApp.Pages.Help;
using SupportToolApp.Pages.Compare;
using SupportToolApp.Pages.WebApps;
using SupportToolApp.Services.Graph;
using SupportToolApp.Services.Mock;
using SupportToolApp.ViewModels;
using SupportToolApp.ViewModels.Tools;

namespace SupportToolApp;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // Credential service — singleton so all pages share the same instance
        builder.Services.AddSingleton<CredentialService>();

        // PowerShell runner — local on Windows, PowellTecRelay elsewhere
        builder.Services.AddSingleton<PsRunnerService>();

        // Auth service (always registered — mock ignores it)
        builder.Services.AddSingleton<IAuthService, GraphAuthService>();

        // Data services — swap implementations based on AppConfig.UseMockData
        if (Config.AppConfig.Instance.UseMockData)
        {
            builder.Services.AddSingleton<IAdService, MockAdService>();
            builder.Services.AddSingleton<IExchangeService, MockExchangeService>();
        }
        else
        {
            builder.Services.AddSingleton<IAdService, GraphAdService>();
            builder.Services.AddSingleton<IExchangeService, GraphExchangeService>();
        }

        // ViewModels
        builder.Services.AddTransient<ToolsViewModel>();
        builder.Services.AddTransient<UserManagerViewModel>();
        builder.Services.AddTransient<EmailViewModel>();
        builder.Services.AddTransient<SpamViewModel>();
        builder.Services.AddTransient<RemoteAccessViewModel>();
        builder.Services.AddTransient<ReportingViewModel>();
        builder.Services.AddTransient<SystemInfoViewModel>();
        builder.Services.AddTransient<SearchViewModel>();
        builder.Services.AddTransient<SettingsViewModel>();

        // Pages
        builder.Services.AddTransient<ToolsPage>();
        builder.Services.AddTransient<UserManagerPage>();
        builder.Services.AddTransient<EmailPage>();
        builder.Services.AddTransient<SpamPage>();
        builder.Services.AddTransient<RemoteAccessPage>();
        builder.Services.AddTransient<ReportingPage>();
        builder.Services.AddTransient<SystemInfoPage>();
        builder.Services.AddTransient<SearchPage>();
        builder.Services.AddTransient<SettingsPage>();
        builder.Services.AddTransient<SetCredentialsPage>();
        builder.Services.AddTransient<HelpPage>();
        builder.Services.AddTransient<ComparePage>();
        builder.Services.AddTransient<WebAppsPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
