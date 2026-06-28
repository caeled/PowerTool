using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Windows.Input;
using SupportToolApp.Services;

namespace SupportToolApp.ViewModels;

// ── Data models ───────────────────────────────────────────────────────────────

public class OverviewData
{
    public string ComputerName  { get; set; } = "";
    public string OS            { get; set; } = "";
    public string LastBoot      { get; set; } = "";
    public string Uptime        { get; set; } = "";
    public string Manufacturer  { get; set; } = "";
    public string Model         { get; set; } = "";
    public string Serial        { get; set; } = "";
    public string BiosVersion   { get; set; } = "";
    public string Domain        { get; set; } = "";
    public bool   PartOfDomain  { get; set; }
    public string LoggedOnUser  { get; set; } = "";
    public string LastLogon     { get; set; } = "";
    public string RamGB         { get; set; } = "";
    public string Processor     { get; set; } = "";
    public int    CpuCores      { get; set; }
    public string IpAddress     { get; set; } = "";
    public string DomainOrWorkgroup => PartOfDomain ? $"Domain: {Domain}" : $"Workgroup: {Domain}";
}

public class RamModule
{
    public string Slot    { get; set; } = "";
    public string Size    { get; set; } = "";
    public string Speed   { get; set; } = "";
    public string MemType { get; set; } = "";
    public string Summary => $"{Size} {MemType} @ {Speed}  [{Slot}]";
}

public class HardwareData
{
    public string CpuName      { get; set; } = "";
    public int    CpuCores     { get; set; }
    public int    CpuLogical   { get; set; }
    public string CpuSpeed     { get; set; } = "";
    public string GpuName      { get; set; } = "";
    public string GpuRam       { get; set; } = "";
    public string GpuDriver    { get; set; } = "";
    public string BiosVersion  { get; set; } = "";
    public string BiosDate     { get; set; } = "";
    public string BiosVendor   { get; set; } = "";
    public int    RamSlotCount { get; set; }
    public bool   HasBattery   { get; set; }
    public string BatteryPct   { get; set; } = "";
    public List<RamModule> RamModules { get; set; } = [];
    public string CpuSummary => $"{CpuName}  —  {CpuCores} cores / {CpuLogical} logical @ {CpuSpeed}";
}

public class DiskInfo
{
    public string Drive      { get; set; } = "";
    public string Label      { get; set; } = "";
    public long   TotalBytes { get; set; }
    public long   FreeBytes  { get; set; }
    public bool   IsNetwork  { get; set; }
    public double UsedPercent => TotalBytes > 0 ? (double)(TotalBytes - FreeBytes) / TotalBytes : 0;
    public string SummaryDisplay => TotalBytes > 0
        ? $"{(TotalBytes - FreeBytes) / 1_073_741_824.0:F1} / {TotalBytes / 1_073_741_824.0:F1} GB"
        : "Size unavailable";
    public string DriveLabel
    {
        get
        {
            var label = string.IsNullOrWhiteSpace(Label) ? Drive : $"{Drive}  {Label}";
            return IsNetwork ? $"{label}  [Network]" : label;
        }
    }
}

public class NetworkAdapter
{
    public string Name        { get; set; } = "";
    public string Mac         { get; set; } = "";
    public string IPs         { get; set; } = "";
    public string Gateway     { get; set; } = "";
    public string DNS         { get; set; } = "";
    public bool   DHCPEnabled { get; set; }
    public string DHCPServer  { get; set; } = "";
    public string IpMode      => DHCPEnabled ? $"DHCP  ({DHCPServer})" : "Static";
}

public class AppItem
{
    public string Name        { get; set; } = "";
    public string Version     { get; set; } = "";
    public string Publisher   { get; set; } = "";
    public string InstallDate { get; set; } = "";
}

public class SecurityData
{
    public string FirewallDomain  { get; set; } = "";
    public string FirewallPrivate { get; set; } = "";
    public string FirewallPublic  { get; set; } = "";
    public string AntiVirus       { get; set; } = "";
    public string BitLocker       { get; set; } = "";
    public string LastHotfix      { get; set; } = "";
    public string UAC             { get; set; } = "";
    public string WUAutoUpdate    { get; set; } = "";
    public string ProductKey      { get; set; } = "";
    public string LocalAdmins     { get; set; } = "";
}

public class LoginEvent
{
    public string User   { get; set; } = "";
    public string Time   { get; set; } = "";
    public string Type   { get; set; } = "";
}

public class SharedFolder
{
    public string Name        { get; set; } = "";
    public string Path        { get; set; } = "";
    public string Description { get; set; } = "";
    public string ShareType   { get; set; } = "";
}

public class ServiceItem
{
    public string Name      { get; set; } = "";
    public string Caption   { get; set; } = "";
    public string State     { get; set; } = "";
    public string StartMode { get; set; } = "";
    public string StartName { get; set; } = "";
}

// ── ViewModel ─────────────────────────────────────────────────────────────────

public class SystemInfoViewModel : BaseViewModel
{
    private readonly CredentialService _creds;

    private string _target = "", _statusMessage = "", _softwareSearch = "", _serviceSearch = "", _errorMessage = "";
    private string _loginCountText = "10";
    private bool _isBusy, _hasData, _hasError, _isLoadingLogins;
    private OverviewData? _overview;
    private HardwareData? _hardware;
    private SecurityData? _security;

    public string Target        { get => _target;        set => SetProperty(ref _target, value); }
    public string StatusMessage { get => _statusMessage; set => SetProperty(ref _statusMessage, value); }
    public string ErrorMessage  { get => _errorMessage;  set { SetProperty(ref _errorMessage, value); HasError = !string.IsNullOrEmpty(value); } }
    public bool   IsBusy          { get => _isBusy;         set => SetProperty(ref _isBusy, value); }
    public bool   HasData         { get => _hasData;        set => SetProperty(ref _hasData, value); }
    public bool   HasError        { get => _hasError;       set => SetProperty(ref _hasError, value); }
    public bool   IsLoadingLogins { get => _isLoadingLogins; set => SetProperty(ref _isLoadingLogins, value); }

    public string LoginCountText
    {
        get => _loginCountText;
        set => SetProperty(ref _loginCountText, value);
    }

    public string CredentialStatus => string.IsNullOrWhiteSpace(_creds.UserUsername)
        ? "No credentials set — local queries only"
        : $"Using: {_creds.UserDomain}\\{_creds.UserUsername}";

    public OverviewData? Overview { get => _overview; set => SetProperty(ref _overview, value); }
    public HardwareData? Hardware { get => _hardware; set => SetProperty(ref _hardware, value); }
    public SecurityData? Security { get => _security; set => SetProperty(ref _security, value); }

    public ObservableCollection<LoginEvent>     RecentLogins    { get; } = [];
    public ObservableCollection<DiskInfo>       Disks           { get; } = [];
    public ObservableCollection<NetworkAdapter> Adapters        { get; } = [];
    public ObservableCollection<SharedFolder>   Shares          { get; } = [];
    public ObservableCollection<AppItem>        AllApps         { get; } = [];
    public ObservableCollection<AppItem>        FilteredApps    { get; } = [];
    public ObservableCollection<ServiceItem>    AllServices     { get; } = [];
    public ObservableCollection<ServiceItem>    FilteredServices{ get; } = [];

    public string SoftwareSearch
    {
        get => _softwareSearch;
        set { if (SetProperty(ref _softwareSearch, value)) ApplySoftwareFilter(); }
    }

    public string ServiceSearch
    {
        get => _serviceSearch;
        set { if (SetProperty(ref _serviceSearch, value)) ApplyServiceFilter(); }
    }

    public string AppCount     => $"{FilteredApps.Count} of {AllApps.Count} apps";
    public string ServiceCount => $"{FilteredServices.Count} of {AllServices.Count} services";

    public ICommand QueryCommand        { get; }
    public ICommand ExportCommand       { get; }
    public ICommand QueryLoginsCommand  { get; }

    public SystemInfoViewModel(CredentialService creds)
    {
        _creds = creds;

        QueryCommand = new Command(async () =>
        {
            ErrorMessage = "";
            HasData = false;
            IsBusy = true;
            Overview = null; Hardware = null; Security = null;
            Disks.Clear(); Adapters.Clear(); Shares.Clear();
            AllApps.Clear(); FilteredApps.Clear();
            AllServices.Clear(); FilteredServices.Clear();
            RecentLogins.Clear();
            SoftwareSearch = ""; ServiceSearch = "";

            var t = Target.Trim();
            StatusMessage = string.IsNullOrWhiteSpace(t) ? "Querying local machine…" : $"Querying {t}…";

            try
            {
                await Task.WhenAll(
                    QueryOverviewAsync(t),
                    QueryHardwareAsync(t),
                    QueryDisksAsync(t),
                    QueryNetworkAsync(t),
                    QuerySharedFoldersAsync(t),
                    QuerySoftwareAsync(t),
                    QuerySecurityAsync(t),
                    QueryServicesAsync(t));
                HasData = true;
                var name = Overview?.ComputerName ?? (string.IsNullOrWhiteSpace(t) ? Environment.MachineName : t);
                StatusMessage = $"Ready — {name}";
                OnPropertyChanged(nameof(AppCount));
                OnPropertyChanged(nameof(ServiceCount));
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Query failed: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        });

        ExportCommand = new Command(async () => { if (HasData) await ExportReportAsync(); });

        QueryLoginsCommand = new Command(async () =>
        {
            if (!HasData || IsLoadingLogins) return;
            if (!int.TryParse(_loginCountText.Trim(), out var count) || count < 1) count = 10;
            if (count > 500) count = 500;
            IsLoadingLogins = true;
            RecentLogins.Clear();
            var t = Target.Trim();
            var logins = await Task.Run(() => QueryLoginsPs(t, count));
            foreach (var l in logins) RecentLogins.Add(l);
            IsLoadingLogins = false;
        });
    }

    // ── PS helpers ────────────────────────────────────────────────────────────

    private string WrapScript(string target, string body)
    {
        var isLocal = string.IsNullOrWhiteSpace(target)
                   || target.Equals("localhost", StringComparison.OrdinalIgnoreCase)
                   || target.Equals(Environment.MachineName, StringComparison.OrdinalIgnoreCase);
        if (isLocal) return body;

        var pass = _creds.UserPassword.Replace("'", "''");
        var user = $"{_creds.UserDomain}\\{_creds.UserUsername}";
        return $$"""
            $pw   = ConvertTo-SecureString '{{pass}}' -AsPlainText -Force
            $cred = New-Object System.Management.Automation.PSCredential('{{user}}', $pw)
            Invoke-Command -ComputerName '{{target}}' -Credential $cred -ScriptBlock {
                {{body}}
            }
            """;
    }

    private static string RunPs(string script)
    {
        var tmp = Path.Combine(Path.GetTempPath(), $"si_{Guid.NewGuid():N}.ps1");
        File.WriteAllText(tmp, script, new UTF8Encoding(true));
        try
        {
            var psi = new ProcessStartInfo("powershell.exe",
                $"-NoProfile -NonInteractive -ExecutionPolicy Bypass -File \"{tmp}\"")
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            using var proc = Process.Start(psi)!;
            var output = proc.StandardOutput.ReadToEnd();
            var error  = proc.StandardError.ReadToEnd();
            proc.WaitForExit();
            if (string.IsNullOrWhiteSpace(output) && !string.IsNullOrWhiteSpace(error))
                try { File.WriteAllText(Path.Combine(Path.GetTempPath(), "si_debug_last.txt"), $"=== SCRIPT ===\n{script}\n\n=== ERROR ===\n{error}"); } catch { }
            return output.Trim();
        }
        finally { try { File.Delete(tmp); } catch { } }
    }

    private static string  Str(JsonElement el, string p) =>
        el.TryGetProperty(p, out var v) && v.ValueKind == JsonValueKind.String ? v.GetString() ?? "" : "";
    private static bool    Bool(JsonElement el, string p) =>
        el.TryGetProperty(p, out var v) && v.ValueKind == JsonValueKind.True;
    private static int     Int(JsonElement el, string p) =>
        el.TryGetProperty(p, out var v) && v.TryGetInt32(out var i) ? i : 0;

    // ── Queries ───────────────────────────────────────────────────────────────

    private async Task QueryOverviewAsync(string target)
    {
        const string body = """
            $ErrorActionPreference = 'SilentlyContinue'
            $os   = Get-CimInstance Win32_OperatingSystem
            $cs   = Get-CimInstance Win32_ComputerSystem
            $bio  = Get-CimInstance Win32_BIOS
            $cpu  = Get-CimInstance Win32_Processor | Select-Object -First 1
            $net  = Get-CimInstance Win32_NetworkAdapterConfiguration | Where-Object { $_.IPEnabled } | Select-Object -First 1
            $up   = (Get-Date) - $os.LastBootUpTime
            $prof = try { Get-CimInstance Win32_UserProfile -ErrorAction Stop | Where-Object { -not $_.Special } | Sort-Object LastUseTime -Descending | Select-Object -First 1 } catch { $null }
            $lastLogon = if ($prof -and $prof.LastUseTime) { "$($prof.LocalPath.Split('\')[-1]) ($($prof.LastUseTime.ToString('yyyy-MM-dd HH:mm')))" } else { '' }
            [ordered]@{
                ComputerName = [string]$cs.Name
                OS           = [string]"$($os.Caption) (Build $($os.BuildNumber))"
                LastBoot     = [string]$os.LastBootUpTime.ToString('g')
                Uptime       = [string]('{0}d {1}h {2}m' -f [int]$up.TotalDays, $up.Hours, $up.Minutes)
                Manufacturer = [string]$cs.Manufacturer
                Model        = [string]$cs.Model
                Serial       = [string]$bio.SerialNumber
                BiosVersion  = [string]$bio.SMBIOSBIOSVersion
                Domain       = [string]$cs.Domain
                PartOfDomain = [bool]$cs.PartOfDomain
                LoggedOnUser = [string]$cs.UserName
                LastLogon    = [string]$lastLogon
                RamGB        = [string]('{0:F1} GB' -f ($cs.TotalPhysicalMemory / 1GB))
                Processor    = [string]$cpu.Name
                CpuCores     = [int]$cpu.NumberOfCores
                IpAddress    = [string]($net.IPAddress | Where-Object { $_ -match '^\d+\.\d+\.\d+\.\d+$' } | Select-Object -First 1)
            } | ConvertTo-Json
            """;
        var json = await Task.Run(() => RunPs(WrapScript(target, body)));
        if (string.IsNullOrWhiteSpace(json)) return;
        using var doc = JsonDocument.Parse(json);
        var r = doc.RootElement;
        Overview = new OverviewData
        {
            ComputerName = Str(r, "ComputerName"), OS           = Str(r, "OS"),
            LastBoot     = Str(r, "LastBoot"),     Uptime       = Str(r, "Uptime"),
            Manufacturer = Str(r, "Manufacturer"), Model        = Str(r, "Model"),
            Serial       = Str(r, "Serial"),       BiosVersion  = Str(r, "BiosVersion"),
            Domain       = Str(r, "Domain"),       PartOfDomain = Bool(r, "PartOfDomain"),
            LoggedOnUser = Str(r, "LoggedOnUser"), LastLogon    = Str(r, "LastLogon"),
            RamGB        = Str(r, "RamGB"),
            Processor    = Str(r, "Processor"),    CpuCores     = Int(r, "CpuCores"),
            IpAddress    = Str(r, "IpAddress"),
        };
    }

    private async Task QueryHardwareAsync(string target)
    {
        const string body = """
            $ErrorActionPreference = 'SilentlyContinue'
            $cpu  = Get-CimInstance Win32_Processor -ErrorAction SilentlyContinue | Select-Object -First 1
            $gpu  = Get-CimInstance Win32_VideoController -ErrorAction SilentlyContinue | Select-Object -First 1
            $ram  = @(Get-CimInstance Win32_PhysicalMemory -ErrorAction SilentlyContinue)
            $bio  = Get-CimInstance Win32_BIOS -ErrorAction SilentlyContinue
            $bat  = Get-CimInstance Win32_Battery -ErrorAction SilentlyContinue
            $memT = @{20='DDR';21='DDR2';24='DDR3';26='DDR4';34='DDR5'}
            $mods = @($ram | ForEach-Object {
                $mt = [int]$_.MemoryType
                [ordered]@{
                    Slot    = [string]$_.DeviceLocator
                    Size    = [string]('{0}GB' -f [math]::Round($_.Capacity/1GB))
                    Speed   = [string]('{0}MHz' -f $_.Speed)
                    MemType = if ($memT.ContainsKey($mt)) { $memT[$mt] } else { 'Unknown' }
                }
            })
            $gpuRam  = if ($gpu -and $gpu.AdapterRAM -gt 0) { '{0:F1} GB' -f ($gpu.AdapterRAM/1GB) } else { 'N/A' }
            $bioDate = if ($bio -and $bio.ReleaseDate) { $bio.ReleaseDate.ToString('yyyy-MM-dd') } else { '' }
            $batPct  = if ($bat) { '{0}%' -f $bat.EstimatedChargeRemaining } else { 'N/A' }
            [ordered]@{
                CpuName      = [string]$cpu.Name
                CpuCores     = [int]$cpu.NumberOfCores
                CpuLogical   = [int]$cpu.NumberOfLogicalProcessors
                CpuSpeed     = [string]('{0} MHz' -f $cpu.MaxClockSpeed)
                GpuName      = [string]$gpu.Name
                GpuRam       = [string]$gpuRam
                GpuDriver    = [string]$gpu.DriverVersion
                BiosVersion  = [string]$bio.SMBIOSBIOSVersion
                BiosDate     = [string]$bioDate
                BiosVendor   = [string]$bio.Manufacturer
                RamSlotCount = [int]($mods.Count)
                RamModules   = $mods
                HasBattery   = [bool]($null -ne $bat)
                BatteryPct   = [string]$batPct
            } | ConvertTo-Json -Depth 3
            """;
        var json = await Task.Run(() => RunPs(WrapScript(target, body)));
        if (string.IsNullOrWhiteSpace(json)) return;
        using var doc = JsonDocument.Parse(json);
        var r = doc.RootElement;
        var hw = new HardwareData
        {
            CpuName      = Str(r, "CpuName"),    CpuCores     = Int(r, "CpuCores"),
            CpuLogical   = Int(r, "CpuLogical"), CpuSpeed     = Str(r, "CpuSpeed"),
            GpuName      = Str(r, "GpuName"),    GpuRam       = Str(r, "GpuRam"),
            GpuDriver    = Str(r, "GpuDriver"),  BiosVersion  = Str(r, "BiosVersion"),
            BiosDate     = Str(r, "BiosDate"),   BiosVendor   = Str(r, "BiosVendor"),
            RamSlotCount = Int(r, "RamSlotCount"), HasBattery = Bool(r, "HasBattery"),
            BatteryPct   = Str(r, "BatteryPct"),
        };
        if (r.TryGetProperty("RamModules", out var mods) && mods.ValueKind == JsonValueKind.Array)
            foreach (var m in mods.EnumerateArray())
                hw.RamModules.Add(new RamModule
                {
                    Slot = Str(m, "Slot"), Size = Str(m, "Size"),
                    Speed = Str(m, "Speed"), MemType = Str(m, "MemType"),
                });
        Hardware = hw;
    }

    private async Task QueryDisksAsync(string target)
    {
        const string body = """
            $ErrorActionPreference = 'SilentlyContinue'
            $disks = @(Get-CimInstance Win32_LogicalDisk | Where-Object { $_.DriveType -in 2,3,4 } | ForEach-Object {
                [ordered]@{ Drive=[string]$_.DeviceID; Label=[string]$_.VolumeName; Total=[long]$_.Size; Free=[long]$_.FreeSpace; IsNetwork=[bool]($_.DriveType -eq 4) }
            })
            $seenDrives = @($disks | ForEach-Object { $_.Drive })
            $mapped = @()
            Get-ChildItem 'Registry::HKEY_USERS' -ErrorAction SilentlyContinue |
                Where-Object { $_.PSChildName -match '^S-1-5-21' -and $_.PSChildName -notmatch '_Classes$' } |
                ForEach-Object {
                    $netKey = "Registry::HKEY_USERS\$($_.PSChildName)\Network"
                    if (Test-Path $netKey) {
                        Get-ChildItem $netKey -ErrorAction SilentlyContinue | ForEach-Object {
                            $letter = "$($_.PSChildName):"
                            $unc = $_.GetValue('RemotePath')
                            if ($unc -and $letter -notin $seenDrives) {
                                $seenDrives += $letter
                                $di = try { [System.IO.DriveInfo]::new($unc) } catch { $null }
                                $nTotal = if ($di) { $di.TotalSize } else { 0 }
                                $nFree  = if ($di) { $di.AvailableFreeSpace } else { 0 }
                                $mapped += [ordered]@{ Drive=$letter; Label=[string]$unc; Total=[long]$nTotal; Free=[long]$nFree; IsNetwork=$true }
                            }
                        }
                    }
                }
            @($disks + $mapped) | ConvertTo-Json
            """;
        var json = await Task.Run(() => RunPs(WrapScript(target, body)));
        if (string.IsNullOrWhiteSpace(json)) return;
        using var doc = JsonDocument.Parse(json);
        var arr = doc.RootElement.ValueKind == JsonValueKind.Array
            ? doc.RootElement.EnumerateArray().ToList()
            : new List<JsonElement> { doc.RootElement };
        MainThread.BeginInvokeOnMainThread(() =>
        {
            Disks.Clear();
            foreach (var d in arr)
                Disks.Add(new DiskInfo
                {
                    Drive = Str(d, "Drive"), Label = Str(d, "Label"),
                    TotalBytes = d.TryGetProperty("Total", out var t) ? t.GetInt64() : 0,
                    FreeBytes  = d.TryGetProperty("Free",  out var f) ? f.GetInt64() : 0,
                    IsNetwork  = Bool(d, "IsNetwork"),
                });
        });
    }

    private async Task QueryNetworkAsync(string target)
    {
        const string body = """
            $ErrorActionPreference = 'SilentlyContinue'
            @(Get-CimInstance Win32_NetworkAdapterConfiguration | Where-Object { $_.IPEnabled } | ForEach-Object {
                [ordered]@{
                    Name        = [string]$_.Description
                    Mac         = [string]$_.MACAddress
                    IPs         = [string]($_.IPAddress -join ', ')
                    Gateway     = [string]($_.DefaultIPGateway -join ', ')
                    DNS         = [string]($_.DNSServerSearchOrder -join ', ')
                    DHCPEnabled = [bool]$_.DHCPEnabled
                    DHCPServer  = [string]$_.DHCPServer
                }
            }) | ConvertTo-Json -Depth 2
            """;
        var json = await Task.Run(() => RunPs(WrapScript(target, body)));
        if (string.IsNullOrWhiteSpace(json)) return;
        using var doc = JsonDocument.Parse(json);
        var arr = doc.RootElement.ValueKind == JsonValueKind.Array
            ? doc.RootElement.EnumerateArray().ToList()
            : new List<JsonElement> { doc.RootElement };
        MainThread.BeginInvokeOnMainThread(() =>
        {
            Adapters.Clear();
            foreach (var a in arr)
                Adapters.Add(new NetworkAdapter
                {
                    Name = Str(a, "Name"), Mac = Str(a, "Mac"),
                    IPs = Str(a, "IPs"), Gateway = Str(a, "Gateway"),
                    DNS = Str(a, "DNS"), DHCPEnabled = Bool(a, "DHCPEnabled"),
                    DHCPServer = Str(a, "DHCPServer"),
                });
        });
    }

    private async Task QuerySoftwareAsync(string target)
    {
        const string body = """
            $ErrorActionPreference = 'SilentlyContinue'
            $apps = @()
            $apps += Get-ItemProperty 'HKLM:\Software\Microsoft\Windows\CurrentVersion\Uninstall\*' -ErrorAction SilentlyContinue
            $apps += Get-ItemProperty 'HKLM:\Software\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\*' -ErrorAction SilentlyContinue
            @($apps | Where-Object { $_.DisplayName } | Sort-Object DisplayName | ForEach-Object {
                [ordered]@{
                    Name        = [string]$_.DisplayName
                    Version     = [string]$_.DisplayVersion
                    Publisher   = [string]$_.Publisher
                    InstallDate = [string]$_.InstallDate
                }
            }) | ConvertTo-Json
            """;
        var json = await Task.Run(() => RunPs(WrapScript(target, body)));
        if (string.IsNullOrWhiteSpace(json)) return;
        using var doc = JsonDocument.Parse(json);
        var arr = doc.RootElement.ValueKind == JsonValueKind.Array
            ? doc.RootElement.EnumerateArray().ToList()
            : new List<JsonElement> { doc.RootElement };
        var items = arr.Select(a => new AppItem
        {
            Name = Str(a, "Name"), Version = Str(a, "Version"),
            Publisher = Str(a, "Publisher"), InstallDate = Str(a, "InstallDate"),
        }).ToList();
        MainThread.BeginInvokeOnMainThread(() =>
        {
            AllApps.Clear();
            foreach (var item in items) AllApps.Add(item);
            ApplySoftwareFilter();
            OnPropertyChanged(nameof(AppCount));
        });
    }

    private async Task QuerySecurityAsync(string target)
    {
        const string body = """
            $ErrorActionPreference = 'SilentlyContinue'
            $fwDomain  = try { [int](Get-ItemProperty 'HKLM:\SYSTEM\CurrentControlSet\Services\SharedAccess\Parameters\FirewallPolicy\DomainProfile'  -ErrorAction Stop).EnableFirewall } catch { -1 }
            $fwPrivate = try { [int](Get-ItemProperty 'HKLM:\SYSTEM\CurrentControlSet\Services\SharedAccess\Parameters\FirewallPolicy\StandardProfile' -ErrorAction Stop).EnableFirewall } catch { -1 }
            $fwPublic  = try { [int](Get-ItemProperty 'HKLM:\SYSTEM\CurrentControlSet\Services\SharedAccess\Parameters\FirewallPolicy\PublicProfile'   -ErrorAction Stop).EnableFirewall } catch { -1 }
            $av  = try { Get-CimInstance -Namespace root/SecurityCenter2 -ClassName AntiVirusProduct -ErrorAction Stop | Select-Object -First 1 } catch { $null }
            $bl  = try { Get-BitLockerVolume -MountPoint C: -ErrorAction Stop } catch { $null }
            $wu  = try { Get-HotFix -ErrorAction Stop | Sort-Object InstalledOn -Descending | Select-Object -First 1 } catch { $null }
            $uac = try { [int](Get-ItemProperty 'HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System' -ErrorAction Stop).EnableLUA } catch { -1 }
            $wua = try { [int](Get-ItemProperty 'HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\WindowsUpdate\Auto Update' -ErrorAction Stop).AUOptions } catch { -1 }
            $fwD  = if ($fwDomain  -eq 1) { 'Enabled' } elseif ($fwDomain  -eq 0) { 'Disabled' } else { 'Unknown' }
            $fwPr = if ($fwPrivate -eq 1) { 'Enabled' } elseif ($fwPrivate -eq 0) { 'Disabled' } else { 'Unknown' }
            $fwPu = if ($fwPublic  -eq 1) { 'Enabled' } elseif ($fwPublic  -eq 0) { 'Disabled' } else { 'Unknown' }
            $avName  = if ($av) { $av.displayName } else { 'None detected' }
            $blState = if ($bl) { $bl.ProtectionStatus.ToString() } else { 'N/A' }
            $lastHf  = if ($wu -and $wu.InstalledOn) { "$($wu.HotFixID) ($($wu.InstalledOn.ToString('yyyy-MM-dd')))" } elseif ($wu) { $wu.HotFixID } else { 'Unknown' }
            $uacStr  = if ($uac -eq 1) { 'Enabled' } elseif ($uac -eq 0) { 'Disabled' } else { 'Unknown' }
            $wuaStr  = if ($wua -eq 2) { 'Notify only' } elseif ($wua -eq 3) { 'Auto Download' } elseif ($wua -eq 4) { 'Auto Install' } else { 'Not configured' }
            $pk      = try { (Get-CimInstance SoftwareLicensingService -ErrorAction Stop).OA3xOriginalProductKey } catch { '' }
            $pkStr   = if ($pk) { $pk } else { 'Not available' }
            $admins  = try { @(Get-LocalGroupMember -Group 'Administrators' -ErrorAction Stop | ForEach-Object { $_.Name }) -join ', ' } catch { 'N/A' }
            [ordered]@{
                FirewallDomain  = [string]$fwD
                FirewallPrivate = [string]$fwPr
                FirewallPublic  = [string]$fwPu
                AntiVirus       = [string]$avName
                BitLocker       = [string]$blState
                LastHotfix      = [string]$lastHf
                UAC             = [string]$uacStr
                WUAutoUpdate    = [string]$wuaStr
                ProductKey      = [string]$pkStr
                LocalAdmins     = [string]$admins
            } | ConvertTo-Json
            """;
        var json = await Task.Run(() => RunPs(WrapScript(target, body)));
        if (string.IsNullOrWhiteSpace(json)) return;
        using var doc = JsonDocument.Parse(json);
        var r = doc.RootElement;
        Security = new SecurityData
        {
            FirewallDomain  = Str(r, "FirewallDomain"),  FirewallPrivate = Str(r, "FirewallPrivate"),
            FirewallPublic  = Str(r, "FirewallPublic"),  AntiVirus       = Str(r, "AntiVirus"),
            BitLocker       = Str(r, "BitLocker"),        LastHotfix      = Str(r, "LastHotfix"),
            UAC             = Str(r, "UAC"),              WUAutoUpdate    = Str(r, "WUAutoUpdate"),
            ProductKey      = Str(r, "ProductKey"),       LocalAdmins     = Str(r, "LocalAdmins"),
        };
    }

    private List<LoginEvent> QueryLoginsPs(string target, int count)
    {
        var body = $"$n = {count}\n$nMax = {count * 20 + 50}\n" + """
            $ErrorActionPreference = 'SilentlyContinue'
            $cutoff  = (Get-Date).AddDays(-5)
            $typeMap = @{2='Interactive';3='Network';4='Batch';5='Service';7='Unlock';8='Network (Cleartext)';9='New Credentials';10='Remote (RDP)';11='Cached Interactive';12='Cached Remote';13='Cached Unlock'}
            $errMsg  = $null
            $events  = try {
                Get-WinEvent -FilterHashtable @{LogName='Security'; Id=4624} -MaxEvents $nMax -ErrorAction Stop |
                    Where-Object {
                        $_.Properties[5].Value -notmatch '^\$' -and
                        $_.Properties[5].Value -ne 'ANONYMOUS LOGON' -and
                        $_.Properties[5].Value -ne '-'
                    } |
                    Select-Object -First $n
            } catch { $errMsg = $_.Exception.Message; @() }
            $events  = @($events)
            if ($errMsg) { Write-Error "LOGIN_ERR: $errMsg" }
            @($events | ForEach-Object {
                $user   = $_.Properties[5].Value
                $domain = $_.Properties[6].Value
                $ltype  = [int]$_.Properties[8].Value
                $ltStr  = if ($typeMap.ContainsKey($ltype)) { $typeMap[$ltype] } else { "Type $ltype" }
                [ordered]@{
                    User = [string]"$domain\$user"
                    Time = [string]$_.TimeCreated.ToString('yyyy-MM-dd HH:mm:ss')
                    Type = [string]$ltStr
                }
            }) | ConvertTo-Json
            """;
        var json = RunPs(WrapScript(target, body));
        if (string.IsNullOrWhiteSpace(json)) return [];
        try
        {
            using var doc = JsonDocument.Parse(json);
            var arr = doc.RootElement.ValueKind == JsonValueKind.Array
                ? doc.RootElement.EnumerateArray().ToList()
                : new List<JsonElement> { doc.RootElement };
            return arr.Select(e => new LoginEvent
            {
                User = Str(e, "User"), Time = Str(e, "Time"), Type = Str(e, "Type"),
            }).ToList();
        }
        catch { return []; }
    }

    private async Task QuerySharedFoldersAsync(string target)
    {
        const string body = """
            $ErrorActionPreference = 'SilentlyContinue'
            @(Get-CimInstance Win32_Share -ErrorAction SilentlyContinue | ForEach-Object {
                [ordered]@{
                    Name        = [string]$_.Name
                    Path        = [string]$_.Path
                    Description = [string]$_.Description
                    ShareType   = [string]$_.Type
                }
            }) | ConvertTo-Json
            """;
        var json = await Task.Run(() => RunPs(WrapScript(target, body)));
        if (string.IsNullOrWhiteSpace(json)) return;
        using var doc = JsonDocument.Parse(json);
        var arr = doc.RootElement.ValueKind == JsonValueKind.Array
            ? doc.RootElement.EnumerateArray().ToList()
            : new List<JsonElement> { doc.RootElement };
        MainThread.BeginInvokeOnMainThread(() =>
        {
            Shares.Clear();
            foreach (var s in arr)
                Shares.Add(new SharedFolder
                {
                    Name = Str(s, "Name"), Path = Str(s, "Path"),
                    Description = Str(s, "Description"), ShareType = Str(s, "ShareType"),
                });
        });
    }

    private async Task QueryServicesAsync(string target)
    {
        const string body = """
            $ErrorActionPreference = 'SilentlyContinue'
            @(Get-CimInstance Win32_Service -ErrorAction SilentlyContinue |
                Sort-Object StartMode, State, Name | ForEach-Object {
                    [ordered]@{
                        Name      = [string]$_.Name
                        Caption   = [string]$_.Caption
                        State     = [string]$_.State
                        StartMode = [string]$_.StartMode
                        StartName = [string]$_.StartName
                    }
                }) | ConvertTo-Json
            """;
        var json = await Task.Run(() => RunPs(WrapScript(target, body)));
        if (string.IsNullOrWhiteSpace(json)) return;
        using var doc = JsonDocument.Parse(json);
        var arr = doc.RootElement.ValueKind == JsonValueKind.Array
            ? doc.RootElement.EnumerateArray().ToList()
            : new List<JsonElement> { doc.RootElement };
        var items = arr.Select(a => new ServiceItem
        {
            Name      = Str(a, "Name"),    Caption   = Str(a, "Caption"),
            State     = Str(a, "State"),   StartMode = Str(a, "StartMode"),
            StartName = Str(a, "StartName"),
        }).ToList();
        MainThread.BeginInvokeOnMainThread(() =>
        {
            AllServices.Clear();
            foreach (var item in items) AllServices.Add(item);
            ApplyServiceFilter();
            OnPropertyChanged(nameof(ServiceCount));
        });
    }

    private void ApplyServiceFilter()
    {
        FilteredServices.Clear();
        var s = _serviceSearch.Trim();
        foreach (var svc in AllServices)
            if (string.IsNullOrEmpty(s) ||
                svc.Name.Contains(s, StringComparison.OrdinalIgnoreCase) ||
                svc.Caption.Contains(s, StringComparison.OrdinalIgnoreCase) ||
                svc.State.Contains(s, StringComparison.OrdinalIgnoreCase) ||
                svc.StartMode.Contains(s, StringComparison.OrdinalIgnoreCase))
                FilteredServices.Add(svc);
        OnPropertyChanged(nameof(ServiceCount));
    }

    private void ApplySoftwareFilter()
    {
        FilteredApps.Clear();
        var s = _softwareSearch.Trim();
        foreach (var app in AllApps)
            if (string.IsNullOrEmpty(s) ||
                app.Name.Contains(s, StringComparison.OrdinalIgnoreCase) ||
                app.Publisher.Contains(s, StringComparison.OrdinalIgnoreCase))
                FilteredApps.Add(app);
        OnPropertyChanged(nameof(AppCount));
    }

    // ── Export ────────────────────────────────────────────────────────────────

    private async Task ExportReportAsync()
    {
        var sb = new StringBuilder();
        var name = Overview?.ComputerName ?? (string.IsNullOrWhiteSpace(Target) ? Environment.MachineName : Target);
        sb.AppendLine($"System Inventory Report — {name}");
        sb.AppendLine($"Generated: {DateTime.Now:f}");
        sb.AppendLine(new string('=', 70));

        if (Overview != null)
        {
            sb.AppendLine("\nOVERVIEW");
            sb.AppendLine($"  Computer:     {Overview.ComputerName}");
            sb.AppendLine($"  OS:           {Overview.OS}");
            sb.AppendLine($"  Last Boot:    {Overview.LastBoot}  (Uptime: {Overview.Uptime})");
            sb.AppendLine($"  Manufacturer: {Overview.Manufacturer}  {Overview.Model}");
            sb.AppendLine($"  Serial:       {Overview.Serial}");
            sb.AppendLine($"  BIOS:         {Overview.BiosVersion}");
            sb.AppendLine($"  {Overview.DomainOrWorkgroup}  |  Logged on: {Overview.LoggedOnUser}");
            if (!string.IsNullOrWhiteSpace(Overview.LastLogon))
                sb.AppendLine($"  Last Logon:   {Overview.LastLogon}");
            sb.AppendLine($"  RAM: {Overview.RamGB}  |  CPU: {Overview.Processor} ({Overview.CpuCores} cores)");
            sb.AppendLine($"  IP: {Overview.IpAddress}");
        }
        if (Hardware != null)
        {
            sb.AppendLine("\nHARDWARE");
            sb.AppendLine($"  CPU:     {Hardware.CpuSummary}");
            sb.AppendLine($"  GPU:     {Hardware.GpuName}  ({Hardware.GpuRam})  Driver: {Hardware.GpuDriver}");
            sb.AppendLine($"  BIOS:    {Hardware.BiosVersion}  ({Hardware.BiosDate})  {Hardware.BiosVendor}");
            foreach (var m in Hardware.RamModules) sb.AppendLine($"  RAM:     {m.Summary}");
            if (Hardware.HasBattery) sb.AppendLine($"  Battery: {Hardware.BatteryPct}");
        }
        if (Disks.Any())
        {
            sb.AppendLine("\nDISKS");
            foreach (var d in Disks)
                sb.AppendLine($"  {d.DriveLabel,-30}  {d.SummaryDisplay}  ({d.UsedPercent:P0} used)");
        }
        if (Adapters.Any())
        {
            sb.AppendLine("\nNETWORK");
            foreach (var a in Adapters)
            {
                sb.AppendLine($"  {a.Name}  [{a.Mac}]  {a.IpMode}");
                sb.AppendLine($"    IP: {a.IPs}   GW: {a.Gateway}   DNS: {a.DNS}");
            }
        }
        if (Shares.Any())
        {
            sb.AppendLine("\nSHARED FOLDERS");
            foreach (var sh in Shares)
                sb.AppendLine($"  {sh.Name,-20}  {sh.Path,-40}  {sh.Description}");
        }
        if (Security != null)
        {
            sb.AppendLine("\nSECURITY");
            sb.AppendLine($"  Firewall: Domain={Security.FirewallDomain}  Private={Security.FirewallPrivate}  Public={Security.FirewallPublic}");
            sb.AppendLine($"  Antivirus: {Security.AntiVirus}");
            sb.AppendLine($"  BitLocker (C:): {Security.BitLocker}");
            sb.AppendLine($"  UAC: {Security.UAC}  |  Auto Update: {Security.WUAutoUpdate}");
            sb.AppendLine($"  Last Hotfix: {Security.LastHotfix}");
            sb.AppendLine($"  Product Key: {Security.ProductKey}");
            sb.AppendLine($"  Local Admins: {Security.LocalAdmins}");
        }
        if (AllApps.Any())
        {
            sb.AppendLine($"\nSOFTWARE  ({AllApps.Count} installed)");
            foreach (var app in AllApps)
                sb.AppendLine($"  {app.Name,-50}  {app.Version,-18}  {app.Publisher}");
        }
        if (AllServices.Any())
        {
            sb.AppendLine($"\nSERVICES  ({AllServices.Count} total)");
            sb.AppendLine($"  {"Name",-40}  {"Caption",-50}  {"State",-10}  {"StartMode",-10}  StartName");
            foreach (var svc in AllServices)
                sb.AppendLine($"  {svc.Name,-40}  {svc.Caption,-50}  {svc.State,-10}  {svc.StartMode,-10}  {svc.StartName}");
        }

        var report = sb.ToString();
        var fileName = $"Inventory-{name}-{DateTime.Now:yyyy-MM-dd}.txt";
        try
        {
#if WINDOWS
            var picker = new Windows.Storage.Pickers.FileSavePicker();
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary;
            picker.SuggestedFileName = fileName;
            picker.FileTypeChoices.Add("Text Report", new List<string> { ".txt" });
            var nw = Application.Current!.Windows[0].Handler?.PlatformView as MauiWinUIWindow;
            var hwnd = nw?.WindowHandle ?? IntPtr.Zero;
            if (hwnd != IntPtr.Zero) WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);
            var file = await picker.PickSaveFileAsync();
            if (file == null) return;
            await Windows.Storage.FileIO.WriteTextAsync(file, report);
            var open = await Application.Current!.Windows[0].Page!.DisplayAlert("Report Saved", $"Saved to:\n{file.Path}", "Open", "Close");
            if (open) System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(file.Path) { UseShellExecute = true });
#else
            var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), fileName);
            await File.WriteAllTextAsync(path, report);
            var open = await Application.Current!.Windows[0].Page!.DisplayAlert("Report Saved", $"Saved to:\n{path}", "Open", "Close");
            if (open) System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(path) { UseShellExecute = true });
#endif
        }
        catch (Exception ex)
        {
            await Application.Current!.Windows[0].Page!.DisplayAlert("Export Failed", ex.Message, "OK");
        }
    }
}
