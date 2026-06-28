using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SupportToolApp.Pages.Monitor;

public class MonitorEntry : INotifyPropertyChanged
{
    private bool _isOnline;
    private int _responseMs = -1;
    private string _lastChecked = "—";

    public string Host { get; set; } = "";

    public bool IsOnline
    {
        get => _isOnline;
        set
        {
            _isOnline = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(StatusColor));
            OnPropertyChanged(nameof(StatusText));
            OnPropertyChanged(nameof(RowColor));
        }
    }

    public int ResponseMs
    {
        get => _responseMs;
        set { _responseMs = value; OnPropertyChanged(); OnPropertyChanged(nameof(ResponseText)); }
    }

    public string LastChecked
    {
        get => _lastChecked;
        set { _lastChecked = value; OnPropertyChanged(); }
    }

    public string StatusColor  => IsOnline ? "#228B22" : "#CC0000";
    public string StatusText   => IsOnline ? "ONLINE"  : "OFFLINE";
    public string ResponseText => ResponseMs >= 0 ? $"{ResponseMs} ms" : "—";
    public string RowColor     => IsOnline ? "#1A2E1A" : "#2E1A1A";

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
