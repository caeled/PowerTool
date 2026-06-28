using System.Text.Json;
using SupportToolApp.Services;

namespace SupportToolApp.Config;

/// <summary>
/// Portable config snapshot — contains all settings except credentials (passwords).
/// </summary>
public class ConfigExport
{
    // AppConfig fields
    public string Domain         { get; set; } = "";
    public string TenantId       { get; set; } = "";
    public string ClientId       { get; set; } = "";
    public string AdminUpn       { get; set; } = "";
    public string TeamViewerPath { get; set; } = "";
    public string SccmConsolePath{ get; set; } = "";
    public string DeployToolName { get; set; } = "";
    public string DeployToolUrl  { get; set; } = "";
    public bool   UseMockData    { get; set; }
    public List<ServerEntry> Servers { get; set; } = [];

    // CredentialService non-credential fields
    public string QuickLink1Name { get; set; } = "";
    public string QuickLink1Url  { get; set; } = "";
    public string QuickLink2Name { get; set; } = "";
    public string QuickLink2Url  { get; set; } = "";
    public string QuickLink3Name { get; set; } = "";
    public string QuickLink3Url  { get; set; } = "";
    public string QuickLink4Name { get; set; } = "";
    public string QuickLink4Url  { get; set; } = "";
    public string FolderName     { get; set; } = "";
    public string LocalFolder    { get; set; } = "";
    public string LibraryName    { get; set; } = "";
    public string LibraryUrl     { get; set; } = "";
    public string LogoPath       { get; set; } = "";
    public string LogoInitials   { get; set; } = "IT";
    public string LogoColor      { get; set; } = "#0078D4";

    public static ConfigExport Capture(CredentialService creds)
    {
        var cfg = AppConfig.Instance;
        return new ConfigExport
        {
            Domain          = cfg.Domain,
            TenantId        = cfg.TenantId,
            ClientId        = cfg.ClientId,
            AdminUpn        = cfg.AdminUpn,
            TeamViewerPath  = cfg.TeamViewerPath,
            SccmConsolePath = cfg.SccmConsolePath,
            DeployToolName  = cfg.DeployToolName,
            DeployToolUrl   = cfg.DeployToolUrl,
            UseMockData     = cfg.UseMockData,
            Servers         = [.. cfg.Servers],
            QuickLink1Name  = creds.QuickLink1Name,
            QuickLink1Url   = creds.QuickLink1Url,
            QuickLink2Name  = creds.QuickLink2Name,
            QuickLink2Url   = creds.QuickLink2Url,
            QuickLink3Name  = creds.QuickLink3Name,
            QuickLink3Url   = creds.QuickLink3Url,
            QuickLink4Name  = creds.QuickLink4Name,
            QuickLink4Url   = creds.QuickLink4Url,
            FolderName      = creds.FolderName,
            LocalFolder     = creds.LocalFolder,
            LibraryName     = creds.LibraryName,
            LibraryUrl      = creds.LibraryUrl,
            LogoPath        = creds.LogoPath,
            LogoInitials    = creds.LogoInitials,
            LogoColor       = creds.LogoColor,
        };
    }

    public void Apply(CredentialService creds)
    {
        var cfg = AppConfig.Instance;
        cfg.Domain          = Domain;
        cfg.TenantId        = TenantId;
        cfg.ClientId        = ClientId;
        cfg.AdminUpn        = AdminUpn;
        cfg.TeamViewerPath  = TeamViewerPath;
        cfg.SccmConsolePath = SccmConsolePath;
        cfg.DeployToolName  = DeployToolName;
        cfg.DeployToolUrl   = DeployToolUrl;
        cfg.UseMockData     = UseMockData;
        cfg.Servers         = [.. Servers];
        cfg.Save();

        creds.SetQuickLinks(
            QuickLink1Name, QuickLink1Url,
            QuickLink2Name, QuickLink2Url,
            QuickLink3Name, QuickLink3Url,
            QuickLink4Name, QuickLink4Url);
        creds.FolderName   = FolderName;
        creds.LocalFolder  = LocalFolder;
        creds.LibraryName  = LibraryName;
        creds.LibraryUrl   = LibraryUrl;
        creds.LogoPath     = LogoPath;
        creds.LogoInitials = LogoInitials;
        creds.LogoColor    = LogoColor;
    }

    public string ToJson() =>
        JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });

    public static ConfigExport? FromJson(string json) =>
        JsonSerializer.Deserialize<ConfigExport>(json);
}
