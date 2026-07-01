using System.Text.Json;

namespace SupportToolApp.Config;

public class AppConfig
{
    private static AppConfig? _instance;
    private static readonly string ConfigPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "SupportToolApp", "config.json");

    public static AppConfig Instance => _instance ??= Load();

    public string Domain { get; set; } = "contoso.com";
    public string TenantId { get; set; } = "";
    public string ClientId { get; set; } = "";
    public string AdminUpn { get; set; } = "";

    public List<ServerEntry> Servers { get; set; } =
    [
        new() { Name = "File Server 1", Host = "fs01.contoso.com", Description = "Primary File Server" },
        new() { Name = "File Server 2", Host = "fs02.contoso.com", Description = "Secondary File Server" },
        new() { Name = "Domain Controller", Host = "dc01.contoso.com", Description = "Primary DC" },
    ];

    public string TeamViewerPath { get; set; } = @"C:\Program Files\TeamViewer\TeamViewer.exe";
    public string SccmConsolePath { get; set; } = @"C:\Program Files (x86)\Microsoft Endpoint Manager\AdminConsole\bin\Microsoft.ConfigurationManagement.exe";

    // Configurable deployment tool (replaces hardcoded SCCM button)
    public string DeployToolName { get; set; } = "SCCM / ConfigMgr Console";
    public string DeployToolUrl  { get; set; } = @"C:\Program Files (x86)\Microsoft Endpoint Manager\AdminConsole\bin\Microsoft.ConfigurationManagement.exe";

    public bool UseMockData { get; set; } = true;

    public List<WebAppEntry> WebApps { get; set; } =
    [
        new() { Name = "Office 365",  Url = "https://office.com",      Color = "#0078D4" },
        new() { Name = "Google Admin",Url = "https://admin.google.com", Color = "#4285F4" },
    ];

    public static AppConfig Load()
    {
        try
        {
            if (File.Exists(ConfigPath))
            {
                var json = File.ReadAllText(ConfigPath);
                return JsonSerializer.Deserialize<AppConfig>(json) ?? new AppConfig();
            }
        }
        catch { }
        return new AppConfig();
    }

    public void Save()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(ConfigPath)!);
        var json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(ConfigPath, json);
    }
}

public class ServerEntry
{
    public string Name { get; set; } = "";
    public string Host { get; set; } = "";
    public string Description { get; set; } = "";
}

public class WebAppEntry
{
    public string Name  { get; set; } = "";
    public string Url   { get; set; } = "";
    public string Color { get; set; } = "#0078D4";
}
