using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using ToolsAvaloniaApp.Helpers;

namespace ToolsAvaloniaApp.ViewModels;

public record ToolDef(
    string Name,
    string Description,
    Func<string> Script,
    string? P1Label = null, string? P1Water = null,
    string? P2Label = null, string? P2Water = null,
    string? P3Label = null, string? P3Water = null);

public class MainViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    private void Set<T>(ref T field, T value, [CallerMemberName] string? name = null)
    {
        if (Equals(field, value)) return;
        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    private string   _target   = "";
    private string   _username = "";
    private string   _password = "";
    private string   _output   = "";
    private bool     _loading;
    private ToolDef? _currentTool;
    private string   _currentKey = "";
    private string   _p1 = "", _p2 = "", _p3 = "";

    public string  Target   { get => _target;   set => Set(ref _target,   value); }
    public string  Username { get => _username;  set => Set(ref _username, value); }
    public string  Password { get => _password;  set => Set(ref _password, value); }
    public string  Output   { get => _output;    set => Set(ref _output,   value); }
    public bool    Loading  { get => _loading;   set => Set(ref _loading,  value); }
    public string  P1       { get => _p1;        set => Set(ref _p1,       value); }
    public string  P2       { get => _p2;        set => Set(ref _p2,       value); }
    public string  P3       { get => _p3;        set => Set(ref _p3,       value); }

    public ToolDef? CurrentTool
    {
        get => _currentTool;
        set
        {
            Set(ref _currentTool, value);
            P1 = ""; P2 = ""; P3 = "";
            OnMany(nameof(ToolTitle), nameof(ToolDescription),
                   nameof(HasTool), nameof(NoTool),
                   nameof(HasP1), nameof(HasP2), nameof(HasP3), nameof(HasParams),
                   nameof(P1Label), nameof(P2Label), nameof(P3Label),
                   nameof(P1Water), nameof(P2Water), nameof(P3Water),
                   nameof(ShowBrowseP1), nameof(ShowBrowseP2));
        }
    }

    public string ToolTitle       => _currentTool?.Name        ?? "";
    public string ToolDescription => _currentTool?.Description ?? "";
    public bool   HasTool         => _currentTool != null;
    public bool   NoTool          => _currentTool == null;

    public bool   HasP1        => !string.IsNullOrEmpty(_currentTool?.P1Label);
    public bool   HasP2        => !string.IsNullOrEmpty(_currentTool?.P2Label);
    public bool   HasP3        => !string.IsNullOrEmpty(_currentTool?.P3Label);
    public bool   HasParams    => HasP1 || HasP2 || HasP3;
    public bool   ShowBrowseP1 => _currentKey == "copyfile" && HasP1;
    public bool   ShowBrowseP2 => _currentKey == "copyfile" && HasP2;
    public string P1Label   => _currentTool?.P1Label ?? "";
    public string P2Label   => _currentTool?.P2Label ?? "";
    public string P3Label   => _currentTool?.P3Label ?? "";
    public string P1Water   => _currentTool?.P1Water ?? "";
    public string P2Water   => _currentTool?.P2Water ?? "";
    public string P3Water   => _currentTool?.P3Water ?? "";

    public ICommand SelectToolCommand    { get; }
    public ICommand RunCommand           { get; }
    public ICommand ClearCredsCommand    { get; }
    public ICommand ClearOutputCommand   { get; }
    public ICommand ExitCommand          { get; }
    public ICommand KnowledgeBaseCommand { get; }

    private readonly Dictionary<string, ToolDef> _tools;

    public MainViewModel()
    {
        _tools = BuildTools();
        SelectToolCommand  = new RelayCommand<string>(key =>
        {
            if (key != null && _tools.TryGetValue(key, out var t))
            { _currentKey = key; CurrentTool = t; Output = ""; }
        });
        RunCommand         = new RelayCommand(async () => await RunCurrent());
        ClearCredsCommand    = new RelayCommand(() => { Target = ""; Username = ""; Password = ""; });
        ClearOutputCommand   = new RelayCommand(() => Output = "");
        ExitCommand          = new RelayCommand(() => Environment.Exit(0));
        KnowledgeBaseCommand = new RelayCommand(() =>
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName        = "https://notebooklm.google.com/notebook/0d70e944-e6cc-4485-a888-f71f4419168b?authuser=1",
                UseShellExecute = true
            }));
    }

    private async Task RunCurrent()
    {
        if (_currentTool == null || Loading) return;
        Loading = true;
        Output  = "Running...";
        try
        {
            // On non-Windows, Copy File uploads the local file through the relay
            if (_currentKey == "copyfile" && !OperatingSystem.IsWindows())
            {
                if (string.IsNullOrWhiteSpace(_p1) || string.IsNullOrWhiteSpace(_p2))
                    Output = "Enter Source (local path) and Destination (server path) above, then click Run.";
                else
                    Output = await PsRunner.UploadFileAsync(_p1, _p2);
            }
            else
            {
                Output = await PsRunner.RunAsync(_currentTool.Script());
            }
        }
        catch (Exception ex) { Output = $"Error: {ex.Message}"; }
        finally { Loading = false; }
    }

    private bool HasCreds => !string.IsNullOrWhiteSpace(_username) && !string.IsNullOrWhiteSpace(_password);
    private bool IsRemote => !string.IsNullOrWhiteSpace(_target);

    private string CredBlock()
    {
        if (!HasCreds) return "";
        return $"""
$secpw = ConvertTo-SecureString '{_password.Replace("'", "''")}' -AsPlainText -Force
$cred  = New-Object -TypeName System.Management.Automation.PSCredential -ArgumentList '{_username.Replace("'", "''")}', $secpw

""";
    }

    private string Wrap(string cmd)
    {
        if (!IsRemote) return cmd;
        // On non-Windows the script runs on the relay host directly — no Invoke-Command needed
        if (!OperatingSystem.IsWindows()) return cmd;
        var cred = HasCreds ? " -Credential $cred" : "";
        return $"{CredBlock()}Invoke-Command -ComputerName '{_target.Trim()}'{cred} -ScriptBlock {{\n{cmd}\n}}";
    }

    private string NeedsTarget(string msg = "Enter a Target Computer name in the bar above, then run again.")
        => $"Write-Host '{msg}'";

    private string SafeP(string v) => v.Replace("'", "''");

    private Dictionary<string, ToolDef> BuildTools() => new()
    {
        // ── System ──────────────────────────────────────────────────────────────
        ["apps"] = new("Installed Apps",
            "All installed applications from the registry, sorted by name.",
            () => Wrap(@"
Get-ItemProperty 'HKLM:\Software\Microsoft\Windows\CurrentVersion\Uninstall\*',
    'HKLM:\Software\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\*' |
Where-Object { $_.DisplayName } |
Select-Object DisplayName, DisplayVersion, Publisher |
Sort-Object DisplayName |
Format-Table -AutoSize | Out-String -Width 220")),

        ["services"] = new("Services",
            "All Windows services with current status and start type.",
            () => Wrap(@"
Get-Service | Sort-Object Status, DisplayName |
Select-Object Name, DisplayName, Status, StartType |
Format-Table -AutoSize | Out-String -Width 220")),

        ["processes"] = new("Processes",
            "Top 50 running processes sorted by CPU usage.",
            () => Wrap(@"
Get-Process | Sort-Object CPU -Descending | Select-Object -First 50 |
Select-Object Name, Id,
    @{N='CPU';E={[math]::Round($_.CPU,1)}},
    @{N='Mem MB';E={[math]::Round($_.WorkingSet/1MB,1)}} |
Format-Table -AutoSize | Out-String -Width 220")),

        ["disk"] = new("Disk Space",
            "Used, free, and total space for all drives.",
            () => Wrap(@"
Get-PSDrive -PSProvider FileSystem |
Select-Object Name,
    @{N='Used GB';E={[math]::Round($_.Used/1GB,2)}},
    @{N='Free GB';E={[math]::Round($_.Free/1GB,2)}},
    @{N='Total GB';E={[math]::Round(($_.Used+$_.Free)/1GB,2)}},
    @{N='% Free';E={if(($_.Used+$_.Free) -gt 0){[math]::Round($_.Free/($_.Used+$_.Free)*100,1)}else{0}}} |
Format-Table -AutoSize | Out-String -Width 220")),

        ["sysinfo"] = new("System Info",
            "OS version, uptime, RAM, hardware model, and BIOS version.",
            () => Wrap(@"
$os   = Get-CimInstance Win32_OperatingSystem
$cs   = Get-CimInstance Win32_ComputerSystem
$bios = Get-CimInstance Win32_BIOS
[PSCustomObject]@{
    ComputerName = $env:COMPUTERNAME
    OS           = $os.Caption
    Version      = $os.Version
    Architecture = $os.OSArchitecture
    LastBoot     = $os.LastBootUpTime
    Uptime       = ((Get-Date) - $os.LastBootUpTime).ToString('d\d\ h\h\ m\m')
    RAM_GB       = [math]::Round($cs.TotalPhysicalMemory/1GB,2)
    Manufacturer = $cs.Manufacturer
    Model        = $cs.Model
    BIOSVersion  = $bios.SMBIOSBIOSVersion
} | Format-List | Out-String -Width 220")),

        ["eventlog"] = new("Event Log",
            "Last 50 system errors and warnings, newest first.",
            () => Wrap(@"
Get-EventLog -LogName System -EntryType Error,Warning -Newest 50 |
Select-Object TimeGenerated, EntryType, Source, EventID,
    @{N='Message';E={$_.Message.Substring(0,[Math]::Min(80,$_.Message.Length))}} |
Format-Table -AutoSize -Wrap | Out-String -Width 320")),

        ["tasks"] = new("Scheduled Tasks",
            "All enabled scheduled tasks with path and current state.",
            () => Wrap(@"
Get-ScheduledTask | Where-Object State -ne 'Disabled' |
Select-Object TaskName, TaskPath, State |
Sort-Object TaskPath, TaskName |
Format-Table -AutoSize | Out-String -Width 220")),

        // ── Network ─────────────────────────────────────────────────────────────
        ["ipconfig"] = new("IP Configuration",
            "Network adapter IP addresses, gateways, and DNS servers.",
            () => Wrap(@"
Get-NetIPConfiguration | ForEach-Object {
    [PSCustomObject]@{
        Adapter = $_.InterfaceAlias
        IPv4    = ($_.IPv4Address.IPAddress) -join ', '
        Gateway = ($_.IPv4DefaultGateway.NextHop) -join ', '
        DNS     = ($_.DNSServer.ServerAddresses) -join ', '
    }
} | Format-Table -AutoSize -Wrap | Out-String -Width 220")),

        ["adapters"] = new("Network Adapters",
            "Physical and virtual network adapters with link speed and MAC address.",
            () => Wrap(@"
Get-NetAdapter | Select-Object Name, InterfaceDescription, Status, LinkSpeed, MacAddress |
Format-Table -AutoSize | Out-String -Width 220")),

        ["ping"] = new("Ping",
            "Pings the target computer 4 times. Enter a target in the bar above.",
            () => !IsRemote
                ? NeedsTarget()
                : $"Test-Connection -ComputerName '{_target.Trim()}' -Count 4 | Format-Table -AutoSize | Out-String -Width 220"),

        ["pingsweep"] = new("Ping Sweep",
            "Sweeps a subnet for live hosts. Enter the first 3 octets (e.g. 192.168.1).",
            () => string.IsNullOrWhiteSpace(_p1)
                ? "Write-Host 'Enter a subnet prefix above (e.g. 192.168.1), then click Run.'"
                : $@"
$subnet = '{SafeP(_p1.Trim())}'
Write-Host ""Sweeping $subnet.1-254 — please wait...""
1..254 | ForEach-Object {{
    $ip = ""$subnet.$_""
    if (Test-Connection -ComputerName $ip -Count 1 -Quiet -TimeoutSeconds 1) {{
        try   {{ $name = [System.Net.Dns]::GetHostEntry($ip).HostName }} catch {{ $name = '' }}
        Write-Host ""ONLINE   $ip   $name""
    }}
}}
Write-Host ""Sweep complete.""",
            P1Label: "Subnet", P1Water: "e.g. 192.168.1"),

        ["porttester"] = new("Port Tester",
            "Tests if a specific TCP port is open on the target computer.",
            () => !IsRemote || string.IsNullOrWhiteSpace(_p1)
                ? NeedsTarget("Enter a Target Computer and Port number above, then run.")
                : $@"Test-NetConnection -ComputerName '{SafeP(_target.Trim())}' -Port {_p1.Trim()} -InformationLevel Detailed |
Select-Object ComputerName, RemotePort, TcpTestSucceeded, PingSucceeded, NameResolutionSucceeded |
Format-List | Out-String -Width 220",
            P1Label: "Port", P1Water: "e.g. 443"),

        ["traceroute"] = new("Traceroute",
            "Traces the network route to the target computer.",
            () => !IsRemote
                ? NeedsTarget()
                : $"tracert -d '{_target.Trim()}' | Out-String -Width 220"),

        ["dns"] = new("DNS Lookup",
            "Resolves the target computer name to IP address and record type.",
            () => !IsRemote
                ? NeedsTarget()
                : $"Resolve-DnsName '{_target.Trim()}' | Format-Table -AutoSize | Out-String -Width 220"),

        ["flushdns"] = new("Flush DNS",
            "Clears the DNS resolver cache on the target (or local) machine.",
            () => Wrap(@"
Clear-DnsClientCache
Write-Host ""DNS cache flushed on $env:COMPUTERNAME.""")),

        ["netstat"] = new("Open Connections",
            "Active TCP connections and listening ports with owning process name.",
            () => Wrap(@"
Get-NetTCPConnection | Where-Object { $_.State -ne 'TimeWait' } |
Select-Object LocalAddress, LocalPort, RemoteAddress, RemotePort, State,
    @{N='Process';E={(Get-Process -Id $_.OwningProcess -EA SilentlyContinue).Name}} |
Sort-Object State, LocalPort |
Format-Table -AutoSize | Out-String -Width 300")),

        ["shares"] = new("Network Shares",
            "Shared folders configured on the machine.",
            () => Wrap(@"
Get-SmbShare | Select-Object Name, Path, Description, ScopeName |
Format-Table -AutoSize | Out-String -Width 220")),

        ["proxy"] = new("Proxy Settings",
            "Shows current proxy configuration (user and system-wide WinHTTP).",
            () => Wrap(@"
$p = Get-ItemProperty 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Internet Settings' -EA SilentlyContinue
Write-Host '--- User Proxy (Internet Settings) ---'
[PSCustomObject]@{
    ProxyEnabled  = $p.ProxyEnable
    ProxyServer   = $p.ProxyServer
    ProxyOverride = $p.ProxyOverride
    AutoConfigURL = $p.AutoConfigURL
} | Format-List | Out-String -Width 220
Write-Host '--- WinHTTP System Proxy ---'
netsh winhttp show proxy | Out-String -Width 220")),

        ["wifiinfo"] = new("WiFi Info",
            "Current wireless connection details, signal strength and channel.",
            () => Wrap("netsh wlan show interfaces | Out-String -Width 220")),

        ["wifiprofiles"] = new("WiFi Profiles",
            "All saved wireless network profiles on the machine.",
            () => Wrap("netsh wlan show profiles | Out-String -Width 220")),

        ["resetnic"] = new("Reset NIC",
            "Disables then re-enables a network adapter by name (run Network Adapters first to get the name).",
            () => string.IsNullOrWhiteSpace(_p1)
                ? "Write-Host 'Enter the adapter name above (from Network Adapters list), then click Run.'"
                : Wrap($@"
$adapter = '{SafeP(_p1)}'
Write-Host ""Disabling $adapter...""
Disable-NetAdapter -Name $adapter -Confirm:$false -ErrorAction Stop
Start-Sleep -Seconds 3
Write-Host ""Enabling $adapter...""
Enable-NetAdapter -Name $adapter -Confirm:$false -ErrorAction Stop
Write-Host ""NIC '$adapter' has been reset successfully."""),
            P1Label: "Adapter Name", P1Water: "e.g. Ethernet  or  Wi-Fi"),

        ["dhcpstats"] = new("DHCP Stats",
            "DHCP server scope statistics. Target must be a DHCP server with RSAT tools available.",
            () => Wrap(@"
Get-DhcpServerv4Statistics -ErrorAction Stop |
Select-Object TotalScopes, ScopesWithDelayOffer, TotalAddresses, InUse, Available, PercentageInUse |
Format-List | Out-String -Width 220")),

        // ── Security ────────────────────────────────────────────────────────────
        ["patches"] = new("Security Patches",
            "Last 50 installed hotfixes, newest first.",
            () => Wrap(@"
Get-HotFix | Sort-Object InstalledOn -Descending | Select-Object -First 50 |
Select-Object HotFixID, Description, InstalledOn, InstalledBy |
Format-Table -AutoSize | Out-String -Width 220")),

        ["firewall"] = new("Firewall Status",
            "Windows Firewall profile settings (Domain, Private, Public).",
            () => Wrap(@"
Get-NetFirewallProfile |
Select-Object Name, Enabled, DefaultInboundAction, DefaultOutboundAction |
Format-Table -AutoSize | Out-String -Width 220")),

        // ── Manage ──────────────────────────────────────────────────────────────
        ["localusers"] = new("Local Users",
            "Local user accounts with enabled state and last logon.",
            () => Wrap(@"
Get-LocalUser | Select-Object Name, Enabled, LastLogon, PasswordLastSet, Description |
Format-Table -AutoSize | Out-String -Width 220")),

        ["localgroups"] = new("Local Groups",
            "Local security groups and their current members.",
            () => Wrap(@"
Get-LocalGroup | ForEach-Object {
    $g = $_
    $members = (Get-LocalGroupMember $g -ErrorAction SilentlyContinue |
                Select-Object -ExpandProperty Name) -join ', '
    [PSCustomObject]@{ Group=$g.Name; Members=$members }
} | Format-Table -AutoSize -Wrap | Out-String -Width 300")),

        ["groupmembers"] = new("Group Membership",
            "Lists all members of a specific local group.",
            () => string.IsNullOrWhiteSpace(_p1)
                ? "Write-Host 'Enter a Group Name above, then click Run.'"
                : Wrap($@"
Get-LocalGroupMember '{SafeP(_p1)}' |
Select-Object Name, ObjectClass, PrincipalSource |
Format-Table -AutoSize | Out-String -Width 220"),
            P1Label: "Group Name", P1Water: "e.g. Administrators"),

        ["addtogroup"] = new("Add to Group",
            "Adds a local or domain user to a local group.",
            () => string.IsNullOrWhiteSpace(_p1) || string.IsNullOrWhiteSpace(_p2)
                ? "Write-Host 'Enter both Group Name and Username above, then click Run.'"
                : Wrap($@"
Add-LocalGroupMember -Group '{SafeP(_p1)}' -Member '{SafeP(_p2)}' -ErrorAction Stop
Write-Host ""SUCCESS: '{SafeP(_p2)}' added to '{SafeP(_p1)}'."""),
            P1Label: "Group Name",  P1Water: "e.g. Administrators",
            P2Label: "Username",    P2Water: "DOMAIN\\user  or  localuser"),

        ["removefromgroup"] = new("Remove from Group",
            "Removes a user from a local group.",
            () => string.IsNullOrWhiteSpace(_p1) || string.IsNullOrWhiteSpace(_p2)
                ? "Write-Host 'Enter both Group Name and Username above, then click Run.'"
                : Wrap($@"
Remove-LocalGroupMember -Group '{SafeP(_p1)}' -Member '{SafeP(_p2)}' -ErrorAction Stop
Write-Host ""SUCCESS: '{SafeP(_p2)}' removed from '{SafeP(_p1)}'."""),
            P1Label: "Group Name",  P1Water: "e.g. Administrators",
            P2Label: "Username",    P2Water: "user to remove"),

        ["createuser"] = new("Create Local User",
            "Creates a new local user account.",
            () => string.IsNullOrWhiteSpace(_p1) || string.IsNullOrWhiteSpace(_p2)
                ? "Write-Host 'Enter at least Username and Password above, then click Run.'"
                : Wrap($@"
$pw = ConvertTo-SecureString '{SafeP(_p2)}' -AsPlainText -Force
$params = @{{ Name='{SafeP(_p1)}'; Password=$pw; PasswordNeverExpires=$true }}
if ('{SafeP(_p3)}') {{ $params['FullName'] = '{SafeP(_p3)}' }}
New-LocalUser @params -ErrorAction Stop
Write-Host ""SUCCESS: Local user '{SafeP(_p1)}' created."""),
            P1Label: "Username",  P1Water: "new account name",
            P2Label: "Password",  P2Water: "initial password",
            P3Label: "Full Name", P3Water: "optional display name"),

        ["psremote"] = new("PS-Remote Shell",
            "Opens an interactive PowerShell remoting session to the target in a new window.",
            () => !IsRemote
                ? NeedsTarget("Enter a Target Computer in the bar above, then click Run.")
                : $@"
$target = '{SafeP(_target.Trim())}'
$user   = '{SafeP(_username)}'
$enterCmd = if ($user) {{
    ""Enter-PSSession -ComputerName '$target' -Credential (Get-Credential '$user')""
}} else {{
    ""Enter-PSSession -ComputerName '$target'""
}}
Start-Process powershell.exe -ArgumentList ""-NoExit"",""-NoProfile"",""-Command $enterCmd""
Write-Host ""Launched PS-Remote shell -> $target""
Write-Host ""A new PowerShell window will open. Authenticate when prompted."""),

        // ── Files ───────────────────────────────────────────────────────────────
        ["copyfile"] = new("Copy File",
            "Copies a file from source to destination. Use full UNC paths for remote machines (\\\\server\\share\\file).",
            () => string.IsNullOrWhiteSpace(_p1) || string.IsNullOrWhiteSpace(_p2)
                ? "Write-Host 'Enter Source and Destination paths above, then click Run.'"
                : $@"
$src = '{SafeP(_p1)}'
$dst = '{SafeP(_p2)}'
Write-Host ""Copying: $src""
Write-Host ""     To: $dst""
Copy-Item -Path $src -Destination $dst -Force -ErrorAction Stop
$size = (Get-Item $src -ErrorAction SilentlyContinue).Length
Write-Host ""Done. $([math]::Round($size/1KB,1)) KB copied.""",
            P1Label: "Source",      P1Water: @"C:\path\file.txt  or  /Users/name/file  or  \\server\share\file",
            P2Label: "Destination", P2Water: @"C:\dest\folder\  or  \\server\share\"),
    };

    private void OnMany(params string[] names)
    {
        foreach (var n in names)
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
    }
}

public class RelayCommand : ICommand
{
    private readonly Func<Task>? _async;
    private readonly Action?     _sync;
    public event EventHandler?   CanExecuteChanged;
    public RelayCommand(Func<Task> a) { _async = a; }
    public RelayCommand(Action a)     { _sync  = a; }
    public bool CanExecute(object? _) => true;
    public void Execute(object? _)
    {
        if (_async != null) _ = _async();
        else _sync?.Invoke();
    }
}

public class RelayCommand<T> : ICommand
{
    private readonly Action<T?> _action;
    public event EventHandler?  CanExecuteChanged;
    public RelayCommand(Action<T?> action) { _action = action; }
    public bool CanExecute(object? _) => true;
    public void Execute(object? parameter) => _action(parameter is T t ? t : default);
}
