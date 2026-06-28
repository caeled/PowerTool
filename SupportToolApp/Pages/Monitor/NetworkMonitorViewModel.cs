using System.Collections.ObjectModel;
using System.Net.NetworkInformation;
using System.Windows.Input;
using SupportToolApp.ViewModels;

namespace SupportToolApp.Pages.Monitor;

public class NetworkMonitorViewModel : BaseViewModel
{
    private string _newHost = "";
    private bool _isMonitoring;
    private int _intervalSeconds = 30;
    private string _statusText = "Idle";
    private CancellationTokenSource? _cts;

    public ObservableCollection<MonitorEntry> Hosts { get; } = [];

    public string NewHost         { get => _newHost;         set => SetProperty(ref _newHost, value); }
    public bool   IsMonitoring    { get => _isMonitoring;    set { SetProperty(ref _isMonitoring, value); OnPropertyChanged(nameof(StartStopText)); } }
    public int    IntervalSeconds { get => _intervalSeconds; set => SetProperty(ref _intervalSeconds, value); }
    public string StatusText      { get => _statusText;      set => SetProperty(ref _statusText, value); }
    public string StartStopText   => IsMonitoring ? "Stop Monitoring" : "Start Monitoring";

    public List<int> IntervalOptions { get; } = [10, 15, 30, 60, 120];

    public ICommand AddHostCommand    { get; }
    public ICommand RemoveHostCommand { get; }
    public ICommand StartStopCommand  { get; }
    public ICommand PingAllCommand    { get; }

    public NetworkMonitorViewModel()
    {
        AddHostCommand = new Command(() =>
        {
            var h = NewHost.Trim();
            if (string.IsNullOrEmpty(h)) return;
            if (Hosts.Any(e => e.Host.Equals(h, StringComparison.OrdinalIgnoreCase))) return;
            Hosts.Add(new MonitorEntry { Host = h });
            NewHost = "";
        });

        RemoveHostCommand = new Command<MonitorEntry>(e => Hosts.Remove(e));

        PingAllCommand = new Command(async () =>
        {
            StatusText = "Pinging…";
            await PingAllAsync();
            StatusText = $"Pinged {Hosts.Count} host(s) — {DateTime.Now:HH:mm:ss}";
        });

        StartStopCommand = new Command(() =>
        {
            if (IsMonitoring)
            {
                _cts?.Cancel();
                IsMonitoring = false;
                StatusText = "Stopped.";
            }
            else
            {
                if (Hosts.Count == 0) return;
                _cts = new CancellationTokenSource();
                IsMonitoring = true;
                StatusText = "Monitoring…";
                _ = MonitorLoopAsync(_cts.Token);
            }
        });
    }

    private async Task MonitorLoopAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            await PingAllAsync();
            var online = Hosts.Count(h => h.IsOnline);
            MainThread.BeginInvokeOnMainThread(() =>
                StatusText = $"{online}/{Hosts.Count} online — {DateTime.Now:HH:mm:ss}");
            try { await Task.Delay(IntervalSeconds * 1000, ct); }
            catch (TaskCanceledException) { break; }
        }
    }

    private async Task PingAllAsync()
    {
        var tasks = Hosts.Select(entry => Task.Run(async () =>
        {
            try
            {
                using var ping = new Ping();
                var reply = await ping.SendPingAsync(entry.Host, 2000);
                var online = reply.Status == IPStatus.Success;
                var ms = online ? (int)reply.RoundtripTime : -1;
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    entry.IsOnline = online;
                    entry.ResponseMs = ms;
                    entry.LastChecked = DateTime.Now.ToString("HH:mm:ss");
                });
            }
            catch
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    entry.IsOnline = false;
                    entry.ResponseMs = -1;
                    entry.LastChecked = DateTime.Now.ToString("HH:mm:ss");
                });
            }
        }));
        await Task.WhenAll(tasks);
    }
}
