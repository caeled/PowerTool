using System.Text.Json;

namespace ToolsAvaloniaApp.Models;

public class QuickLink
{
    public string Name { get; set; } = "";
    public string Url  { get; set; } = "";

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(Name) &&
        !string.IsNullOrWhiteSpace(Url) &&
        (Url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
         Url.StartsWith("https://", StringComparison.OrdinalIgnoreCase));
}

public class QuickLinksConfig
{
    public QuickLink[] Links { get; set; } = Enumerable.Range(0, 4).Select(_ => new QuickLink()).ToArray();

    private static string ConfigPath()
    {
        var candidates = new[]
        {
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".config", "PowellTecTools", "quicklinks.json"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PowellTecTools", "quicklinks.json"),
            Path.Combine(AppContext.BaseDirectory, "quicklinks.json"),
        };
        return candidates.FirstOrDefault(File.Exists) ?? candidates[0];
    }

    public static QuickLinksConfig Load()
    {
        try
        {
            var path = ConfigPath();
            if (!File.Exists(path)) return new QuickLinksConfig();
            var cfg = JsonSerializer.Deserialize<QuickLinksConfig>(File.ReadAllText(path));
            if (cfg == null) return new QuickLinksConfig();
            // Ensure always 4 slots
            if (cfg.Links.Length < 4)
            {
                var links = cfg.Links.ToList();
                while (links.Count < 4) links.Add(new QuickLink());
                cfg.Links = links.ToArray();
            }
            return cfg;
        }
        catch { return new QuickLinksConfig(); }
    }

    public void Save()
    {
        try
        {
            var path = ConfigPath();
            var dir  = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
            File.WriteAllText(path, JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true }));
        }
        catch { }
    }
}
