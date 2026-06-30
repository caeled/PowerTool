using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace ToolsAvaloniaApp.Helpers;

public static class PsRunner
{
    private static string? _relayUrl;
    private static string? _relayApiKey;
    private static bool    _configLoaded;

    private static void LoadConfig()
    {
        if (_configLoaded) return;
        _configLoaded = true;
        try
        {
            var candidates = new[]
            {
                Path.Combine(AppContext.BaseDirectory, "relay.json"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "relay.json"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PowellTecTools", "relay.json"),
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".config", "PowellTecTools", "relay.json"),
            };
            var path = candidates.FirstOrDefault(File.Exists);
            if (path == null) return;
            var doc = JsonDocument.Parse(File.ReadAllText(path));
            doc.RootElement.TryGetProperty("RelayUrl",    out var u); _relayUrl    = u.GetString();
            doc.RootElement.TryGetProperty("RelayApiKey", out var k); _relayApiKey = k.GetString();
        }
        catch { }
    }

    private static HttpClient MakeClient()
    {
        var http = new HttpClient { Timeout = TimeSpan.FromSeconds(120) };
        if (!string.IsNullOrWhiteSpace(_relayApiKey))
            http.DefaultRequestHeaders.Add("X-Api-Key", _relayApiKey);
        return http;
    }

    private static string RelayBase => _relayUrl!.TrimEnd('/');

    private static readonly JsonSerializerOptions _jsonOpts =
        new() { PropertyNameCaseInsensitive = true };

    public static async Task<string> RunAsync(string script)
    {
        LoadConfig();

        if (!OperatingSystem.IsWindows())
        {
            if (string.IsNullOrWhiteSpace(_relayUrl))
                return "ERROR: No relay configured.\nCreate relay.json next to the app:\n{\"RelayUrl\":\"http://192.168.1.x:5000\",\"RelayApiKey\":\"changeme\"}";
            try
            {
                using var http = MakeClient();
                var body    = JsonSerializer.Serialize(new { Script = script });
                var content = new StringContent(body, Encoding.UTF8, "application/json");
                var resp    = await http.PostAsync($"{RelayBase}/run", content);
                var json    = await resp.Content.ReadAsStringAsync();
                var result  = JsonSerializer.Deserialize<RelayResponse>(json, _jsonOpts);
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
            using var proc  = Process.Start(psi)!;
            var outTask     = proc.StandardOutput.ReadToEndAsync();
            var errTask     = proc.StandardError.ReadToEndAsync();
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

    public static async Task<string> UploadFileAsync(string localPath, string destPath,
        string? target = null, string? username = null, string? password = null)
    {
        LoadConfig();
        if (string.IsNullOrWhiteSpace(_relayUrl))
            return "ERROR: No relay configured.";
        try
        {
            var fileName = Path.GetFileName(localPath);
            var fileSize = new FileInfo(localPath).Length;
            var timeout  = TimeSpan.FromSeconds(30 + fileSize / 1024.0 / 1024.0);
            using var http    = new HttpClient { Timeout = timeout };
            if (!string.IsNullOrWhiteSpace(_relayApiKey))
                http.DefaultRequestHeaders.Add("X-Api-Key", _relayApiKey);
            using var form    = new MultipartFormDataContent();
            using var stream  = File.OpenRead(localPath);
            using var fileContent = new StreamContent(stream);
            form.Add(new StringContent(destPath), "destPath");
            form.Add(fileContent, "file", fileName);
            if (!string.IsNullOrWhiteSpace(target))   form.Add(new StringContent(target),   "target");
            if (!string.IsNullOrWhiteSpace(username)) form.Add(new StringContent(username), "username");
            if (!string.IsNullOrWhiteSpace(password)) form.Add(new StringContent(password), "password");
            var resp   = await http.PostAsync($"{RelayBase}/upload", form);
            var json   = await resp.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<RelayResponse>(json, _jsonOpts);
            return (result?.Output ?? json).Trim();
        }
        catch (Exception ex) { return $"UPLOAD ERROR: {ex.Message}"; }
    }
}

record RelayResponse(string? Output, bool Success);
