using System.Collections.ObjectModel;
using System.Windows.Input;
using SupportToolApp.Config;
using SupportToolApp.Models;
using SupportToolApp.Services;

namespace SupportToolApp.ViewModels;

public class RemoteAccessViewModel : BaseViewModel
{
    private readonly IAdService _ad;
    private ServerEntry? _selectedServer;
    private AdComputer? _selectedComputer;
    private string _targetComputer = "";
    private string _remoteCommand = "";
    private string _statusMessage = "", _remoteOutput = "";
    private bool _hasStatus, _hasOutput;

    public ObservableCollection<ServerEntry> Servers { get; } = [];
    public ObservableCollection<AdComputer> Computers { get; } = [];

    public string TargetComputer { get => _targetComputer; set => SetProperty(ref _targetComputer, value); }
    public string RemoteCommand { get => _remoteCommand; set => SetProperty(ref _remoteCommand, value); }

    public ServerEntry? SelectedServer
    {
        get => _selectedServer;
        set
        {
            SetProperty(ref _selectedServer, value);
            if (value != null) TargetComputer = value.Host;
        }
    }

    public AdComputer? SelectedComputer
    {
        get => _selectedComputer;
        set
        {
            SetProperty(ref _selectedComputer, value);
            if (value != null) TargetComputer = value.DnsHostName;
        }
    }

    public string StatusMessage { get => _statusMessage; set { SetProperty(ref _statusMessage, value); HasStatus = !string.IsNullOrEmpty(value); } }
    public string RemoteOutput { get => _remoteOutput; set { SetProperty(ref _remoteOutput, value); HasOutput = !string.IsNullOrEmpty(value); } }
    public bool HasStatus { get => _hasStatus; set => SetProperty(ref _hasStatus, value); }
    public bool HasOutput { get => _hasOutput; set => SetProperty(ref _hasOutput, value); }

    public string DeployToolName => AppConfig.Instance.DeployToolName;

    public ICommand RdpServerCommand { get; }
    public ICommand RdpCustomCommand { get; }
    public ICommand RdpComputerCommand { get; }
    public ICommand TeamViewerCommand { get; }
    public ICommand SccmCommand { get; }
    public ICommand DeployToolCommand { get; }
    public ICommand ComputerMgmtCommand { get; }
    public ICommand EventViewerCommand { get; }
    public ICommand RemoteCmdCommand { get; }
    public ICommand ClearTargetCommand { get; }
    public ICommand PopOutCommand { get; }

    public RemoteAccessViewModel(IAdService ad)
    {
        _ad = ad;

        foreach (var s in AppConfig.Instance.Servers)
            Servers.Add(s);

        RdpServerCommand = new Command(async () =>
        {
            var host = await ResolveTargetAsync();
            if (host != null) LaunchRdp(host);
        });
        RdpCustomCommand = new Command<string>(host => { if (!string.IsNullOrWhiteSpace(host)) LaunchRdp(host); });
        RdpComputerCommand = new Command(async () =>
        {
            var host = await ResolveTargetAsync();
            if (host != null) LaunchRdp(host);
        });
        TeamViewerCommand = new Command(() => LaunchTool(AppConfig.Instance.TeamViewerPath));
        SccmCommand = new Command(() => LaunchTool(AppConfig.Instance.SccmConsolePath));
        DeployToolCommand = new Command(() => LaunchTool(AppConfig.Instance.DeployToolUrl));
        ComputerMgmtCommand = new Command(async () =>
        {
            var host = await ResolveTargetAsync();
            if (host == null) return;
            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "mmc.exe",
                    Arguments = $"compmgmt.msc /computer:\\\\{host}",
                    UseShellExecute = true
                });
                StatusMessage = $"Computer Management opened for {host}";
            }
            catch (Exception ex) { StatusMessage = $"Failed: {ex.Message}"; }
        });
        EventViewerCommand = new Command(async () =>
        {
            var host = await ResolveTargetAsync();
            if (host == null) return;
            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "eventvwr.msc",
                    Arguments = $"/computer:{host}",
                    UseShellExecute = true
                });
                StatusMessage = $"Event Viewer opened for {host}";
            }
            catch (Exception ex) { StatusMessage = $"Failed: {ex.Message}"; }
        });
        RemoteCmdCommand = new Command(async () =>
        {
            var host = await ResolveTargetAsync();
            if (host == null) return;
            if (string.IsNullOrWhiteSpace(RemoteCommand))
            {
                StatusMessage = "Enter a command to run.";
                return;
            }
            try
            {
                var psi = new System.Diagnostics.ProcessStartInfo("powershell.exe",
                    $"-NoProfile -Command \"Invoke-Command -ComputerName '{host}' -ScriptBlock {{ {RemoteCommand} }}\"")
                {
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                var proc = System.Diagnostics.Process.Start(psi)!;
                var output = await proc.StandardOutput.ReadToEndAsync();
                var error = await proc.StandardError.ReadToEndAsync();
                await proc.WaitForExitAsync();
                RemoteOutput = string.IsNullOrWhiteSpace(output) ? (error.Trim()) : output.Trim();
                StatusMessage = $"Command ran on {host}";
            }
            catch (Exception ex) { StatusMessage = $"Failed: {ex.Message}"; }
        });

        ClearTargetCommand = new Command(() =>
        {
            TargetComputer = "";
            SelectedServer = null;
            SelectedComputer = null;
        });

        PopOutCommand = new Command(() =>
        {
            if (string.IsNullOrWhiteSpace(RemoteOutput)) return;
            var window = new Window
            {
                Title = $"Output — {TargetComputer}",
                Width = 800,
                Height = 600,
                Page = new ContentPage
                {
                    Content = new Grid
                    {
                        Padding = new Thickness(0),
                        BackgroundColor = Colors.Black,
                        Children =
                        {
                            new ScrollView
                            {
                                Padding = new Thickness(12),
                                Content = new Label
                                {
                                    Text = RemoteOutput,
                                    FontFamily = "Courier New",
                                    FontSize = 12,
                                    TextColor = Colors.LimeGreen,
                                    LineBreakMode = LineBreakMode.WordWrap
                                }
                            }
                        }
                    }
                }
            };
            Application.Current!.OpenWindow(window);
        });

        _ = LoadComputersAsync();
    }

    private async Task LoadComputersAsync()
    {
        var list = await _ad.GetComputersAsync();
        foreach (var c in list) Computers.Add(c);
    }

    private async Task<string?> ResolveTargetAsync()
    {
        if (!string.IsNullOrWhiteSpace(TargetComputer))
            return TargetComputer.Trim();

        var result = await Application.Current!.Windows[0].Page!.DisplayPromptAsync(
            "Target Computer",
            "Enter a computer name or IP address:",
            placeholder: "hostname or IP");

        if (!string.IsNullOrWhiteSpace(result))
        {
            TargetComputer = result.Trim();
            return TargetComputer;
        }
        return null;
    }

    private void LaunchRdp(string host)
    {
        try
        {
            System.Diagnostics.Process.Start("mstsc", $"/v:{host}");
            StatusMessage = $"RDP launched to {host}";
        }
        catch (Exception ex) { StatusMessage = $"Failed to launch RDP: {ex.Message}"; }
    }

    private void LaunchTool(string path)
    {
        if (string.IsNullOrWhiteSpace(path)) { StatusMessage = "No path or URL configured. Set it in Credentials."; return; }
        try
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = path,
                UseShellExecute = true  // handles both exe paths and http:// URLs
            });
            StatusMessage = $"Launched: {path}";
        }
        catch (Exception ex) { StatusMessage = $"Failed to launch: {ex.Message}"; }
    }
}
