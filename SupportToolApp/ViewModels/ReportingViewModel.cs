#pragma warning disable CS0618 // DisplayAlert obsolete until MAUI async variant resolves from ViewModel context
using System.Collections.ObjectModel;
using System.Text;
using System.Windows.Input;
using SupportToolApp.Services;

namespace SupportToolApp.ViewModels;

public class ReportRow { public string Col1 { get; set; } = ""; public string Col2 { get; set; } = ""; public string Col3 { get; set; } = ""; }

public class ReportingViewModel : BaseViewModel
{
    private readonly IAdService _ad;
    private readonly IExchangeService _exchange;
    private ObservableCollection<ReportRow> _rows = [];
    private string _col1Header = "Name", _col2Header = "Info", _col3Header = "Status";

    public ObservableCollection<ReportRow> ReportRows { get => _rows; set => SetProperty(ref _rows, value); }
    public string Col1Header { get => _col1Header; set => SetProperty(ref _col1Header, value); }
    public string Col2Header { get => _col2Header; set => SetProperty(ref _col2Header, value); }
    public string Col3Header { get => _col3Header; set => SetProperty(ref _col3Header, value); }

    public ICommand LicenseReportCommand { get; }
    public ICommand LockedUsersCommand { get; }
    public ICommand LastLogonCommand { get; }
    public ICommand SharedMailboxesCommand { get; }
    public ICommand ExportCsvCommand { get; }

    public ReportingViewModel(IAdService ad, IExchangeService exchange)
    {
        _ad = ad;
        _exchange = exchange;

        LicenseReportCommand = new Command(async () =>
        {
            Col1Header = "User"; Col2Header = "License"; Col3Header = "Licensed";
            var data = await _exchange.GetLicenseReportAsync();
            ReportRows = new ObservableCollection<ReportRow>(data.Select(r =>
                new ReportRow { Col1 = r.DisplayName, Col2 = string.Join(", ", r.Licenses), Col3 = r.IsLicensed ? "Yes" : "No" }));
        });

        LockedUsersCommand = new Command(async () =>
        {
            Col1Header = "Username"; Col2Header = "Display Name"; Col3Header = "Last Logon";
            var data = await _ad.GetLockedOutUsersAsync();
            ReportRows = new ObservableCollection<ReportRow>(data.Select(u =>
                new ReportRow { Col1 = u.SamAccountName, Col2 = u.DisplayName, Col3 = u.LastLogon?.ToString("g") ?? "Never" }));
        });

        LastLogonCommand = new Command(async () =>
        {
            Col1Header = "Username"; Col2Header = "Display Name"; Col3Header = "Last Logon";
            var data = await _ad.SearchUsersAsync("");
            ReportRows = new ObservableCollection<ReportRow>(data.OrderBy(u => u.LastLogon).Select(u =>
                new ReportRow { Col1 = u.SamAccountName, Col2 = u.DisplayName, Col3 = u.LastLogon?.ToString("g") ?? "Never" }));
        });

        SharedMailboxesCommand = new Command(async () =>
        {
            Col1Header = "Mailbox"; Col2Header = "Address"; Col3Header = "Size";
            var data = await _exchange.GetSharedMailboxesAsync();
            ReportRows = new ObservableCollection<ReportRow>(data.Select(m =>
                new ReportRow { Col1 = m.DisplayName, Col2 = m.PrimarySmtpAddress, Col3 = $"{m.TotalItemSizeBytes / 1_048_576:N0} MB" }));
        });

        ExportCsvCommand = new Command(async () =>
        {
            if (ReportRows.Count == 0) return;
            var sb = new StringBuilder();
            sb.AppendLine($"{Col1Header},{Col2Header},{Col3Header}");
            foreach (var r in ReportRows) sb.AppendLine($"\"{r.Col1}\",\"{r.Col2}\",\"{r.Col3}\"");
            var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), $"Report_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
            await File.WriteAllTextAsync(path, sb.ToString());
            await ((Page)Application.Current!.Windows[0].Page!).DisplayAlertAsync("Exported", $"Saved to:\n{path}", "OK");
        });
    }
}
