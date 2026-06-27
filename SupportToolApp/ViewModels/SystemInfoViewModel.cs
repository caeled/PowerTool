using System.Diagnostics;
using System.Text.Json;
using System.Windows.Input;

namespace SupportToolApp.ViewModels;

public class DiskInfo
{
    public string Drive { get; set; } = "";
    public long TotalBytes { get; set; }
    public long FreeBytes { get; set; }
    public double UsedPercent => TotalBytes > 0 ? (double)(TotalBytes - FreeBytes) / TotalBytes : 0;
    public string SummaryDisplay => $"{(TotalBytes - FreeBytes) / 1_073_741_824.0:F1}/{TotalBytes / 1_073_741_824.0:F1} GB";
}

public class SystemInfoData
{
    public string ComputerName { get; set; } = "";
    public string OperatingSystem { get; set; } = "";
    public string Manufacturer { get; set; } = "";
    public string Model { get; set; } = "";
    public string SerialNumber { get; set; } = "";
    public long TotalRamBytes { get; set; }
    public string TotalRamDisplay => $"{TotalRamBytes / 1_073_741_824.0:F1} GB";
    public string Processor { get; set; } = "";
    public string IpAddress { get; set; } = "";
    public string LastBootTime { get; set; } = "";
    public List<DiskInfo> Disks { get; set; } = [];
}

public class SystemInfoViewModel : BaseViewModel
{
    private SystemInfoData? _info;
    private bool _hasInfo;
    private bool _isBusy;
    private string _errorMessage = "";
    private bool _hasError;

    public SystemInfoData? Info { get => _info; set { SetProperty(ref _info, value); HasInfo = value != null; } }
    public bool HasInfo { get => _hasInfo; set => SetProperty(ref _hasInfo, value); }
    public bool IsBusy { get => _isBusy; set => SetProperty(ref _isBusy, value); }
    public string ErrorMessage { get => _errorMessage; set { SetProperty(ref _errorMessage, value); HasError = !string.IsNullOrEmpty(value); } }
    public bool HasError { get => _hasError; set => SetProperty(ref _hasError, value); }

    public ICommand QueryCommand { get; }

    public SystemInfoViewModel()
    {
        QueryCommand = new Command<string>(async target =>
        {
            ErrorMessage = "";
            Info = null;
            IsBusy = true;
            try
            {
                var result = await Task.Run(() => QueryViaPowerShell(target));
                Info = result;
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Failed: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        });
    }

    private static SystemInfoData QueryViaPowerShell(string? target)
    {
        var isLocal = string.IsNullOrWhiteSpace(target)
                   || target.Equals("localhost", StringComparison.OrdinalIgnoreCase)
                   || target.Equals(Environment.MachineName, StringComparison.OrdinalIgnoreCase);

        var computerParam = isLocal ? "" : $"-ComputerName '{target}'";

        var script = $$"""
$ErrorActionPreference = 'Stop'
$os   = Get-WmiObject Win32_OperatingSystem {{computerParam}}
$cs   = Get-WmiObject Win32_ComputerSystem  {{computerParam}}
$bios = Get-WmiObject Win32_BIOS            {{computerParam}}
$cpu  = Get-WmiObject Win32_Processor       {{computerParam}} | Select-Object -First 1
$net  = Get-WmiObject Win32_NetworkAdapterConfiguration {{computerParam}} | Where-Object { $_.IPEnabled } | Select-Object -First 1
$disks = Get-WmiObject Win32_LogicalDisk    {{computerParam}} | Where-Object { $_.DriveType -eq 3 }

$bootTime = [System.Management.ManagementDateTimeConverter]::ToDateTime($os.LastBootUpTime).ToString('g')

$result = [ordered]@{
    ComputerName  = $cs.Name
    OS            = "$($os.Caption) (Build $($os.Version))"
    Manufacturer  = $cs.Manufacturer
    Model         = $cs.Model
    Serial        = $bios.SerialNumber
    RamBytes      = [long]$cs.TotalPhysicalMemory
    Processor     = $cpu.Name
    IpAddress     = ($net.IPAddress | Where-Object { $_ -match '^\d+\.\d+\.\d+\.\d+$' } | Select-Object -First 1)
    LastBoot      = $bootTime
    Disks         = @($disks | ForEach-Object {
        [ordered]@{ Drive = $_.DeviceID; Total = [long]$_.Size; Free = [long]$_.FreeSpace }
    })
}

$result | ConvertTo-Json -Depth 3
""";

        var psi = new ProcessStartInfo("powershell.exe",
            $"-NoProfile -NonInteractive -Command \"{script.Replace("\"", "\\\"")}\"")
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        // Simpler: write script to temp file to avoid quoting nightmares
        var tempScript = Path.Combine(Path.GetTempPath(), $"sysinfo_{Guid.NewGuid():N}.ps1");
        File.WriteAllText(tempScript, script);

        try
        {
            var psi2 = new ProcessStartInfo("powershell.exe",
                $"-NoProfile -NonInteractive -ExecutionPolicy Bypass -File \"{tempScript}\"")
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var proc = Process.Start(psi2)!;
            var json = proc.StandardOutput.ReadToEnd();
            var err  = proc.StandardError.ReadToEnd();
            proc.WaitForExit();

            if (string.IsNullOrWhiteSpace(json))
                throw new Exception(string.IsNullOrWhiteSpace(err) ? "No output from PowerShell." : err.Trim());

            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            var info = new SystemInfoData
            {
                ComputerName  = root.GetStringOrEmpty("ComputerName"),
                OperatingSystem = root.GetStringOrEmpty("OS"),
                Manufacturer  = root.GetStringOrEmpty("Manufacturer"),
                Model         = root.GetStringOrEmpty("Model"),
                SerialNumber  = root.GetStringOrEmpty("Serial"),
                TotalRamBytes = root.TryGetProperty("RamBytes", out var ram) ? ram.GetInt64() : 0,
                Processor     = root.GetStringOrEmpty("Processor"),
                IpAddress     = root.GetStringOrEmpty("IpAddress"),
                LastBootTime  = root.GetStringOrEmpty("LastBoot"),
            };

            if (root.TryGetProperty("Disks", out var disks) && disks.ValueKind == JsonValueKind.Array)
            {
                foreach (var d in disks.EnumerateArray())
                {
                    info.Disks.Add(new DiskInfo
                    {
                        Drive      = d.GetStringOrEmpty("Drive"),
                        TotalBytes = d.TryGetProperty("Total", out var t) ? t.GetInt64() : 0,
                        FreeBytes  = d.TryGetProperty("Free",  out var f) ? f.GetInt64() : 0,
                    });
                }
            }

            return info;
        }
        finally
        {
            try { File.Delete(tempScript); } catch { }
        }
    }
}

internal static class JsonElementExtensions
{
    public static string GetStringOrEmpty(this JsonElement el, string prop)
        => el.TryGetProperty(prop, out var v) && v.ValueKind == JsonValueKind.String
            ? v.GetString() ?? ""
            : "";
}
