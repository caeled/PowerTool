#pragma warning disable CS0618 // DisplayAlert obsolete until MAUI async variant resolves from ViewModel context
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text.Json;
using System.Windows.Input;
using SupportToolApp.Helpers;
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

    public string KbNumber
    {
        get
        {
            var m = System.Text.RegularExpressions.Regex.Match(Title, @"KB\d+");
            return m.Success ? m.Value : "";
        }
    }
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

public class SchedTaskEntry
{
    public string TaskName { get; set; } = "";
    public string Status { get; set; } = "";
    public string LastRun { get; set; } = "";
    public string NextRun { get; set; } = "";
    public string Author { get; set; } = "";
    public string StatusColor => Status == "Running" ? "#228B22" : Status == "Disabled" ? "#999" : "#1565C0";
}

public class StartupEntry
{
    public string Name { get; set; } = "";
    public string Command { get; set; } = "";
    public string Location { get; set; } = "";
}

public class RemoteProcEntry
{
    public string Pid    { get; set; } = "";
    public string Name   { get; set; } = "";
    public string CpuPct { get; set; } = "";
    public string MemMb  { get; set; } = "";
}

public class RdpSessionEntry
{
    public string SessionId  { get; set; } = "";
    public string Username   { get; set; } = "";
    public string State      { get; set; } = "";
    public string IdleTime   { get; set; } = "";
    public string LogonTime  { get; set; } = "";
}

public class PwdExpiryEntry
{
    public string Username   { get; set; } = "";
    public string Enabled    { get; set; } = "";
    public string ExpiryDate { get; set; } = "";
    public string DaysLeft   { get; set; } = "";
    public string ExpiryColor { get; set; } = "White";
}

public class LocalUserMgmtEntry
{
    public string Name         { get; set; } = "";
    public string Enabled      { get; set; } = "";
    public string EnabledColor { get; set; } = "White";
    public string Description  { get; set; } = "";
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

    // ── DHCP Stats ───────────────────────────────────────────────────────────
    private string _dhcpServers = "", _dhcpAlertPct = "95", _dhcpOutput = "";
    private bool _dhcpLoading;
    public string DhcpServers  { get => _dhcpServers;  set => SetProperty(ref _dhcpServers,  value); }
    public string DhcpAlertPct { get => _dhcpAlertPct; set => SetProperty(ref _dhcpAlertPct, value); }
    public string DhcpOutput   { get => _dhcpOutput;   set => SetProperty(ref _dhcpOutput,   value); }
    public bool   DhcpLoading  { get => _dhcpLoading;  set => SetProperty(ref _dhcpLoading,  value); }
    public ICommand LoadDhcpStatsCommand  { get; }
    public ICommand LaunchPowerCopyCommand { get; }

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
    public ICommand OpenKbCommand { get; } = new Command<string>(async kb =>
    {
        if (string.IsNullOrWhiteSpace(kb)) return;
        await Launcher.OpenAsync(new Uri($"https://catalog.update.microsoft.com/Search.aspx?q={kb}"));
    });

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

    // ── Account Lockout ──────────────────────────────────────────────────
    private string _lockoutUsername = "";
    private string _lockoutDetails = "";
    private string _lockoutStatusText = "";
    private string _lockoutStatusColor = "#555555";
    private bool _lockoutStatusVisible;
    private bool _lockoutLoading;
    public string LockoutUsername     { get => _lockoutUsername;     set => SetProperty(ref _lockoutUsername, value); }
    public string LockoutDetails      { get => _lockoutDetails;      set => SetProperty(ref _lockoutDetails, value); }
    public string LockoutStatusText   { get => _lockoutStatusText;   set => SetProperty(ref _lockoutStatusText, value); }
    public string LockoutStatusColor  { get => _lockoutStatusColor;  set => SetProperty(ref _lockoutStatusColor, value); }
    public bool   LockoutStatusVisible{ get => _lockoutStatusVisible; set => SetProperty(ref _lockoutStatusVisible, value); }
    public bool   LockoutLoading      { get => _lockoutLoading;       set => SetProperty(ref _lockoutLoading, value); }
    public ICommand CheckLockoutCommand  { get; private set; } = null!;
    public ICommand UnlockAccountCommand { get; private set; } = null!;
    public ICommand ResetBadPwdCommand   { get; private set; } = null!;

    // ── RDP Sessions ─────────────────────────────────────────────────────
    private bool _rdpLoading;
    private string _rdpStatus = "";
    private RdpSessionEntry? _rdpSelectedSession;
    public bool   RdpLoading         { get => _rdpLoading;          set => SetProperty(ref _rdpLoading, value); }
    public string RdpStatus          { get => _rdpStatus;           set => SetProperty(ref _rdpStatus, value); }
    public RdpSessionEntry? RdpSelectedSession { get => _rdpSelectedSession; set => SetProperty(ref _rdpSelectedSession, value); }
    public ObservableCollection<RdpSessionEntry> RdpSessions { get; } = [];
    public ICommand ListRdpCommand { get; private set; } = null!;
    public ICommand KickRdpCommand { get; private set; } = null!;

    // ── Password Expiry ───────────────────────────────────────────────────
    private string _pwdExpiryUsername = "";
    private bool _pwdExpiryLoading;
    public string PwdExpiryUsername { get => _pwdExpiryUsername; set => SetProperty(ref _pwdExpiryUsername, value); }
    public bool   PwdExpiryLoading  { get => _pwdExpiryLoading;  set => SetProperty(ref _pwdExpiryLoading, value); }
    public ObservableCollection<PwdExpiryEntry> PwdExpiryResults { get; } = [];
    public ICommand CheckPwdExpiryCommand { get; private set; } = null!;

    // ── User Management ───────────────────────────────────────────────────
    private string _userMgmtDomain = ".", _userMgmtUsername = "", _userMgmtPassword = "";
    private string _newUserUsername = "", _newUserFullName = "", _newUserDescription = "";
    private string _newUserPassword = "", _newUserGroup = "Users";
    private bool   _newUserAddRdp;
    private bool   _newUserPwdNeverExpires;
    private string _resetPwdUsername = "", _resetPwdPassword = "";
    private bool   _resetPwdMustChange;
    private string _userMgmtStatusText = "", _userMgmtStatusColor = "#228B22";
    private bool   _userMgmtStatusVisible, _userMgmtLoading;
    public string UserMgmtDomain      { get => _userMgmtDomain;      set => SetProperty(ref _userMgmtDomain, value); }
    public string UserMgmtUsername    { get => _userMgmtUsername;    set => SetProperty(ref _userMgmtUsername, value); }
    public string UserMgmtPassword    { get => _userMgmtPassword;    set => SetProperty(ref _userMgmtPassword, value); }
    public string NewUserUsername      { get => _newUserUsername;      set => SetProperty(ref _newUserUsername, value); }
    public string NewUserFullName      { get => _newUserFullName;      set => SetProperty(ref _newUserFullName, value); }
    public string NewUserDescription   { get => _newUserDescription;   set => SetProperty(ref _newUserDescription, value); }
    public string NewUserPassword      { get => _newUserPassword;      set => SetProperty(ref _newUserPassword, value); }
    public string NewUserGroup         { get => _newUserGroup;         set => SetProperty(ref _newUserGroup, value); }
    public bool   NewUserPwdNeverExpires { get => _newUserPwdNeverExpires; set => SetProperty(ref _newUserPwdNeverExpires, value); }
    public bool   NewUserAddRdp          { get => _newUserAddRdp;          set => SetProperty(ref _newUserAddRdp, value); }
    public string ResetPwdUsername     { get => _resetPwdUsername;     set => SetProperty(ref _resetPwdUsername, value); }
    public string ResetPwdPassword     { get => _resetPwdPassword;     set => SetProperty(ref _resetPwdPassword, value); }
    public bool   ResetPwdMustChange   { get => _resetPwdMustChange;   set => SetProperty(ref _resetPwdMustChange, value); }
    public string UserMgmtStatusText   { get => _userMgmtStatusText;   set => SetProperty(ref _userMgmtStatusText, value); }
    public string UserMgmtStatusColor  { get => _userMgmtStatusColor;  set => SetProperty(ref _userMgmtStatusColor, value); }
    public bool   UserMgmtStatusVisible{ get => _userMgmtStatusVisible; set => SetProperty(ref _userMgmtStatusVisible, value); }
    public bool   UserMgmtLoading      { get => _userMgmtLoading;       set => SetProperty(ref _userMgmtLoading, value); }
    public ObservableCollection<LocalUserMgmtEntry> LocalUsersMgmt { get; } = [];
    public ICommand CreateUserCommand    { get; private set; } = null!;
    public ICommand ResetPasswordCommand { get; private set; } = null!;
    public ICommand LoadUsersMgmtCommand { get; private set; } = null!;

    // ── DNS ──────────────────────────────────────────────────────────────
    private string _dnsQuery = "";
    private string _dnsOutput = "";
    private bool _dnsLoading;
    public string DnsQuery  { get => _dnsQuery;  set => SetProperty(ref _dnsQuery, value); }
    public string DnsOutput { get => _dnsOutput; set => SetProperty(ref _dnsOutput, value); }
    public bool DnsLoading  { get => _dnsLoading; set => SetProperty(ref _dnsLoading, value); }
    public ICommand DnsLookupCommand { get; }
    public ICommand DnsFlushCommand { get; }

    // ── Flush DNS ────────────────────────────────────────────────────────
    private string _flushDnsOutput = "";
    private bool _flushDnsLoading;
    public string FlushDnsOutput { get => _flushDnsOutput; set => SetProperty(ref _flushDnsOutput, value); }
    public bool FlushDnsLoading  { get => _flushDnsLoading; set => SetProperty(ref _flushDnsLoading, value); }
    public ICommand FlushDnsCommand { get; }

    // ── NetStat ──────────────────────────────────────────────────────────
    private string _netStatOutput = "";
    private bool _netStatLoading;
    public string NetStatOutput { get => _netStatOutput; set => SetProperty(ref _netStatOutput, value); }
    public bool NetStatLoading  { get => _netStatLoading; set => SetProperty(ref _netStatLoading, value); }
    public ICommand NetStatCommand { get; }

    // ── Traceroute ───────────────────────────────────────────────────────
    private string _traceTarget = "", _traceOutput = "";
    private bool _traceLoading;
    public string TraceTarget { get => _traceTarget; set => SetProperty(ref _traceTarget, value); }
    public string TraceOutput { get => _traceOutput; set => SetProperty(ref _traceOutput, value); }
    public bool TraceLoading  { get => _traceLoading; set => SetProperty(ref _traceLoading, value); }
    public ICommand TraceRouteCommand { get; }

    // ── WiFi Profiles ────────────────────────────────────────────────────
    private string _wifiOutput = "";
    private bool _wifiLoading;
    public string WifiOutput { get => _wifiOutput; set => SetProperty(ref _wifiOutput, value); }
    public bool WifiLoading  { get => _wifiLoading; set => SetProperty(ref _wifiLoading, value); }
    public ICommand LoadWifiCommand { get; }

    // ── WiFi Connect ──────────────────────────────────────────────────────────
    private string _selectedWifiProfile = "";
    private bool _wifiAutoConnect;
    private bool _wifiConnecting;
    public string SelectedWifiProfile { get => _selectedWifiProfile; set => SetProperty(ref _selectedWifiProfile, value); }
    public bool WifiAutoConnect { get => _wifiAutoConnect; set => SetProperty(ref _wifiAutoConnect, value); }
    public bool WifiConnecting  { get => _wifiConnecting;  set => SetProperty(ref _wifiConnecting, value); }
    public ObservableCollection<string> WifiProfileNames { get; } = [];
    public ICommand ConnectWifiCommand { get; }

    // ── WiFi Setup ────────────────────────────────────────────────────────────
    private string _wifiSetupSsid = "", _wifiSetupPassword = "", _wifiSetupOutput = "";
    private string _wifiSetupSecurity = "WPA2PSK";
    private bool _wifiSetupAutoConnect = true, _wifiSetupLoading;
    public string WifiSetupSsid     { get => _wifiSetupSsid;     set => SetProperty(ref _wifiSetupSsid, value); }
    public string WifiSetupPassword { get => _wifiSetupPassword; set => SetProperty(ref _wifiSetupPassword, value); }
    public string WifiSetupSecurity { get => _wifiSetupSecurity; set => SetProperty(ref _wifiSetupSecurity, value); }
    public string WifiSetupOutput   { get => _wifiSetupOutput;   set => SetProperty(ref _wifiSetupOutput, value); }
    public bool WifiSetupAutoConnect { get => _wifiSetupAutoConnect; set => SetProperty(ref _wifiSetupAutoConnect, value); }
    public bool WifiSetupLoading    { get => _wifiSetupLoading;  set => SetProperty(ref _wifiSetupLoading, value); }
    public List<string> WifiSecurityTypes { get; } = ["WPA2PSK", "WPA3SAE", "open"];
    public ICommand AddWifiProfileCommand { get; }

    // ── Winget Install ───────────────────────────────────────────────────────
    private string _wingetSearch = "", _wingetOutput = "";
    private bool _wingetLoading;
    public string WingetSearch { get => _wingetSearch; set => SetProperty(ref _wingetSearch, value); }
    public string WingetOutput { get => _wingetOutput; set => SetProperty(ref _wingetOutput, value); }
    public bool WingetLoading  { get => _wingetLoading; set => SetProperty(ref _wingetLoading, value); }
    public ICommand WingetSearchCommand { get; }
    public ICommand WingetInstallCommand { get; }

    // ── Registry Push ────────────────────────────────────────────────────────
    private string _regPushPath = "", _regPushName = "", _regPushValue = "", _regPushOutput = "";
    private string _regPushType = "String";
    private bool _regPushLoading;
    public string RegPushPath   { get => _regPushPath;   set => SetProperty(ref _regPushPath, value); }
    public string RegPushName   { get => _regPushName;   set => SetProperty(ref _regPushName, value); }
    public string RegPushValue  { get => _regPushValue;  set => SetProperty(ref _regPushValue, value); }
    public string RegPushType   { get => _regPushType;   set => SetProperty(ref _regPushType, value); }
    public string RegPushOutput { get => _regPushOutput; set => SetProperty(ref _regPushOutput, value); }
    public bool RegPushLoading  { get => _regPushLoading; set => SetProperty(ref _regPushLoading, value); }
    public List<string> RegTypes { get; } = ["String", "DWord", "QWord", "ExpandString", "MultiString", "Binary"];
    public ICommand RegPushCommand { get; }
    public ICommand RegReadCommand { get; }

    // ── Pending Updates ──────────────────────────────────────────────────────
    private string _pendingUpdatesOutput = "";
    private bool _pendingUpdatesLoading;
    public string PendingUpdatesOutput { get => _pendingUpdatesOutput; set => SetProperty(ref _pendingUpdatesOutput, value); }
    public bool PendingUpdatesLoading  { get => _pendingUpdatesLoading; set => SetProperty(ref _pendingUpdatesLoading, value); }
    public ICommand CheckPendingUpdatesCommand { get; }

    // ── Run Updates ──────────────────────────────────────────────────────────
    private string _runUpdatesOutput = "";
    private bool _runUpdatesLoading;
    public string RunUpdatesOutput { get => _runUpdatesOutput; set => SetProperty(ref _runUpdatesOutput, value); }
    public bool RunUpdatesLoading  { get => _runUpdatesLoading; set => SetProperty(ref _runUpdatesLoading, value); }
    public ICommand UpdateScanCommand { get; }
    public ICommand UpdateDownloadCommand { get; }
    public ICommand UpdateInstallCommand { get; }

    // ── Update Log ───────────────────────────────────────────────────────────
    private string _updateLogOutput = "";
    private bool _updateLogLoading;
    public string UpdateLogOutput { get => _updateLogOutput; set => SetProperty(ref _updateLogOutput, value); }
    public bool UpdateLogLoading  { get => _updateLogLoading; set => SetProperty(ref _updateLogLoading, value); }
    public ICommand LoadUpdateLogCommand { get; }

    // ── Ping ─────────────────────────────────────────────────────────────────
    private string _pingTarget = "", _pingOutput = "";
    private string _pingCount = "4";
    private bool _pingLoading;
    public string PingTarget { get => _pingTarget; set => SetProperty(ref _pingTarget, value); }
    public string PingOutput { get => _pingOutput; set => SetProperty(ref _pingOutput, value); }
    public string PingCount  { get => _pingCount;  set => SetProperty(ref _pingCount, value); }
    public bool PingLoading  { get => _pingLoading; set => SetProperty(ref _pingLoading, value); }
    public ICommand PingCommand { get; }

    // ── Proxy Settings ───────────────────────────────────────────────────
    private string _proxyOutput = "";
    private bool _proxyLoading;
    public string ProxyOutput { get => _proxyOutput; set => SetProperty(ref _proxyOutput, value); }
    public bool ProxyLoading  { get => _proxyLoading; set => SetProperty(ref _proxyLoading, value); }
    public ICommand LoadProxyCommand { get; }

    // ── Reset NIC ────────────────────────────────────────────────────────
    private string _resetNicOutput = "";
    private bool _resetNicLoading;
    private string _selectedNic = "";
    public string ResetNicOutput { get => _resetNicOutput; set => SetProperty(ref _resetNicOutput, value); }
    public bool ResetNicLoading  { get => _resetNicLoading; set => SetProperty(ref _resetNicLoading, value); }
    public ObservableCollection<string> NicList { get; } = [];
    public string SelectedNic    { get => _selectedNic; set => SetProperty(ref _selectedNic, value); }
    public ICommand LoadNicsCommand { get; }
    public ICommand ResetNicCommand { get; }

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

    // ── Scheduled Tasks ──────────────────────────────────────────────────
    private bool _schedTasksLoading;
    public bool SchedTasksLoading { get => _schedTasksLoading; set => SetProperty(ref _schedTasksLoading, value); }
    public ObservableCollection<SchedTaskEntry> SchedTasks { get; } = [];
    private SchedTaskEntry? _selectedSchedTask;
    public SchedTaskEntry? SelectedSchedTask { get => _selectedSchedTask; set => SetProperty(ref _selectedSchedTask, value); }
    public ICommand LoadSchedTasksCommand { get; private set; } = null!;
    public ICommand RunSchedTaskCommand { get; private set; } = null!;
    public ICommand DisableSchedTaskCommand { get; private set; } = null!;
    public ICommand EnableSchedTaskCommand { get; private set; } = null!;

    // ── Windows Activation ───────────────────────────────────────────────
    private bool _activationLoading;
    private string _activationOutput = "";
    public bool ActivationLoading { get => _activationLoading; set => SetProperty(ref _activationLoading, value); }
    public string ActivationOutput { get => _activationOutput; set => SetProperty(ref _activationOutput, value); }
    public ICommand CheckActivationCommand { get; private set; } = null!;

    // ── Temp Cleanup ─────────────────────────────────────────────────────
    private bool _tempCleanLoading;
    private string _tempCleanOutput = "";
    private string _tempCleanDomain = ".";
    private string _tempCleanUsername = "";
    private string _tempCleanPassword = "";
    public bool TempCleanLoading { get => _tempCleanLoading; set => SetProperty(ref _tempCleanLoading, value); }
    public string TempCleanOutput { get => _tempCleanOutput; set => SetProperty(ref _tempCleanOutput, value); }
    public string TempCleanDomain   { get => _tempCleanDomain;   set => SetProperty(ref _tempCleanDomain, value); }
    public string TempCleanUsername { get => _tempCleanUsername; set => SetProperty(ref _tempCleanUsername, value); }
    public string TempCleanPassword { get => _tempCleanPassword; set => SetProperty(ref _tempCleanPassword, value); }
    public ObservableCollection<string> TempCleanFolders { get; } =
        ["C:\\Windows\\Temp", "C:\\Windows\\Prefetch", "%TEMP% (session user)"];
    private string _tempCleanSelectedFolder = "C:\\Windows\\Temp";
    public string TempCleanSelectedFolder
    {
        get => _tempCleanSelectedFolder;
        set => SetProperty(ref _tempCleanSelectedFolder, value);
    }
    public ICommand TempCleanScanCommand { get; private set; } = null!;
    public ICommand TempCleanRunCommand { get; private set; } = null!;
    public ICommand TempCleanListCommand { get; private set; } = null!;

    // ── Startup Programs ─────────────────────────────────────────────────
    private bool _startupLoading;
    public bool StartupLoading { get => _startupLoading; set => SetProperty(ref _startupLoading, value); }
    public ObservableCollection<StartupEntry> StartupItems { get; } = [];
    public ICommand LoadStartupCommand { get; private set; } = null!;

    // ── Remote PowerShell ────────────────────────────────────────────────
    private string _remotePsDomain = ".";
    private string _remotePsUsername = "";
    private string _remotePsPassword = "";
    private string _remotePsStatus = "";
    public string RemotePsDomain   { get => _remotePsDomain;   set => SetProperty(ref _remotePsDomain, value); }
    public string RemotePsUsername { get => _remotePsUsername; set => SetProperty(ref _remotePsUsername, value); }
    public string RemotePsPassword { get => _remotePsPassword; set => SetProperty(ref _remotePsPassword, value); }
    public string RemotePsStatus   { get => _remotePsStatus;   set => SetProperty(ref _remotePsStatus, value); }
    public ICommand ConnectRemotePsCommand { get; private set; } = null!;

    // ── PS Runner ────────────────────────────────────────────────────────
    private string _psRunDomain = ".";
    private string _psRunUsername = "";
    private string _psRunPassword = "";
    private string _psRunScript = "";
    private string _psRunOutput = "";
    private string _psRunStatus = "";
    public string PsRunDomain   { get => _psRunDomain;   set => SetProperty(ref _psRunDomain, value); }
    public string PsRunUsername { get => _psRunUsername; set => SetProperty(ref _psRunUsername, value); }
    public string PsRunPassword { get => _psRunPassword; set => SetProperty(ref _psRunPassword, value); }
    public string PsRunScript   { get => _psRunScript;   set => SetProperty(ref _psRunScript, value); }
    public string PsRunOutput   { get => _psRunOutput;   set => SetProperty(ref _psRunOutput, value); }
    public string PsRunStatus   { get => _psRunStatus;   set => SetProperty(ref _psRunStatus, value); }
    public ICommand RunPsOnRemoteCommand { get; private set; } = null!;
    public ICommand ClearPsRunCommand    { get; private set; } = null!;

    // ── Remote Directory Browser ─────────────────────────────────────────
    private string _dirSubfolder = "", _dirStatus = "", _dirOutput = "";
    private string? _dirSelectedShare;
    private bool _dirLoading;
    private string _dirDomain = "", _dirUsername = "", _dirPassword = "";
    public string  DirDomain       { get => _dirDomain;       set => SetProperty(ref _dirDomain, value); }
    public string  DirUsername     { get => _dirUsername;     set => SetProperty(ref _dirUsername, value); }
    public string  DirPassword     { get => _dirPassword;     set => SetProperty(ref _dirPassword, value); }
    public string  DirSubfolder    { get => _dirSubfolder;    set => SetProperty(ref _dirSubfolder, value); }
    public string  DirStatus       { get => _dirStatus;       set => SetProperty(ref _dirStatus, value); }
    public string  DirOutput       { get => _dirOutput;       set => SetProperty(ref _dirOutput, value); }
    public string? DirSelectedShare{ get => _dirSelectedShare; set => SetProperty(ref _dirSelectedShare, value); }
    public bool    DirLoading      { get => _dirLoading;       set => SetProperty(ref _dirLoading, value); }
    public ObservableCollection<string> DirShares { get; } = [];
    public ICommand ScanSharesCommand { get; private set; } = null!;
    public ICommand DirListCommand    { get; private set; } = null!;

    // ── Remote Process Kill ───────────────────────────────────────────────
    private string _procKillSearch = "", _procKillStatus = "";
    private bool _procKillLoading;
    private RemoteProcEntry? _procKillSelected;
    public string  ProcKillSearch   { get => _procKillSearch;   set { SetProperty(ref _procKillSearch, value); FilterRemoteProcesses(); } }
    public string  ProcKillStatus   { get => _procKillStatus;   set => SetProperty(ref _procKillStatus, value); }
    public bool    ProcKillLoading  { get => _procKillLoading;  set => SetProperty(ref _procKillLoading, value); }
    public RemoteProcEntry? ProcKillSelected { get => _procKillSelected; set => SetProperty(ref _procKillSelected, value); }
    public ObservableCollection<RemoteProcEntry> RemoteProcesses { get; } = [];
    private List<RemoteProcEntry> _allRemoteProcs = [];
    public ICommand LoadProcKillCommand    { get; private set; } = null!;
    public ICommand KillRemoteProcCommand  { get; private set; } = null!;

    // ── WinRM Toggle ─────────────────────────────────────────────────────
    private string _winRmStatusText = "", _winRmStatusColor = "#555", _winRmOutput = "";
    private string _winRmDomain = "", _winRmUsername = "", _winRmPassword = "";
    private bool _winRmStatusVisible, _winRmLoading;
    public string WinRmDomain        { get => _winRmDomain;        set => SetProperty(ref _winRmDomain, value); }
    public string WinRmUsername      { get => _winRmUsername;      set => SetProperty(ref _winRmUsername, value); }
    public string WinRmPassword      { get => _winRmPassword;      set => SetProperty(ref _winRmPassword, value); }
    public string WinRmStatusText    { get => _winRmStatusText;    set => SetProperty(ref _winRmStatusText, value); }
    public string WinRmStatusColor   { get => _winRmStatusColor;   set => SetProperty(ref _winRmStatusColor, value); }
    public string WinRmOutput        { get => _winRmOutput;        set => SetProperty(ref _winRmOutput, value); }
    public bool   WinRmStatusVisible { get => _winRmStatusVisible; set => SetProperty(ref _winRmStatusVisible, value); }
    public bool   WinRmLoading       { get => _winRmLoading;       set => SetProperty(ref _winRmLoading, value); }
    public ICommand WinRmCheckCommand   { get; private set; } = null!;
    public ICommand WinRmEnableCommand  { get; private set; } = null!;
    public ICommand WinRmDisableCommand { get; private set; } = null!;

    // ── Wake on LAN ──────────────────────────────────────────────────────
    private string _wolMac = "", _wolBroadcast = "255.255.255.255";
    private string _wolStatusText = "", _wolStatusColor = "#228B22", _wolHistory = "";
    private bool _wolStatusVisible, _wolLoading;
    public string WolMac          { get => _wolMac;          set => SetProperty(ref _wolMac, value); }
    public string WolBroadcast    { get => _wolBroadcast;    set => SetProperty(ref _wolBroadcast, value); }
    public string WolStatusText   { get => _wolStatusText;   set => SetProperty(ref _wolStatusText, value); }
    public string WolStatusColor  { get => _wolStatusColor;  set => SetProperty(ref _wolStatusColor, value); }
    public string WolHistory      { get => _wolHistory;      set => SetProperty(ref _wolHistory, value); }
    public bool   WolStatusVisible{ get => _wolStatusVisible; set => SetProperty(ref _wolStatusVisible, value); }
    public bool   WolLoading      { get => _wolLoading;       set => SetProperty(ref _wolLoading, value); }
    public ICommand WakeOnLanCommand { get; private set; } = null!;

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

    // ── Auto Logon ───────────────────────────────────────────────────────────
    private string _autoLogonUsername = "", _autoLogonPassword = "", _autoLogonCount = "1", _autoLogonScript = "";
    private string _autoLogonOutput = "";
    private bool _autoLogonLoading;
    public string AutoLogonUsername { get => _autoLogonUsername; set => SetProperty(ref _autoLogonUsername, value); }
    public string AutoLogonPassword { get => _autoLogonPassword; set => SetProperty(ref _autoLogonPassword, value); }
    public string AutoLogonCount    { get => _autoLogonCount;    set => SetProperty(ref _autoLogonCount, value); }
    public string AutoLogonScript   { get => _autoLogonScript;   set => SetProperty(ref _autoLogonScript, value); }
    public string AutoLogonOutput   { get => _autoLogonOutput;   set => SetProperty(ref _autoLogonOutput, value); }
    public bool AutoLogonLoading    { get => _autoLogonLoading;  set => SetProperty(ref _autoLogonLoading, value); }
    public ICommand AutoLogonEnableCommand  { get; private set; } = null!;
    public ICommand AutoLogonDisableCommand { get; private set; } = null!;
    public ICommand AutoLogonCheckCommand   { get; private set; } = null!;

    // ── Default Printer (Set) ─────────────────────────────────────────────────
    private string _selectedPrinterName = "", _setPrinterOutput = "";
    private bool _setPrinterLoading;
    public string SelectedPrinterName { get => _selectedPrinterName; set => SetProperty(ref _selectedPrinterName, value); }
    public string SetPrinterOutput    { get => _setPrinterOutput;    set => SetProperty(ref _setPrinterOutput, value); }
    public bool SetPrinterLoading     { get => _setPrinterLoading;   set => SetProperty(ref _setPrinterLoading, value); }
    public ObservableCollection<string> PrinterNames { get; } = [];
    public ICommand LoadPrintersForDefaultCommand { get; private set; } = null!;
    public ICommand SetDefaultPrinterCommand      { get; private set; } = null!;

    // ── Trusted Sites ────────────────────────────────────────────────────────
    private string _trustedSiteDomain = "", _trustedSiteOutput = "";
    private bool _trustedSiteHttp, _trustedSiteLoading;
    public string TrustedSiteDomain  { get => _trustedSiteDomain;  set => SetProperty(ref _trustedSiteDomain, value); }
    public string TrustedSiteOutput  { get => _trustedSiteOutput;  set => SetProperty(ref _trustedSiteOutput, value); }
    public bool TrustedSiteHttp      { get => _trustedSiteHttp;    set => SetProperty(ref _trustedSiteHttp, value); }
    public bool TrustedSiteLoading   { get => _trustedSiteLoading; set => SetProperty(ref _trustedSiteLoading, value); }
    public ICommand AddTrustedSiteCommand    { get; private set; } = null!;
    public ICommand ListTrustedSitesCommand  { get; private set; } = null!;
    public ICommand RemoveTrustedSiteCommand { get; private set; } = null!;

    // ── Quick Access ─────────────────────────────────────────────────────────
    private string _quickAccessPath = "", _quickAccessOutput = "";
    private bool _quickAccessLoading;
    public string QuickAccessPath    { get => _quickAccessPath;    set => SetProperty(ref _quickAccessPath, value); }
    public string QuickAccessOutput  { get => _quickAccessOutput;  set => SetProperty(ref _quickAccessOutput, value); }
    public bool QuickAccessLoading   { get => _quickAccessLoading; set => SetProperty(ref _quickAccessLoading, value); }
    public ICommand QuickAccessBrowseCommand { get; private set; } = null!;
    public ICommand QuickAccessPinCommand    { get; private set; } = null!;
    public ICommand QuickAccessUnpinCommand  { get; private set; } = null!;

    // ── Windows Key ──────────────────────────────────────────────────────────
    private string _winKeyValue = "", _winKeyOutput = "";
    private bool _winKeyLoading;
    public string WinKeyValue   { get => _winKeyValue;   set => SetProperty(ref _winKeyValue, value); }
    public string WinKeyOutput  { get => _winKeyOutput;  set => SetProperty(ref _winKeyOutput, value); }
    public bool WinKeyLoading   { get => _winKeyLoading; set => SetProperty(ref _winKeyLoading, value); }
    public ICommand WinKeyInstallCommand  { get; private set; } = null!;
    public ICommand WinKeyActivateCommand { get; private set; } = null!;

    // ── WSUS Client ──────────────────────────────────────────────────────────
    private string _wsusServerUrl = "", _wsusTargetGroup = "", _wsusOutput = "";
    private string _wsusAuOption = "DownloadAndInstall";
    private bool _wsusLoading;
    public string WsusServerUrl   { get => _wsusServerUrl;   set => SetProperty(ref _wsusServerUrl, value); }
    public string WsusTargetGroup { get => _wsusTargetGroup; set => SetProperty(ref _wsusTargetGroup, value); }
    public string WsusAuOption    { get => _wsusAuOption;    set => SetProperty(ref _wsusAuOption, value); }
    public string WsusOutput      { get => _wsusOutput;      set => SetProperty(ref _wsusOutput, value); }
    public bool WsusLoading       { get => _wsusLoading;     set => SetProperty(ref _wsusLoading, value); }
    public List<string> WsusAuOptions { get; } = ["Notify", "DownloadOnly", "DownloadAndInstall", "AllowUserConfig"];
    public ICommand WsusApplyCommand   { get; private set; } = null!;
    public ICommand WsusDisableCommand { get; private set; } = null!;
    public ICommand WsusCheckCommand   { get; private set; } = null!;

    // ── Hosts File Editor ──────────────────────────────────────────────────────
    private string _hostsEntryIp = "", _hostsEntryHost = "", _hostsRemoveHost = "", _hostsOutput = "";
    private bool _hostsLoading;
    public string HostsEntryIp    { get => _hostsEntryIp;    set => SetProperty(ref _hostsEntryIp, value); }
    public string HostsEntryHost  { get => _hostsEntryHost;  set => SetProperty(ref _hostsEntryHost, value); }
    public string HostsRemoveHost { get => _hostsRemoveHost; set => SetProperty(ref _hostsRemoveHost, value); }
    public string HostsOutput     { get => _hostsOutput;     set => SetProperty(ref _hostsOutput, value); }
    public bool   HostsLoading    { get => _hostsLoading;    set => SetProperty(ref _hostsLoading, value); }
    public ICommand HostsViewCommand   { get; private set; } = null!;
    public ICommand HostsAddCommand    { get; private set; } = null!;
    public ICommand HostsRemoveCommand { get; private set; } = null!;

    // ── Reset AD User Password ─────────────────────────────────────────────────
    private string _adResetUsername = "", _adResetPassword = "", _adResetOutput = "";
    private bool _adResetForceChange = true, _adResetUnlock, _adResetLoading;
    public string AdResetUsername    { get => _adResetUsername;    set => SetProperty(ref _adResetUsername, value); }
    public string AdResetPassword    { get => _adResetPassword;    set => SetProperty(ref _adResetPassword, value); }
    public string AdResetOutput      { get => _adResetOutput;      set => SetProperty(ref _adResetOutput, value); }
    public bool   AdResetForceChange { get => _adResetForceChange; set => SetProperty(ref _adResetForceChange, value); }
    public bool   AdResetUnlock      { get => _adResetUnlock;      set => SetProperty(ref _adResetUnlock, value); }
    public bool   AdResetLoading     { get => _adResetLoading;     set => SetProperty(ref _adResetLoading, value); }
    public ICommand AdResetCommand { get; private set; } = null!;

    // ── Windows Disk Cleanup ───────────────────────────────────────────────────
    private string _cleanupDays = "7", _cleanupOutput = "", _selectedCleanupDrive = "C:";
    private bool _cleanupLoading, _cleanWinTemp = true, _cleanSwDist = true,
                 _cleanUserTemp = true, _cleanIisLogs, _cleanRecycleBin = true;
    public string CleanupDays          { get => _cleanupDays;            set => SetProperty(ref _cleanupDays, value); }
    public string CleanupOutput        { get => _cleanupOutput;          set => SetProperty(ref _cleanupOutput, value); }
    public bool   CleanupLoading       { get => _cleanupLoading;         set => SetProperty(ref _cleanupLoading, value); }
    public bool   CleanWinTemp         { get => _cleanWinTemp;           set => SetProperty(ref _cleanWinTemp, value); }
    public bool   CleanSwDist          { get => _cleanSwDist;            set => SetProperty(ref _cleanSwDist, value); }
    public bool   CleanUserTemp        { get => _cleanUserTemp;          set => SetProperty(ref _cleanUserTemp, value); }
    public bool   CleanIisLogs         { get => _cleanIisLogs;           set => SetProperty(ref _cleanIisLogs, value); }
    public bool   CleanRecycleBin      { get => _cleanRecycleBin;        set => SetProperty(ref _cleanRecycleBin, value); }
    public string SelectedCleanupDrive { get => _selectedCleanupDrive;   set => SetProperty(ref _selectedCleanupDrive, value); }
    public ObservableCollection<string> CleanupDrives { get; } = ["C:"];
    public ICommand RunCleanupCommand       { get; private set; } = null!;
    public ICommand LoadCleanupDrivesCommand { get; private set; } = null!;

    // ── Keep Alive ─────────────────────────────────────────────────────────────
    private bool _keepAliveActive;
    private int _keepAliveDurationHours = 1;
    private string _keepAliveStatus = "Not active";
    private CancellationTokenSource? _keepAliveCts;
    public bool   KeepAliveActive        { get => _keepAliveActive;        set { SetProperty(ref _keepAliveActive, value); OnPropertyChanged(nameof(KeepAliveButtonText)); } }
    public int    KeepAliveDurationHours { get => _keepAliveDurationHours; set => SetProperty(ref _keepAliveDurationHours, value); }
    public string KeepAliveStatus        { get => _keepAliveStatus;        set => SetProperty(ref _keepAliveStatus, value); }
    public string KeepAliveButtonText    => KeepAliveActive ? "Stop Keep Alive" : "Start Keep Alive";
    public List<int> KeepAliveDurationOptions { get; } = [1, 2, 4, 8];
    public ICommand ToggleKeepAliveCommand { get; private set; } = null!;

    // ── Constructor ──────────────────────────────────────────────────────────

    private readonly CredentialService _creds;
    private readonly PsRunnerService _ps;

    public ToolsViewModel(CredentialService creds, PsRunnerService ps)
    {
        _creds = creds;
        _ps = ps;
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

        LoadDhcpStatsCommand = new Command(async () =>
        {
            DhcpLoading = true;
            DhcpOutput = "";
            try
            {
                var servers = string.IsNullOrWhiteSpace(DhcpServers) ? "localhost" : DhcpServers;
                var pct = int.TryParse(DhcpAlertPct, out var p) ? p : 95;
                var result = await Task.Run(() => RunPsSync(PsScripts.DhcpStats(servers, pct)));
                DhcpOutput = string.IsNullOrWhiteSpace(result) ? "No output — ensure the DhcpServer module is available and you have access." : result.Trim();
            }
            catch (Exception ex) { DhcpOutput = $"Error: {ex.Message}"; }
            finally { DhcpLoading = false; }
        });

        LaunchPowerCopyCommand = new Command(() =>
        {
            if (!OperatingSystem.IsWindows())
            {
                StatusMessage = "PowerCopy is a Windows desktop tool and is only available on Windows.";
                return;
            }
            // Walk up from AppContext.BaseDirectory until we find a sibling ToolBox folder
            var dir = new System.IO.DirectoryInfo(AppContext.BaseDirectory);
            while (dir != null && !System.IO.Directory.Exists(System.IO.Path.Combine(dir.FullName, "ToolBox")))
                dir = dir.Parent;
            var script = dir != null
                ? System.IO.Path.Combine(dir.FullName, "ToolBox", "PowerCopy.ps1")
                : "";
            if (!System.IO.File.Exists(script))
            {
                ((Page?)Application.Current?.Windows[0].Page)?.DisplayAlert("Not Found", $"PowerCopy.ps1 not found.\nLooked in: {script}", "OK");
                return;
            }
            var psi = new System.Diagnostics.ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = $"-Sta -NoProfile -ExecutionPolicy Bypass -File \"{script}\"",
                UseShellExecute = true
            };
            System.Diagnostics.Process.Start(psi);
        });

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
            await Task.Run(() => RunPsSync(PsScripts.RemoteReboot(target)));
            RebootActionLoading = false;
            for (int i = 30; i >= 0; i--)
            {
                RebootOutput = i > 0
                    ? $"Reboot command sent to {target}. Shutting down in {i}s..."
                    : $"{target} is rebooting — waiting for it to come back...";
                if (i > 0) await Task.Delay(1000);
            }
            // Ping every 15s for up to 5 minutes
            var deadline = DateTime.UtcNow.AddMinutes(5);
            while (DateTime.UtcNow < deadline)
            {
                await Task.Delay(15000);
                var up = await Task.Run(() => RunPsSync(
                    $"Test-Connection -ComputerName '{target}' -Count 1 -Quiet -ErrorAction SilentlyContinue"));
                if (up?.Trim() == "True")
                {
                    RebootOutput = $"{target} is back online.";
                    break;
                }
                var remaining = (int)(deadline - DateTime.UtcNow).TotalSeconds;
                RebootOutput = $"{target} is rebooting — checking again in 15s (timeout in {remaining}s)...";
            }
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
            var path = await AppFileDialog.OpenFileAsync("Select script to deploy", ".ps1", ".bat", ".cmd", ".exe");
            if (string.IsNullOrEmpty(path)) return;
            DeployScriptName    = Path.GetFileName(path);
            DeployScriptContent = await File.ReadAllTextAsync(path);
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

        // ── Account Lockout ──────────────────────────────────────────────────
        CheckLockoutCommand = new Command(async () =>
        {
            if (string.IsNullOrWhiteSpace(LockoutUsername)) { StatusMessage = "Enter a username."; return; }
            LockoutLoading = true; LockoutDetails = ""; LockoutStatusVisible = false;
            var target = await ResolveTargetAsync();
            if (target == null) { LockoutLoading = false; return; }
            var body =
                $"$u = Get-LocalUser -Name '{LockoutUsername}' -ErrorAction SilentlyContinue\n" +
                $"if (-not $u) {{ throw \"User '{LockoutUsername}' not found.\" }}\n" +
                $"[PSCustomObject]@{{\n" +
                $"  Name              = $u.Name\n" +
                $"  Enabled           = $u.Enabled\n" +
                $"  LastLogon         = if ($u.LastLogon) {{ $u.LastLogon.ToString('g') }} else {{ 'Never' }}\n" +
                $"  PasswordExpires   = if ($u.PasswordExpires) {{ $u.PasswordExpires.ToString('g') }} else {{ 'Never' }}\n" +
                $"  PasswordNeverExpires = $u.PasswordNeverExpires\n" +
                $"}}";
            var script = IsLocal(target)
                ? body + "\n| ConvertTo-Json -Depth 2"
                : $"Invoke-Command -ComputerName '{target}' -ScriptBlock {{\n{body}\n}} | ConvertTo-Json -Depth 2";
            var result = await Task.Run(() => RunPsSync(script));
            LockoutLoading = false;
            try
            {
                var obj = JsonSerializer.Deserialize<JsonElement>(result);
                bool enabled = obj.GetProperty("Enabled").GetBoolean();
                LockoutStatusText  = enabled ? $"{LockoutUsername} - Account OK (enabled)" : $"{LockoutUsername} - ACCOUNT DISABLED/LOCKED";
                LockoutStatusColor = enabled ? "#228B22" : "#CC2200";
                LockoutStatusVisible = true;
                LockoutDetails =
                    $"Name:                  {obj.GetProperty("Name").GetString()}\n" +
                    $"Enabled:               {enabled}\n" +
                    $"Last Logon:            {obj.GetProperty("LastLogon").GetString()}\n" +
                    $"Password Expires:      {obj.GetProperty("PasswordExpires").GetString()}\n" +
                    $"Password Never Expires:{obj.GetProperty("PasswordNeverExpires").GetBoolean()}";
            }
            catch { LockoutDetails = result; LockoutStatusVisible = false; }
        });

        UnlockAccountCommand = new Command(async () =>
        {
            if (string.IsNullOrWhiteSpace(LockoutUsername)) { StatusMessage = "Enter a username."; return; }
            var target = await ResolveTargetAsync();
            if (target == null) return;
            LockoutLoading = true;
            var body = $"Enable-LocalUser -Name '{LockoutUsername}'";
            var script = IsLocal(target)
                ? body
                : $"Invoke-Command -ComputerName '{target}' -ScriptBlock {{ {body} }} | Out-String -Stream";
            var result = await Task.Run(() => RunPsSync(script));
            LockoutLoading = false;
            LockoutStatusText = string.IsNullOrWhiteSpace(result) ? $"{LockoutUsername} enabled successfully." : result.Trim();
            LockoutStatusColor = "#228B22"; LockoutStatusVisible = true;
            CheckLockoutCommand.Execute(null);
        });

        ResetBadPwdCommand = new Command(async () =>
        {
            if (string.IsNullOrWhiteSpace(LockoutUsername)) { StatusMessage = "Enter a username."; return; }
            var target = await ResolveTargetAsync();
            if (target == null) return;
            LockoutLoading = true;
            var body =
                $"$u = Get-LocalUser -Name '{LockoutUsername}' -ErrorAction SilentlyContinue\n" +
                $"if ($u) {{ Enable-LocalUser -Name '{LockoutUsername}'; 'Local user re-enabled (clears lock).' }}\n" +
                $"else {{ $adUser = [ADSI]'WinNT://./{LockoutUsername},user'; $adUser.IsAccountLocked = $false; $adUser.SetInfo(); 'AD lock cleared.' }}";
            var script = IsLocal(target)
                ? body
                : $"Invoke-Command -ComputerName '{target}' -ScriptBlock {{\n{body}\n}} | Out-String -Stream";
            var result = await Task.Run(() => RunPsSync(script));
            LockoutLoading = false;
            LockoutStatusText = string.IsNullOrWhiteSpace(result) ? "Done." : result.Trim();
            LockoutStatusColor = "#228B22"; LockoutStatusVisible = true;
        });

        // ── RDP Sessions ─────────────────────────────────────────────────
        ListRdpCommand = new Command(async () =>
        {
            RdpLoading = true; RdpStatus = ""; RdpSessions.Clear();
            var target = await ResolveTargetAsync();
            if (target == null) { RdpLoading = false; return; }
            var script = IsLocal(target)
                ? "qwinsta"
                : $"Invoke-Command -ComputerName '{target}' -ScriptBlock {{ qwinsta }} | Out-String -Stream";
            var raw = await Task.Run(() => RunPsSync(script));
            RdpLoading = false;
            foreach (var line in raw.Split('\n').Skip(1))
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                var parts = System.Text.RegularExpressions.Regex.Split(line.Trim(), @"\s{2,}");
                if (parts.Length >= 3)
                    RdpSessions.Add(new RdpSessionEntry
                    {
                        SessionId = parts.Length > 2 ? parts[2].Trim() : "",
                        Username  = parts.Length > 1 ? parts[1].Trim() : parts[0].Trim(),
                        State     = parts.Length > 3 ? parts[3].Trim() : "",
                        IdleTime  = parts.Length > 4 ? parts[4].Trim() : "",
                        LogonTime = parts.Length > 5 ? parts[5].Trim() : "",
                    });
            }
            RdpStatus = $"{RdpSessions.Count} session(s) found.";
        });

        KickRdpCommand = new Command(async () =>
        {
            if (RdpSelectedSession == null) { RdpStatus = "Select a session to kick."; return; }
            var target = await ResolveTargetAsync();
            if (target == null) return;
            bool confirm = await Application.Current!.Windows[0].Page!.DisplayAlert(
                "Kick Session", $"Log off session {RdpSelectedSession.SessionId} ({RdpSelectedSession.Username}) on {target}?", "Kick", "Cancel");
            if (!confirm) return;
            RdpLoading = true;
            var script = IsLocal(target)
                ? $"logoff {RdpSelectedSession.SessionId}"
                : $"Invoke-Command -ComputerName '{target}' -ScriptBlock {{ logoff {RdpSelectedSession.SessionId} }}";
            await Task.Run(() => RunPsSync(script));
            RdpLoading = false;
            RdpStatus = $"Session {RdpSelectedSession.SessionId} logged off.";
            ListRdpCommand.Execute(null);
        });

        // ── Password Expiry ──────────────────────────────────────────────
        CheckPwdExpiryCommand = new Command(async () =>
        {
            PwdExpiryLoading = true; PwdExpiryResults.Clear();
            var target = await ResolveTargetAsync();
            if (target == null) { PwdExpiryLoading = false; return; }
            var filter = string.IsNullOrWhiteSpace(PwdExpiryUsername) ? "" : $"-Name '{PwdExpiryUsername}'";
            // Select-Object calculated properties are more reliable through WinRM deserialization
            var body =
                $"Get-LocalUser {filter} | Select-Object " +
                $"@{{N='Username';  E={{$_.Name}}}}, " +
                $"@{{N='Enabled';   E={{if ($_.Enabled) {{'Enabled'}} else {{'Disabled'}}}}}}, " +
                $"@{{N='ExpiryDate';E={{if ($_.PasswordExpires) {{$_.PasswordExpires.ToString('yyyy-MM-dd')}} else {{'Never'}}}}}}, " +
                $"@{{N='DaysLeft';  E={{if ($_.PasswordExpires) {{\"$([int]($_.PasswordExpires-(Get-Date)).TotalDays) days\"}} else {{'N/A'}}}}}}, " +
                $"@{{N='IsExpired'; E={{$_.PasswordExpires -and $_.PasswordExpires -lt (Get-Date)}}}}, " +
                $"@{{N='IsSoon';    E={{$_.PasswordExpires -and $_.PasswordExpires -gt (Get-Date) -and $_.PasswordExpires -le (Get-Date).AddDays(14)}}}}";
            // -InputObject @(...) forces array output in PS 5.1 even for a single result
            var script = IsLocal(target)
                ? $"ConvertTo-Json -Depth 2 -InputObject @({body})"
                : $"ConvertTo-Json -Depth 2 -InputObject @(Invoke-Command -ComputerName '{target}' -ScriptBlock {{ @({body}) }})";
            var raw = await Task.Run(() => RunPsSync(script));
            PwdExpiryLoading = false;
            if (string.IsNullOrWhiteSpace(raw) || raw.StartsWith("STDERR") || raw.StartsWith("ERROR"))
            {
                StatusMessage = $"Password Expiry error: {raw.Trim()}";
                return;
            }
            try
            {
                var items = JsonSerializer.Deserialize<List<JsonElement>>(raw);
                foreach (var item in items ?? [])
                {
                    bool expired = item.TryGetProperty("IsExpired", out var ep) && ep.GetBoolean();
                    bool soon    = item.TryGetProperty("IsSoon",    out var sp) && sp.GetBoolean();
                    PwdExpiryResults.Add(new PwdExpiryEntry
                    {
                        Username   = item.TryGetProperty("Username",   out var u) ? u.GetString() ?? "" : "",
                        Enabled    = item.TryGetProperty("Enabled",    out var en)? en.GetString() ?? "" : "",
                        ExpiryDate = item.TryGetProperty("ExpiryDate", out var ed)? ed.GetString() ?? "" : "",
                        DaysLeft   = item.TryGetProperty("DaysLeft",   out var dl)? dl.GetString() ?? "" : "",
                        ExpiryColor = expired ? "#FF4444" : soon ? "#FFA500" : "#228B22",
                    });
                }
            }
            catch (Exception ex) { StatusMessage = $"Password Expiry parse error: {ex.Message} | raw: {raw[..Math.Min(120, raw.Length)]}"; }
        });

        // ── User Management ──────────────────────────────────────────────
        LoadUsersMgmtCommand = new Command(async () =>
        {
            var target = await ResolveTargetAsync();
            if (target == null) return;
            UserMgmtLoading = true;
            await RefreshLocalUsersMgmt(target);
            UserMgmtLoading = false;
        });

        CreateUserCommand = new Command(async () =>
        {
            if (string.IsNullOrWhiteSpace(NewUserUsername) || string.IsNullOrWhiteSpace(NewUserPassword))
            { UserMgmtStatusText = "Username and password are required."; UserMgmtStatusColor = "#CC2200"; UserMgmtStatusVisible = true; return; }
            var target = await ResolveTargetAsync();
            if (target == null) return;
            UserMgmtLoading = true; UserMgmtStatusVisible = false;
            var pw = NewUserPassword.Replace("'", "''");
            var neverExpires = NewUserPwdNeverExpires ? "-PasswordNeverExpires" : "";
            var fullName = string.IsNullOrWhiteSpace(NewUserFullName) ? "" : $"-FullName '{NewUserFullName.Replace("'", "''")}'";
            var desc = string.IsNullOrWhiteSpace(NewUserDescription) ? "" : $"-Description '{NewUserDescription.Replace("'", "''")}'";
            var group = string.IsNullOrWhiteSpace(NewUserGroup) ? "Users" : NewUserGroup.Trim();
            var rdpLine = NewUserAddRdp
                ? $"Add-LocalGroupMember -Group 'Remote Desktop Users' -Member '{NewUserUsername}' -ErrorAction SilentlyContinue\n"
                : "";
            var rdpMsg = NewUserAddRdp ? " and Remote Desktop Users" : "";
            var body =
                $"$secpw = ConvertTo-SecureString '{pw}' -AsPlainText -Force\n" +
                $"New-LocalUser -Name '{NewUserUsername}' -Password $secpw {fullName} {desc} {neverExpires} -ErrorAction Stop\n" +
                $"Add-LocalGroupMember -Group '{group}' -Member '{NewUserUsername}' -ErrorAction SilentlyContinue\n" +
                rdpLine +
                $"\"User '{NewUserUsername}' created and added to {group}{rdpMsg}.\"";
            var script = BuildUserMgmtScript(target, body);
            var result = await Task.Run(() => RunPsSync(script));
            UserMgmtLoading = false;
            var ok = !result.StartsWith("ERROR") && !result.StartsWith("STDERR");
            UserMgmtStatusText = result.Trim();
            UserMgmtStatusColor = ok ? "#228B22" : "#CC2200";
            UserMgmtStatusVisible = true;
            if (ok) { NewUserUsername = ""; NewUserFullName = ""; NewUserDescription = ""; NewUserPassword = ""; }
            await RefreshLocalUsersMgmt(target);
        });

        ResetPasswordCommand = new Command(async () =>
        {
            if (string.IsNullOrWhiteSpace(ResetPwdUsername) || string.IsNullOrWhiteSpace(ResetPwdPassword))
            { UserMgmtStatusText = "Username and new password are required."; UserMgmtStatusColor = "#CC2200"; UserMgmtStatusVisible = true; return; }
            var target = await ResolveTargetAsync();
            if (target == null) return;
            UserMgmtLoading = true; UserMgmtStatusVisible = false;
            var pw = ResetPwdPassword.Replace("'", "''");
            var mustChange = ResetPwdMustChange
                ? $"net user '{ResetPwdUsername}' /logonpasswordchg:yes 2>$null\n"
                : "";
            var body =
                $"$secpw = ConvertTo-SecureString '{pw}' -AsPlainText -Force\n" +
                $"Set-LocalUser -Name '{ResetPwdUsername}' -Password $secpw -ErrorAction Stop\n" +
                $"{mustChange}" +
                $"\"Password reset for '{ResetPwdUsername}'.\"";
            var script = BuildUserMgmtScript(target, body);
            var result = await Task.Run(() => RunPsSync(script));
            UserMgmtLoading = false;
            var ok = !result.StartsWith("ERROR") && !result.StartsWith("STDERR");
            UserMgmtStatusText = result.Trim();
            UserMgmtStatusColor = ok ? "#228B22" : "#CC2200";
            UserMgmtStatusVisible = true;
            if (ok) { ResetPwdPassword = ""; }
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
            var path = await AppFileDialog.OpenFileAsync("Select file to copy");
            if (!string.IsNullOrEmpty(path)) CopyLocalPath = path;
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

        LoadSchedTasksCommand = new Command(async () =>
        {
            SchedTasksLoading = true; SchedTasks.Clear(); StatusMessage = "";
            var target = await ResolveTargetAsync(); if (target == null) { SchedTasksLoading = false; return; }
            var results = await Task.Run(() => {
                var json = RunPsSync(PsScripts.SchedTasks(target));
                if (string.IsNullOrWhiteSpace(json)) return new List<SchedTaskEntry>();
                try { return JsonSerializer.Deserialize<List<SchedTaskEntry>>(json) ?? []; }
                catch { return new List<SchedTaskEntry>(); }
            });
            foreach (var t in results) SchedTasks.Add(t);
            StatusMessage = $"Loaded {results.Count} scheduled tasks from {target}.";
            SchedTasksLoading = false;
        });

        RunSchedTaskCommand = new Command(async () =>
        {
            if (SelectedSchedTask == null) { StatusMessage = "Select a task first."; return; }
            var target = await ResolveTargetAsync(); if (target == null) return;
            var isLocal = IsLocal(target);
            var cmd = isLocal
                ? $"Start-ScheduledTask -TaskName '{SelectedSchedTask.TaskName}'"
                : $"Invoke-Command -ComputerName '{target}' -ScriptBlock {{ Start-ScheduledTask -TaskName '{SelectedSchedTask.TaskName}' }}";
            await Task.Run(() => RunPsSync(cmd));
            StatusMessage = $"Started task '{SelectedSchedTask.TaskName}' on {target}.";
        });

        DisableSchedTaskCommand = new Command(async () =>
        {
            if (SelectedSchedTask == null) { StatusMessage = "Select a task first."; return; }
            var target = await ResolveTargetAsync(); if (target == null) return;
            var isLocal = IsLocal(target);
            var cmd = isLocal
                ? $"Disable-ScheduledTask -TaskName '{SelectedSchedTask.TaskName}'"
                : $"Invoke-Command -ComputerName '{target}' -ScriptBlock {{ Disable-ScheduledTask -TaskName '{SelectedSchedTask.TaskName}' }}";
            await Task.Run(() => RunPsSync(cmd));
            StatusMessage = $"Disabled task '{SelectedSchedTask.TaskName}' on {target}.";
        });

        EnableSchedTaskCommand = new Command(async () =>
        {
            if (SelectedSchedTask == null) { StatusMessage = "Select a task first."; return; }
            var target = await ResolveTargetAsync(); if (target == null) return;
            var isLocal = IsLocal(target);
            var cmd = isLocal
                ? $"Enable-ScheduledTask -TaskName '{SelectedSchedTask.TaskName}'"
                : $"Invoke-Command -ComputerName '{target}' -ScriptBlock {{ Enable-ScheduledTask -TaskName '{SelectedSchedTask.TaskName}' }}";
            await Task.Run(() => RunPsSync(cmd));
            StatusMessage = $"Enabled task '{SelectedSchedTask.TaskName}' on {target}.";
        });

        CheckActivationCommand = new Command(async () =>
        {
            ActivationLoading = true; ActivationOutput = ""; StatusMessage = "";
            var target = await ResolveTargetAsync(); if (target == null) { ActivationLoading = false; return; }
            var output = await Task.Run(() => RunPsSync(PsScripts.Activation(target)));
            ActivationOutput = string.IsNullOrWhiteSpace(output) ? "No output returned." : output.Trim();
            StatusMessage = $"Activation check complete on {target}.";
            ActivationLoading = false;
        });

        TempCleanScanCommand = new Command(async () =>
        {
            TempCleanLoading = true; TempCleanOutput = "Scanning..."; StatusMessage = "";
            var target = await ResolveTargetAsync(); if (target == null) { TempCleanLoading = false; return; }
            var hasCreds = !string.IsNullOrWhiteSpace(TempCleanUsername) && !string.IsNullOrWhiteSpace(TempCleanPassword);
            var output = await Task.Run(() => RunPsSync(PsScripts.TempCleanScan(target, hasCreds ? TempCleanDomain : null, hasCreds ? TempCleanUsername : null, hasCreds ? TempCleanPassword : null)));
            TempCleanOutput = string.IsNullOrWhiteSpace(output) ? "Nothing found." : output.Trim();
            StatusMessage = $"Scan complete on {target}.";
            TempCleanLoading = false;
        });

        TempCleanRunCommand = new Command(async () =>
        {
            TempCleanLoading = true; TempCleanOutput = "Cleaning..."; StatusMessage = "";
            var target = await ResolveTargetAsync(); if (target == null) { TempCleanLoading = false; return; }
            var hasCreds = !string.IsNullOrWhiteSpace(TempCleanUsername) && !string.IsNullOrWhiteSpace(TempCleanPassword);
            var output = await Task.Run(() => RunPsSync(PsScripts.TempCleanRun(target, hasCreds ? TempCleanDomain : null, hasCreds ? TempCleanUsername : null, hasCreds ? TempCleanPassword : null)));
            TempCleanOutput = string.IsNullOrWhiteSpace(output) ? "Clean complete." : output.Trim();
            StatusMessage = $"Temp cleanup complete on {target}.";
            TempCleanLoading = false;
        });

        TempCleanListCommand = new Command(async () =>
        {
            if (string.IsNullOrWhiteSpace(TempCleanSelectedFolder)) { StatusMessage = "Select a folder first."; return; }
            TempCleanLoading = true; TempCleanOutput = "Listing files..."; StatusMessage = "";
            var target = await ResolveTargetAsync(); if (target == null) { TempCleanLoading = false; return; }
            var folder = TempCleanSelectedFolder == "%TEMP% (session user)" ? "$env:TEMP" : TempCleanSelectedFolder;
            var hasCreds = !string.IsNullOrWhiteSpace(TempCleanUsername) && !string.IsNullOrWhiteSpace(TempCleanPassword);
            var output = await Task.Run(() => RunPsSync(PsScripts.TempListFiles(target, folder, hasCreds ? TempCleanDomain : null, hasCreds ? TempCleanUsername : null, hasCreds ? TempCleanPassword : null)));
            TempCleanOutput = string.IsNullOrWhiteSpace(output) ? "(empty folder)" : output.Trim();
            StatusMessage = $"Listed {TempCleanSelectedFolder} on {target}.";
            TempCleanLoading = false;
        });

        LoadStartupCommand = new Command(async () =>
        {
            StartupLoading = true; StartupItems.Clear(); StatusMessage = "";
            var target = await ResolveTargetAsync(); if (target == null) { StartupLoading = false; return; }
            var results = await Task.Run(() => {
                var json = RunPsSync(PsScripts.StartupPrograms(target));
                if (string.IsNullOrWhiteSpace(json)) return new List<StartupEntry>();
                try { return JsonSerializer.Deserialize<List<StartupEntry>>(json) ?? []; }
                catch { return new List<StartupEntry>(); }
            });
            foreach (var s in results) StartupItems.Add(s);
            StatusMessage = $"Loaded {results.Count} startup entries from {target}.";
            StartupLoading = false;
        });

        ConnectRemotePsCommand = new Command(async () =>
        {
            if (!OperatingSystem.IsWindows())
            { RemotePsStatus = "Interactive PS sessions need a local console — Windows only. Use 'Run PS on Remote' instead."; return; }
            if (string.IsNullOrWhiteSpace(RemotePsUsername) || string.IsNullOrWhiteSpace(RemotePsPassword))
            { RemotePsStatus = "Admin credentials are required for PSRemoting."; return; }
            var target = await ResolveTargetAsync();
            if (target == null) return;
            if (IsLocal(target)) { RemotePsStatus = "Target must be a remote computer."; return; }

            var userFull = string.IsNullOrWhiteSpace(RemotePsDomain) || RemotePsDomain == "."
                ? RemotePsUsername
                : $"{RemotePsDomain}\\{RemotePsUsername}";

            // Write a temp script that builds creds and enters the PS session
            var script =
                $"$secpw = ConvertTo-SecureString '{RemotePsPassword}' -AsPlainText -Force\n" +
                $"$cred = New-Object System.Management.Automation.PSCredential('{userFull}', $secpw)\n" +
                $"Write-Host 'Connecting to {target} as {userFull}...' -ForegroundColor Cyan\n" +
                $"Enter-PSSession -ComputerName '{target}' -Credential $cred\n";

            var temp = Path.Combine(Path.GetTempPath(), $"psremote_{Guid.NewGuid().ToString("N")[..8]}.ps1");
            File.WriteAllText(temp, script, new System.Text.UTF8Encoding(encoderShouldEmitUTF8Identifier: true));

            var psi = new System.Diagnostics.ProcessStartInfo("powershell.exe")
            {
                Arguments = $"-NoExit -File \"{temp}\"",
                UseShellExecute = true
            };
            System.Diagnostics.Process.Start(psi);
            RemotePsStatus = $"Launched PS session to {target} as {userFull}.";
        });

        ClearPsRunCommand = new Command(() =>
        {
            PsRunScript = "";
            PsRunOutput = "";
            PsRunStatus = "";
        });

        RunPsOnRemoteCommand = new Command(async () =>
        {
            if (string.IsNullOrWhiteSpace(PsRunScript))
            { PsRunStatus = "Nothing to run — paste some PowerShell first."; return; }
            var target = await ResolveTargetAsync();
            if (target == null) return;

            PsRunStatus = "Running...";
            PsRunOutput = "";

            string result;
            if (IsLocal(target))
            {
                // Wrap in a scriptblock so Out-String captures all output like a console
                var localScript = $"& {{\n{PsRunScript}\n}} | Out-String -Stream";
                result = await Task.Run(() => RunPsSync(localScript));
            }
            else
            {
                // Wrap in Invoke-Command with optional credentials
                var hasCreds = !string.IsNullOrWhiteSpace(PsRunUsername) && !string.IsNullOrWhiteSpace(PsRunPassword);
                string script;
                if (hasCreds)
                {
                    var userFull = string.IsNullOrWhiteSpace(PsRunDomain) || PsRunDomain == "."
                        ? PsRunUsername
                        : $"{PsRunDomain}\\{PsRunUsername}";
                    // Escape single-quotes in password
                    var pw = PsRunPassword.Replace("'", "''");
                    // Embed the user's script as a ScriptBlock
                    script =
                        $"$secpw = ConvertTo-SecureString '{pw}' -AsPlainText -Force\n" +
                        $"$cred = New-Object System.Management.Automation.PSCredential('{userFull}', $secpw)\n" +
                        $"$ErrorActionPreference = 'Stop'\n" +
                        $"try {{\n" +
                        $"  Invoke-Command -ComputerName '{target}' -Credential $cred -ScriptBlock {{\n" +
                        $"{PsRunScript}\n" +
                        $"  }} | Out-String -Stream\n" +
                        $"}} catch {{ \"ERROR: $_\" }}";
                }
                else
                {
                    script =
                        $"$ErrorActionPreference = 'Stop'\n" +
                        $"try {{\n" +
                        $"  Invoke-Command -ComputerName '{target}' -ScriptBlock {{\n" +
                        $"{PsRunScript}\n" +
                        $"  }} | Out-String -Stream\n" +
                        $"}} catch {{ \"ERROR: $_\" }}";
                }
                result = await Task.Run(() => RunPsSync(script));
            }

            PsRunOutput = result;
            PsRunStatus = "Done.";
        });

        // ── Remote Directory Browser ─────────────────────────────────────
        ScanSharesCommand = new Command(async () =>
        {
            var target = await ResolveTargetAsync();
            if (target == null) return;
            DirLoading = true; DirStatus = "Scanning admin shares..."; DirShares.Clear(); DirSelectedShare = null;

            var credPart = BuildDirCredPart();
            var credFlag = string.IsNullOrWhiteSpace(credPart) ? "" : " -Credential $cred";
            var script = $@"
{credPart}
try {{
    $r = Invoke-Command -ComputerName '{target}'{credFlag} -ScriptBlock {{
        [System.IO.DriveInfo]::GetDrives() |
            Where-Object {{ $_.IsReady }} |
            ForEach-Object {{ [string]($_.Name[0] + '$') }}
    }} -ErrorAction Stop
    ConvertTo-Json -InputObject @($r | ForEach-Object {{ [string]$_ }})
}} catch {{
    Write-Output ""ERROR: $_""
}}";

            var raw = await Task.Run(() => RunPsSync(script));
            List<string>? shares = null;
            try { shares = System.Text.Json.JsonSerializer.Deserialize<List<string>>(raw); } catch { }
            MainThread.BeginInvokeOnMainThread(() =>
            {
                foreach (var s in shares ?? []) DirShares.Add(s);
                DirStatus = (shares?.Count ?? 0) > 0 ? $"Found {shares!.Count} share(s)." : $"No accessible admin shares found. {(string.IsNullOrWhiteSpace(raw) ? "" : raw.Trim())}";
                if ((shares?.Count ?? 0) > 0) DirSelectedShare = shares![0];
            });
            DirLoading = false;
        });

        DirListCommand = new Command(async () =>
        {
            if (string.IsNullOrWhiteSpace(DirSelectedShare)) { DirStatus = "Select a share first."; return; }
            var target = await ResolveTargetAsync();
            if (target == null) return;
            DirLoading = true; DirOutput = "";
            // Convert C$ → C:\ for remote path
            var driveLetter = DirSelectedShare.TrimEnd('$');
            var sub = string.IsNullOrWhiteSpace(DirSubfolder) ? "" : DirSubfolder.Trim().TrimStart('\\', '/');
            var remotePath = string.IsNullOrWhiteSpace(sub) ? $@"{driveLetter}:\" : $@"{driveLetter}:\{sub}";
            DirStatus = $"Listing {target}\\{remotePath}...";

            var credPart = BuildDirCredPart();
            var script = $@"
{credPart}
Invoke-Command -ComputerName '{target}'{(string.IsNullOrWhiteSpace(credPart) ? "" : " -Credential $cred")} -ScriptBlock {{
    param($p)
    Get-ChildItem -Path $p -Force -ErrorAction Stop | ForEach-Object {{
        if ($_.PSIsContainer) {{
            ""[DIR]  $($_.Name)""
        }} else {{
            $size = if ($_.Length -lt 1KB) {{ ""$($_.Length) B"" }}
                    elseif ($_.Length -lt 1MB) {{ ""{0:N0} KB"" -f ($_.Length/1KB) }}
                    else {{ ""{0:N1} MB"" -f ($_.Length/1MB) }}
            ""       $($_.Name.PadRight(40)) $($size.PadLeft(10))  $($_.LastWriteTime.ToString('yyyy-MM-dd HH:mm'))""
        }}
    }} | Sort-Object
}} -ArgumentList '{remotePath}'";

            var result = await Task.Run(() => RunPsSync(script));
            DirOutput = string.IsNullOrWhiteSpace(result) ? "(empty directory)" : result;
            var lineCount = string.IsNullOrWhiteSpace(result) ? 0 : result.Split('\n').Length;
            DirStatus = $"{remotePath} — {lineCount} item(s)";
            DirLoading = false;
        });

        // ── Remote Process Kill ───────────────────────────────────────────
        LoadProcKillCommand = new Command(async () =>
        {
            ProcKillLoading = true; ProcKillStatus = ""; _allRemoteProcs.Clear(); RemoteProcesses.Clear();
            var target = await ResolveTargetAsync();
            if (target == null) { ProcKillLoading = false; return; }
            var body =
                "Get-Process | Select-Object Id, ProcessName," +
                "@{N='CpuPct';E={''}},@{N='MemMb';E={[int]($_.WorkingSet64/1MB)}} | Sort-Object ProcessName";
            var script = IsLocal(target)
                ? $"ConvertTo-Json -Depth 2 -InputObject @({body})"
                : $"ConvertTo-Json -Depth 2 -InputObject @(Invoke-Command -ComputerName '{target}' -ScriptBlock {{ @({body}) }})";
            var items = await RunPsJson<List<JsonElement>>(target, script);
            _allRemoteProcs = (items ?? []).Select(i => new RemoteProcEntry
            {
                Pid    = i.TryGetProperty("Id",          out var id)  ? id.ToString()              : "",
                Name   = i.TryGetProperty("ProcessName", out var nm)  ? nm.GetString() ?? ""       : "",
                MemMb  = i.TryGetProperty("MemMb",       out var mem) ? $"{mem.GetInt32()} MB"     : "",
                CpuPct = "",
            }).ToList();
            FilterRemoteProcesses();
            ProcKillStatus = $"{_allRemoteProcs.Count} processes.";
            ProcKillLoading = false;
        });

        KillRemoteProcCommand = new Command(async () =>
        {
            if (ProcKillSelected == null) { ProcKillStatus = "Select a process first."; return; }
            var target = await ResolveTargetAsync();
            if (target == null) return;
            bool confirm = await Application.Current!.Windows[0].Page!.DisplayAlert(
                "Kill Process", $"Kill {ProcKillSelected.Name} (PID {ProcKillSelected.Pid}) on {target}?", "Kill", "Cancel");
            if (!confirm) return;
            ProcKillLoading = true;
            var script = IsLocal(target)
                ? $"Stop-Process -Id {ProcKillSelected.Pid} -Force"
                : $"Invoke-Command -ComputerName '{target}' -ScriptBlock {{ Stop-Process -Id {ProcKillSelected.Pid} -Force }}";
            await Task.Run(() => RunPsSync(script));
            ProcKillStatus = $"Killed {ProcKillSelected.Name} (PID {ProcKillSelected.Pid}).";
            ProcKillLoading = false;
            LoadProcKillCommand.Execute(null);
        });

        // ── WinRM Toggle ─────────────────────────────────────────────────
        WinRmCheckCommand = new Command(async () =>
        {
            var target = await ResolveTargetAsync();
            if (target == null) return;
            WinRmLoading = true; WinRmOutput = "";
            var credPart = BuildWinRmCredPart();
            var credFlag = string.IsNullOrWhiteSpace(credPart) ? "" : " -Credential $cred";
            var script = IsLocal(target)
                ? "Get-Service WinRM | Select-Object Name, Status, StartType | ConvertTo-Json"
                : $@"{credPart}
try {{
    $svc = Invoke-Command -ComputerName '{target}'{credFlag} -ScriptBlock {{
        Get-Service WinRM | Select-Object Name, Status, StartType
    }} -ErrorAction Stop
    # If we got here, WinRM is responding — note trigger-start shows Status=1 (Stopped) even when functional
    $svc | Select-Object Name, Status, StartType | ConvertTo-Json
}} catch {{
    Write-Output ""CONN_ERROR: $_""
}}";
            var raw = await Task.Run(() => RunPsSync(script));
            WinRmLoading = false;
            if (raw.TrimStart().StartsWith("CONN_ERROR:"))
            {
                WinRmStatusText = $"WinRM UNAVAILABLE on {target}"; WinRmStatusColor = "#CC2200";
                WinRmStatusVisible = true; WinRmOutput = raw;
                return;
            }
            try
            {
                var obj = JsonSerializer.Deserialize<JsonElement>(raw);
                var status = obj.GetProperty("Status").ToString();
                var running = status == "4" || status.Equals("Running", StringComparison.OrdinalIgnoreCase);
                var startType = obj.GetProperty("StartType").ToString();
                // Trigger-start: service shows Stopped (1) but WinRM IS functional — Invoke-Command succeeded
                if (!running)
                {
                    WinRmStatusText  = $"WinRM FUNCTIONAL on {target} (trigger-start — service idle)";
                    WinRmStatusColor = "#E6A817";
                }
                else
                {
                    WinRmStatusText  = $"WinRM RUNNING on {target}";
                    WinRmStatusColor = "#228B22";
                }
                WinRmStatusVisible = true;
                // StartType serializes as object with Value/value — extract the friendly name
                string startTypeName = startType;
                try {
                    var st = JsonSerializer.Deserialize<JsonElement>(startType);
                    startTypeName = st.TryGetProperty("Value", out var v) ? v.GetString() ?? startType : startType;
                } catch { }
                WinRmOutput = $"Service: WinRM\nStatus:  {(running ? "Running" : "Stopped (trigger/on-demand)")}\nStartType: {startTypeName}\n\nNote: Connection succeeded — WinRM is accepting remote sessions.";
            }
            catch { WinRmOutput = raw; WinRmStatusVisible = false; }
        });

        WinRmEnableCommand = new Command(async () =>
        {
            var target = await ResolveTargetAsync();
            if (target == null) return;
            WinRmLoading = true; WinRmStatusVisible = false; WinRmOutput = "Enabling WinRM...";
            var credPart = BuildWinRmCredPart();
            var credFlag = string.IsNullOrWhiteSpace(credPart) ? "" : " -Credential $cred";
            var script = IsLocal(target)
                ? "Enable-PSRemoting -Force -SkipNetworkProfileCheck | Out-String"
                : $@"{credPart}
Invoke-Command -ComputerName '{target}'{credFlag} -ScriptBlock {{
    Enable-PSRemoting -Force -SkipNetworkProfileCheck
}} | Out-String -Stream";
            var result = await Task.Run(() => RunPsSync(script));
            WinRmLoading = false;
            WinRmOutput = result;
            WinRmStatusText = "WinRM enabled. Run Check Status to confirm.";
            WinRmStatusColor = "#228B22"; WinRmStatusVisible = true;
        });

        WinRmDisableCommand = new Command(async () =>
        {
            var target = await ResolveTargetAsync();
            if (target == null) return;
            bool confirm = await Application.Current!.Windows[0].Page!.DisplayAlert(
                "Disable WinRM", $"Disable WinRM on {target}? This will break remote management.", "Disable", "Cancel");
            if (!confirm) return;
            WinRmLoading = true; WinRmStatusVisible = false; WinRmOutput = "Disabling WinRM...";
            var credPart = BuildWinRmCredPart();
            var credFlag = string.IsNullOrWhiteSpace(credPart) ? "" : " -Credential $cred";
            var script = IsLocal(target)
                ? "Disable-PSRemoting -Force; Stop-Service WinRM -Force; Set-Service WinRM -StartupType Disabled"
                : $@"{credPart}
Invoke-Command -ComputerName '{target}'{credFlag} -ScriptBlock {{
    Disable-PSRemoting -Force; Stop-Service WinRM -Force; Set-Service WinRM -StartupType Disabled
}} | Out-String -Stream";
            var result = await Task.Run(() => RunPsSync(script));
            WinRmLoading = false;
            WinRmOutput = string.IsNullOrWhiteSpace(result) ? "WinRM disabled." : result;
            WinRmStatusText = "WinRM disabled.";
            WinRmStatusColor = "#CC2200"; WinRmStatusVisible = true;
        });

        // ── Wake on LAN ──────────────────────────────────────────────────
        WakeOnLanCommand = new Command(async () =>
        {
            var mac = WolMac.Trim().Replace(":", "").Replace("-", "").Replace(".", "");
            if (mac.Length != 12) { WolStatusText = "Invalid MAC address — expected format AA:BB:CC:DD:EE:FF"; WolStatusColor = "#CC2200"; WolStatusVisible = true; return; }
            if (!System.Net.IPAddress.TryParse(WolBroadcast.Trim(), out var broadcastIp))
            { WolStatusText = "Invalid broadcast IP address."; WolStatusColor = "#CC2200"; WolStatusVisible = true; return; }

            WolLoading = true; WolStatusVisible = false;
            await Task.Run(() =>
            {
                var macBytes  = Enumerable.Range(0, 6).Select(i => Convert.ToByte(mac.Substring(i * 2, 2), 16)).ToArray();
                var magic     = new byte[6 + 16 * 6];
                for (int i = 0; i < 6; i++) magic[i] = 0xFF;
                for (int i = 0; i < 16; i++) Array.Copy(macBytes, 0, magic, 6 + i * 6, 6);
                using var client = new System.Net.Sockets.UdpClient();
                client.EnableBroadcast = true;
                client.Send(magic, magic.Length, new System.Net.IPEndPoint(broadcastIp, 9));
            });
            WolLoading = false;
            var formatted = string.Join(":", Enumerable.Range(0, 6).Select(i => mac.Substring(i * 2, 2).ToUpper()));
            WolStatusText  = $"Magic packet sent to {formatted} via {WolBroadcast.Trim()}";
            WolStatusColor = "#228B22"; WolStatusVisible = true;
            WolHistory = $"[{DateTime.Now:HH:mm:ss}] Sent WoL to {formatted} → {WolBroadcast.Trim()}\n" + WolHistory;
        });

        // ── Flush DNS ────────────────────────────────────────────────────
        FlushDnsCommand = new Command(async () => await RunTool(
            "Flush DNS", () => FlushDnsLoading = true, () => FlushDnsLoading = false,
            async target =>
            {
                var result = await Task.Run(() => RunPsSync(PsScripts.FlushDns(target)));
                FlushDnsOutput = string.IsNullOrWhiteSpace(result) ? "DNS cache flushed (no output)." : result.Trim();
            }));

        // ── NetStat ──────────────────────────────────────────────────────
        NetStatCommand = new Command(async () => await RunTool(
            "NetStat", () => NetStatLoading = true, () => NetStatLoading = false,
            async target =>
            {
                var result = await Task.Run(() => RunPsSync(PsScripts.NetStat(target)));
                NetStatOutput = string.IsNullOrWhiteSpace(result) ? "No output." : result.Trim();
            }));

        // ── Traceroute ───────────────────────────────────────────────────
        TraceRouteCommand = new Command(async () =>
        {
            if (string.IsNullOrWhiteSpace(TraceTarget)) { StatusMessage = "Enter a hostname or IP to trace."; return; }
            await RunTool(
                "Traceroute", () => TraceLoading = true, () => TraceLoading = false,
                async target =>
                {
                    var result = await Task.Run(() => RunPsSync(PsScripts.TraceRoute(target, TraceTarget)));
                    TraceOutput = string.IsNullOrWhiteSpace(result) ? "No output." : result.Trim();
                });
        });

        // ── WiFi Profiles ────────────────────────────────────────────────
        LoadWifiCommand = new Command(async () => await RunTool(
            "WiFi Profiles", () => WifiLoading = true, () => WifiLoading = false,
            async target =>
            {
                var result = await Task.Run(() => RunPsSync(PsScripts.WifiProfiles(target)));
                WifiOutput = string.IsNullOrWhiteSpace(result) ? "No output." : result.Trim();
                // parse "All User Profile     : ProfileName" lines
                WifiProfileNames.Clear();
                foreach (var line in (result ?? "").Split('\n'))
                {
                    var idx = line.IndexOf(':');
                    if (idx >= 0 && line.Contains("Profile", StringComparison.OrdinalIgnoreCase))
                    {
                        var name = line[(idx + 1)..].Trim();
                        if (!string.IsNullOrWhiteSpace(name)) WifiProfileNames.Add(name);
                    }
                }
                if (WifiProfileNames.Count > 0) SelectedWifiProfile = WifiProfileNames[0];
            }));

        ConnectWifiCommand = new Command(async () =>
        {
            if (string.IsNullOrWhiteSpace(SelectedWifiProfile)) { WifiOutput = "Select a profile first."; return; }
            WifiConnecting = true;
            try
            {
                var target = TargetComputer.Trim();
                if (string.IsNullOrWhiteSpace(target)) target = "localhost";
                var result = await Task.Run(() => RunPsSync(PsScripts.ConnectWifi(target, SelectedWifiProfile, WifiAutoConnect)));
                WifiOutput = string.IsNullOrWhiteSpace(result) ? "Done." : result.Trim();
            }
            finally { WifiConnecting = false; }
        });

        AddWifiProfileCommand = new Command(async () => await RunTool(
            "WiFi Setup", () => WifiSetupLoading = true, () => WifiSetupLoading = false,
            async target =>
            {
                if (string.IsNullOrWhiteSpace(WifiSetupSsid)) { WifiSetupOutput = "SSID is required."; return; }
                var result = await Task.Run(() => RunPsSync(PsScripts.AddWifiProfile(target, WifiSetupSsid, WifiSetupPassword, WifiSetupSecurity, WifiSetupAutoConnect)));
                WifiSetupOutput = string.IsNullOrWhiteSpace(result) ? "Done." : result.Trim();
            }));

        PingCommand = new Command(async () => await RunTool(
            "Ping", () => PingLoading = true, () => PingLoading = false,
            async target =>
            {
                if (string.IsNullOrWhiteSpace(PingTarget)) { PingOutput = "Enter a hostname or IP to ping."; return; }
                if (!int.TryParse(PingCount, out var count) || count < 1) count = 4;
                var result = await Task.Run(() => RunPsSync(PsScripts.Ping(target, PingTarget, count)));
                PingOutput = string.IsNullOrWhiteSpace(result) ? "No response." : result.Trim();
            }));

        // ── Proxy Settings ───────────────────────────────────────────────
        LoadProxyCommand = new Command(async () => await RunTool(
            "Proxy Settings", () => ProxyLoading = true, () => ProxyLoading = false,
            async target =>
            {
                var result = await Task.Run(() => RunPsSync(PsScripts.ProxySettings(target)));
                ProxyOutput = string.IsNullOrWhiteSpace(result) ? "No proxy settings found." : result.Trim();
            }));

        // ── Reset NIC ────────────────────────────────────────────────────
        LoadNicsCommand = new Command(async () => await RunTool(
            "NIC List", () => ResetNicLoading = true, () => ResetNicLoading = false,
            async target =>
            {
                var entries = await RunPsJson<List<string>>(target, PsScripts.ListNics(target));
                NicList.Clear();
                foreach (var n in entries ?? []) NicList.Add(n);
                SelectedNic = NicList.Count > 0 ? NicList[0] : "";
            }));

        ResetNicCommand = new Command(async () =>
        {
            if (string.IsNullOrWhiteSpace(SelectedNic)) { StatusMessage = "Select a NIC first."; return; }
            await RunTool(
                "Reset NIC", () => ResetNicLoading = true, () => ResetNicLoading = false,
                async target =>
                {
                    var result = await Task.Run(() => RunPsSync(PsScripts.ResetNic(target, SelectedNic)));
                    ResetNicOutput = string.IsNullOrWhiteSpace(result) ? "Reset complete." : result.Trim();
                });
        });

        WingetSearchCommand = new Command(async () => await RunTool(
            "Winget Search", () => WingetLoading = true, () => WingetLoading = false,
            async target =>
            {
                if (string.IsNullOrWhiteSpace(WingetSearch)) { WingetOutput = "Enter a package name to search."; return; }
                var result = await Task.Run(() => RunPsSync(PsScripts.WingetSearch(target, WingetSearch)));
                WingetOutput = string.IsNullOrWhiteSpace(result) ? "No results." : result.Trim();
            }));

        WingetInstallCommand = new Command(async () => await RunTool(
            "Winget Install", () => WingetLoading = true, () => WingetLoading = false,
            async target =>
            {
                if (string.IsNullOrWhiteSpace(WingetSearch)) { WingetOutput = "Enter a package ID or name to install."; return; }
                var result = await Task.Run(() => RunPsSync(PsScripts.WingetInstall(target, WingetSearch)));
                WingetOutput = string.IsNullOrWhiteSpace(result) ? "Done." : result.Trim();
            }));

        RegPushCommand = new Command(async () => await RunTool(
            "Registry Push", () => RegPushLoading = true, () => RegPushLoading = false,
            async target =>
            {
                if (string.IsNullOrWhiteSpace(RegPushPath) || string.IsNullOrWhiteSpace(RegPushName))
                { RegPushOutput = "Path and Name are required."; return; }
                var result = await Task.Run(() => RunPsSync(PsScripts.RegPush(target, RegPushPath, RegPushName, RegPushValue, RegPushType)));
                RegPushOutput = string.IsNullOrWhiteSpace(result) ? "Done." : result.Trim();
            }));

        RegReadCommand = new Command(async () => await RunTool(
            "Registry Read", () => RegPushLoading = true, () => RegPushLoading = false,
            async target =>
            {
                if (string.IsNullOrWhiteSpace(RegPushPath)) { RegPushOutput = "Path is required."; return; }
                var result = await Task.Run(() => RunPsSync(PsScripts.RegRead(target, RegPushPath)));
                RegPushOutput = string.IsNullOrWhiteSpace(result) ? "No values found." : result.Trim();
            }));

        CheckPendingUpdatesCommand = new Command(async () => await RunTool(
            "Pending Updates", () => PendingUpdatesLoading = true, () => PendingUpdatesLoading = false,
            async target =>
            {
                PendingUpdatesOutput = "Querying Windows Update... this may take 30-60 seconds.";
                var result = await Task.Run(() => RunPsSync(PsScripts.PendingUpdates(target)));
                PendingUpdatesOutput = string.IsNullOrWhiteSpace(result) ? "No pending updates found." : result.Trim();
            }));

        UpdateScanCommand = new Command(async () => await RunTool(
            "Update Scan", () => RunUpdatesLoading = true, () => RunUpdatesLoading = false,
            async target =>
            {
                RunUpdatesOutput = "Starting update scan...";
                var result = await Task.Run(() => RunPsSync(PsScripts.RunUpdateStage(target, "StartScan")));
                RunUpdatesOutput = "Scan initiated.\n" + (result?.Trim() ?? "");
            }));

        UpdateDownloadCommand = new Command(async () => await RunTool(
            "Update Download", () => RunUpdatesLoading = true, () => RunUpdatesLoading = false,
            async target =>
            {
                RunUpdatesOutput = "Starting update download...";
                var result = await Task.Run(() => RunPsSync(PsScripts.RunUpdateStage(target, "StartDownload")));
                RunUpdatesOutput = "Download initiated.\n" + (result?.Trim() ?? "");
            }));

        UpdateInstallCommand = new Command(async () => await RunTool(
            "Update Install", () => RunUpdatesLoading = true, () => RunUpdatesLoading = false,
            async target =>
            {
                RunUpdatesOutput = "Starting update installation...";
                var result = await Task.Run(() => RunPsSync(PsScripts.RunUpdateStage(target, "StartInstall")));
                RunUpdatesOutput = "Install initiated. Machine may reboot if required.\n" + (result?.Trim() ?? "");
            }));

        LoadUpdateLogCommand = new Command(async () => await RunTool(
            "Update Log", () => UpdateLogLoading = true, () => UpdateLogLoading = false,
            async target =>
            {
                var result = await Task.Run(() => RunPsSync(PsScripts.UpdateLog(target)));
                UpdateLogOutput = string.IsNullOrWhiteSpace(result) ? "No update log entries found." : result.Trim();
            }));

        // ── Auto Logon ───────────────────────────────────────────────────
        AutoLogonEnableCommand = new Command(async () => await RunTool(
            "Auto Logon", () => AutoLogonLoading = true, () => AutoLogonLoading = false,
            async target =>
            {
                if (string.IsNullOrWhiteSpace(AutoLogonUsername)) { AutoLogonOutput = "Username is required."; return; }
                if (!int.TryParse(AutoLogonCount, out var cnt) || cnt < 1) cnt = 1;
                var result = await Task.Run(() => RunPsSync(PsScripts.AutoLogonEnable(target, AutoLogonUsername, AutoLogonPassword, cnt, AutoLogonScript)));
                AutoLogonOutput = string.IsNullOrWhiteSpace(result) ? "Auto Logon enabled." : result.Trim();
            }));

        AutoLogonDisableCommand = new Command(async () => await RunTool(
            "Auto Logon", () => AutoLogonLoading = true, () => AutoLogonLoading = false,
            async target =>
            {
                var result = await Task.Run(() => RunPsSync(PsScripts.AutoLogonDisable(target)));
                AutoLogonOutput = string.IsNullOrWhiteSpace(result) ? "Auto Logon disabled." : result.Trim();
            }));

        AutoLogonCheckCommand = new Command(async () => await RunTool(
            "Auto Logon", () => AutoLogonLoading = true, () => AutoLogonLoading = false,
            async target =>
            {
                var result = await Task.Run(() => RunPsSync(PsScripts.AutoLogonCheck(target)));
                AutoLogonOutput = string.IsNullOrWhiteSpace(result) ? "Could not read auto logon settings." : result.Trim();
            }));

        // ── Default Printer (Set) ─────────────────────────────────────────
        LoadPrintersForDefaultCommand = new Command(async () => await RunTool(
            "Default Printer", () => SetPrinterLoading = true, () => SetPrinterLoading = false,
            async target =>
            {
                var names = await RunPsJson<List<string>>(target, PsScripts.PrinterNames(target));
                PrinterNames.Clear();
                foreach (var n in names ?? []) PrinterNames.Add(n);
                SetPrinterOutput = $"{PrinterNames.Count} printer(s) loaded.";
            }));

        SetDefaultPrinterCommand = new Command(async () => await RunTool(
            "Default Printer", () => SetPrinterLoading = true, () => SetPrinterLoading = false,
            async target =>
            {
                if (string.IsNullOrWhiteSpace(SelectedPrinterName)) { SetPrinterOutput = "Select a printer first."; return; }
                var result = await Task.Run(() => RunPsSync(PsScripts.SetDefaultPrinter(target, SelectedPrinterName)));
                SetPrinterOutput = string.IsNullOrWhiteSpace(result) ? $"'{SelectedPrinterName}' set as default." : result.Trim();
            }));

        // ── Trusted Sites ─────────────────────────────────────────────────
        AddTrustedSiteCommand = new Command(async () => await RunTool(
            "Trusted Sites", () => TrustedSiteLoading = true, () => TrustedSiteLoading = false,
            async target =>
            {
                if (string.IsNullOrWhiteSpace(TrustedSiteDomain)) { TrustedSiteOutput = "Enter a domain first."; return; }
                var result = await Task.Run(() => RunPsSync(PsScripts.AddTrustedSite(target, TrustedSiteDomain, TrustedSiteHttp)));
                TrustedSiteOutput = string.IsNullOrWhiteSpace(result) ? $"Added: {TrustedSiteDomain}" : result.Trim();
            }));

        ListTrustedSitesCommand = new Command(async () => await RunTool(
            "Trusted Sites", () => TrustedSiteLoading = true, () => TrustedSiteLoading = false,
            async target =>
            {
                var result = await Task.Run(() => RunPsSync(PsScripts.ListTrustedSites(target)));
                TrustedSiteOutput = string.IsNullOrWhiteSpace(result) ? "No trusted sites configured." : result.Trim();
            }));

        RemoveTrustedSiteCommand = new Command(async () => await RunTool(
            "Trusted Sites", () => TrustedSiteLoading = true, () => TrustedSiteLoading = false,
            async target =>
            {
                if (string.IsNullOrWhiteSpace(TrustedSiteDomain)) { TrustedSiteOutput = "Enter a domain to remove first."; return; }
                var result = await Task.Run(() => RunPsSync(PsScripts.RemoveTrustedSite(target, TrustedSiteDomain)));
                TrustedSiteOutput = string.IsNullOrWhiteSpace(result) ? $"Removed: {TrustedSiteDomain}" : result.Trim();
            }));

        // ── Quick Access ──────────────────────────────────────────────────
        QuickAccessBrowseCommand = new Command(async () =>
        {
            var folder = await AppFileDialog.BrowseFolderAsync("Select folder for Quick Access");
            if (!string.IsNullOrEmpty(folder)) QuickAccessPath = folder;
        });

        QuickAccessPinCommand = new Command(async () => await RunTool(
            "Quick Access", () => QuickAccessLoading = true, () => QuickAccessLoading = false,
            async _ =>
            {
                if (string.IsNullOrWhiteSpace(QuickAccessPath)) { QuickAccessOutput = "Enter a folder path first."; return; }
                var result = await Task.Run(() => RunPsSync(PsScripts.QuickAccessPin(QuickAccessPath)));
                QuickAccessOutput = string.IsNullOrWhiteSpace(result) ? $"Pinned: {QuickAccessPath}" : result.Trim();
            }));

        QuickAccessUnpinCommand = new Command(async () => await RunTool(
            "Quick Access", () => QuickAccessLoading = true, () => QuickAccessLoading = false,
            async _ =>
            {
                if (string.IsNullOrWhiteSpace(QuickAccessPath)) { QuickAccessOutput = "Enter a folder path first."; return; }
                var result = await Task.Run(() => RunPsSync(PsScripts.QuickAccessUnpin(QuickAccessPath)));
                QuickAccessOutput = string.IsNullOrWhiteSpace(result) ? $"Unpinned: {QuickAccessPath}" : result.Trim();
            }));

        // ── Windows Key ───────────────────────────────────────────────────
        WinKeyInstallCommand = new Command(async () => await RunTool(
            "Windows Key", () => WinKeyLoading = true, () => WinKeyLoading = false,
            async target =>
            {
                var key = WinKeyValue.Trim();
                if (key.Length < 5) { WinKeyOutput = "Enter a valid product key."; return; }
                var result = await Task.Run(() => RunPsSync(PsScripts.WinKeyInstall(target, key)));
                WinKeyOutput = string.IsNullOrWhiteSpace(result) ? "Key installed." : result.Trim();
            }));

        WinKeyActivateCommand = new Command(async () => await RunTool(
            "Windows Key", () => WinKeyLoading = true, () => WinKeyLoading = false,
            async target =>
            {
                var result = await Task.Run(() => RunPsSync(PsScripts.WinKeyActivate(target)));
                WinKeyOutput = string.IsNullOrWhiteSpace(result) ? "Activation attempted." : result.Trim();
            }));

        // ── WSUS Client ───────────────────────────────────────────────────
        WsusApplyCommand = new Command(async () => await RunTool(
            "WSUS Client", () => WsusLoading = true, () => WsusLoading = false,
            async target =>
            {
                if (string.IsNullOrWhiteSpace(WsusServerUrl)) { WsusOutput = "Enter the WSUS server URL first."; return; }
                var result = await Task.Run(() => RunPsSync(PsScripts.WsusApply(target, WsusServerUrl, WsusTargetGroup, WsusAuOption)));
                WsusOutput = string.IsNullOrWhiteSpace(result) ? "WSUS settings applied." : result.Trim();
            }));

        WsusDisableCommand = new Command(async () => await RunTool(
            "WSUS Client", () => WsusLoading = true, () => WsusLoading = false,
            async target =>
            {
                var result = await Task.Run(() => RunPsSync(PsScripts.WsusDisable(target)));
                WsusOutput = string.IsNullOrWhiteSpace(result) ? "WSUS disabled — machine will use Microsoft Update." : result.Trim();
            }));

        WsusCheckCommand = new Command(async () => await RunTool(
            "WSUS Client", () => WsusLoading = true, () => WsusLoading = false,
            async target =>
            {
                var result = await Task.Run(() => RunPsSync(PsScripts.WsusCheck(target)));
                WsusOutput = string.IsNullOrWhiteSpace(result) ? "No WSUS settings found." : result.Trim();
            }));

        HostsViewCommand = new Command(async () =>
        {
            if (string.IsNullOrWhiteSpace(TargetComputer)) return;
            HostsLoading = true;
            var result = await Task.Run(() => RunPsSync(PsScripts.HostsView(TargetComputer.Trim())));
            HostsOutput = string.IsNullOrWhiteSpace(result) ? "No output." : result.Trim();
            HostsLoading = false;
        });
        HostsAddCommand = new Command(async () =>
        {
            if (string.IsNullOrWhiteSpace(TargetComputer) || string.IsNullOrWhiteSpace(HostsEntryIp) || string.IsNullOrWhiteSpace(HostsEntryHost)) return;
            HostsLoading = true;
            var result = await Task.Run(() => RunPsSync(PsScripts.HostsAdd(TargetComputer.Trim(), HostsEntryIp, HostsEntryHost)));
            HostsOutput = string.IsNullOrWhiteSpace(result) ? "Done." : result.Trim();
            HostsLoading = false;
        });
        HostsRemoveCommand = new Command(async () =>
        {
            if (string.IsNullOrWhiteSpace(TargetComputer) || string.IsNullOrWhiteSpace(HostsRemoveHost)) return;
            HostsLoading = true;
            var result = await Task.Run(() => RunPsSync(PsScripts.HostsRemove(TargetComputer.Trim(), HostsRemoveHost)));
            HostsOutput = string.IsNullOrWhiteSpace(result) ? "Done." : result.Trim();
            HostsLoading = false;
        });

        AdResetCommand = new Command(async () =>
        {
            if (string.IsNullOrWhiteSpace(AdResetUsername) || string.IsNullOrWhiteSpace(AdResetPassword)) return;
            AdResetLoading = true;
            var result = await Task.Run(() => RunPsSync(PsScripts.ResetAdPassword(AdResetUsername, AdResetPassword, AdResetForceChange, AdResetUnlock)));
            AdResetOutput = string.IsNullOrWhiteSpace(result) ? "Done." : result.Trim();
            AdResetLoading = false;
        });

        LoadCleanupDrivesCommand = new Command(async () => await RunTool(
            "Disk Cleanup", () => CleanupLoading = true, () => CleanupLoading = false,
            async target =>
            {
                var result = await RunPsJson<List<string>>(target, """
                    Get-WmiObject Win32_LogicalDisk -Filter "DriveType=3" |
                    Select-Object -ExpandProperty DeviceID |
                    ConvertTo-Json -Compress
                    """);
                if (result is { Count: > 0 })
                {
                    CleanupDrives.Clear();
                    foreach (var d in result) CleanupDrives.Add(d);
                    SelectedCleanupDrive = CleanupDrives[0];
                    CleanupOutput = $"Found {CleanupDrives.Count} drive(s): {string.Join(", ", CleanupDrives)}";
                }
            }));

        RunCleanupCommand = new Command(async () => await RunTool(
            "Disk Cleanup", () => CleanupLoading = true, () => CleanupLoading = false,
            async target =>
            {
                if (!int.TryParse(CleanupDays, out var days) || days < 0) days = 7;
                var drive = string.IsNullOrWhiteSpace(SelectedCleanupDrive) ? "C:" : SelectedCleanupDrive.TrimEnd('\\');
                var result = await Task.Run(() => RunPsSync(PsScripts.DiskCleanup(target, drive, days, CleanWinTemp, CleanSwDist, CleanUserTemp, CleanIisLogs, CleanRecycleBin)));
                CleanupOutput = string.IsNullOrWhiteSpace(result) ? "Cleanup complete." : result.Trim();
            }));

        ToggleKeepAliveCommand = new Command(() =>
        {
            if (KeepAliveActive)
            {
                _keepAliveCts?.Cancel();
                SupportToolApp.Helpers.KeepAliveHelper.Disable();
                KeepAliveActive = false;
                KeepAliveStatus = "Stopped.";
            }
            else
            {
                SupportToolApp.Helpers.KeepAliveHelper.Enable();
                KeepAliveActive = true;
                var endTime = DateTime.Now.AddHours(KeepAliveDurationHours);
                KeepAliveStatus = $"Active until {endTime:HH:mm:ss}";
                _keepAliveCts = new CancellationTokenSource();
                _ = Task.Run(async () =>
                {
                    try { await Task.Delay(TimeSpan.FromHours(KeepAliveDurationHours), _keepAliveCts.Token); }
                    catch (TaskCanceledException) { return; }
                    SupportToolApp.Helpers.KeepAliveHelper.Disable();
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        KeepAliveActive = false;
                        KeepAliveStatus = "Expired.";
                    });
                });
            }
        });

        // Pre-fill credential fields from the shared service on startup
        ApplyCredentials();
    }

    private void ApplyCredentials()
    {
        if (_creds.HasUserCred)
        {
            DeployDomain      = _creds.UserDomain;
            DeployUsername    = _creds.UserUsername;
            DeployPassword    = _creds.UserPassword;
            CopyDomain        = _creds.UserDomain;
            CopyUsername      = _creds.UserUsername;
            CopyPassword      = _creds.UserPassword;
            RunAsDomain       = _creds.UserDomain;
            RunAsUsername     = _creds.UserUsername;
            RunAsPassword     = _creds.UserPassword;
            TempCleanDomain   = _creds.UserDomain;
            TempCleanUsername = _creds.UserUsername;
            TempCleanPassword = _creds.UserPassword;
            RemotePsDomain   = _creds.UserDomain;
            RemotePsUsername = _creds.UserUsername;
            RemotePsPassword = _creds.UserPassword;
            PsRunDomain   = _creds.UserDomain;
            PsRunUsername = _creds.UserUsername;
            PsRunPassword = _creds.UserPassword;
            UserMgmtDomain   = _creds.UserDomain;
            UserMgmtUsername = _creds.UserUsername;
            UserMgmtPassword = _creds.UserPassword;
            DirDomain        = _creds.UserDomain;
            DirUsername      = _creds.UserUsername;
            DirPassword      = _creds.UserPassword;
            WinRmDomain      = _creds.UserDomain;
            WinRmUsername    = _creds.UserUsername;
            WinRmPassword    = _creds.UserPassword;
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

    // Delegates to PsRunnerService: local PowerShell on Windows, relay elsewhere.
    private string RunPsSync(string script) => _ps.Run(script);

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

    private string BuildUserMgmtScript(string target, string body)
    {
        if (IsLocal(target)) return body;
        var hasCreds = !string.IsNullOrWhiteSpace(UserMgmtUsername) && !string.IsNullOrWhiteSpace(UserMgmtPassword);
        if (!hasCreds)
            return $"Invoke-Command -ComputerName '{target}' -ScriptBlock {{\n{body}\n}} | Out-String -Stream";
        var userFull = string.IsNullOrWhiteSpace(UserMgmtDomain) || UserMgmtDomain == "."
            ? UserMgmtUsername : $"{UserMgmtDomain}\\{UserMgmtUsername}";
        var pw = UserMgmtPassword.Replace("'", "''");
        return
            $"$secpw = ConvertTo-SecureString '{pw}' -AsPlainText -Force\n" +
            $"$cred = New-Object System.Management.Automation.PSCredential('{userFull}', $secpw)\n" +
            $"$ErrorActionPreference = 'Stop'\n" +
            $"try {{\n" +
            $"  Invoke-Command -ComputerName '{target}' -Credential $cred -ScriptBlock {{\n{body}\n  }} | Out-String -Stream\n" +
            $"}} catch {{ \"ERROR: $_\" }}";
    }

    private string BuildWinRmCredPart()
    {
        if (string.IsNullOrWhiteSpace(WinRmUsername) || string.IsNullOrWhiteSpace(WinRmPassword))
            return "";
        var userFull = string.IsNullOrWhiteSpace(WinRmDomain) || WinRmDomain == "."
            ? WinRmUsername : $"{WinRmDomain}\\{WinRmUsername}";
        var pw = WinRmPassword.Replace("'", "''");
        return
            $"$secpw = ConvertTo-SecureString '{pw}' -AsPlainText -Force\n" +
            $"$cred = New-Object System.Management.Automation.PSCredential('{userFull}', $secpw)";
    }

    // Returns PS lines that define $cred, or empty string if no Dir creds set.
    private string BuildDirCredPart()
    {
        if (string.IsNullOrWhiteSpace(DirUsername) || string.IsNullOrWhiteSpace(DirPassword))
            return "";
        var userFull = string.IsNullOrWhiteSpace(DirDomain) || DirDomain == "."
            ? DirUsername : $"{DirDomain}\\{DirUsername}";
        var pw = DirPassword.Replace("'", "''");
        return
            $"$secpw = ConvertTo-SecureString '{pw}' -AsPlainText -Force\n" +
            $"$cred = New-Object System.Management.Automation.PSCredential('{userFull}', $secpw)";
    }

    private void FilterRemoteProcesses()
    {
        RemoteProcesses.Clear();
        var q = _procKillSearch.Trim();
        var filtered = string.IsNullOrWhiteSpace(q)
            ? _allRemoteProcs
            : _allRemoteProcs.Where(p => p.Name.Contains(q, StringComparison.OrdinalIgnoreCase));
        foreach (var p in filtered) RemoteProcesses.Add(p);
    }

    private async Task RefreshLocalUsersMgmt(string target)
    {
        var body = "Get-LocalUser | Select-Object Name, Enabled, Description | Sort-Object Name";
        string script;
        if (IsLocal(target))
        {
            script = $"@({body}) | ConvertTo-Json -Depth 2";
        }
        else
        {
            var hasCreds = !string.IsNullOrWhiteSpace(UserMgmtUsername) && !string.IsNullOrWhiteSpace(UserMgmtPassword);
            if (hasCreds)
            {
                var userFull = string.IsNullOrWhiteSpace(UserMgmtDomain) || UserMgmtDomain == "."
                    ? UserMgmtUsername : $"{UserMgmtDomain}\\{UserMgmtUsername}";
                var pw = UserMgmtPassword.Replace("'", "''");
                script =
                    $"$secpw = ConvertTo-SecureString '{pw}' -AsPlainText -Force\n" +
                    $"$cred = New-Object System.Management.Automation.PSCredential('{userFull}', $secpw)\n" +
                    $"@(Invoke-Command -ComputerName '{target}' -Credential $cred -ScriptBlock {{ @({body}) }}) | ConvertTo-Json -Depth 2";
            }
            else
            {
                script = $"@(Invoke-Command -ComputerName '{target}' -ScriptBlock {{ @({body}) }}) | ConvertTo-Json -Depth 2";
            }
        }
        var items = await RunPsJson<List<JsonElement>>(target, script);
        MainThread.BeginInvokeOnMainThread(() =>
        {
            LocalUsersMgmt.Clear();
            foreach (var item in items ?? [])
            {
                var enabled = item.TryGetProperty("Enabled", out var ep) && ep.GetBoolean();
                LocalUsersMgmt.Add(new LocalUserMgmtEntry
                {
                    Name        = item.TryGetProperty("Name", out var np) ? np.GetString() ?? "" : "",
                    Enabled     = enabled ? "Enabled" : "Disabled",
                    EnabledColor= enabled ? "#228B22" : "#888888",
                    Description = item.TryGetProperty("Description", out var dp) ? dp.GetString() ?? "" : "",
                });
            }
        });
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

    public static string Printers(string target) =>
        "$ErrorActionPreference = 'SilentlyContinue'\n" +
        $"$isLocal = ('{target}' -eq 'localhost' -or '{target}' -eq $env:COMPUTERNAME)\n" +
        "$def = if ($isLocal) { try { [System.Drawing.Printing.PrinterSettings]::new().PrinterName } catch { '' } } else { '' }\n" +
        $"@(if ($isLocal) {{ Get-Printer -ErrorAction SilentlyContinue }} else {{ Get-Printer -ComputerName '{target}' -ErrorAction SilentlyContinue }}) |\n" +
        "    Select-Object Name, DriverName, PortName,\n" +
        "        @{N='IsDefault';E={$_.Name -eq $def}},\n" +
        "        @{N='Status';E={[string]$_.PrinterStatus}} | ConvertTo-Json -Depth 2\n";

    // Wraps a script body in local/remote dispatch boilerplate.
    // On Windows uses WinRM (Invoke-Command -ComputerName).
    // On Mac/Linux uses SSH-based remoting (Invoke-Command -HostName) — requires PS 7+ and SSH on target.
    private static string ScriptWithTarget(string target, string body, int depth)
    {
        var remoteCmd = RemoteInvokeCmd(target, "-ScriptBlock $sb");
        return
            "$ErrorActionPreference = 'SilentlyContinue'\n" +
            $"$isLocal = ('{target}' -eq 'localhost' -or '{target}' -eq $env:COMPUTERNAME)\n" +
            "$sb = {\n" +
            body + "\n" +
            "}\n" +
            $"if ($isLocal) {{ @(& $sb) | ConvertTo-Json -Depth {depth} }}\n" +
            $"else {{ @({remoteCmd}) | ConvertTo-Json -Depth {depth} }}\n";
    }

    // For scripts that output plain text (not JSON) — avoids WinRM PSObject serialization
    private static string TextScriptWithTarget(string target, string body)
    {
        var remoteCmd = RemoteInvokeCmd(target, "-ScriptBlock $sb | ForEach-Object { \"$_\" }");
        return
            "$ErrorActionPreference = 'SilentlyContinue'\n" +
            $"$isLocal = ('{target}' -eq 'localhost' -or '{target}' -eq $env:COMPUTERNAME)\n" +
            "$sb = {\n" +
            body + "\n" +
            "}\n" +
            $"if ($isLocal) {{ & $sb }}\n" +
            $"else {{ {remoteCmd} }}\n";
    }

    // Returns the appropriate Invoke-Command prefix for the current platform.
    // Windows: WinRM (-ComputerName). Mac/Linux: SSH remoting (-HostName, requires PS 7+).
    private static string RemoteInvokeCmd(string target, string scriptArgs)
    {
#if WINDOWS
        return $"Invoke-Command -ComputerName '{target}' {scriptArgs}";
#else
        return $"Invoke-Command -HostName '{target}' {scriptArgs}";
#endif
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
        return
            "$ErrorActionPreference = 'Stop'\n" +
            $"$t = '{target}'; $p = '{path}'\n" +
            "$isLocal = ($t -eq 'localhost' -or $t -eq $env:COMPUTERNAME)\n" +
            "$sb = {\n" +
            "    param($p)\n" +
            "    Add-Printer -ConnectionName $p\n" +
            "    $share = $p.TrimStart('\\\\').Split('\\\\')[-1]\n" +
            "    $found = Get-Printer -ErrorAction SilentlyContinue | Where-Object { $_.Name -like \"*$share*\" }\n" +
            "    if ($found) { \"Added and verified: $($found.Name)\" }\n" +
            "    else { 'Printer added — driver may still be installing. Run Load Printers to confirm.' }\n" +
            "}\n" +
            "if ($isLocal) { & $sb $p }\n" +
            "else { Invoke-Command -ComputerName $t -ScriptBlock $sb -ArgumentList $p | ForEach-Object { \"$_\" } }\n";
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

    public static string SchedTasks(string target) => ScriptWithTarget(target, """
            Get-ScheduledTask | ForEach-Object {
                $info = $_ | Get-ScheduledTaskInfo -ErrorAction SilentlyContinue
                [PSCustomObject]@{
                    TaskName = $_.TaskName
                    Status   = $_.State.ToString()
                    LastRun  = if ($info.LastRunTime) { $info.LastRunTime.ToString('yyyy-MM-dd HH:mm') } else { 'Never' }
                    NextRun  = if ($info.NextRunTime) { $info.NextRunTime.ToString('yyyy-MM-dd HH:mm') } else { 'N/A' }
                    Author   = $_.Author
                }
            }
        """, 2);

    public static string Activation(string target) => TextScriptWithTarget(target, """
            $sl = Get-WmiObject -Query "SELECT * FROM SoftwareLicensingProduct WHERE PartialProductKey IS NOT NULL AND Name LIKE 'Windows%'"
            foreach ($p in $sl) {
                $status = switch ($p.LicenseStatus) {
                    0 { 'Unlicensed' } 1 { 'Licensed' } 2 { 'OOB Grace' }
                    3 { 'OOT Grace' } 4 { 'Non-Genuine Grace' } 5 { 'Notification' }
                    6 { 'Extended Grace' } default { "Unknown ($($p.LicenseStatus))" }
                }
                "Name        : $($p.Name)"
                "Status      : $status"
                "Description : $($p.Description)"
                "Product Key : $($p.PartialProductKey)XXXXX"
                "Grace Mins  : $($p.GracePeriodRemaining)"
                ""
            }
        """);

    public static string TempCleanScan(string target, string? domain = null, string? username = null, string? password = null)
    {
        var body = """
            $paths = @($env:TEMP, "C:\Windows\Temp", "C:\Windows\Prefetch")
            foreach ($p in $paths) {
                if (Test-Path $p) {
                    $files = Get-ChildItem $p -Recurse -File -ErrorAction SilentlyContinue
                    $size  = ($files | Measure-Object Length -Sum).Sum / 1MB
                    "$p : $([math]::Round($size,1)) MB ($($files.Count) files)"
                }
            }
        """;
        return BuildCredScript(target, body, domain, username, password);
    }

    public static string TempCleanRun(string target, string? domain = null, string? username = null, string? password = null)
    {
        var body = """
            $paths = @("C:\Windows\Temp", "C:\Windows\Prefetch")
            $deleted = 0; $failed = 0; $freed = 0.0
            foreach ($p in $paths) {
                if (-not (Test-Path $p)) { continue }
                $files = Get-ChildItem $p -Recurse -File -ErrorAction SilentlyContinue
                "--- $p ($($files.Count) files) ---"
                foreach ($f in $files) {
                    try {
                        $size = $f.Length
                        Remove-Item $f.FullName -Force
                        $freed += $size; $deleted++
                        "  Deleted: $($f.Name)"
                    } catch {
                        $failed++
                        "  SKIPPED: $($f.Name) - $($_.Exception.Message)"
                    }
                }
            }
            ""
            "Deleted $deleted file(s), skipped $failed - freed $([math]::Round($freed/1MB,1)) MB"
        """;
        return BuildCredScript(target, body, domain, username, password);
    }

    public static string TempListFiles(string target, string folder, string? domain = null, string? username = null, string? password = null)
    {
        // Quote literal paths; leave PS variables (starting with $) unquoted
        var psPath = folder.StartsWith("$") ? folder : $"\"{folder}\"";
        var body =
            $"$p = {psPath}\n" +
            "if (-not (Test-Path $p)) { \"Folder not found: $p\"; return }\n" +
            "$files = Get-ChildItem $p -File -ErrorAction SilentlyContinue | Sort-Object LastWriteTime -Descending\n" +
            "\"$($files.Count) file(s) in $p\"\n" +
            "\"-\" * 60\n" +
            "foreach ($f in $files) {\n" +
            "    \"{0}  {1,12}  {2}\" -f $f.LastWriteTime.ToString('yyyy-MM-dd HH:mm'), $f.Length, $f.Name\n" +
            "}\n";
        return BuildCredScript(target, body, domain, username, password);
    }

    // Builds a plain-text script that optionally uses credentials for remote execution
    private static string BuildCredScript(string target, string body, string? domain, string? username, string? password)
    {
        if (IsLocalStatic(target) || string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            return TextScriptWithTarget(target, body);

        var userFull = string.IsNullOrWhiteSpace(domain) || domain == "." ? username : $"{domain}\\{username}";
        return
            "$ErrorActionPreference = 'Stop'\n" +
            $"$secpw = ConvertTo-SecureString '{password}' -AsPlainText -Force\n" +
            $"$cred  = New-Object System.Management.Automation.PSCredential('{userFull}', $secpw)\n" +
            "$sb = {\n" + body + "\n}\n" +
            "try {\n" +
            $"    Invoke-Command -ComputerName '{target}' -Credential $cred -ScriptBlock $sb | ForEach-Object {{ \"$_\" }}\n" +
            "} catch {\n" +
            "    \"ERROR: $_\"\n" +
            "}\n";
    }

    private static bool IsLocalStatic(string target) =>
        string.IsNullOrWhiteSpace(target)
        || target.Equals("localhost", StringComparison.OrdinalIgnoreCase)
        || target.Equals(Environment.MachineName, StringComparison.OrdinalIgnoreCase);

    public static string StartupPrograms(string target) => ScriptWithTarget(target, """
            $items = @()
            $regPaths = @(
                @{ Path='HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\Run'; Loc='HKLM Run' },
                @{ Path='HKCU:\SOFTWARE\Microsoft\Windows\CurrentVersion\Run'; Loc='HKCU Run' },
                @{ Path='HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\RunOnce'; Loc='HKLM RunOnce' },
                @{ Path='HKCU:\SOFTWARE\Microsoft\Windows\CurrentVersion\RunOnce'; Loc='HKCU RunOnce' }
            )
            foreach ($r in $regPaths) {
                if (Test-Path $r.Path) {
                    Get-ItemProperty $r.Path | Get-Member -MemberType NoteProperty |
                        Where-Object { $_.Name -notmatch '^PS' } | ForEach-Object {
                            $items += [PSCustomObject]@{
                                Name     = $_.Name
                                Command  = (Get-ItemProperty $r.Path).$($_.Name)
                                Location = $r.Loc
                            }
                        }
                }
            }
            $startupFolders = @(
                @{ Path=[System.Environment]::GetFolderPath('Startup'); Loc='User Startup Folder' },
                @{ Path="C:\ProgramData\Microsoft\Windows\Start Menu\Programs\Startup"; Loc='All Users Startup Folder' }
            )
            foreach ($f in $startupFolders) {
                if (Test-Path $f.Path) {
                    Get-ChildItem $f.Path -ErrorAction SilentlyContinue | ForEach-Object {
                        $items += [PSCustomObject]@{ Name=$_.Name; Command=$_.FullName; Location=$f.Loc }
                    }
                }
            }
            $items
        """, 2);

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

    public static string FlushDns(string target) => TextScriptWithTarget(target, """
            ipconfig /flushdns
        """);

    public static string NetStat(string target) => TextScriptWithTarget(target, """
            netstat -ano
        """);

    public static string TraceRoute(string target, string host) =>
        "$ErrorActionPreference = 'SilentlyContinue'\n" +
        $"$isLocal = ('{target}' -eq 'localhost' -or '{target}' -eq $env:COMPUTERNAME)\n" +
        $"$sb = {{ tracert -d -h 20 '{host}' }}\n" +
        $"if ($isLocal) {{ & $sb }}\n" +
        $"else {{ Invoke-Command -ComputerName '{target}' -ScriptBlock $sb | ForEach-Object {{ \"$_\" }} }}\n";

    public static string WifiProfiles(string target) => TextScriptWithTarget(target, """
            netsh wlan show profiles
        """);

    public static string ProxySettings(string target) => TextScriptWithTarget(target, """
            $r = Get-ItemProperty 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Internet Settings' -ErrorAction SilentlyContinue
            "Proxy Enabled : $(if ($r.ProxyEnable -eq 1) { 'YES' } else { 'NO' })"
            "Proxy Server  : $($r.ProxyServer)"
            "Bypass List   : $($r.ProxyOverride)"
            "Auto Config   : $($r.AutoConfigURL)"
        """);

    public static string ListNics(string target) => ScriptWithTarget(target, """
            Get-NetAdapter | Where-Object { $_.Status -ne 'Not Present' } |
                ForEach-Object { [string]"$($_.Name) [$($_.Status)]" }
        """, 1);

    public static string ResetNic(string target, string nicName) =>
        "$ErrorActionPreference = 'Stop'\n" +
        $"$isLocal = ('{target}' -eq 'localhost' -or '{target}' -eq $env:COMPUTERNAME)\n" +
        $"$sb = {{\n" +
        $"    $a = Get-NetAdapter -Name '{nicName}' -ErrorAction Stop\n" +
        $"    Write-Output \"Disabling '$($a.Name)'...\"\n" +
        $"    Disable-NetAdapter -Name '{nicName}' -Confirm:$false\n" +
        $"    Start-Sleep -Seconds 3\n" +
        $"    Enable-NetAdapter -Name '{nicName}' -Confirm:$false\n" +
        $"    Start-Sleep -Seconds 2\n" +
        $"    $a2 = Get-NetAdapter -Name '{nicName}'\n" +
        $"    Write-Output \"Done. Status: $($a2.Status)\"\n" +
        $"}}\n" +
        $"if ($isLocal) {{ & $sb }}\n" +
        $"else {{ Invoke-Command -ComputerName '{target}' -ScriptBlock $sb | ForEach-Object {{ \"$_\" }} }}\n";

    public static string Ping(string target, string pingHost, int count) =>
        "$ErrorActionPreference = 'SilentlyContinue'\n" +
        $"$isLocal = ('{target}' -eq 'localhost' -or '{target}' -eq $env:COMPUTERNAME)\n" +
        $"$sb = {{ ping -n {count} '{pingHost}' }}\n" +
        $"if ($isLocal) {{ & $sb }}\n" +
        $"else {{ Invoke-Command -ComputerName '{target}' -ScriptBlock $sb | ForEach-Object {{ \"$_\" }} }}\n";

    public static string ConnectWifi(string target, string profile, bool autoConnect) =>
        "$ErrorActionPreference = 'SilentlyContinue'\n" +
        $"$isLocal = ('{target}' -eq 'localhost' -or '{target}' -eq $env:COMPUTERNAME)\n" +
        $"$sb = {{\n" +
        $"    netsh wlan connect name='{profile}'\n" +
        (autoConnect ? $"    netsh wlan set profileparameter name='{profile}' connectionmode=auto\n" : "") +
        $"    'Connect command sent for profile: {profile}'\n" +
        $"}}\n" +
        $"if ($isLocal) {{ & $sb }}\n" +
        $"else {{ Invoke-Command -ComputerName '{target}' -ScriptBlock $sb | ForEach-Object {{ \"$_\" }} }}\n";

    public static string WingetSearch(string target, string query) => TextScriptWithTarget(target, $"""
            winget search "{query}" --accept-source-agreements 2>&1
        """);

    public static string WingetInstall(string target, string packageId) => TextScriptWithTarget(target, $"""
            winget install --id "{packageId}" --silent --accept-package-agreements --accept-source-agreements 2>&1
        """);

    public static string RegPush(string target, string path, string name, string value, string type) =>
        TextScriptWithTarget(target, $$"""
            $regPath = '{{path}}'
            if (-not (Test-Path $regPath)) { New-Item -Path $regPath -Force | Out-Null }
            Set-ItemProperty -Path $regPath -Name '{{name}}' -Value '{{value}}' -Type {{type}}
            $v = (Get-ItemProperty -Path $regPath -Name '{{name}}' -ErrorAction SilentlyContinue).'{{name}}'
            "Set: $regPath\{{name}} = $v [{{type}}]"
        """);

    public static string RegRead(string target, string path) =>
        TextScriptWithTarget(target, $$"""
            $props = Get-ItemProperty -Path '{{path}}' -ErrorAction SilentlyContinue
            if (-not $props) { "Path not found: {{path}}"; return }
            $props.PSObject.Properties |
                Where-Object { $_.Name -notmatch '^PS' } |
                ForEach-Object { "$($_.Name) = $($_.Value)" }
        """);

    public static string PendingUpdates(string target) => TextScriptWithTarget(target, """
            try {
                $s = New-Object -ComObject Microsoft.Update.Session
                $q = $s.CreateUpdateSearcher()
                $r = $q.Search("IsInstalled=0 and Type='Software' and IsHidden=0")
                if ($r.Updates.Count -eq 0) { "No pending updates."; return }
                "Found $($r.Updates.Count) pending update(s):`n"
                $r.Updates | ForEach-Object {
                    $sz = if ($_.MaxDownloadSize -gt 0) { " [{0:N1} MB]" -f ($_.MaxDownloadSize/1MB) } else { "" }
                    $sev = if ($_.MsrcSeverity) { " [$($_.MsrcSeverity)]" } else { "" }
                    "  - $($_.Title)$sev$sz"
                }
            } catch {
                "Error querying Windows Update: $_"
            }
        """);

    public static string RunUpdateStage(string target, string stage) => TextScriptWithTarget(target, $"""
            UsoClient {stage}
            "UsoClient {stage} command sent."
            Start-Sleep -Seconds 3
            $wuStatus = Get-Service -Name wuauserv -ErrorAction SilentlyContinue
            "Windows Update service status: $($wuStatus.Status)"
        """);

    public static string UpdateLog(string target) => TextScriptWithTarget(target, """
            try {
                Get-WinEvent -FilterHashtable @{ LogName='System'; ProviderName='Microsoft-Windows-WindowsUpdateClient' } -MaxEvents 50 -ErrorAction Stop |
                    Sort-Object TimeCreated -Descending |
                    ForEach-Object {
                        "$($_.TimeCreated.ToString('yyyy-MM-dd HH:mm'))  [$($_.Id)]  $($_.Message -replace '\r?\n',' ')"
                    }
            } catch {
                "Could not read Windows Update event log: $_"
            }
        """);

    public static string AddWifiProfile(string target, string ssid, string password, string security, bool autoConnect)
    {
        var connMode = autoConnect ? "auto" : "manual";
        var (auth, enc) = security switch
        {
            "WPA3SAE" => ("WPA3SAE", "AES"),
            "open"    => ("open", "none"),
            _         => ("WPA2PSK", "AES"),
        };
        var keyXml = security != "open"
            ? $"<sharedKey><keyType>passPhrase</keyType><protected>false</protected><keyMaterial>{password}</keyMaterial></sharedKey>"
            : "";
        var xml = $@"<?xml version=""1.0""?>
<WLANProfile xmlns=""http://www.microsoft.com/networking/WLAN/profile/v1"">
    <name>{ssid}</name>
    <SSIDConfig><SSID><name>{ssid}</name></SSID></SSIDConfig>
    <connectionType>ESS</connectionType>
    <connectionMode>{connMode}</connectionMode>
    <MSM><security><authEncryption>
        <authentication>{auth}</authentication>
        <encryption>{enc}</encryption>
        <useOneX>false</useOneX>
    </authEncryption>{keyXml}</security></MSM>
</WLANProfile>";

        // Pass XML as ArgumentList — no admin share or temp-file copy needed
        return
            "$ErrorActionPreference = 'SilentlyContinue'\n" +
            $"$isLocal = ('{target}' -eq 'localhost' -or '{target}' -eq $env:COMPUTERNAME)\n" +
            "$xml = @'\n" + xml + "\n'@\n" +
            "$tmp = [System.IO.Path]::GetTempFileName() + '.xml'\n" +
            "[System.IO.File]::WriteAllText($tmp, $xml)\n" +
            $"if ($isLocal) {{\n" +
            $"    netsh wlan add profile filename=$tmp user=all\n" +
            $"    Remove-Item $tmp -ErrorAction SilentlyContinue\n" +
            $"}} else {{\n" +
            $"    Invoke-Command -ComputerName '{target}' -ArgumentList $xml -ScriptBlock {{\n" +
            $"        param($x)\n" +
            $"        $f = \"$env:TEMP\\wlan_$([System.IO.Path]::GetRandomFileName()).xml\"\n" +
            $"        [System.IO.File]::WriteAllText($f, $x)\n" +
            $"        netsh wlan add profile filename=$f user=all\n" +
            $"        Remove-Item $f -ErrorAction SilentlyContinue\n" +
            $"    }} | ForEach-Object {{ \"$_\" }}\n" +
            $"}}\n";
    }

    public static string AutoLogonEnable(string target, string username, string password, int count, string script) =>
        TextScriptWithTarget(target, $$"""
            $reg = "HKLM:\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Winlogon"
            Set-ItemProperty $reg "AutoAdminLogon" -Value "1" -Type String
            Set-ItemProperty $reg "DefaultUsername" -Value "{{username}}" -Type String
            Set-ItemProperty $reg "DefaultPassword" -Value "{{password}}" -Type String
            Set-ItemProperty $reg "AutoLogonCount"  -Value {{count}} -Type DWord
            if ("{{script}}") {
                Set-ItemProperty "HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\RunOnce" "(Default)" -Value "{{script}}" -Type String
            }
            "Done"
        """);

    public static string AutoLogonDisable(string target) =>
        TextScriptWithTarget(target, """
            $reg = "HKLM:\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Winlogon"
            Set-ItemProperty $reg "AutoAdminLogon" -Value "0" -Type String
            Remove-ItemProperty $reg "DefaultPassword" -ErrorAction SilentlyContinue
            Remove-ItemProperty $reg "DefaultUsername" -ErrorAction SilentlyContinue
            Remove-ItemProperty $reg "AutoLogonCount"  -ErrorAction SilentlyContinue
            "Done"
        """);

    public static string AutoLogonCheck(string target) =>
        TextScriptWithTarget(target, """
            $reg = "HKLM:\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Winlogon"
            $v = Get-ItemProperty $reg -ErrorAction SilentlyContinue
            "AutoAdminLogon : $($v.AutoAdminLogon)"
            "DefaultUsername: $($v.DefaultUsername)"
            "AutoLogonCount : $($v.AutoLogonCount)"
        """);

    public static string PrinterNames(string target) =>
        "$ErrorActionPreference = 'SilentlyContinue'\n" +
        $"$isLocal = ('{target}' -eq 'localhost' -or '{target}' -eq $env:COMPUTERNAME)\n" +
        $"@(if ($isLocal) {{ Get-Printer -ErrorAction SilentlyContinue }} else {{ Get-Printer -ComputerName '{target}' -ErrorAction SilentlyContinue }}) |\n" +
        "    Select-Object -ExpandProperty Name | Sort-Object | ConvertTo-Json -Depth 1\n";

    public static string SetDefaultPrinter(string target, string name) =>
        TextScriptWithTarget(target, $$"""
            $p = Get-Printer -ErrorAction SilentlyContinue | Where-Object { $_.Name -eq "{{name}}" }
            if ($p) {
                (New-Object -ComObject WScript.Network).SetDefaultPrinter("{{name}}")
                "Default printer set to: {{name}}"
            }
            else { "Printer not found: {{name}}" }
        """);

    public static string AddTrustedSite(string target, string domain, bool useHttp) =>
        TextScriptWithTarget(target, $$"""
            $base = "HKCU:\Software\Microsoft\Windows\CurrentVersion\Internet Settings\ZoneMap\Domains"
            New-Item -Path "$base\{{domain}}" -Force | Out-Null
            Set-ItemProperty -Path "$base\{{domain}}" -Name "{{(useHttp ? "http" : "https")}}" -Value 2 -Type DWord
            "Added {{domain}} to Trusted Sites"
        """);

    public static string ListTrustedSites(string target) =>
        TextScriptWithTarget(target, """
            $base = "HKCU:\Software\Microsoft\Windows\CurrentVersion\Internet Settings\ZoneMap\Domains"
            if (-not (Test-Path $base)) { "No trusted sites registry key found."; exit }
            $sites = Get-ChildItem $base -ErrorAction SilentlyContinue
            if (-not $sites) { "No trusted sites configured."; exit }
            $sites | ForEach-Object {
                $domain = $_.PSChildName
                $props = Get-ItemProperty $_.PSPath -ErrorAction SilentlyContinue
                $schemes = @()
                if ($props.https -eq 2) { $schemes += "https" }
                if ($props.http  -eq 2) { $schemes += "http" }
                "$domain  [$($schemes -join ', ')]"
            }
        """);

    public static string RemoveTrustedSite(string target, string domain) =>
        TextScriptWithTarget(target, $$"""
            $base = "HKCU:\Software\Microsoft\Windows\CurrentVersion\Internet Settings\ZoneMap\Domains"
            $path = "$base\{{domain}}"
            if (Test-Path $path) {
                Remove-Item $path -Recurse -Force
                "Removed: {{domain}}"
            } else {
                "Not found: {{domain}}"
            }
        """);

    public static string QuickAccessPin(string path) =>
        "$ErrorActionPreference = 'SilentlyContinue'\n" +
        "$qa = New-Object -ComObject shell.application\n" +
        $"if (-not (Test-Path '{path}')) {{ \"Path not found: {path}\"; exit }}\n" +
        $"$qa.Namespace('{path}').Self.InvokeVerb('pintohome')\n";

    public static string QuickAccessUnpin(string path) =>
        "$ErrorActionPreference = 'SilentlyContinue'\n" +
        "$qa = New-Object -ComObject shell.application\n" +
        $"$item = $qa.Namespace('shell:::{{679f85cb-0220-4080-b29b-5540cc05aab6}}').Items() | Where-Object {{ $_.Path -eq '{path}' }}\n" +
        $"if ($item) {{ $item.InvokeVerb('unpinfromhome') }} else {{ \"Not pinned: {path}\" }}\n";

    public static string WinKeyInstall(string target, string key) =>
        TextScriptWithTarget(target, $"""
            cscript //nologo "$env:SystemRoot\system32\slmgr.vbs" -ipk {key} 2>&1
        """);

    public static string WinKeyActivate(string target) =>
        TextScriptWithTarget(target, """
            cscript //nologo "$env:SystemRoot\system32\slmgr.vbs" /ato 2>&1
        """);

    public static string WsusApply(string target, string serverUrl, string targetGroup, string auOption) =>
        TextScriptWithTarget(target, $$"""
            $reg    = "HKLM:\SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate"
            $regAU  = "$reg\AU"
            New-Item -Path $reg   -Force | Out-Null
            New-Item -Path $regAU -Force | Out-Null
            Set-ItemProperty $reg "WUServer"       -Value "{{serverUrl}}" -Type String
            Set-ItemProperty $reg "WUStatusServer" -Value "{{serverUrl}}" -Type String
            if ("{{targetGroup}}") {
                Set-ItemProperty $reg "TargetGroupEnabled" -Value 1 -Type DWord
                Set-ItemProperty $reg "TargetGroup"        -Value "{{targetGroup}}" -Type String
            }
            Set-ItemProperty $regAU "UseWUServer" -Value 1 -Type DWord
            $auMap = @{Notify=2;DownloadOnly=3;DownloadAndInstall=4;AllowUserConfig=5}
            $opt = $auMap["{{auOption}}"]
            if ($opt) { Set-ItemProperty $regAU "AUOptions" -Value $opt -Type DWord }
            "WSUS settings applied. Server: {{serverUrl}}"
        """);

    public static string WsusDisable(string target) =>
        TextScriptWithTarget(target, """
            $reg   = "HKLM:\SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate"
            $regAU = "$reg\AU"
            if (Test-Path $regAU) { Set-ItemProperty $regAU "UseWUServer" -Value 0 -Type DWord }
            "WSUS disabled."
        """);

    public static string WsusCheck(string target) =>
        TextScriptWithTarget(target, """
            $reg   = "HKLM:\SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate"
            $regAU = "$reg\AU"
            if (-not (Test-Path $reg)) { "No WSUS policy registry keys found."; exit }
            $wuv  = Get-ItemProperty $reg   -ErrorAction SilentlyContinue
            $auv  = Get-ItemProperty $regAU -ErrorAction SilentlyContinue
            $auMap = @{2='Notify';3='DownloadOnly';4='DownloadAndInstall';5='AllowUserConfig'}
            "WUServer      : $($wuv.WUServer)"
            "UseWUServer   : $($auv.UseWUServer)"
            "TargetGroup   : $($wuv.TargetGroup)"
            "AUOptions     : $($auMap[$auv.AUOptions]) ($($auv.AUOptions))"
        """);

    public static string HostsView(string target) =>
        $$"""
            $h = '\\{{target}}\C$\Windows\System32\drivers\etc\hosts'
            if (-not (Test-Path $h)) { "Cannot access hosts file on {{target}}"; exit }
            Get-Content $h
        """;

    public static string HostsAdd(string target, string ip, string hostname) =>
        $$"""
            $h = '\\{{target}}\C$\Windows\System32\drivers\etc\hosts'
            if (-not (Test-Path $h)) { "Cannot access hosts file on {{target}}"; exit }
            $existing = Get-Content $h
            if ($existing | Where-Object { $_ -match [regex]::Escape("{{hostname}}") }) { "Entry for {{hostname}} already exists."; exit }
            "{{ip}}`t{{hostname}}" | Out-File -Encoding ASCII -Append $h
            "Added: {{ip}} -> {{hostname}}"
        """;

    public static string HostsRemove(string target, string hostname) =>
        $$"""
            $h = '\\{{target}}\C$\Windows\System32\drivers\etc\hosts'
            if (-not (Test-Path $h)) { "Cannot access hosts file on {{target}}"; exit }
            $lines = Get-Content $h
            $newlines = $lines | Where-Object { $_ -notmatch [regex]::Escape("{{hostname}}") }
            $removed = $lines.Count - $newlines.Count
            $newlines | Set-Content -Encoding ASCII $h
            "Removed $removed line(s) containing '{{hostname}}'"
        """;

    public static string ResetAdPassword(string username, string password, bool forceChange, bool unlock) =>
        $$"""
            Import-Module ActiveDirectory -ErrorAction Stop
            $user = Get-ADUser "{{username}}" -ErrorAction Stop
            Set-ADAccountPassword "{{username}}" -Reset -NewPassword (ConvertTo-SecureString "{{password}}" -AsPlainText -Force)
            if ({{(forceChange ? "$true" : "$false")}}) { Set-ADUser "{{username}}" -ChangePasswordAtLogon $true }
            if ({{(unlock ? "$true" : "$false")}}) { Unlock-ADAccount "{{username}}" }
            "Password reset for $($user.SamAccountName) ($($user.Name)). Force change: {{forceChange}}. Unlocked: {{unlock}}."
        """;

    public static string DiskCleanup(string target, string drive, int days, bool winTemp, bool swDist, bool userTemp, bool iisLogs, bool recycleBin) =>
        TextScriptWithTarget(target, $$"""
            $drive  = "{{drive}}"
            $days   = {{days}}
            $cutoff = (Get-Date).AddDays(-$days)
            $results = @()

            if ({{(winTemp  ? "$true" : "$false")}}) {
                $n = (Get-ChildItem "$drive\Windows\Temp\*" -Recurse -Force -ErrorAction SilentlyContinue | Where-Object { $_.CreationTime -lt $cutoff } | Remove-Item -Force -Recurse -ErrorAction SilentlyContinue -PassThru).Count
                $results += "Windows Temp: $n item(s) removed"
            }
            if ({{(swDist   ? "$true" : "$false")}}) {
                Get-Service wuauserv -ErrorAction SilentlyContinue | Stop-Service -Force -ErrorAction SilentlyContinue
                $n = (Get-ChildItem "$drive\Windows\SoftwareDistribution\*" -Recurse -Force -ErrorAction SilentlyContinue | Where-Object { $_.CreationTime -lt $cutoff } | Remove-Item -Force -Recurse -ErrorAction SilentlyContinue -PassThru).Count
                $results += "SoftwareDistribution: $n item(s) removed"
            }
            if ({{(userTemp  ? "$true" : "$false")}}) {
                $n = (Get-ChildItem "$drive\Users\*\AppData\Local\Temp\*" -Recurse -Force -ErrorAction SilentlyContinue | Where-Object { $_.CreationTime -lt $cutoff } | Remove-Item -Force -Recurse -ErrorAction SilentlyContinue -PassThru).Count
                $results += "User Temp: $n item(s) removed"
            }
            if ({{(iisLogs   ? "$true" : "$false")}}) {
                $n = (Get-ChildItem "$drive\inetpub\logs\LogFiles\*" -Recurse -Force -ErrorAction SilentlyContinue | Where-Object { $_.CreationTime -lt $cutoff } | Remove-Item -Force -Recurse -ErrorAction SilentlyContinue -PassThru).Count
                $results += "IIS Logs: $n item(s) removed"
            }
            if ({{(recycleBin ? "$true" : "$false")}}) {
                $rb = "$drive\`$Recycle.Bin"
                $n = (Get-ChildItem $rb -Recurse -Force -ErrorAction SilentlyContinue | Remove-Item -Force -Recurse -ErrorAction SilentlyContinue -PassThru).Count
                $results += "Recycle Bin ($drive): $n item(s) removed"
            }

            $results
            Get-WmiObject Win32_LogicalDisk -Filter 'DriveType=3' | ForEach-Object {
                "Drive $($_.DeviceID) $([math]::Round($_.FreeSpace/1GB,1)) GB free of $([math]::Round($_.Size/1GB,1)) GB"
            }
        """);

    public static string DhcpStats(string servers, int alertPct) => $$"""
        $ErrorActionPreference = 'Stop'
        try { Import-Module DhcpServer -ErrorAction Stop }
        catch { "ERROR: DhcpServer module not available. Install RSAT or run on a Windows Server with the DHCP role."; exit }
        $serverList = '{{servers}}' -split ',' | ForEach-Object { $_.Trim() } | Where-Object { $_ }
        $alert = {{alertPct}}
        $lines = @()
        foreach ($srv in $serverList) {
            $lines += "=== $srv ==="
            try {
                $scopes = Get-DhcpServerv4Scope -ComputerName $srv
                foreach ($s in $scopes) {
                    $stats = Get-DhcpServerv4ScopeStatistics -ComputerName $srv -ScopeId $s.ScopeId
                    $total = $stats.AddressesFree + $stats.AddressesInUse
                    $pct   = if ($total -gt 0) { [math]::Round($stats.AddressesInUse / $total * 100, 1) } else { 0 }
                    $flag  = if ($pct -ge $alert) { " *** ALERT ***" } else { "" }
                    $lines += "  Scope $($s.ScopeId)  [$($s.Name)]"
                    $lines += "    In Use: $($stats.AddressesInUse) / $total  ($pct%)$flag"
                    $lines += "    Free: $($stats.AddressesFree)  Reserved: $($stats.Reserved)"
                }
            } catch {
                $lines += "  ERROR: $_"
            }
        }
        $lines
        """;
}

internal static class JsonElementExtensions2
{
    public static string GetStringOrEmpty(this JsonElement el, string prop)
        => el.TryGetProperty(prop, out var v) && v.ValueKind == JsonValueKind.String
            ? v.GetString() ?? "" : "";
}
