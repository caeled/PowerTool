using System.Runtime.InteropServices;
using System.Text;
using System.Xml.Linq;

namespace SupportToolApp.Pages.Compare;

public partial class ComparePage : ContentPage
{
    private string? _pathA, _pathB;
    private List<Dictionary<string, string>> _rowsA = [];
    private List<Dictionary<string, string>> _rowsB = [];
    private List<string> _onlyA = [], _onlyB = [], _both = [];

    public ComparePage()
    {
        InitializeComponent();
    }

    private async void OnBrowseA(object? sender, EventArgs e)
    {
        var path = await PickFile();
        if (path == null) return;
        _pathA = path;
        LabelA.Text = Path.GetFileName(path);
        LabelA.TextColor = Colors.Black;
        await TryLoadColumns();
    }

    private async void OnBrowseB(object? sender, EventArgs e)
    {
        var path = await PickFile();
        if (path == null) return;
        _pathB = path;
        LabelB.Text = Path.GetFileName(path);
        LabelB.TextColor = Colors.Black;
        await TryLoadColumns();
    }

    private Task<string?> PickFile()
    {
#if WINDOWS
        return Task.Run(() =>
        {
            var ofn = new OpenFileName
            {
                lStructSize  = Marshal.SizeOf<OpenFileName>(),
                lpstrFilter  = "Data Files\0*.csv;*.xml\0CSV Files\0*.csv\0XML Files\0*.xml\0All Files\0*.*\0\0",
                lpstrFile    = new string('\0', 260),
                nMaxFile     = 260,
                lpstrTitle   = "Select CSV or XML file",
                Flags        = 0x00000004 | 0x00000800 // OFN_NOCHANGEDIR | OFN_FILEMUSTEXIST
            };
            var ok = GetOpenFileName(ref ofn);
            var path = ok ? ofn.lpstrFile.TrimEnd('\0') : null;
            return string.IsNullOrEmpty(path) ? null : path;
        });
#else
        return FilePicker.Default.PickAsync(new PickOptions
        {
            PickerTitle = "Select CSV or XML file",
            FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
            {
                { DevicePlatform.macOS, ["csv", "xml"] },
            })
        }).ContinueWith(t => t.Result?.FullPath);
#endif
    }

#if WINDOWS
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    private struct OpenFileName
    {
        public int    lStructSize;
        public IntPtr hwndOwner;
        public IntPtr hInstance;
        public string lpstrFilter;
        public string lpstrCustomFilter;
        public int    nMaxCustFilter;
        public int    nFilterIndex;
        public string lpstrFile;
        public int    nMaxFile;
        public string lpstrFileTitle;
        public int    nMaxFileTitle;
        public string lpstrInitialDir;
        public string lpstrTitle;
        public int    Flags;
        public short  nFileOffset;
        public short  nFileExtension;
        public string lpstrDefExt;
        public IntPtr lCustData;
        public IntPtr lpfnHook;
        public string lpTemplateName;
        public IntPtr pvReserved;
        public int    dwReserved;
        public int    flagsEx;
    }

    [DllImport("comdlg32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    private static extern bool GetOpenFileName(ref OpenFileName ofn);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    private struct SaveFileName
    {
        public int    lStructSize;
        public IntPtr hwndOwner;
        public IntPtr hInstance;
        public string lpstrFilter;
        public string lpstrCustomFilter;
        public int    nMaxCustFilter;
        public int    nFilterIndex;
        public string lpstrFile;
        public int    nMaxFile;
        public string lpstrFileTitle;
        public int    nMaxFileTitle;
        public string lpstrInitialDir;
        public string lpstrTitle;
        public int    Flags;
        public short  nFileOffset;
        public short  nFileExtension;
        public string lpstrDefExt;
        public IntPtr lCustData;
        public IntPtr lpfnHook;
        public string lpTemplateName;
        public IntPtr pvReserved;
        public int    dwReserved;
        public int    flagsEx;
    }

    [DllImport("comdlg32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    private static extern bool GetSaveFileName(ref SaveFileName ofn);
#endif

    private async Task TryLoadColumns()
    {
        if (_pathA == null || _pathB == null) return;

        try
        {
            _rowsA = LoadFile(_pathA);
            _rowsB = LoadFile(_pathB);

            var keysB = _rowsB.FirstOrDefault()?.Keys ?? (IEnumerable<string>)[];
            var cols = (_rowsA.FirstOrDefault()?.Keys ?? (IEnumerable<string>)[])
                       .Intersect(keysB)
                       .ToList();

            ColumnPicker.ItemsSource = cols;
            if (cols.Count > 0) ColumnPicker.SelectedIndex = 0;

            SetStatus($"Loaded — File A: {_rowsA.Count} rows, File B: {_rowsB.Count} rows. Pick a column and click Compare.");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Load Error", ex.Message, "OK");
        }
    }

    private static List<Dictionary<string, string>> LoadFile(string path)
    {
        var ext = Path.GetExtension(path).ToLowerInvariant();
        return ext == ".xml" ? LoadXml(path) : LoadCsv(path);
    }

    private static List<Dictionary<string, string>> LoadCsv(string path)
    {
        var rows = new List<Dictionary<string, string>>();
        var lines = File.ReadAllLines(path, Encoding.UTF8);
        if (lines.Length < 2) return rows;

        var headers = SplitCsvLine(lines[0]);
        foreach (var line in lines.Skip(1))
        {
            if (string.IsNullOrWhiteSpace(line)) continue;
            var values = SplitCsvLine(line);
            var row = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < headers.Count; i++)
                row[headers[i]] = i < values.Count ? values[i] : "";
            rows.Add(row);
        }
        return rows;
    }

    private static List<string> SplitCsvLine(string line)
    {
        var result = new List<string>();
        var sb = new StringBuilder();
        bool inQuotes = false;
        foreach (char c in line)
        {
            if (c == '"') { inQuotes = !inQuotes; continue; }
            if (c == ',' && !inQuotes) { result.Add(sb.ToString().Trim()); sb.Clear(); continue; }
            sb.Append(c);
        }
        result.Add(sb.ToString().Trim());
        return result;
    }

    private static List<Dictionary<string, string>> LoadXml(string path)
    {
        var rows = new List<Dictionary<string, string>>();
        var doc = XDocument.Load(path);
        var items = doc.Root?.Elements() ?? [];
        foreach (var el in items)
        {
            var row = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var child in el.Elements())
                row[child.Name.LocalName] = child.Value;
            if (row.Count > 0) rows.Add(row);
        }
        return rows;
    }

    private async void OnCompare(object? sender, EventArgs e)
    {
        if (_pathA == null || _pathB == null) { await DisplayAlert("Compare", "Please select both files first.", "OK"); return; }
        if (ColumnPicker.SelectedItem is not string col) { await DisplayAlert("Compare", "Please select a column to compare on.", "OK"); return; }

        var setA = _rowsA.Select(r => r.TryGetValue(col, out var v) ? v : "").ToHashSet(StringComparer.OrdinalIgnoreCase);
        var setB = _rowsB.Select(r => r.TryGetValue(col, out var v) ? v : "").ToHashSet(StringComparer.OrdinalIgnoreCase);

        _onlyA = setA.Where(v => !setB.Contains(v)).OrderBy(v => v).ToList();
        _onlyB = setB.Where(v => !setA.Contains(v)).OrderBy(v => v).ToList();
        _both  = setA.Where(v =>  setB.Contains(v)).OrderBy(v => v).ToList();

        ListOnlyA.ItemsSource = _onlyA;
        ListOnlyB.ItemsSource = _onlyB;
        ListBoth.ItemsSource  = _both;

        HeaderA.Text    = $"Only in A ({_onlyA.Count})";
        HeaderB.Text    = $"Only in B ({_onlyB.Count})";
        HeaderBoth.Text = $"In Both ({_both.Count})";

        SetStatus($"Compared on '{col}' — {_onlyA.Count} only in A, {_both.Count} in both, {_onlyB.Count} only in B.");
        ExportButton.IsVisible = true;
    }

    private void OnClear(object? sender, EventArgs e)
    {
        _pathA = _pathB = null;
        _rowsA.Clear(); _rowsB.Clear();
        LabelA.Text = LabelB.Text = "No file selected";
        LabelA.TextColor = LabelB.TextColor = Color.FromArgb("#777777");
        ColumnPicker.ItemsSource = null;
        ColumnPicker.Title = "(load files first)";
        ListOnlyA.ItemsSource = ListOnlyB.ItemsSource = ListBoth.ItemsSource = null;
        HeaderA.Text = "Only in A (0)";
        HeaderB.Text = "Only in B (0)";
        HeaderBoth.Text = "In Both (0)";
        ExportButton.IsVisible = false;
        StatusLabel.IsVisible = false;
    }

    private async void OnExport(object? sender, EventArgs e)
    {
        try
        {
            var sb = new StringBuilder();
            sb.AppendLine("Result,Value");
            foreach (var v in _onlyA) sb.AppendLine($"Only in A,\"{v}\"");
            foreach (var v in _both)  sb.AppendLine($"In Both,\"{v}\"");
            foreach (var v in _onlyB) sb.AppendLine($"Only in B,\"{v}\"");

#if WINDOWS
            var savePath = await Task.Run(() =>
            {
                var ofn = new SaveFileName
                {
                    lStructSize  = Marshal.SizeOf<SaveFileName>(),
                    lpstrFilter  = "CSV Files\0*.csv\0All Files\0*.*\0\0",
                    lpstrFile    = $"Compare-Results-{DateTime.Now:yyyy-MM-dd}" + new string('\0', 200),
                    nMaxFile     = 260,
                    lpstrTitle   = "Save Compare Results",
                    lpstrDefExt  = "csv",
                    Flags        = 0x00000004 | 0x00000002 // OFN_NOCHANGEDIR | OFN_OVERWRITEPROMPT
                };
                return GetSaveFileName(ref ofn) ? ofn.lpstrFile.TrimEnd('\0') : null;
            });
            if (string.IsNullOrEmpty(savePath)) return;
            await File.WriteAllTextAsync(savePath, sb.ToString());
            SetStatus($"Exported to {savePath}");
#else
            var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                $"Compare-Results-{DateTime.Now:yyyy-MM-dd}.csv");
            await File.WriteAllTextAsync(path, sb.ToString());
            SetStatus($"Exported to {path}");
#endif
        }
        catch (Exception ex)
        {
            await DisplayAlert("Export Failed", ex.Message, "OK");
        }
    }

    private void SetStatus(string msg)
    {
        StatusLabel.Text = msg;
        StatusLabel.IsVisible = true;
    }
}
