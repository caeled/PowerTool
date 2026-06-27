using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text.Json;
using System.Windows.Input;
using SupportToolApp.Services;

namespace SupportToolApp.ViewModels.Tools;

// ── Shared models ────────────────────────────────────────────────────────────

public class AppEntry
{
    public string Name { get; set; } = "";
    public string Publisher { get; set; } = "";
    public string Version { get; set; } = "";
    public string InstallDate { get; set; } = "";
}

public class ServiceEntry
{
    public string Name { get; set; } = "";
    public string DisplayName { get; set; } = "";
    public string Status { get; set; } = "";
    public string StartType { get; set; } = "";
    public bool IsRunning => Status.Equals("Running", StringComparison.OrdinalIgnoreCase);
}

public class LocalUserEntry
{
    public string Name { get; set; } = "";
    public string FullName { get; set; } = "";
    public bool Enabled { get; set; }
    public string Description { get; set; } = "";
    public string LastLogon { get; set; } = "";
}

public class LocalGroupEntry
{
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public List<string> Members { get; set; } = [];
    public string MemberSummary => Members.Count == 0 ? "(none)" : string.Join(", ", Members);
}

public class PrinterEntry
{
    public string Name { get; set; } = "";
    public string DriverName { get; set; } = "";
    public string PortName { get; set; } = "";
    public bool IsDefault { get; set; }
    public string Status { get; set; } = "";
}

public class RebootCheckEntry
{
    public string Computer { get; set; } = "";
    public bool PendingReboot { get; set; }
    public string Reason { get; set; } = "";
    public string StatusText => PendingReboot ? "REBOOT NEEDED" : "OK";
    public string StatusColor => PendingReboot ? "#CC4400" : "#228B22";
}

public class PortResult
{
    public int Port { get; set; }
    public bool Open { get; set; }
    public string StatusText => Open ? "OPEN" : "CLOSED";
    public string StatusColor => Open ? "#228B22" : "#CC4400";
}

public class UpdateEntry
{
    public string Title { get; set; } = "";
    public string Severity { get; set; } = "";
    public string InstalledOn { get; set; } = "";
    public bool IsInstalled { get; set; }
}

public class NetworkAdapterEntry
{
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public string IPAddress { get; set; } = "";
    public string SubnetMask { get; set; } = "";
    public string DefaultGateway { get; set; } = "";
    public string MacAddress { get; set; } = "";
    public string DnsServers { get; set; } = "";
    public string Status { get; set; } = "";
}

public class ProcessEntry
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string CPU { get; set; } = "";
    public string WorkingSet { get; set; } = "";
    public string StartTime { get; set; } = "";
    public string Owner { get; set; } = "";
}

public class PatchEntry
{
    public string HotFixId { get; set; } = "";
    public string Description { get; set; } = "";
    public string InstalledOn { get; set; } = "";
    public string InstalledBy { get; set; } = "";
}

public class EventLogEntry
{
    public string TimeGenerated { get; set; } = "";
    public string EntryType { get; set; } = "";
    public string Source { get; set; } = "";
    public string EventId { get; set; } = "";
    public string Message { get; set; } = "";
    public string TypeColor => EntryType switch
    {
        "Error" => "#CC4400",
        "Warning" => "#B8860B",
        _ => "#228B22"
    };
}

public class ShareEntry
{
    public string Name { get; set; } = "";
    public string Path { get; set; } = "";
    public string Description { get; set; } = "";
    public string Type { get; set; } = "";
}

public class PingSweepResult
{
    public string IPAddress { get; set; } = "";
    public bool Online { get; set; }
    public string Hostname { get; set; } = "";
    public string StatusText => Online ? "ONLINE" : "OFFLINE";
    public string StatusColor => Online ? "#228B22" : "#999999";
}

public class DeployLogEntry
{
    public string Time { get; set; } = DateTime.Now.ToString("HH:mm:ss");
    public string Message { get; set; } = "";
}

public class DriveEntry
{
    public string Drive { get; set; } = "";
    public string Label { get; set; } = "";
    public double SizeGB { get; set; }
    public double UsedGB { get; set; }
    public double FreeGB { get; set; }
    public int PctUsed { get; set; }
    public string PctColor => PctUsed > 85 ? "#CC4400" : PctUsed > 70 ? "#B8860B" : "#228B22";
}

// ── ViewModel ────────────────────────────────────────────────────────────────

public class ToolsViewModel : BaseViewModel
{
    // Shared target
    private string _targetComputer = "";
    public string TargetComputer
    {
        get => _targetComputer;
        set => SetProperty(ref _targetComputer, value);
    }

    private string _statusMessage = "";
    private bool _hasStatus;
    public string StatusMessage { get => _statusMessage; set { SetProperty(ref _statusMessage, value); HasStatus = !string.IsNullOrEmpty(value); } }
    public bool HasStatus { get => _hasStatus; set => SetProperty(ref _hasStatus, value); }

    // ── Installed Apps ───────────────────────────────────────────────────────
    private string _appSearch = "";
    private bool _appsLoading;
    public string AppSearch { get => _appSearch; set { SetProperty(ref _appSearch, value); FilterApps(); } }
    public bool AppsLoading { get => _appsLoading; set => SetProperty(ref _appsLoading, value); }
    public ObservableCollection<AppEntry> Apps { get; } = [];
    private List<AppEntry> _allApps = [];
    public ICommand ClearTargetCommand { get; }
    public ICommand LoadAppsCommand { get; }

    // ── Services ─────────────────────────────────────────────────────────────
    private string _serviceSearch = "";
    private bool _servicesLoading;
    private ServiceEntry? _selectedService;
    public string ServiceSearch { get => _serviceSearch; set { SetProperty(ref _serviceSearch, value); FilterServices(); } }
    public bool ServicesLoading { get => _servicesLoading; set => SetProperty(ref _servicesLoading, value); }
    public ServiceEntry? SelectedService { get => _selectedService; set => SetProperty(ref _selectedService, value); }
    public ObservableCollection<ServiceEntry> Services { get; } = [];
    private List<ServiceEntry> _allServices = [];
    public ICommand LoadServicesCommand { get; }
    public ICommand StartServiceCommand { get; }
    public ICommand StopServiceCommand { get; }
    public ICommand RestartServiceCommand { get; }

    // ── Local Users & Groups ─────────────────────────────────────────────────
    private bool _localUsersLoading;
    public bool LocalUsersLoading { get => _localUsersLoading; set => SetProperty(ref _localUsersLoading, value); }
    public ObservableCollection<LocalUserEntry> LocalUsers { get; } = [];
    public ObservableCollection<LocalGroupEntry> LocalGroups { get; } = [];
    public ICommand LoadLocalUsersCommand { get; }

    // ── Printers ─────────────────────────────────────────────────────────────
    private bool _printersLoading;
    private string _newPrinterPath = "";
    public bool PrintersLoading { get => _printersLoading; set => SetProperty(ref _printersLoading, value); }
    public string NewPrinterPath { get => _newPrinterPath; set => SetProperty(ref _newPrinterPath, value); }
    public ObservableCollection<PrinterEntry> Printers { get; } = [];
    public ICommand LoadPrintersCommand { get; }
    public ICommand AddPrinterCommand { get; }
    public ICommand RemovePrinterCommand { get; }

    // ── Pending Reboot ───────────────────────────────────────────────────────
    private string _rebootCheckList = "";
    private bool _rebootLoading;
    public string RebootCheckList { get => _rebootCheckList; set => SetProperty(ref _rebootCheckList, value); }
    public bool RebootLoading { get => _rebootLoading; set => SetProperty(ref _rebootLoading, value); }
    public ObservableCollection<RebootCheckEntry> RebootResults { get; } = [];
    public ICommand CheckRebootCommand { get; }

    // ── TCP Port Tester ──────────────────────────────────────────────────────
    private string _portHost = "";
    private string _portList = "80,443,3389,445,135,22";
    private bool _portsLoading;
    public string PortHost { get => _portHost; set => SetProperty(ref _portHost, value); }
    public string PortList { get => _portList; set => SetProperty(ref _portList, value); }
    public bool PortsLoading { get => _portsLoading; set => SetProperty(ref _portsLoading, value); }
    public ObservableCollection<PortResult> PortResults { get; } = [];
    public ICommand TestPortsCommand { get; }

    // ── Windows Updates ──────────────────────────────────────────────────────
    private bool _updatesLoading;
    public bool UpdatesLoading { get => _updatesLoading; set => SetProperty(ref _updatesLoading, value); }
    public ObservableCollection<UpdateEntry> Updates { get; } = [];
    public ICommand LoadUpdatesCommand { get; }

    // ── Network Info ─────────────────────────────────────────────────────────
    private bool _networkLoading;
    public bool NetworkLoading { get => _networkLoading; set => SetProperty(ref _networkLoading, value); }
    public ObservableCollection<NetworkAdapterEntry> NetworkAdapters { get; } = [];
    public ICommand LoadNetworkCommand { get; }

    // ── Logged On User ───────────────────────────────────────────────────────
    private bool _loggedOnLoading;
    private string _loggedOnResult = "";
    public bool LoggedOnLoading { get => _loggedOnLoading; set => SetProperty(ref _loggedOnLoading, value); }
    public string LoggedOnResult { get => _loggedOnResult; set => SetProperty(ref _loggedOnResult, value); }
    public ICommand GetLoggedOnUserCommand { get; }

    // ── Local Admins ─────────────────────────────────────────────────────────
    private bool _localAdminsLoading;
    public bool LocalAdminsLoading { get => _localAdminsLoading; set => SetProperty(ref _localAdminsLoading, value); }
    public ObservableCollection<string> LocalAdmins { get; } = [];
    public ICommand LoadLocalAdminsCommand { get; }

    // ── Security Patches ─────────────────────────────────────────────────────
    private bool _patchesLoading;
    public bool PatchesLoading { get => _patchesLoading; set => SetProperty(ref _patchesLoading, value); }
    public ObservableCollection<PatchEntry> Patches { get; } = [];
    public ICommand LoadPatchesCommand { get; }

    // ── Process Manager ──────────────────────────────────────────────────────
    private bool _processesLoading;
    private string _processSearch = "";
    private ProcessEntry? _selectedProcess;
    public bool ProcessesLoading { get => _processesLoading; set => SetProperty(ref _processesLoading, value); }
    public string ProcessSearch { get => _processSearch; set { SetProperty(ref _processSearch, value); FilterProcesses(); } }
    public ProcessEntry? SelectedProcess { get => _selectedProcess; set => SetProperty(ref _selectedProcess, value); }
    public ObservableCollection<ProcessEntry> Processes { get; } = [];
    private List<ProcessEntry> _allProcesses = [];
    public ICommand LoadProcessesCommand { get; }
    public ICommand KillProcessCommand { get; }

    // ── Remote Reboot ────────────────────────────────────────────────────────
    private bool _rebootActionLoading;
    private string _rebootOutput = "";
    public bool RebootActionLoading { get => _rebootActionLoading; set => SetProperty(ref _rebootActionLoading, value); }
    public string RebootOutput { get => _rebootOutput; set => SetProperty(ref _rebootOutput, value); }
    public ICommand RebootNowCommand { get; }
    public ICommand CancelRebootCommand { get; }
    public ICommand EnableRdpCommand { get; }

    // ── Ping Sweep ───────────────────────────────────────────────────────────
    private bool _sweepLoading;
    private string _sweepSubnet = "";
    public bool SweepLoading { get => _sweepLoading; set => SetProperty(ref _sweepLoading, value); }
    public string SweepSubnet { get => _sweepSubnet; set => SetProperty(ref _sweepSubnet, value); }
    public ObservableCollection<PingSweepResult> SweepResults { get; } = [];
    public ICommand PingSweepCommand { get; }

    // ── Run As ───────────────────────────────────────────────────────────────
    private string _runAsUsername = "";
    private string _runAsPassword = "";
    private string _runAsDomain = ".";
    private string _runAsCommand = "";
    private string _runAsArgs = "";
    private string _runAsOutput = "";
    private bool _runAsLoading;
    public string RunAsUsername { get => _runAsUsername; set => SetProperty(ref _runAsUsername, value); }
    public string RunAsPassword { get => _runAsPassword; set => SetProperty(ref _runAsPassword, value); }
    public string RunAsDomain   { get => _runAsDomain;   set => SetProperty(ref _runAsDomain, value); }
    public string RunAsCommand  { get => _runAsCommand;  set => SetProperty(ref _runAsCommand, value); }
    public string RunAsArgs     { get => _runAsArgs;     set => SetProperty(ref _runAsArgs, value); }
    public string RunAsOutput   { get => _runAsOutput;   set => SetProperty(ref _runAsOutput, value); }
    public bool RunAsLoading    { get => _runAsLoading;  set => SetProperty(ref _runAsLoading, value); }
    public ICommand RunAsExecuteCommand { get; }
    public ICommand RunAsStartServiceCommand { get; }
    public ICommand RunAsStopServiceCommand { get; }

    // ── Event Log ────────────────────────────────────────────────────────────
    private bool _eventLogLoading;
    private string _eventLogName = "System";
    private string _eventLogCount = "50";
    public bool EventLogLoading  { get => _eventLogLoading; set => SetProperty(ref _eventLogLoading, value); }
    public string EventLogName   { get => _eventLogName;    set => SetProperty(ref _eventLogName, value); }
    public string EventLogCount  { get => _eventLogCount;   set => SetProperty(ref _eventLogCount, value); }
    public ObservableCollection<EventLogEntry> EventLogEntries { get; } = [];
    public ICommand LoadEventLogCommand { get; }

    // ── Shared Folders ───────────────────────────────────────────────────────
    private bool _sharesLoading;
    private string _newShareName = "";
    private string _newSharePath = "";
    private string _newShareDesc = "";
    public bool SharesLoading    { get => _sharesLoading;  set => SetProperty(ref _sharesLoading, value); }
    public string NewShareName   { get => _newShareName;   set => SetProperty(ref _newShareName, value); }
    public string NewSharePath   { get => _newSharePath;   set => SetProperty(ref _newSharePath, value); }
    public string NewShareDesc   { get => _newShareDesc;   set => SetProperty(ref _newShareDesc, value); }
    public ObservableCollection<ShareEntry> Shares { get; } = [];
    public ICommand LoadSharesCommand { get; }
    public ICommand AddShareCommand { get; }
    public ICommand RemoveShareCommand { get; }

    // ── Disk Space ───────────────────────────────────────────────────────
    private bool _diskSpaceLoading;
    public bool DiskSpaceLoading { get => _diskSpaceLoading; set => SetProperty(ref _diskSpaceLoading, value); }
    public ObservableCollection<DriveEntry> DriveEntries { get; } = [];
    public ICommand LoadDiskSpaceCommand { get; }

    // ── Group Membership ─────────────────────────────────────────────────
    private string _grpMemberGroup = "Administrators";
    private string _grpMemberUser = "";
    private bool _groupMemberLoading;
    public string GrpMemberGroup { get => _grpMemberGroup; set => SetProperty(ref _grpMemberGroup, value); }
    public string GrpMemberUser  { get => _grpMemberUser;  set => SetProperty(ref _grpMemberUser, value); }
    public bool GroupMemberLoading { get => _groupMemberLoading; set => SetProperty(ref _groupMemberLoading, value); }
    public ObservableCollection<string> GroupMembers { get; } = [];
    public ICommand AddToGroupCommand { get; }
    public ICommand RemoveFromGroupCommand { get; }
    public ICommand ListGroupMembersCommand { get; }

    // ── DNS ──────────────────────────────────────────────────────────────
    private string _dnsQuery = "";
    private string _dnsOutput = "";
    private bool _dnsLoading;
    public string DnsQuery  { get => _dnsQuery;  set => SetProperty(ref _dnsQuery, value); }
    public string DnsOutput { get => _dnsOutput; set => SetProperty(ref _dnsOutput, value); }
    public bool DnsLoading  { get => _dnsLoading; set => SetProperty(ref _dnsLoading, value); }
    public ICommand DnsLookupCommand { get; }
    public ICommand DnsFlushCommand { get; }

    // ── Remote Registry ──────────────────────────────────────────────────
    private string _regHive = "HKLM";
    private string _regKey = "";
    private string _regValueName = "";
    private string _regValueData = "";
    private string _regOutput = "";
    private bool _registryLoading;
    public string RegHive      { get => _regHive;      set => SetProperty(ref _regHive, value); }
    public string RegKey       { get => _regKey;       set => SetProperty(ref _regKey, value); }
    public string RegValueName { get => _regValueName; set => SetProperty(ref _regValueName, value); }
    public string RegValueData { get => _regValueData; set => SetProperty(ref _regValueData, value); }
    public string RegOutput    { get => _regOutput;    set => SetProperty(ref _regOutput, value); }
    public bool RegistryLoading { get => _registryLoading; set => SetProperty(ref _registryLoading, value); }
    public ICommand RegGetCommand { get; }
    public ICommand RegSetCommand { get; }
    public ICommand RegDeleteCommand { get; }

    // ── GP Update ────────────────────────────────────────────────────────
    private bool _gpUpdateLoading;
    private string _gpUpdateOutput = "";
    public bool GpUpdateLoading   { get => _gpUpdateLoading;  set => SetProperty(ref _gpUpdateLoading, value); }
    public string GpUpdateOutput  { get => _gpUpdateOutput;   set => SetProperty(ref _gpUpdateOutput, value); }
    public ICommand GpUpdateCommand { get; }

    // ── Copy File ────────────────────────────────────────────────────────
    private string _copyLocalPath = "";
    private string _copyRemoteDest = "C:\\Windows\\Temp";
    private string _copyUsername = "";
    private string _copyPassword = "";
    private string _copyDomain = ".";
    private bool _copyLoading;
    private bool _copyCreateDir = true;
    public string CopyLocalPath   { get => _copyLocalPath;   set => SetProperty(ref _copyLocalPath, value); }
    public string CopyRemoteDest  { get => _copyRemoteDest;  set => SetProperty(ref _copyRemoteDest, value); }
    public string CopyUsername    { get => _copyUsername;    set => SetProperty(ref _copyUsername, value); }
    public string CopyPassword    { get => _copyPassword;    set => SetProperty(ref _copyPassword, value); }
    public string CopyDomain      { get => _copyDomain;      set => SetProperty(ref _copyDomain, value); }
    public bool CopyLoading       { get => _copyLoading;     set => SetProperty(ref _copyLoading, value); }
    public bool CopyCreateDir     { get => _copyCreateDir;   set => SetProperty(ref _copyCreateDir, value); }
    public ObservableCollection<DeployLogEntry> CopyLog { get; } = [];
    public ICommand CopyBrowseCommand { get; }
    public ICommand CopyRunCommand { get; }

    // ── File Drop & Execute ──────────────────────────────────────────────────
    private string _deployUsername = "";
    private string _deployPassword = "";
    private string _deployDomain = ".";
    private string _deployRemotePath = "C:\\Windows\\Temp";
    private string _deployScriptContent = "";
    private string _deployScriptName = "deploy_script.ps1";
    private bool _deployExecute = true;
    private bool _deployLoading;
    private string _deployOutput = "";
    public string DeployUsername      { get => _deployUsername;     set => SetProperty(ref _deployUsername, value); }
    public string DeployPassword      { get => _deployPassword;     set => SetProperty(ref _deployPassword, value); }
    public string DeployDomain        { get => _deployDomain;       set => SetProperty(ref _deployDomain, value); }
    public string DeployRemotePath    { get => _deployRemotePath;   set => SetProperty(ref _deployRemotePath, value); }
    public string DeployScriptContent { get => _deployScriptContent; set => SetProperty(ref _deployScriptContent, value); }
    public string DeployScriptName    { get => _deployScriptName;   set => SetProperty(ref _deployScriptName, value); }
    public bool DeployExecute         { get => _deployExecute;      set => SetProperty(ref _deployExecute, value); }
    public bool DeployLoading         { get => _deployLoading;      set => SetProperty(ref _deployLoading, value); }
    public string DeployOutput        { get => _deployOutput;       set => SetProperty(ref _deployOutput, value); }
    public ObservableCollection<DeployLogEntry> DeployLog { get; } = [];
    public ICommand DeployRunCommand { get; }
    public ICommand DeployBrowseCommand { get; }

    // ── Constructor ──────────────────────────────────────────────────────────

    private readonly CredentialService _creds;

    public ToolsViewModel(CredentialService creds)
    {
        _creds = creds;
        creds.Changed += () => MainThread.BeginInvokeOnMainThread(ApplyCredentials);

        ClearTargetCommand = new Command(() => TargetComputer = "");

        LoadAppsCommand = new Command(async () => await RunTool(
            "Installed Apps", () => AppsLoading = true, () => AppsLoading = false,
            async target =>
            {
                var entries = await RunPsJson<List<AppEntry>>(target, PsScripts.InstalledApps(target));
                _allApps = entries ?? [];
                FilterApps();
            }));

        LoadServicesCommand = new Command(async () => await RunTool(
            "Services", () => ServicesLoading = true, () => ServicesLoading = false,
            async target =>
            {
                var entries = await RunPsJson<List<ServiceEntry>>(target, PsScripts.Services(target));
                _allServices = entries ?? [];
                FilterServices();
            }));

        StartServiceCommand = new Command(async () =>
        {
            if (SelectedService == null) return;
            await RunServiceAction("Start", SelectedService.Name);
            LoadServicesCommand.Execute(null);
        });

        StopServiceCommand = new Command(async () =>
        {
            if (SelectedService == null) return;
            await RunServiceAction("Stop", SelectedService.Name);
            LoadServicesCommand.Execute(null);
        });

        RestartServiceCommand = new Command(async () =>
        {
            if (SelectedService == null) return;
            await RunServiceAction("Restart", SelectedService.Name);
            LoadServicesCommand.Execute(null);
        });

        LoadLocalUsersCommand = new Command(async () => await RunTool(
            "Local Users & Groups", () => LocalUsersLoading = true, () => LocalUsersLoading = false,
            async target =>
            {
                var users = await RunPsJson<List<LocalUserEntry>>(target, PsScripts.LocalUsers(target));
                LocalUsers.Clear();
                foreach (var u in users ?? []) LocalUsers.Add(u);

                var groups = await RunPsJson<List<LocalGroupEntry>>(target, PsScripts.LocalGroups(target));
                LocalGroups.Clear();
                foreach (var g in groups ?? []) LocalGroups.Add(g);
            }));

        LoadPrintersCommand = new Command(async () => await RunTool(
            "Printers", () => PrintersLoading = true, () => PrintersLoading = false,
            async target =>
            {
                var entries = await RunPsJson<List<PrinterEntry>>(target, PsScripts.Printers(target));
                Printers.Clear();
                foreach (var p in entries ?? []) Printers.Add(p);
            }));

        AddPrinterCommand = new Command(async () =>
        {
            if (string.IsNullOrWhiteSpace(NewPrinterPath)) return;
            var target = await ResolveTargetAsync();
            if (target == null) return;
            PrintersLoading = true;
            var result = await Task.Run(() => RunPsSync(PsScripts.AddPrinter(target, NewPrinterPath)));
            PrintersLoading = false;
            StatusMessage = string.IsNullOrWhiteSpace(result) ? "No response — check share path and that the printer is shared." : result.Trim();
            NewPrinterPath = "";
            // Don't auto-reload — status message would be overwritten; user can click Load Printers
        });

        RemovePrinterCommand = new Command<PrinterEntry>(async printer =>
        {
            if (printer == null) return;
            var target = await ResolveTargetAsync();
            if (target == null) return;
            await RunPsRaw(PsScripts.RemovePrinter(target, printer.Name));
            LoadPrintersCommand.Execute(null);
        });

        CheckRebootCommand = new Command(async () =>
        {
            RebootLoading = true;
            RebootResults.Clear();
            StatusMessage = "";
            try
            {
                var computers = string.IsNullOrWhiteSpace(RebootCheckList)
                    ? new[] { string.IsNullOrWhiteSpace(TargetComputer) ? "localhost" : TargetComputer }
                    : RebootCheckList.Split(['\n', ','], StringSplitOptions.RemoveEmptyEntries)
                                     .Select(s => s.Trim()).Where(s => s.Length > 0).ToArray();

                foreach (var computer in computers)
                {
                    var result = await Task.Run(() =>
                    {
                        var script = PsScripts.PendingReboot(computer);
                        var json = RunPsSync(script);
                        if (string.IsNullOrWhiteSpace(json)) return new RebootCheckEntry { Computer = computer, PendingReboot = false, Reason = "Could not connect" };
                        try
                        {
                            using var doc = JsonDocument.Parse(json);
                            var root = doc.RootElement;
                            return new RebootCheckEntry
                            {
                                Computer = computer,
                                PendingReboot = root.TryGetProperty("Pending", out var p) && p.GetBoolean(),
                                Reason = root.GetStringOrEmpty("Reason")
                            };
                        }
                        catch { return new RebootCheckEntry { Computer = computer, PendingReboot = false, Reason = "Parse error" }; }
                    });
                    RebootResults.Add(result);
                }
                StatusMessage = $"Checked {computers.Length} computer(s).";
            }
            finally { RebootLoading = false; }
        });

        TestPortsCommand = new Command(async () =>
        {
            var host = !string.IsNullOrWhiteSpace(PortHost) ? PortHost : TargetComputer;
            if (string.IsNullOrWhiteSpace(host)) { StatusMessage = "Enter a host to test."; return; }

            var ports = PortList.Split([',', ' '], StringSplitOptions.RemoveEmptyEntries)
                                .Select(p => int.TryParse(p.Trim(), out var n) ? n : -1)
                                .Where(p => p > 0).ToList();
            if (ports.Count == 0) { StatusMessage = "Enter at least one port number."; return; }

            PortsLoading = true;
            PortResults.Clear();
            StatusMessage = "";

            var results = await Task.Run(() =>
            {
                var script = PsScripts.TestPorts(host, ports);
                var json = RunPsSync(script);
                if (string.IsNullOrWhiteSpace(json)) return new List<PortResult>();
                try
                {
                    using var doc = JsonDocument.Parse(json);
                    return doc.RootElement.EnumerateArray().Select(el => new PortResult
                    {
                        Port = el.TryGetProperty("Port", out var pt) ? pt.GetInt32() : 0,
                        Open = el.TryGetProperty("Open", out var op) && op.GetBoolean()
                    }).ToList();
                }
                catch { return new List<PortResult>(); }
            });

            foreach (var r in results) PortResults.Add(r);
            StatusMessage = $"Tested {results.Count} port(s) on {host}.";
            PortsLoading = false;
        });

        LoadUpdatesCommand = new Command(async () => await RunTool(
            "Windows Updates", () => UpdatesLoading = true, () => UpdatesLoading = false,
            async target =>
            {
                var entries = await RunPsJson<List<UpdateEntry>>(target, PsScripts.WindowsUpdates(target));
                Updates.Clear();
                foreach (var u in entries ?? []) Updates.Add(u);
            }));

        LoadNetworkCommand = new Command(async () => await RunTool(
            "Network Info", () => NetworkLoading = true, () => NetworkLoading = false,
            async target =>
            {
                var entries = await RunPsJson<List<NetworkAdapterEntry>>(target, PsScripts.NetworkInfo(target));
                NetworkAdapters.Clear();
                foreach (var a in entries ?? []) NetworkAdapters.Add(a);
            }));

        GetLoggedOnUserCommand = new Command(async () => await RunTool(
            "Logged On User", () => LoggedOnLoading = true, () => LoggedOnLoading = false,
            async target =>
            {
                var result = await Task.Run(() => RunPsSync(PsScripts.LoggedOnUser(target)));
                LoggedOnResult = string.IsNullOrWhiteSpace(result) ? "Could not determine logged-on user." : result.Trim();
            }));

        LoadLocalAdminsCommand = new Command(async () => await RunTool(
            "Local Admins", () => LocalAdminsLoading = true, () => LocalAdminsLoading = false,
            async target =>
            {
                var entries = await RunPsJson<List<string>>(target, PsScripts.LocalAdmins(target));
                LocalAdmins.Clear();
                foreach (var a in entries ?? []) LocalAdmins.Add(a);
            }));

        LoadPatchesCommand = new Command(async () => await RunTool(
            "Security Patches", () => PatchesLoading = true, () => PatchesLoading = false,
            async target =>
            {
                var entries = await RunPsJson<List<PatchEntry>>(target, PsScripts.SecurityPatches(target));
                Patches.Clear();
                foreach (var p in entries ?? []) Patches.Add(p);
            }));

        LoadProcessesCommand = new Command(async () => await RunTool(
            "Processes", () => ProcessesLoading = true, () => ProcessesLoading = false,
            async target =>
            {
                var entries = await RunPsJson<List<ProcessEntry>>(target, PsScripts.ProcessList(target));
                _allProcesses = entries ?? [];
                FilterProcesses();
            }));

        KillProcessCommand = new Command(async () =>
        {
            if (SelectedProcess == null) return;
            var target = await ResolveTargetAsync();
            if (target == null) return;
            bool confirm = await Application.Current!.Windows[0].Page!.DisplayAlert(
                "Kill Process", $"Kill '{SelectedProcess.Name}' (PID {SelectedProcess.Id}) on {target}?", "Kill", "Cancel");
            if (!confirm) return;
            var script = PsScripts.KillProcess(target, SelectedProcess.Id);
            await RunPsRaw(script);
            StatusMessage = $"Killed PID {SelectedProcess.Id}.";
            LoadProcessesCommand.Execute(null);
        });

        RebootNowCommand = new Command(async () =>
        {
            var target = await ResolveTargetAsync();
            if (target == null) return;
            bool confirm = await Application.Current!.Windows[0].Page!.DisplayAlert(
                "Reboot", $"Reboot {target} now?", "Reboot", "Cancel");
            if (!confirm) return;
            RebootActionLoading = true;
            var result = await Task.Run(() => RunPsSync(PsScripts.RemoteReboot(target)));
            RebootOutput = string.IsNullOrWhiteSpace(result) ? $"Reboot initiated on {target}." : result.Trim();
            RebootActionLoading = false;
        });

        CancelRebootCommand = new Command(async () =>
        {
            var target = await ResolveTargetAsync();
            if (target == null) return;
            RebootActionLoading = true;
            var result = await Task.Run(() => RunPsSync(PsScripts.CancelReboot(target)));
            RebootOutput = string.IsNullOrWhiteSpace(result) ? $"Shutdown cancelled on {target}." : result.Trim();
            RebootActionLoading = false;
        });

        EnableRdpCommand = new Command(async () =>
        {
            var target = await ResolveTargetAsync();
            if (target == null) return;
            RebootActionLoading = true;
            var result = await Task.Run(() => RunPsSync(PsScripts.EnableRdp(target)));
            RebootOutput = string.IsNullOrWhiteSpace(result) ? $"RDP enabled on {target}." : result.Trim();
            RebootActionLoading = false;
        });

        RunAsExecuteCommand = new Command(async () =>
        {
            if (string.IsNullOrWhiteSpace(RunAsUsername) || string.IsNullOrWhiteSpace(RunAsPassword))
            { StatusMessage = "Username and password are required."; return; }
            if (string.IsNullOrWhiteSpace(RunAsCommand))
            { StatusMessage = "Enter a command or program to run."; return; }
            var target = await ResolveTargetAsync();
            if (target == null) return;
            RunAsLoading = true;
            RunAsOutput = "";
            var result = await Task.Run(() => RunPsSync(PsScripts.RunAs(target, RunAsDomain, RunAsUsername, RunAsPassword, RunAsCommand, RunAsArgs)));
            RunAsOutput = string.IsNullOrWhiteSpace(result) ? "Process launched (GUI app or no output)." : result.Trim();
            RunAsLoading = false;
        });

        DeployBrowseCommand = new Command(async () =>
        {
            var result = await FilePicker.Default.PickAsync(new PickOptions
            {
                PickerTitle = "Select script to deploy",
                FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
                {
                    { DevicePlatform.WinUI, new[] { ".ps1", ".bat", ".cmd", ".exe" } }
                })
            });
            if (result == null) return;
            DeployScriptName = Path.GetFileName(result.FullPath);
            DeployScriptContent = await File.ReadAllTextAsync(result.FullPath);
        });

        DeployRunCommand = new Command(async () =>
        {
            if (string.IsNullOrWhiteSpace(DeployUsername) || string.IsNullOrWhiteSpace(DeployPassword))
            { StatusMessage = "Credentials required for file drop."; return; }
            if (string.IsNullOrWhiteSpace(DeployScriptContent))
            { StatusMessage = "Paste or browse to a script first."; return; }
            var target = await ResolveTargetAsync();
            if (target == null) return;

            DeployLoading = true;
            DeployLog.Clear();
            DeployOutput = "";

            void Log(string msg)
            {
                var entry = new DeployLogEntry { Message = msg };
                MainThread.BeginInvokeOnMainThread(() => DeployLog.Add(entry));
            }

            await Task.Run(() =>
            {
                try
                {
                    var domainPart = string.IsNullOrWhiteSpace(DeployDomain) || DeployDomain == "." ? "." : DeployDomain;
                    var userFull = domainPart == "." ? DeployUsername : $"{domainPart}\\{DeployUsername}";
                    var remoteFull = Path.Combine(DeployRemotePath, DeployScriptName).Replace("/", "\\");
                    var ext = Path.GetExtension(DeployScriptName).ToLower();
                    var isLocal = target.Equals("localhost", StringComparison.OrdinalIgnoreCase)
                                || target.Equals(Environment.MachineName, StringComparison.OrdinalIgnoreCase);

                    Log($"[1/3] Writing {DeployScriptName} to {target}:{DeployRemotePath}...");
                    var localTemp = Path.Combine(Path.GetTempPath(), DeployScriptName);
                    File.WriteAllText(localTemp, DeployScriptContent, System.Text.Encoding.UTF8);
                    try
                    {
                        string writeResult;
                        if (isLocal)
                        {
                            if (!Directory.Exists(DeployRemotePath)) Directory.CreateDirectory(DeployRemotePath);
                            File.Copy(localTemp, remoteFull, overwrite: true);
                            writeResult = $"Copied to {remoteFull}";
                        }
                        else
                        {
                            // Base64-encode content so Invoke-Command handles the write — bypasses remote UAC on admin shares
                            writeResult = RunPsSync(PsScripts.RemoteWriteBase64(target, userFull, DeployPassword, remoteFull, DeployScriptContent)) ?? "";
                        }
                        Log($"      {(string.IsNullOrWhiteSpace(writeResult) ? $"Written {DeployScriptContent.Length} bytes." : writeResult.Trim())}");
                    }
                    finally { try { File.Delete(localTemp); } catch { } }

                    if (DeployExecute)
                    {
                        Log($"[2/3] Executing {remoteFull} on {target}...");

                        var output = RunPsSync(PsScripts.RemoteExecuteCapture(target, userFull, DeployPassword, remoteFull, ext, isLocal));
                        Log("      --- Output ---");
                        foreach (var line in (output ?? "(no output)").Trim().Split('\n'))
                            Log("      " + line.TrimEnd());
                        Log("[3/3] Done.");
                    }
                    else
                    {
                        Log($"[2/3] Skipped execution — file is at {remoteFull} on {target}.");
                        Log("[3/3] Done.");
                    }
                }
                catch (Exception ex) { Log($"Error: {ex.Message}"); }
            });

            DeployLoading = false;
        });

        RunAsStartServiceCommand = new Command(async () =>
        {
            if (string.IsNullOrWhiteSpace(RunAsUsername) || string.IsNullOrWhiteSpace(RunAsPassword))
            { StatusMessage = "Username and password are required."; return; }
            if (string.IsNullOrWhiteSpace(RunAsCommand))
            { StatusMessage = "Enter a service name in the Command field."; return; }
            var target = await ResolveTargetAsync();
            if (target == null) return;
            RunAsLoading = true;
            var result = await Task.Run(() => RunPsSync(PsScripts.RunAsService(target, RunAsDomain, RunAsUsername, RunAsPassword, RunAsCommand, "Start")));
            RunAsOutput = string.IsNullOrWhiteSpace(result) ? $"Start-Service '{RunAsCommand}' issued." : result.Trim();
            RunAsLoading = false;
        });

        RunAsStopServiceCommand = new Command(async () =>
        {
            if (string.IsNullOrWhiteSpace(RunAsUsername) || string.IsNullOrWhiteSpace(RunAsPassword))
            { StatusMessage = "Username and password are required."; return; }
            if (string.IsNullOrWhiteSpace(RunAsCommand))
            { StatusMessage = "Enter a service name in the Command field."; return; }
            var target = await ResolveTargetAsync();
            if (target == null) return;
            RunAsLoading = true;
            var result = await Task.Run(() => RunPsSync(PsScripts.RunAsService(target, RunAsDomain, RunAsUsername, RunAsPassword, RunAsCommand, "Stop")));
            RunAsOutput = string.IsNullOrWhiteSpace(result) ? $"Stop-Service '{RunAsCommand}' issued." : result.Trim();
            RunAsLoading = false;
        });

        LoadEventLogCommand = new Command(async () => await RunTool(
            "Event Log", () => EventLogLoading = true, () => EventLogLoading = false,
            async target =>
            {
                var count = int.TryParse(EventLogCount, out var n) ? n : 50;
                var entries = await RunPsJson<List<EventLogEntry>>(target, PsScripts.EventLog(target, EventLogName, count));
                EventLogEntries.Clear();
                foreach (var e in entries ?? []) EventLogEntries.Add(e);
            }));

        LoadSharesCommand = new Command(async () => await RunTool(
            "Shared Folders", () => SharesLoading = true, () => SharesLoading = false,
            async target =>
            {
                var entries = await RunPsJson<List<ShareEntry>>(target, PsScripts.SharedFolders(target));
                Shares.Clear();
                foreach (var s in entries ?? []) Shares.Add(s);
            }));

        AddShareCommand = new Command(async () =>
        {
            if (string.IsNullOrWhiteSpace(NewShareName) || string.IsNullOrWhiteSpace(NewSharePath))
            { StatusMessage = "Share name and path are required."; return; }
            var target = await ResolveTargetAsync();
            if (target == null) return;
            await RunPsRaw(PsScripts.AddShare(target, NewShareName, NewSharePath, NewShareDesc));
            NewShareName = ""; NewSharePath = ""; NewShareDesc = "";
            LoadSharesCommand.Execute(null);
        });

        RemoveShareCommand = new Command<ShareEntry>(async share =>
        {
            if (share == null) return;
            var target = await ResolveTargetAsync();
            if (target == null) return;
            bool confirm = await Application.Current!.Windows[0].Page!.DisplayAlert(
                "Remove Share", $"Remove share '{share.Name}' on {target}?", "Remove", "Cancel");
            if (!confirm) return;
            await RunPsRaw(PsScripts.RemoveShare(target, share.Name));
            LoadSharesCommand.Execute(null);
        });

        LoadDiskSpaceCommand = new Command(async () => await RunTool(
            "Disk Space", () => DiskSpaceLoading = true, () => DiskSpaceLoading = false,
            async target =>
            {
                var entries = await RunPsJson<List<DriveEntry>>(target, PsScripts.DiskSpace(target));
                DriveEntries.Clear();
                foreach (var d in entries ?? []) DriveEntries.Add(d);
            }));

        ListGroupMembersCommand = new Command(async () => await RunTool(
            "Group Members", () => GroupMemberLoading = true, () => GroupMemberLoading = false,
            async target =>
            {
                GroupMembers.Clear();
                if (string.IsNullOrWhiteSpace(GrpMemberGroup) && !string.IsNullOrWhiteSpace(GrpMemberUser))
                {
                    // User → list groups they belong to
                    var groups = await RunPsJson<List<string>>(target, PsScripts.ListUserGroups(target, GrpMemberUser));
                    foreach (var g in groups ?? []) GroupMembers.Add(g);
                }
                else
                {
                    var entries = await RunPsJson<List<string>>(target, PsScripts.ListGroupMembers(target, GrpMemberGroup));
                    foreach (var m in entries ?? []) GroupMembers.Add(m);
                }
            }));

        AddToGroupCommand = new Command(async () =>
        {
            if (string.IsNullOrWhiteSpace(GrpMemberGroup) || string.IsNullOrWhiteSpace(GrpMemberUser))
            { StatusMessage = "Group and user are required."; return; }
            var target = await ResolveTargetAsync();
            if (target == null) return;
            GroupMemberLoading = true;
            var result = await Task.Run(() => RunPsSync(PsScripts.AddToGroup(target, GrpMemberGroup, GrpMemberUser)));
            StatusMessage = string.IsNullOrWhiteSpace(result) ? $"Added {GrpMemberUser} to {GrpMemberGroup}." : result.Trim();
            GroupMemberLoading = false;
            ListGroupMembersCommand.Execute(null);
        });

        RemoveFromGroupCommand = new Command(async () =>
        {
            if (string.IsNullOrWhiteSpace(GrpMemberGroup) || string.IsNullOrWhiteSpace(GrpMemberUser))
            { StatusMessage = "Group and user are required."; return; }
            var target = await ResolveTargetAsync();
            if (target == null) return;
            bool confirm = await Application.Current!.Windows[0].Page!.DisplayAlert(
                "Remove from Group", $"Remove '{GrpMemberUser}' from '{GrpMemberGroup}' on {target}?", "Remove", "Cancel");
            if (!confirm) return;
            GroupMemberLoading = true;
            var result = await Task.Run(() => RunPsSync(PsScripts.RemoveFromGroup(target, GrpMemberGroup, GrpMemberUser)));
            StatusMessage = string.IsNullOrWhiteSpace(result) ? $"Removed {GrpMemberUser} from {GrpMemberGroup}." : result.Trim();
            GroupMemberLoading = false;
            ListGroupMembersCommand.Execute(null);
        });

        DnsLookupCommand = new Command(async () =>
        {
            if (string.IsNullOrWhiteSpace(DnsQuery)) { StatusMessage = "Enter a hostname or IP to look up."; return; }
            DnsLoading = true;
            DnsOutput = "";
            var target = await ResolveTargetAsync();
            if (target == null) { DnsLoading = false; return; }
            var result = await Task.Run(() => RunPsSync(PsScripts.DnsLookup(target, DnsQuery)));
            DnsOutput = string.IsNullOrWhiteSpace(result) ? "No results." : result.Trim();
            DnsLoading = false;
        });

        DnsFlushCommand = new Command(async () =>
        {
            DnsLoading = true;
            var target = await ResolveTargetAsync();
            if (target == null) { DnsLoading = false; return; }
            var result = await Task.Run(() => RunPsSync(PsScripts.DnsFlush(target)));
            StatusMessage = string.IsNullOrWhiteSpace(result) ? $"DNS cache flushed on {target}." : result.Trim();
            DnsLoading = false;
        });

        RegGetCommand = new Command(async () =>
        {
            if (string.IsNullOrWhiteSpace(RegKey)) { StatusMessage = "Enter a registry key path."; return; }
            var target = await ResolveTargetAsync();
            if (target == null) return;
            RegistryLoading = true;
            RegOutput = "";
            var result = await Task.Run(() => RunPsSync(PsScripts.RegGet(target, RegHive, RegKey, RegValueName)));
            RegOutput = string.IsNullOrWhiteSpace(result) ? "(no value or key not found)" : result.Trim();
            RegistryLoading = false;
        });

        RegSetCommand = new Command(async () =>
        {
            if (string.IsNullOrWhiteSpace(RegKey) || string.IsNullOrWhiteSpace(RegValueName))
            { StatusMessage = "Key and value name are required."; return; }
            var target = await ResolveTargetAsync();
            if (target == null) return;
            RegistryLoading = true;
            RegOutput = "";
            var result = await Task.Run(() => RunPsSync(PsScripts.RegSet(target, RegHive, RegKey, RegValueName, RegValueData)));
            RegOutput = string.IsNullOrWhiteSpace(result) ? $"Set {RegHive}\\{RegKey}\\{RegValueName} = {RegValueData}" : result.Trim();
            RegistryLoading = false;
        });

        RegDeleteCommand = new Command(async () =>
        {
            if (string.IsNullOrWhiteSpace(RegKey) || string.IsNullOrWhiteSpace(RegValueName))
            { StatusMessage = "Key and value name are required."; return; }
            var target = await ResolveTargetAsync();
            if (target == null) return;
            bool confirm = await Application.Current!.Windows[0].Page!.DisplayAlert(
                "Delete Registry Value", $"Delete {RegHive}\\{RegKey}\\{RegValueName} on {target}?", "Delete", "Cancel");
            if (!confirm) return;
            RegistryLoading = true;
            RegOutput = "";
            var result = await Task.Run(() => RunPsSync(PsScripts.RegDelete(target, RegHive, RegKey, RegValueName)));
            RegOutput = string.IsNullOrWhiteSpace(result) ? $"Deleted {RegValueName} from {RegHive}\\{RegKey}." : result.Trim();
            RegistryLoading = false;
        });

        GpUpdateCommand = new Command(async () =>
        {
            GpUpdateLoading = true;
            GpUpdateOutput = "";
            var target = await ResolveTargetAsync();
            if (target == null) { GpUpdateLoading = false; return; }
            var result = await Task.Run(() => RunPsSync(PsScripts.GpUpdate(target)));
            GpUpdateOutput = string.IsNullOrWhiteSpace(result) ? "gpupdate completed (no output)." : result.Trim();
            GpUpdateLoading = false;
        });

        CopyBrowseCommand = new Command(async () =>
        {
            var result = await FilePicker.Default.PickAsync(new PickOptions { PickerTitle = "Select file to copy" });
            if (result == null) return;
            CopyLocalPath = result.FullPath;
        });

        CopyRunCommand = new Command(async () =>
        {
            if (string.IsNullOrWhiteSpace(CopyLocalPath))
            { StatusMessage = "Select a local file first."; return; }
            if (!File.Exists(CopyLocalPath))
            { StatusMessage = "Local file not found."; return; }
            var target = await ResolveTargetAsync();
            if (target == null) return;

            CopyLoading = true;
            CopyLog.Clear();

            void Log(string msg)
            {
                var entry = new DeployLogEntry { Message = msg };
                MainThread.BeginInvokeOnMainThread(() => CopyLog.Add(entry));
            }

            await Task.Run(() =>
            {
                try
                {
                    var fileName = Path.GetFileName(CopyLocalPath);
                    var remoteFull = Path.Combine(CopyRemoteDest, fileName).Replace("/", "\\");
                    var isLocal = IsLocal(target);
                    var domainPart = string.IsNullOrWhiteSpace(CopyDomain) || CopyDomain == "." ? "." : CopyDomain;
                    var userFull = domainPart == "." ? CopyUsername : $"{domainPart}\\{CopyUsername}";

                    Log($"[1/2] Copying {fileName} to {target}:{CopyRemoteDest}...");

                    if (isLocal)
                    {
                        if (CopyCreateDir && !Directory.Exists(CopyRemoteDest)) Directory.CreateDirectory(CopyRemoteDest);
                        File.Copy(CopyLocalPath, remoteFull, overwrite: true);
                        Log($"      Copied to {remoteFull}");
                    }
                    else
                    {
                        var useCreds = !string.IsNullOrWhiteSpace(CopyUsername) && !string.IsNullOrWhiteSpace(CopyPassword);
                        var credSuffix = useCreds
                            ? $" -Credential (New-Object PSCredential('{userFull}',(ConvertTo-SecureString '{CopyPassword}' -AsPlainText -Force)))"
                            : "";
                        if (CopyCreateDir)
                        {
                            var mkdir = RunPsSync($"Invoke-Command -ComputerName '{target}'{credSuffix} -ScriptBlock {{ New-Item -ItemType Directory -Path '{CopyRemoteDest}' -Force | Out-Null; 'dir ok' }}");
                            Log($"      Dir: {(mkdir?.Trim() == "dir ok" ? "ready" : mkdir?.Trim() ?? "skipped")}");
                        }
                        string writeResult;
                        if (useCreds)
                        {
                            var content = File.ReadAllText(CopyLocalPath);
                            writeResult = RunPsSync(PsScripts.RemoteWriteBase64(target, userFull, CopyPassword, remoteFull, content)) ?? "";
                        }
                        else
                        {
                            // No explicit creds — write via UNC admin share using current user's token
                            var uncPath = $"\\\\{target}\\{remoteFull.Replace(":", "$")}";
                            File.Copy(CopyLocalPath, uncPath, overwrite: true);
                            writeResult = $"Written via UNC to {uncPath}";
                        }
                        Log($"      {(string.IsNullOrWhiteSpace(writeResult) ? $"Written to {remoteFull}" : writeResult.Trim())}");
                    }

                    Log("[2/2] Done.");
                }
                catch (Exception ex) { Log($"Error: {ex.Message}"); }
            });

            CopyLoading = false;
        });

        PingSweepCommand = new Command(async () =>
        {
            var subnet = SweepSubnet.Trim();
            if (string.IsNullOrWhiteSpace(subnet)) { StatusMessage = "Enter a subnet (e.g. 192.168.1)."; return; }
            SweepLoading = true;
            SweepResults.Clear();
            StatusMessage = "";
            var results = await Task.Run(() =>
            {
                var script = PsScripts.PingSweep(subnet);
                var json = RunPsSync(script);
                if (string.IsNullOrWhiteSpace(json)) return new List<PingSweepResult>();
                try { return JsonSerializer.Deserialize<List<PingSweepResult>>(json) ?? []; }
                catch { return new List<PingSweepResult>(); }
            });
            foreach (var r in results) SweepResults.Add(r);
            StatusMessage = $"Sweep complete: {results.Count(r => r.Online)} online / {results.Count} total.";
            SweepLoading = false;
        });

        // Pre-fill credential fields from the shared service on startup
        ApplyCredentials();
    }

    private void ApplyCredentials()
    {
        if (_creds.HasUserCred)
        {
            DeployDomain   = _creds.UserDomain;
            DeployUsername = _creds.UserUsername;
            DeployPassword = _creds.UserPassword;
            CopyDomain     = _creds.UserDomain;
            CopyUsername   = _creds.UserUsername;
            CopyPassword   = _creds.UserPassword;
            RunAsDomain    = _creds.UserDomain;
            RunAsUsername  = _creds.UserUsername;
            RunAsPassword  = _creds.UserPassword;
        }
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private async Task RunTool(string name, Action setBusy, Action clearBusy, Func<string, Task> action)
    {
        setBusy();
        StatusMessage = "";
        try
        {
            var target = await ResolveTargetAsync();
            if (target == null) return;
            await action(target);
            StatusMessage = $"{name} loaded from {target}.";
        }
        catch (Exception ex) { StatusMessage = $"Error: {ex.Message}"; }
        finally { clearBusy(); }
    }

    private async Task<string?> ResolveTargetAsync()
    {
        if (!string.IsNullOrWhiteSpace(TargetComputer)) return TargetComputer.Trim();
        var result = await Application.Current!.Windows[0].Page!.DisplayPromptAsync(
            "Target Computer", "Enter a computer name or IP (blank = this machine):",
            placeholder: "hostname or IP");
        if (result != null) TargetComputer = result.Trim();
        return string.IsNullOrWhiteSpace(result) ? "localhost" : result.Trim();
    }

    private async Task<T?> RunPsJson<T>(string target, string script)
    {
        return await Task.Run(() =>
        {
            var json = RunPsSync(script);
            if (string.IsNullOrWhiteSpace(json)) return default;
            try { return JsonSerializer.Deserialize<T>(json); }
            catch { return default; }
        });
    }

    private async Task RunServiceAction(string action, string serviceName)
    {
        var target = await ResolveTargetAsync();
        if (target == null) return;
        var isLocal = IsLocal(target);
        var remote = isLocal ? "" : $"-ComputerName '{target}'";
        var script = $"{action}-Service -Name '{serviceName}' {remote} -Force -ErrorAction SilentlyContinue";
        await Task.Run(() => RunPsSync(script));
        StatusMessage = $"{action}ed {serviceName} on {target}.";
    }

    private static string RunPsSync(string script)
    {
        var temp = Path.Combine(Path.GetTempPath(), $"tool_{Guid.NewGuid():N}.ps1");
        File.WriteAllText(temp, script);
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
            var output = proc.StandardOutput.ReadToEnd();
            proc.WaitForExit();
            return output;
        }
        finally { try { File.Delete(temp); } catch { } }
    }

    private async Task RunPsRaw(string script) => await Task.Run(() => RunPsSync(script));

    private void FilterApps()
    {
        Apps.Clear();
        var q = _appSearch.Trim();
        var filtered = string.IsNullOrWhiteSpace(q)
            ? _allApps
            : _allApps.Where(a => a.Name.Contains(q, StringComparison.OrdinalIgnoreCase)
                                || a.Publisher.Contains(q, StringComparison.OrdinalIgnoreCase));
        foreach (var a in filtered.OrderBy(a => a.Name)) Apps.Add(a);
    }

    private void FilterProcesses()
    {
        Processes.Clear();
        var q = _processSearch.Trim();
        var filtered = string.IsNullOrWhiteSpace(q)
            ? _allProcesses
            : _allProcesses.Where(p => p.Name.Contains(q, StringComparison.OrdinalIgnoreCase));
        foreach (var p in filtered.OrderBy(p => p.Name)) Processes.Add(p);
    }

    private void FilterServices()
    {
        Services.Clear();
        var q = _serviceSearch.Trim();
        var filtered = string.IsNullOrWhiteSpace(q)
            ? _allServices
            : _allServices.Where(s => s.DisplayName.Contains(q, StringComparison.OrdinalIgnoreCase)
                                   || s.Name.Contains(q, StringComparison.OrdinalIgnoreCase));
        foreach (var s in filtered.OrderBy(s => s.DisplayName)) Services.Add(s);
    }

    private static bool IsLocal(string target) =>
        string.IsNullOrWhiteSpace(target)
        || target.Equals("localhost", StringComparison.OrdinalIgnoreCase)
        || target.Equals(Environment.MachineName, StringComparison.OrdinalIgnoreCase);
}

// ── PowerShell script templates ───────────────────────────────────────────────

internal static class PsScripts
{
    public static string InstalledApps(string target) => ScriptWithTarget(target, """
            $paths = @(
                'HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\*',
                'HKLM:\SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\*',
                'HKCU:\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\*'
            )
            Get-ItemProperty $paths |
                Where-Object { $_.DisplayName } |
                Select-Object @{N='Name';E={$_.DisplayName}},
                              @{N='Publisher';E={$_.Publisher}},
                              @{N='Version';E={$_.DisplayVersion}},
                              @{N='InstallDate';E={$_.InstallDate}} |
                Sort-Object Name
        """, 2);

    public static string Services(string target) => ScriptWithTarget(target, """
            Get-Service | Select-Object Name, DisplayName,
                @{N='Status';E={$_.Status.ToString()}},
                @{N='StartType';E={$_.StartType.ToString()}}
        """, 2);

    public static string LocalUsers(string target) => ScriptWithTarget(target, """
            Get-LocalUser | Select-Object Name, FullName, Enabled, Description,
                @{N='LastLogon';E={if($_.LastLogon){$_.LastLogon.ToString('g')}else{'Never'}}}
        """, 2);

    public static string LocalGroups(string target) => ScriptWithTarget(target, """
            Get-LocalGroup | ForEach-Object {
                $members = @(Get-LocalGroupMember $_.Name -ErrorAction SilentlyContinue | Select-Object -ExpandProperty Name)
                [ordered]@{ Name=$_.Name; Description=$_.Description; Members=$members }
            }
        """, 3);

    public static string Printers(string target) => ScriptWithTarget(target, """
            Get-WmiObject Win32_Printer | Select-Object Name, DriverName, PortName,
                @{N='IsDefault';E={$_.Default}},
                @{N='Status';E={$_.PrinterStatus}}
        """, 2);

    // Wraps a script body in local/remote dispatch boilerplate
    private static string ScriptWithTarget(string target, string body, int depth)
    {
        return
            "$ErrorActionPreference = 'SilentlyContinue'\n" +
            $"$isLocal = ('{target}' -eq 'localhost' -or '{target}' -eq $env:COMPUTERNAME)\n" +
            "$sb = {\n" +
            body + "\n" +
            "}\n" +
            $"if ($isLocal) {{ @(& $sb) | ConvertTo-Json -Depth {depth} }}\n" +
            $"else {{ @(Invoke-Command -ComputerName '{target}' -ScriptBlock $sb) | ConvertTo-Json -Depth {depth} }}\n";
    }

    public static string NetworkInfo(string target) => ScriptWithTarget(target, """
            Get-NetAdapter | Where-Object { $_.Status -ne 'Not Present' } | ForEach-Object {
                $adapter = $_
                $ip = Get-NetIPConfiguration -InterfaceIndex $adapter.InterfaceIndex -ErrorAction SilentlyContinue
                [ordered]@{
                    Name           = $adapter.Name
                    Description    = $adapter.InterfaceDescription
                    MacAddress     = $adapter.MacAddress
                    Status         = $adapter.Status.ToString()
                    IPAddress      = ($ip.IPv4Address.IPAddress -join ', ')
                    SubnetMask     = ($ip.IPv4Address | ForEach-Object { $_.PrefixLength } | ForEach-Object { "/$_" }) -join ', '
                    DefaultGateway = ($ip.IPv4DefaultGateway.NextHop -join ', ')
                    DnsServers     = ($ip.DNSServer.ServerAddresses -join ', ')
                }
            }
        """, 3);

    public static string LoggedOnUser(string target)
    {
        return
            "$ErrorActionPreference = 'SilentlyContinue'\n" +
            $"$isLocal = ('{target}' -eq 'localhost' -or '{target}' -eq $env:COMPUTERNAME)\n" +
            "$sb = {\n" +
            "    $explorer = Get-WmiObject Win32_Process -Filter \"Name='explorer.exe'\" | Select-Object -First 1\n" +
            "    if ($explorer) {\n" +
            "        $owner = $explorer.GetOwner()\n" +
            "        \"Logged on: $($owner.Domain)\\$($owner.User)\"\n" +
            "    } else {\n" +
            "        $cs = Get-WmiObject Win32_ComputerSystem | Select-Object -ExpandProperty UserName\n" +
            "        if ($cs) { \"Logged on: $cs\" } else { 'No user logged on interactively.' }\n" +
            "    }\n" +
            "}\n" +
            $"if ($isLocal) {{ & $sb }}\n" +
            $"else {{ \"$(Invoke-Command -ComputerName '{target}' -ScriptBlock $sb)\" }}\n";
    }

    public static string LocalAdmins(string target)
    {
        return
            "$ErrorActionPreference = 'SilentlyContinue'\n" +
            $"$isLocal = ('{target}' -eq 'localhost' -or '{target}' -eq $env:COMPUTERNAME)\n" +
            "$sb = {\n" +
            "    $inMembers = $false\n" +
            "    $members = @(net localgroup Administrators 2>$null | ForEach-Object {\n" +
            "        if ($_ -match '^-{10}') { $inMembers = $true; return }\n" +
            "        if ($inMembers -and $_ -notmatch 'The command completed' -and $_.Trim() -ne '') { $_.Trim() }\n" +
            "    })\n" +
            "    $members\n" +
            "}\n" +
            $"if ($isLocal) {{ @(& $sb) | ConvertTo-Json -Depth 1 }}\n" +
            $"else {{ @(Invoke-Command -ComputerName '{target}' -ScriptBlock $sb | ForEach-Object {{ \"$_\" }}) | ConvertTo-Json -Depth 1 }}\n";
    }

    public static string SecurityPatches(string target) => ScriptWithTarget(target, """
            Get-HotFix |
                Sort-Object InstalledOn -Descending |
                Select-Object @{N='HotFixId';E={$_.HotFixID}},
                              @{N='Description';E={$_.Description}},
                              @{N='InstalledOn';E={if($_.InstalledOn){$_.InstalledOn.ToString('d')}else{'Unknown'}}},
                              @{N='InstalledBy';E={$_.InstalledBy}}
        """, 2);

    public static string ProcessList(string target) => ScriptWithTarget(target, """
            Get-Process | Select-Object Id, Name,
                @{N='CPU';E={if($_.CPU){[math]::Round($_.CPU,1).ToString()}else{'0'}}},
                @{N='WorkingSet';E={[math]::Round($_.WorkingSet64/1MB,1).ToString() + ' MB'}},
                @{N='StartTime';E={try{$_.StartTime.ToString('g')}catch{'?'}}},
                @{N='Owner';E={''}}
        """, 2);

    public static string KillProcess(string target, int pid)
    {
        return
            "$ErrorActionPreference = 'SilentlyContinue'\n" +
            $"$isLocal = ('{target}' -eq 'localhost' -or '{target}' -eq $env:COMPUTERNAME)\n" +
            $"if ($isLocal) {{ Stop-Process -Id {pid} -Force }}\n" +
            $"else {{ Invoke-Command -ComputerName '{target}' -ScriptBlock {{ Stop-Process -Id {pid} -Force }} }}\n";
    }

    public static string RemoteReboot(string target)
    {
        return
            "$ErrorActionPreference = 'SilentlyContinue'\n" +
            $"$isLocal = ('{target}' -eq 'localhost' -or '{target}' -eq $env:COMPUTERNAME)\n" +
            $"if ($isLocal) {{ shutdown /r /t 30 /c 'IT Support Tool: Reboot initiated' }}\n" +
            $"else {{ shutdown /r /m \\\\{target} /t 30 /c 'IT Support Tool: Reboot initiated' }}\n" +
            "Write-Output 'Reboot scheduled in 30 seconds.'\n";
    }

    public static string CancelReboot(string target)
    {
        return
            "$ErrorActionPreference = 'SilentlyContinue'\n" +
            $"$isLocal = ('{target}' -eq 'localhost' -or '{target}' -eq $env:COMPUTERNAME)\n" +
            $"if ($isLocal) {{ shutdown /a }}\n" +
            $"else {{ shutdown /a /m \\\\{target} }}\n" +
            "Write-Output 'Shutdown/reboot cancelled.'\n";
    }

    public static string EnableRdp(string target)
    {
        return
            "$ErrorActionPreference = 'SilentlyContinue'\n" +
            $"$isLocal = ('{target}' -eq 'localhost' -or '{target}' -eq $env:COMPUTERNAME)\n" +
            "$sb = {\n" +
            "    Set-ItemProperty -Path 'HKLM:\\System\\CurrentControlSet\\Control\\Terminal Server' -Name 'fDenyTSConnections' -Value 0\n" +
            "    Enable-NetFirewallRule -DisplayGroup 'Remote Desktop' -ErrorAction SilentlyContinue\n" +
            "    Write-Output 'RDP enabled.'\n" +
            "}\n" +
            $"if ($isLocal) {{ & $sb }}\n" +
            $"else {{ Invoke-Command -ComputerName '{target}' -ScriptBlock $sb }}\n";
    }

    public static string RunAs(string target, string domain, string username, string password, string command, string args)
    {
        var domainPart = string.IsNullOrWhiteSpace(domain) || domain == "." ? "." : domain;
        var userFull = domainPart == "." ? username : $"{domainPart}\\{username}";
        var outFile = $@"C:\Windows\Temp\_runas_{Guid.NewGuid().ToString("N")[..8]}.txt";
        return
            "$ErrorActionPreference = 'SilentlyContinue'\n" +
            $"$secpw = ConvertTo-SecureString '{password}' -AsPlainText -Force\n" +
            $"$cred = New-Object PSCredential('{userFull}', $secpw)\n" +
            $"$isLocal = ('{target}' -eq 'localhost' -or '{target}' -eq $env:COMPUTERNAME)\n" +
            $"if ($isLocal) {{\n" +
            // Local: Start-Process writes output to C:\Windows\Temp (accessible to all users)
            $"    Start-Process cmd.exe -ArgumentList \"/c {command} {args} > `\"{outFile}`\" 2>&1\" -Credential $cred -NoNewWindow -Wait -ErrorAction Stop\n" +
            $"    if (Test-Path '{outFile}') {{ Get-Content '{outFile}'; Remove-Item '{outFile}' -Force }}\n" +
            $"    else {{ Write-Output '(no output — may be GUI or instant-exit)' }}\n" +
            $"}} else {{\n" +
            // Remote: Invoke-Command runs as the provided credentials, captures output directly
            $"    $sb = {{ param($cmd, $a) cmd.exe /c \"$cmd $a\" 2>&1 | ForEach-Object {{ \"$_\" }} }}\n" +
            $"    Invoke-Command -ComputerName '{target}' -Credential $cred -ScriptBlock $sb -ArgumentList '{command}','{args}' | ForEach-Object {{ \"$_\" }}\n" +
            $"}}\n";
    }

    public static string RunAsService(string target, string domain, string username, string password, string serviceName, string action)
    {
        var userFull = string.IsNullOrWhiteSpace(domain) || domain == "." ? username : $"{domain}\\{username}";
        return
            "$ErrorActionPreference = 'SilentlyContinue'\n" +
            $"$secpw = ConvertTo-SecureString '{password}' -AsPlainText -Force\n" +
            $"$cred = New-Object System.Management.Automation.PSCredential('{userFull}', $secpw)\n" +
            $"$isLocal = ('{target}' -eq 'localhost' -or '{target}' -eq $env:COMPUTERNAME)\n" +
            $"if ($isLocal) {{\n" +
            $"    {action}-Service -Name '{serviceName}' -Force\n" +
            $"    Write-Output '{action}-Service complete.'\n" +
            $"}} else {{\n" +
            $"    Invoke-Command -ComputerName '{target}' -Credential $cred -ScriptBlock {{ {action}-Service -Name '{serviceName}' -Force }}\n" +
            $"    Write-Output '{action}-Service on {target} complete.'\n" +
            $"}}\n";
    }

    public static string EventLog(string target, string logName, int count) => ScriptWithTarget(target,
            $"Get-WinEvent -LogName '{logName}' -MaxEvents {count} -ErrorAction SilentlyContinue |\n" +
            "    Select-Object @{N='TimeGenerated';E={$_.TimeCreated.ToString('g')}},\n" +
            "                  @{N='EntryType';E={$_.LevelDisplayName}},\n" +
            "                  @{N='Source';E={$_.ProviderName}},\n" +
            "                  @{N='EventId';E={$_.Id.ToString()}},\n" +
            "                  @{N='Message';E={$_.Message -replace '`r`n',' ' -replace '`n',' '}}\n",
            2);

    public static string SharedFolders(string target) => ScriptWithTarget(target, """
            Get-WmiObject -Class Win32_Share |
                Select-Object Name,
                              @{N='Path';E={$_.Path}},
                              @{N='Description';E={$_.Description}},
                              @{N='Type';E={switch($_.Type){0{'Disk'} 1{'Print'} 2{'Device'} 3{'IPC'} default{'Other'}}}}
        """, 2);

    public static string AddShare(string target, string name, string path, string desc)
    {
        return
            "$ErrorActionPreference = 'SilentlyContinue'\n" +
            $"$isLocal = ('{target}' -eq 'localhost' -or '{target}' -eq $env:COMPUTERNAME)\n" +
            "$sb = {\n" +
            $"    New-SmbShare -Name '{name}' -Path '{path}' -Description '{desc}' -FullAccess Everyone\n" +
            "}\n" +
            $"if ($isLocal) {{ & $sb }}\n" +
            $"else {{ Invoke-Command -ComputerName '{target}' -ScriptBlock $sb }}\n";
    }

    public static string RemoveShare(string target, string name)
    {
        return
            "$ErrorActionPreference = 'SilentlyContinue'\n" +
            $"$isLocal = ('{target}' -eq 'localhost' -or '{target}' -eq $env:COMPUTERNAME)\n" +
            "$sb = {\n" +
            $"    Remove-SmbShare -Name '{name}' -Force\n" +
            "}\n" +
            $"if ($isLocal) {{ & $sb }}\n" +
            $"else {{ Invoke-Command -ComputerName '{target}' -ScriptBlock $sb }}\n";
    }

    public static string RemoteWriteBase64(string target, string user, string password, string remotePath, string content)
    {
        var b64 = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(content));
        return
            "$ErrorActionPreference = 'SilentlyContinue'\n" +
            $"$secpw = ConvertTo-SecureString '{password}' -AsPlainText -Force\n" +
            $"$cred = New-Object PSCredential('{user}', $secpw)\n" +
            "$sb = {\n" +
            "    param($path, $b64)\n" +
            "    $bytes = [Convert]::FromBase64String($b64)\n" +
            "    $text = [System.Text.Encoding]::UTF8.GetString($bytes)\n" +
            "    $dir = Split-Path $path -Parent\n" +
            "    if (-not (Test-Path $dir)) { New-Item -ItemType Directory -Path $dir -Force | Out-Null }\n" +
            "    [System.IO.File]::WriteAllText($path, $text, [System.Text.Encoding]::UTF8)\n" +
            $"    Write-Output \"Written $($text.Length) chars to $path\"\n" +
            "}\n" +
            $"Invoke-Command -ComputerName '{target}' -Credential $cred -ScriptBlock $sb -ArgumentList '{remotePath}','{b64}'\n";
    }

    public static string CopyViaAdminShare(string target, string user, string password, string localPath, string remotePath)
    {
        var drive = char.ToLower(remotePath[0]);
        var rest = remotePath[2..].TrimStart('\\', '/').Replace('/', '\\');
        var uncShare = $@"\\{target}\{drive}$";
        var uncFull  = $@"\\{target}\{drive}$\{rest}";
        return
            "$ErrorActionPreference = 'SilentlyContinue'\n" +
            // Drop any existing session to this server first to avoid "multiple connections" error
            $"net use '{uncShare}' /delete /y 2>$null | Out-Null\n" +
            "$ErrorActionPreference = 'Stop'\n" +
            "try {\n" +
            $"    net use '{uncShare}' '{password}' /user:'{user}'\n" +
            $"    $dir = Split-Path '{uncFull}' -Parent\n" +
            $"    if (-not (Test-Path $dir)) {{ New-Item -ItemType Directory -Path $dir -Force | Out-Null }}\n" +
            $"    Copy-Item -Path '{localPath}' -Destination '{uncFull}' -Force\n" +
            $"    Write-Output 'Copied to {uncFull}'\n" +
            "} catch {\n" +
            "    Write-Output \"Copy error: $_\"\n" +
            "} finally {\n" +
            $"    net use '{uncShare}' /delete /y 2>$null | Out-Null\n" +
            "}\n";
    }

    public static string RemoteExecuteCapture(string target, string user, string password, string remotePath, string ext, bool isLocal)
    {
        var run = ext == ".ps1"
            ? "    powershell.exe -NoProfile -ExecutionPolicy Bypass -File $path 2>&1 | ForEach-Object { \"$_\" }\n"
            : "    cmd.exe /c $path 2>&1 | ForEach-Object { \"$_\" }\n";
        return
            "$ErrorActionPreference = 'SilentlyContinue'\n" +
            $"$secpw = ConvertTo-SecureString '{password}' -AsPlainText -Force\n" +
            $"$cred = New-Object PSCredential('{user}', $secpw)\n" +
            "$sb = {\n" +
            "    param($path)\n" +
            run +
            "}\n" +
            (isLocal
                ? $"& $sb '{remotePath}'\n"
                : $"Invoke-Command -ComputerName '{target}' -Credential $cred -ScriptBlock $sb -ArgumentList '{remotePath}' | ForEach-Object {{ \"$_\" }}\n");
    }

    public static string LocalExecuteToFile(string remotePath, string ext, string outFile)
    {
        var run = ext == ".ps1"
            ? $"$out = powershell.exe -NoProfile -ExecutionPolicy Bypass -File '{remotePath}' 2>&1"
            : $"$out = cmd.exe /c '{remotePath}' 2>&1";
        return
            "$ErrorActionPreference = 'SilentlyContinue'\n" +
            run + "\n" +
            $"[System.IO.File]::WriteAllText('{outFile}', ($out -join \"`n\"), [System.Text.Encoding]::UTF8)\n";
    }

    public static string RemoteExecuteToShare(string target, string user, string password, string remotePath, string ext, string uncOut, string localMachine, string localUser, string localPassword)
    {
        var localShare = $@"\\{localMachine}\c$";
        var run = ext == ".ps1"
            ? "    $out = powershell.exe -NoProfile -ExecutionPolicy Bypass -File $path 2>&1\n"
            : "    $out = cmd.exe /c $path 2>&1\n";
        return
            "$ErrorActionPreference = 'SilentlyContinue'\n" +
            $"$secpw = ConvertTo-SecureString '{password}' -AsPlainText -Force\n" +
            $"$cred = New-Object PSCredential('{user}', $secpw)\n" +
            "$sb = {\n" +
            "    param($path, $outPath, $localShare, $localUser, $localPass)\n" +
            $"    net use $localShare $localPass /user:\"$localUser\" | Out-Null\n" +
            "    try {\n" +
            "    " + run +
            "        [System.IO.File]::WriteAllText($outPath, ($out -join \"`n\"), [System.Text.Encoding]::UTF8)\n" +
            "    } finally { net use $localShare /delete /y | Out-Null }\n" +
            "}\n" +
            $"Invoke-Command -ComputerName '{target}' -Credential $cred -ScriptBlock $sb -ArgumentList '{remotePath}','{uncOut}','{localShare}','{localMachine}\\{localUser}','{localPassword}'\n";
    }

    // kept for reference — superseded by RemoteExecuteToShare
    public static string RemoteExecuteViaShare(string target, string user, string password, string remotePath, string ext)
    {
        var outFile = $"_deploy_out_{Guid.NewGuid().ToString("N")[..8]}.txt";
        var remoteOut = $@"C:\Windows\Temp\{outFile}";
        var uncShare  = $@"\\{target}\c$";
        var uncOut    = $@"\\{target}\c$\Windows\Temp\{outFile}";
        var runLine   = ext == ".ps1"
            ? "    $out = powershell.exe -NoProfile -ExecutionPolicy Bypass -File $path 2>&1\n    [System.IO.File]::WriteAllText($outPath, ($out -join \"`n\"), [System.Text.Encoding]::UTF8)\n"
            : "    $out = cmd.exe /c $path 2>&1\n    [System.IO.File]::WriteAllText($outPath, ($out -join \"`n\"), [System.Text.Encoding]::UTF8)\n";
        return
            "$ErrorActionPreference = 'SilentlyContinue'\n" +
            $"$secpw = ConvertTo-SecureString '{password}' -AsPlainText -Force\n" +
            $"$cred = New-Object PSCredential('{user}', $secpw)\n" +
            $"$isLocal = ('{target}' -eq 'localhost' -or '{target}' -eq $env:COMPUTERNAME)\n" +
            "$sb = {\n" +
            "    param($path, $outPath)\n" +
            runLine +
            "}\n" +
            // Local: run directly, read the output file from the local path
            $"if ($isLocal) {{\n" +
            $"    & $sb '{remotePath}' '{remoteOut}'\n" +
            $"    if (Test-Path '{remoteOut}') {{ Get-Content '{remoteOut}'; Remove-Item '{remoteOut}' -Force }}\n" +
            "} else {\n" +
            // Remote: Invoke-Command writes output file on target, pull it back via admin share
            "    $invokeErr = $null\n" +
            "    try {\n" +
            $"        Invoke-Command -ComputerName '{target}' -Credential $cred -ScriptBlock $sb -ArgumentList '{remotePath}','{remoteOut}' -ErrorAction Stop\n" +
            "    } catch { $invokeErr = $_.ToString() }\n" +
            $"    net use '{uncShare}' '{password}' /user:'{user}' | Out-Null\n" +
            "    try {\n" +
            $"        if ($invokeErr) {{ Write-Output \"WinRM error: $invokeErr\" }}\n" +
            $"        elseif (Test-Path '{uncOut}') {{ Get-Content '{uncOut}'; Remove-Item '{uncOut}' -Force }}\n" +
            $"        else {{ Write-Output '(script ran but wrote no output)' }}\n" +
            "    } finally {\n" +
            $"        net use '{uncShare}' /delete /y | Out-Null\n" +
            "    }\n" +
            "}\n";
    }

    public static string DiskSpace(string target)
    {
        // ConvertTo-Json runs INSIDE the scriptblock so deserialization over WinRM doesn't corrupt property names
        return
            "$ErrorActionPreference = 'SilentlyContinue'\n" +
            $"$isLocal = ('{target}' -eq 'localhost' -or '{target}' -eq $env:COMPUTERNAME)\n" +
            "$sb = {\n" +
            "    $drives = @(Get-WmiObject -Class Win32_LogicalDisk -Filter \"DriveType=3\" | ForEach-Object {\n" +
            "        [PSCustomObject]@{\n" +
            "            Drive   = $_.DeviceID\n" +
            "            Label   = if ($_.VolumeName) { $_.VolumeName } else { '' }\n" +
            "            SizeGB  = [math]::Round($_.Size/1GB,1)\n" +
            "            UsedGB  = [math]::Round(($_.Size-$_.FreeSpace)/1GB,1)\n" +
            "            FreeGB  = [math]::Round($_.FreeSpace/1GB,1)\n" +
            "            PctUsed = if ($_.Size -gt 0) { [math]::Round(($_.Size-$_.FreeSpace)/$_.Size*100) } else { 0 }\n" +
            "        }\n" +
            "    })\n" +
            "    $j = $drives | ConvertTo-Json -Depth 2\n" +
            "    if ($j -match '^\\s*\\{') { \"[$j]\" } else { $j }\n" +
            "}\n" +
            $"if ($isLocal) {{ & $sb }}\n" +
            $"else {{ Invoke-Command -ComputerName '{target}' -ScriptBlock $sb }}\n";
    }

    public static string ListGroupMembers(string target, string group)
    {
        return
            "$ErrorActionPreference = 'SilentlyContinue'\n" +
            $"$isLocal = ('{target}' -eq 'localhost' -or '{target}' -eq $env:COMPUTERNAME)\n" +
            "$sb = {\n" +
            $"    param($g)\n" +
            $"    @(Get-LocalGroupMember -Group $g -ErrorAction SilentlyContinue | Select-Object -ExpandProperty Name)\n" +
            "}\n" +
            $"if ($isLocal) {{ @(& $sb '{group}') | ConvertTo-Json -Depth 1 }}\n" +
            $"else {{ @(Invoke-Command -ComputerName '{target}' -ScriptBlock $sb -ArgumentList '{group}' | ForEach-Object {{ \"$_\" }}) | ConvertTo-Json -Depth 1 }}\n";
    }

    public static string ListUserGroups(string target, string user)
    {
        return
            "$ErrorActionPreference = 'SilentlyContinue'\n" +
            $"$isLocal = ('{target}' -eq 'localhost' -or '{target}' -eq $env:COMPUTERNAME)\n" +
            "$sb = {\n" +
            $"    param($u)\n" +
            "    @(Get-LocalGroup | ForEach-Object {\n" +
            "        $grp = $_\n" +
            "        $members = @(Get-LocalGroupMember -Group $grp.Name -ErrorAction SilentlyContinue)\n" +
            "        if ($members | Where-Object { $_.Name -like \"*$u*\" }) { $grp.Name }\n" +
            "    }) | ConvertTo-Json -Depth 1\n" +
            "}\n" +
            $"if ($isLocal) {{ & $sb '{user}' }}\n" +
            $"else {{ Invoke-Command -ComputerName '{target}' -ScriptBlock $sb -ArgumentList '{user}' }}\n";
    }

    public static string AddToGroup(string target, string group, string user)
    {
        return
            "$ErrorActionPreference = 'SilentlyContinue'\n" +
            $"$isLocal = ('{target}' -eq 'localhost' -or '{target}' -eq $env:COMPUTERNAME)\n" +
            "$sb = {\n" +
            $"    param($g, $u)\n" +
            $"    Add-LocalGroupMember -Group $g -Member $u -ErrorAction Stop\n" +
            $"    Write-Output \"Added $u to $g\"\n" +
            "}\n" +
            $"if ($isLocal) {{ & $sb '{group}' '{user}' }}\n" +
            $"else {{ Invoke-Command -ComputerName '{target}' -ScriptBlock $sb -ArgumentList '{group}','{user}' }}\n";
    }

    public static string RemoveFromGroup(string target, string group, string user)
    {
        return
            "$ErrorActionPreference = 'SilentlyContinue'\n" +
            $"$isLocal = ('{target}' -eq 'localhost' -or '{target}' -eq $env:COMPUTERNAME)\n" +
            "$sb = {\n" +
            $"    param($g, $u)\n" +
            $"    Remove-LocalGroupMember -Group $g -Member $u -ErrorAction Stop\n" +
            $"    Write-Output \"Removed $u from $g\"\n" +
            "}\n" +
            $"if ($isLocal) {{ & $sb '{group}' '{user}' }}\n" +
            $"else {{ Invoke-Command -ComputerName '{target}' -ScriptBlock $sb -ArgumentList '{group}','{user}' }}\n";
    }

    public static string DnsLookup(string target, string query)
    {
        return
            "$ErrorActionPreference = 'SilentlyContinue'\n" +
            $"$isLocal = ('{target}' -eq 'localhost' -or '{target}' -eq $env:COMPUTERNAME)\n" +
            "$sb = {\n" +
            $"    param($q)\n" +
            "    if (Get-Command Resolve-DnsName -ErrorAction SilentlyContinue) {\n" +
            "        Resolve-DnsName $q -ErrorAction SilentlyContinue | Out-String\n" +
            "    } else {\n" +
            "        [System.Net.Dns]::GetHostAddresses($q) | ForEach-Object { $_.IPAddressToString } | Out-String\n" +
            "    }\n" +
            "}\n" +
            $"if ($isLocal) {{ & $sb '{query}' }}\n" +
            $"else {{ Invoke-Command -ComputerName '{target}' -ScriptBlock $sb -ArgumentList '{query}' | Out-String }}\n";
    }

    public static string DnsFlush(string target)
    {
        return
            "$ErrorActionPreference = 'SilentlyContinue'\n" +
            $"$isLocal = ('{target}' -eq 'localhost' -or '{target}' -eq $env:COMPUTERNAME)\n" +
            "$sb = {\n" +
            "    Clear-DnsClientCache -ErrorAction SilentlyContinue\n" +
            "    Write-Output 'DNS client cache cleared.'\n" +
            "}\n" +
            $"if ($isLocal) {{ & $sb }}\n" +
            $"else {{ Invoke-Command -ComputerName '{target}' -ScriptBlock $sb }}\n";
    }

    public static string RegGet(string target, string hive, string key, string valueName)
    {
        return
            "$ErrorActionPreference = 'Continue'\n" +
            $"$isLocal = ('{target}' -eq 'localhost' -or '{target}' -eq $env:COMPUTERNAME)\n" +
            "$sb = {\n" +
            $"    param($h, $k, $v)\n" +
            "    try {\n" +
            "        $path = \"$($h):\\$k\"\n" +
            "        if ([string]::IsNullOrWhiteSpace($v)) {\n" +
            "            $subkeys = @(Get-ChildItem -Path $path -ErrorAction SilentlyContinue | Select-Object -ExpandProperty PSChildName)\n" +
            "            $vals    = Get-ItemProperty -Path $path -ErrorAction SilentlyContinue\n" +
            "            $props   = if ($vals) { @($vals.PSObject.Properties | Where-Object { $_.Name -notmatch '^PS' } | ForEach-Object { \"  $($_.Name) = $($_.Value)\" }) } else { @() }\n" +
            "            $out  = \"Path: $path`n`nSubkeys:`n\"\n" +
            "            $out += if ($subkeys.Count -gt 0) { ($subkeys | ForEach-Object { \"  $_\" }) -join \"`n\" } else { '  (none)' }\n" +
            "            $out += \"`n`nValues:`n\"\n" +
            "            $out += if ($props.Count -gt 0) { $props -join \"`n\" } else { '  (none)' }\n" +
            "            $out\n" +
            "        } else {\n" +
            "            $val = (Get-ItemProperty -Path $path -Name $v -ErrorAction Stop).$v\n" +
            "            \"$v = $val\"\n" +
            "        }\n" +
            "    } catch { \"ERROR: $_\" }\n" +
            "}\n" +
            $"if ($isLocal) {{ (& $sb '{hive}' '{key}' '{valueName}') | Out-String }}\n" +
            $"else {{ (Invoke-Command -ComputerName '{target}' -ScriptBlock $sb -ArgumentList '{hive}','{key}','{valueName}') | Out-String }}\n";
    }

    public static string RegSet(string target, string hive, string key, string valueName, string data)
    {
        var path = $"Registry::{hive}\\{key}";
        return
            "$ErrorActionPreference = 'SilentlyContinue'\n" +
            $"$isLocal = ('{target}' -eq 'localhost' -or '{target}' -eq $env:COMPUTERNAME)\n" +
            "$sb = {\n" +
            $"    Set-ItemProperty -Path '{path}' -Name '{valueName}' -Value '{data}' -ErrorAction Stop\n" +
            $"    Write-Output \"Set '{valueName}' = '{data}' on '{path}'\"\n" +
            "}\n" +
            $"if ($isLocal) {{ & $sb }}\n" +
            $"else {{ Invoke-Command -ComputerName '{target}' -ScriptBlock $sb | Out-String }}\n";
    }

    public static string RegDelete(string target, string hive, string key, string valueName)
    {
        var path = $"Registry::{hive}\\{key}";
        return
            "$ErrorActionPreference = 'SilentlyContinue'\n" +
            $"$isLocal = ('{target}' -eq 'localhost' -or '{target}' -eq $env:COMPUTERNAME)\n" +
            "$sb = {\n" +
            $"    Remove-ItemProperty -Path '{path}' -Name '{valueName}' -ErrorAction Stop\n" +
            $"    Write-Output \"Deleted '{valueName}' from '{path}'\"\n" +
            "}\n" +
            $"if ($isLocal) {{ & $sb }}\n" +
            $"else {{ Invoke-Command -ComputerName '{target}' -ScriptBlock $sb | Out-String }}\n";
    }

    public static string GpUpdate(string target)
    {
        return
            "$ErrorActionPreference = 'SilentlyContinue'\n" +
            $"$isLocal = ('{target}' -eq 'localhost' -or '{target}' -eq $env:COMPUTERNAME)\n" +
            $"if ($isLocal) {{ gpupdate /force 2>&1 | Out-String }}\n" +
            $"else {{ Invoke-Command -ComputerName '{target}' -ScriptBlock {{ & gpupdate /force 2>&1 | Out-String }} }}\n";
    }

    public static string PingSweep(string subnet) => $$"""
        $ErrorActionPreference = 'SilentlyContinue'
        $subnet = '{{subnet}}'
        $pool = [runspacefactory]::CreateRunspacePool(1, 50)
        $pool.Open()
        $jobs = 1..254 | ForEach-Object {
            $ip = "$subnet.$_"
            $ps = [powershell]::Create()
            $ps.RunspacePool = $pool
            [void]$ps.AddScript({
                param($ip)
                $online = $false
                try {
                    $ping = New-Object System.Net.NetworkInformation.Ping
                    $r = $ping.Send($ip, 500)
                    $online = ($r.Status -eq [System.Net.NetworkInformation.IPStatus]::Success)
                } catch {}
                $hostname = ''
                if ($online) { try { $hostname = [System.Net.Dns]::GetHostEntry($ip).HostName } catch {} }
                [ordered]@{ IPAddress=$ip; Online=$online; Hostname=$hostname }
            }).AddParameter('ip', $ip)
            @{ PS=$ps; Handle=$ps.BeginInvoke() }
        }
        $results = $jobs | ForEach-Object {
            $_.PS.EndInvoke($_.Handle)
            $_.PS.Dispose()
        } | Sort-Object { [version]($_.IPAddress) }
        $pool.Close(); $pool.Dispose()
        $results | ConvertTo-Json -Depth 2
        """;

    public static string AddPrinter(string target, string path)
    {
        // Schedule as NT AUTHORITY\INTERACTIVE — runs as whoever is logged in, no password needed.
        // Schedule 10s out, drop the WinRM session, wait locally, reconnect fresh to verify.
        return
            "$ErrorActionPreference = 'Continue'\n" +
            $"$t = '{target}'; $p = '{path}'\n" +
            "$isLocal = ($t -eq 'localhost' -or $t -eq $env:COMPUTERNAME)\n" +
            // Step 1: register the task on the remote machine and exit the session
            "$register = {\n" +
            "    param($p)\n" +
            "    $taskName = '_SupportToolAddPrinter'\n" +
            "    $inner = \"Add-Printer -ConnectionName '$p'\"\n" +
            "    $enc = [Convert]::ToBase64String([Text.Encoding]::Unicode.GetBytes($inner))\n" +
            "    $action = New-ScheduledTaskAction -Execute 'powershell.exe' -Argument \"-NonInteractive -ExecutionPolicy Bypass -EncodedCommand $enc\"\n" +
            "    $trigger = New-ScheduledTaskTrigger -Once -At (Get-Date).AddSeconds(10)\n" +
            "    $principal = New-ScheduledTaskPrincipal -UserId 'NT AUTHORITY\\INTERACTIVE' -LogonType Interactive -RunLevel Highest\n" +
            "    Unregister-ScheduledTask -TaskName $taskName -Confirm:$false -ErrorAction SilentlyContinue\n" +
            "    Register-ScheduledTask -TaskName $taskName -Action $action -Trigger $trigger -Principal $principal -Force -ErrorAction Stop | Out-Null\n" +
            "    Write-Output 'Task registered — firing in 10s'\n" +
            "}\n" +
            // Step 2: check the printer after the task has had time to run
            "$check = {\n" +
            "    param($p)\n" +
            "    $share = $p.TrimStart('\\').Split('\\')[-1]\n" +
            "    $found = Get-WmiObject Win32_Printer | Where-Object { $_.Name -like \"*$share*\" }\n" +
            "    if ($found) { Write-Output \"Verified in WMI: $($found.Name)\" }\n" +
            "    else { Write-Output 'Printer not found in WMI — add may have failed or driver still installing' }\n" +
            "    Unregister-ScheduledTask -TaskName '_SupportToolAddPrinter' -Confirm:$false -ErrorAction SilentlyContinue\n" +
            "}\n" +
            "if ($isLocal) {\n" +
            "    & $register $p\n" +
            "    Start-Sleep -Seconds 15\n" +
            "    & $check $p\n" +
            "} else {\n" +
            "    Invoke-Command -ComputerName $t -ScriptBlock $register -ArgumentList $p | ForEach-Object { \"$_\" }\n" +
            "    Write-Output 'Waiting 15s for task to fire...'\n" +
            "    Start-Sleep -Seconds 15\n" +
            "    Write-Output 'Reconnecting to verify...'\n" +
            "    Invoke-Command -ComputerName $t -ScriptBlock $check -ArgumentList $p | ForEach-Object { \"$_\" }\n" +
            "}\n";
    }

    public static string RemovePrinter(string target, string name)
    {
        return
            "$ErrorActionPreference = 'SilentlyContinue'\n" +
            $"$isLocal = ('{target}' -eq 'localhost' -or '{target}' -eq $env:COMPUTERNAME)\n" +
            "$sb = {\n" +
            $"    param($n)\n" +
            $"    Remove-Printer -Name $n -ErrorAction Stop\n" +
            $"    Write-Output \"Removed: $n\"\n" +
            "}\n" +
            $"if ($isLocal) {{ & $sb '{name}' }}\n" +
            $"else {{ Invoke-Command -ComputerName '{target}' -ScriptBlock $sb -ArgumentList '{name}' }}\n";
    }

    public static string PendingReboot(string computer) => $$"""
        $ErrorActionPreference = 'SilentlyContinue'
        $comp = '{{computer}}'
        $isLocal = ($comp -eq 'localhost' -or $comp -eq $env:COMPUTERNAME)
        $sb = {
            $pending = $false; $reasons = @()
            if (Test-Path 'HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\WindowsUpdate\Auto Update\RebootRequired') {
                $pending = $true; $reasons += 'Windows Update'
            }
            if (Test-Path 'HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\Component Based Servicing\RebootPending') {
                $pending = $true; $reasons += 'Component Servicing'
            }
            if ((Get-ItemProperty 'HKLM:\SYSTEM\CurrentControlSet\Control\Session Manager' -Name PendingFileRenameOperations -ErrorAction SilentlyContinue).PendingFileRenameOperations) {
                $pending = $true; $reasons += 'File Rename'
            }
            [ordered]@{ Pending=$pending; Reason=($reasons -join ', ') }
        }
        if ($isLocal) { & $sb | ConvertTo-Json }
        else { Invoke-Command -ComputerName $comp -ScriptBlock $sb | ConvertTo-Json }
        """;

    public static string TestPorts(string host, List<int> ports) => $$"""
        $ErrorActionPreference = 'SilentlyContinue'
        $results = @({{string.Join(",", ports)}}) | ForEach-Object {
            $port = $_
            $open = (Test-NetConnection -ComputerName '{{host}}' -Port $port -WarningAction SilentlyContinue -InformationLevel Quiet)
            [ordered]@{ Port=$port; Open=[bool]$open }
        }
        $results | ConvertTo-Json -Depth 2
        """;

    public static string WindowsUpdates(string target) => $$"""
        $ErrorActionPreference = 'SilentlyContinue'
        $isLocal = ('{{target}}' -eq 'localhost' -or '{{target}}' -eq $env:COMPUTERNAME)
        $sb = {
            $session = New-Object -ComObject Microsoft.Update.Session
            $searcher = $session.CreateUpdateSearcher()
            $count = $searcher.GetTotalHistoryCount()
            if ($count -gt 0) {
                $searcher.QueryHistory(0, [Math]::Min($count,50)) | ForEach-Object {
                    [ordered]@{
                        Title = $_.Title
                        Severity = if($_.Categories.Count -gt 0){$_.Categories.Item(0).Name}else{''}
                        InstalledOn = $_.Date.ToString('g')
                        IsInstalled = ($_.ResultCode -eq 2)
                    }
                }
            }
        }
        if ($isLocal) { (& $sb) | ConvertTo-Json -Depth 2 }
        else { (Invoke-Command -ComputerName '{{target}}' -ScriptBlock $sb) | ConvertTo-Json -Depth 2 }
        """;
}

internal static class JsonElementExtensions2
{
    public static string GetStringOrEmpty(this JsonElement el, string prop)
        => el.TryGetProperty(prop, out var v) && v.ValueKind == JsonValueKind.String
            ? v.GetString() ?? "" : "";
}
