using System.Diagnostics;
using System.Text;

namespace ToolsAvaloniaApp.Helpers;

public static class PsRunner
{
    public static async Task<string> RunAsync(string script)
    {
        var psExe = OperatingSystem.IsWindows() ? "powershell.exe" : "pwsh";

        // Write script to temp file (avoids BOM/encoding issues with stdin)
        var tmp = Path.Combine(Path.GetTempPath(), $"pttools_{Guid.NewGuid():N}.ps1");
        await File.WriteAllTextAsync(tmp, script, new UTF8Encoding(false));

        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = psExe,
                Arguments = $"-NoProfile -ExecutionPolicy Bypass -File \"{tmp}\"",
                RedirectStandardOutput = true,
                RedirectStandardError  = true,
                UseShellExecute = false,
                CreateNoWindow  = true
            };

            using var proc = Process.Start(psi)!;
            var outTask = proc.StandardOutput.ReadToEndAsync();
            var errTask = proc.StandardError.ReadToEndAsync();
            await proc.WaitForExitAsync();

            var stdout = await outTask;
            var stderr = await errTask;

            var sb = new StringBuilder();
            if (!string.IsNullOrWhiteSpace(stdout)) sb.Append(stdout);
            if (!string.IsNullOrWhiteSpace(stderr)) sb.AppendLine().Append("ERRORS: ").Append(stderr);
            return sb.ToString().Trim();
        }
        finally
        {
            try { File.Delete(tmp); } catch { }
        }
    }
}