using System.Diagnostics;
using System.Text;
using System.Text.Json;

namespace SupportToolApp.Services;

/// <summary>
/// Single execution path for PowerShell across platforms.
/// Windows runs powershell.exe locally; everything else (Android, Mac Catalyst,
/// iOS) routes the script to the PowellTecRelay service over HTTP (POST /run).
/// Replaces the duplicated private runners that lived in ToolsViewModel and
/// SystemInfoViewModel.
/// </summary>
public class PsRunnerService
{
    private readonly CredentialService _creds;

    private static readonly JsonSerializerOptions CaseInsensitive = new() { PropertyNameCaseInsensitive = true };
    private sealed record RelayRunResponse(string Output, bool Success);

    // One shared client per app — a new HttpClient per call exhausts sockets.
    // Per-request headers are set on HttpRequestMessage, never on this client,
    // so concurrent calls can't race on DefaultRequestHeaders.
    private static readonly HttpClient Http = new() { Timeout = TimeSpan.FromSeconds(120) };

    public PsRunnerService(CredentialService creds) => _creds = creds;

    /// <summary>True when scripts execute via the relay instead of local PowerShell.</summary>
    public static bool UsesRelay => !OperatingSystem.IsWindows();

    /// <summary>
    /// Run a PowerShell script and return its output. Error results come back as
    /// strings prefixed with "ERROR:", "RELAY ERROR:" or "STDERR:" so existing
    /// call sites keep their current behavior.
    /// </summary>
    public string Run(string script) => UsesRelay ? RunViaRelay(script) : RunLocal(script);

    public Task<string> RunAsync(string script) => Task.Run(() => Run(script));

    /// <summary>Hit the relay's GET /ping health endpoint. Returns a human-readable status line.</summary>
    public Task<string> TestRelayAsync() => TestRelayAsync(_creds.RelayUrl);

    /// <summary>Ping an arbitrary relay URL (lets Settings test the typed value before saving).</summary>
    public static async Task<string> TestRelayAsync(string relayUrl)
    {
        if (string.IsNullOrWhiteSpace(relayUrl))
            return "No relay URL configured.";
        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(6));
            var resp = await Http.GetAsync($"{relayUrl.TrimEnd('/')}/ping", cts.Token);
            var body = await resp.Content.ReadAsStringAsync(cts.Token);
            return resp.IsSuccessStatusCode
                ? $"✓ Relay reachable — {body}"
                : $"✗ Relay answered {(int)resp.StatusCode}: {body}";
        }
        catch (TaskCanceledException) { return "✗ Cannot reach relay: timed out after 6s."; }
        catch (Exception ex)          { return $"✗ Cannot reach relay: {ex.Message}"; }
    }

    private string RunViaRelay(string script)
    {
        if (!_creds.HasRelay)
            return "ERROR: No relay configured. Enter the relay URL in Settings.";
        try
        {
            using var req = new HttpRequestMessage(HttpMethod.Post, $"{_creds.RelayUrl.TrimEnd('/')}/run");
            if (!string.IsNullOrWhiteSpace(_creds.RelayApiKey))
                req.Headers.Add("X-Api-Key", _creds.RelayApiKey);
            req.Content = new StringContent(
                JsonSerializer.Serialize(new { Script = script }), Encoding.UTF8, "application/json");

            // Sync-over-async is deliberate: callers already wrap Run() in Task.Run,
            // and HttpClient's synchronous Send() isn't supported by the Android handler.
            var response = Http.SendAsync(req).GetAwaiter().GetResult();
            var json = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            if (!response.IsSuccessStatusCode)
                return $"RELAY ERROR: HTTP {(int)response.StatusCode} — {json}";

            // The relay serializes with camelCase ("output"/"success"), so the
            // property lookup MUST be case-insensitive — a case-sensitive
            // TryGetProperty("Output") silently misses and returns the raw blob.
            var result = JsonSerializer.Deserialize<RelayRunResponse>(json, CaseInsensitive);
            return result?.Output ?? json;
        }
        catch (Exception ex) { return $"RELAY ERROR: {ex.Message}"; }
    }

    private static string RunLocal(string script)
    {
        var temp = Path.Combine(Path.GetTempPath(), $"tool_{Guid.NewGuid():N}.ps1");
        File.WriteAllText(temp, script, new UTF8Encoding(encoderShouldEmitUTF8Identifier: true));
        try
        {
            var psi = new ProcessStartInfo("powershell.exe",
                $"-NoProfile -NonInteractive -ExecutionPolicy Bypass -File \"{temp}\"")
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            using var proc = Process.Start(psi)!;
            var stdout = proc.StandardOutput.ReadToEnd();
            var stderr = proc.StandardError.ReadToEnd();
            proc.WaitForExit();
            if (!string.IsNullOrWhiteSpace(stderr) && string.IsNullOrWhiteSpace(stdout))
                return $"STDERR: {stderr.Trim()}";
            return stdout;
        }
        finally { try { File.Delete(temp); } catch { } }
    }
}
