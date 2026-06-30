using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace ToolsAvaloniaApp.Helpers;

public static class PsRunner
{
    // Relay config — loaded once from relay.json next to the app
    private static string? _relayUrl;
    private static string? _relayApiKey;
    private static bool    _configLoaded;

    private static void LoadConfig()
    {
        if (_configLoaded) return;
        _configLoaded = true;
        try
        {
            var dir  = AppContext.BaseDirectory;
            var path = Path.Combine(dir, "relay.json");
            if (!File.Exists(path)) return;
            var doc = JsonDocument.Parse(File.ReadAllText(path));
            doc.RootElement.TryGetProperty("RelayUrl",    out var u); _relayUrl    = u.GetString();
            doc.RootElement.TryGetProperty("RelayApiKey", out var k); _relayApiKey = k.GetString();
        }
        catch { }
    }

    public static async Task<string> RunAsync(string script)
    {
        LoadConfig();

        // On non-Windows, route through the relay service
        if (!OperatingSystem.IsWindows())
        {
            if (string.IsNullOrWhiteSpace(_relayUrl))
                return "ERROR: No relay configured.\nCreate relay.json next to the app:\n{\"RelayUrl\":\"http://192.168.1.x:5000\",\"RelayApiKey\":\"changeme\"}";
            try
            {
                using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(120) };
                if (!string.IsNullOrWhiteSpace(_relayApiKey))
                    http.DefaultRequestHeaders.Add("X-Api-Key", _relayApiKey);
                var body    = JsonSerializer.Serialize(new { Script = script });
                var content = new StringContent(body, Encoding.UTF8, "application/json");
                var resp    = await http.PostAsync($"{_relayUrl.TrimEnd('/')}/run", content);
                var json    = await resp.Content.ReadAsStringAsync();
                var result  = JsonSerializer.Deserialize<RelayResponse>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return (result?.Output ?? json).Trim();
            }
            catch (Exception ex) { return $"RELAY ERROR: {ex.Message}"; }
        }

        // Windows — run locally
        var tmp = Path.Combine(Path.GetTempPath(), $"pttools_{Guid.NewGuid():N}.ps1");
        await File.WriteAllTextAsync(tmp, script, new UTF8Encoding(false));
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName               = "powershell.exe",
                Arguments              = $"-NoProfile -ExecutionPolicy Bypass -File \"{tmp}\"",
                RedirectStandardOutput = true,
                RedirectStandardError  = true,
                UseShellExecute        = false,
                CreateNoWindow         = true
            };
            using var proc   = Process.Start(psi)!;
            var outTask      = proc.StandardOutput.ReadToEndAsync();
            var errTask      = proc.StandardError.ReadToEndAsync();
            await proc.WaitForExitAsync();
            var stdout = await outTask;
            var stderr = await errTask;
            var sb = new StringBuilder();
            if (!string.IsNullOrWhiteSpace(stdout)) sb.Append(stdout);
            if (!string.IsNullOrWhiteSpace(stderr)) sb.AppendLine().Append("ERRORS: ").Append(stderr);
            return sb.ToString().Trim();
        }
        finally { try { File.Delete(tmp); } catch { } }
    }
}

record RelayResponse(string? Output, bool Success);