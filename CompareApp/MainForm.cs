using System.Text;
using System.Xml.Linq;

namespace CompareApp;

public class MainForm : Form
{
    private string? _pathA, _pathB;
    private List<Dictionary<string, string>> _rowsA = [];
    private List<Dictionary<string, string>> _rowsB = [];
    private List<string> _onlyA = [], _onlyB = [], _both = [];

    // File A row
    private Label _labelA = null!;
    private Button _btnBrowseA = null!;

    // File B row
    private Label _labelB = null!;
    private Button _btnBrowseB = null!;

    // Controls row
    private ComboBox _colPicker = null!;
    private Button _btnCompare = null!, _btnClear = null!, _btnExport = null!;

    // Status
    private Label _status = null!;

    // Results
    private ListBox _listOnlyA = null!, _listBoth = null!, _listOnlyB = null!;
    private Label _headerA = null!, _headerBoth = null!, _headerB = null!;

    public MainForm()
    {
        Text = "Compare Files";
        Size = new Size(900, 620);
        MinimumSize = new Size(700, 480);
        StartPosition = FormStartPosition.CenterScreen;
        Font = new Font("Segoe UI", 9f);

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            RowCount = 5,
            ColumnCount = 1,
            Padding = new Padding(12)
        };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));  // File A
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));  // File B
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));  // Controls
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 22));  // Status
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));  // Results

        layout.Controls.Add(BuildFileRow("File A:", out _labelA, out _btnBrowseA, OnBrowseA), 0, 0);
        layout.Controls.Add(BuildFileRow("File B:", out _labelB, out _btnBrowseB, OnBrowseB), 0, 1);
        layout.Controls.Add(BuildControlsRow(), 0, 2);
        layout.Controls.Add(_status = new Label { Dock = DockStyle.Fill, ForeColor = Color.DimGray, TextAlign = ContentAlignment.MiddleLeft }, 0, 3);
        layout.Controls.Add(BuildResultsPanel(), 0, 4);

        Controls.Add(layout);
    }

    private static Panel BuildFileRow(string label, out Label pathLabel, out Button browseBtn, EventHandler handler)
    {
        var p = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(240, 247, 255) };
        p.Paint += (_, e) =>
        {
            using var pen = new Pen(Color.FromArgb(0, 120, 212));
            e.Graphics.DrawRectangle(pen, 0, 0, p.Width - 1, p.Height - 1);
        };

        var lbl = new Label { Text = label, Width = 60, Left = 10, Top = 12, Font = new Font("Segoe UI", 9f, FontStyle.Bold) };
        pathLabel = new Label { Text = "No file selected", Left = 74, Top = 12, ForeColor = Color.Gray, AutoSize = false, Width = 800, Anchor = AnchorStyles.Left | AnchorStyles.Right };
        browseBtn = new Button
        {
            Text = "Browse", Width = 80, Height = 26, Left = 860, Top = 8,
            Anchor = AnchorStyles.Right | AnchorStyles.Top,
            BackColor = Color.FromArgb(0, 120, 212), ForeColor = Color.White, FlatStyle = FlatStyle.Flat
        };
        browseBtn.Click += handler;
        p.Controls.AddRange([lbl, pathLabel, browseBtn]);
        return p;
    }

    private Panel BuildControlsRow()
    {
        var p = new Panel { Dock = DockStyle.Fill };

        var lbl = new Label { Text = "Compare on column:", Left = 0, Top = 10, AutoSize = true, Font = new Font("Segoe UI", 9f, FontStyle.Bold) };
        _colPicker = new ComboBox { Left = 148, Top = 7, Width = 260, DropDownStyle = ComboBoxStyle.DropDownList };
        _colPicker.Items.Add("(load files first)");
        _colPicker.SelectedIndex = 0;
        _colPicker.Enabled = false;

        _btnCompare = new Button { Text = "Compare", Left = 418, Top = 6, Width = 90, Height = 28, BackColor = Color.FromArgb(0, 120, 212), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9f, FontStyle.Bold) };
        _btnClear   = new Button { Text = "Clear",   Left = 516, Top = 6, Width = 70, Height = 28, BackColor = Color.Gray, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
        _btnExport  = new Button { Text = "Export to CSV", Left = 596, Top = 6, Width = 110, Height = 28, BackColor = Color.FromArgb(34, 139, 34), ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Visible = false };

        _btnCompare.Click += OnCompare;
        _btnClear.Click   += OnClear;
        _btnExport.Click  += OnExport;

        p.Controls.AddRange([lbl, _colPicker, _btnCompare, _btnClear, _btnExport]);
        return p;
    }

    private Panel BuildResultsPanel()
    {
        var p = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, ColumnStyles = { new ColumnStyle(SizeType.Percent, 33.3f), new ColumnStyle(SizeType.Percent, 33.3f), new ColumnStyle(SizeType.Percent, 33.3f) }, RowCount = 2 };
        p.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
        p.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        _headerA    = MakeHeader("Only in A (0)",  Color.FromArgb(211, 47, 47));
        _headerBoth = MakeHeader("In Both (0)",    Color.FromArgb(56, 142, 60));
        _headerB    = MakeHeader("Only in B (0)",  Color.FromArgb(21, 101, 192));

        _listOnlyA = MakeList(); _listBoth = MakeList(); _listOnlyB = MakeList();

        p.Controls.Add(_headerA,    0, 0);
        p.Controls.Add(_headerBoth, 1, 0);
        p.Controls.Add(_headerB,    2, 0);
        p.Controls.Add(_listOnlyA,  0, 1);
        p.Controls.Add(_listBoth,   1, 1);
        p.Controls.Add(_listOnlyB,  2, 1);

        return p;
    }

    private static Label MakeHeader(string text, Color back) =>
        new() { Text = text, Dock = DockStyle.Fill, BackColor = back, ForeColor = Color.White,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold), TextAlign = ContentAlignment.MiddleCenter };

    private static ListBox MakeList() =>
        new() { Dock = DockStyle.Fill, Font = new Font("Courier New", 9f), BorderStyle = BorderStyle.FixedSingle };

    // ── Browse ────────────────────────────────────────────────────────────────
    private void OnBrowseA(object? sender, EventArgs e)
    {
        using var d = new OpenFileDialog { Filter = "Data Files|*.csv;*.xml|CSV|*.csv|XML|*.xml|All|*.*", Title = "Select File A" };
        if (d.ShowDialog() != DialogResult.OK) return;
        _pathA = d.FileName;
        _labelA.Text = d.FileName; _labelA.ForeColor = Color.Black;
        TryLoadColumns();
    }

    private void OnBrowseB(object? sender, EventArgs e)
    {
        using var d = new OpenFileDialog { Filter = "Data Files|*.csv;*.xml|CSV|*.csv|XML|*.xml|All|*.*", Title = "Select File B" };
        if (d.ShowDialog() != DialogResult.OK) return;
        _pathB = d.FileName;
        _labelB.Text = d.FileName; _labelB.ForeColor = Color.Black;
        TryLoadColumns();
    }

    private void TryLoadColumns()
    {
        if (_pathA == null || _pathB == null) return;
        try
        {
            _rowsA = LoadFile(_pathA);
            _rowsB = LoadFile(_pathB);

            var keysB = _rowsB.FirstOrDefault()?.Keys ?? (IEnumerable<string>)[];
            var cols = (_rowsA.FirstOrDefault()?.Keys ?? (IEnumerable<string>)[])
                       .Intersect(keysB).ToList();

            _colPicker.Items.Clear();
            _colPicker.Items.AddRange(cols.Cast<object>().ToArray());
            _colPicker.Enabled = cols.Count > 0;
            if (cols.Count > 0) _colPicker.SelectedIndex = 0;

            SetStatus($"Loaded — File A: {_rowsA.Count} rows, File B: {_rowsB.Count} rows. Pick a column and click Compare.");
        }
        catch (Exception ex) { MessageBox.Show(ex.Message, "Load Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }

    // ── Compare ───────────────────────────────────────────────────────────────
    private void OnCompare(object? sender, EventArgs e)
    {
        if (_pathA == null || _pathB == null) { MessageBox.Show("Select both files first."); return; }
        if (_colPicker.SelectedItem is not string col) { MessageBox.Show("Select a column."); return; }

        var setA = _rowsA.Select(r => r.TryGetValue(col, out var v) ? v : "").ToHashSet(StringComparer.OrdinalIgnoreCase);
        var setB = _rowsB.Select(r => r.TryGetValue(col, out var v) ? v : "").ToHashSet(StringComparer.OrdinalIgnoreCase);

        _onlyA = setA.Where(v => !setB.Contains(v)).OrderBy(v => v).ToList();
        _onlyB = setB.Where(v => !setA.Contains(v)).OrderBy(v => v).ToList();
        _both  = setA.Where(v =>  setB.Contains(v)).OrderBy(v => v).ToList();

        _listOnlyA.Items.Clear(); _listOnlyA.Items.AddRange(_onlyA.Cast<object>().ToArray());
        _listBoth.Items.Clear();  _listBoth.Items.AddRange(_both.Cast<object>().ToArray());
        _listOnlyB.Items.Clear(); _listOnlyB.Items.AddRange(_onlyB.Cast<object>().ToArray());

        _headerA.Text    = $"Only in A ({_onlyA.Count})";
        _headerBoth.Text = $"In Both ({_both.Count})";
        _headerB.Text    = $"Only in B ({_onlyB.Count})";

        SetStatus($"Compared on '{col}' — {_onlyA.Count} only in A, {_both.Count} in both, {_onlyB.Count} only in B.");
        _btnExport.Visible = true;
    }

    // ── Clear ─────────────────────────────────────────────────────────────────
    private void OnClear(object? sender, EventArgs e)
    {
        _pathA = _pathB = null;
        _rowsA.Clear(); _rowsB.Clear();
        _labelA.Text = _labelB.Text = "No file selected";
        _labelA.ForeColor = _labelB.ForeColor = Color.Gray;
        _colPicker.Items.Clear(); _colPicker.Items.Add("(load files first)"); _colPicker.SelectedIndex = 0; _colPicker.Enabled = false;
        _listOnlyA.Items.Clear(); _listBoth.Items.Clear(); _listOnlyB.Items.Clear();
        _headerA.Text = "Only in A (0)"; _headerBoth.Text = "In Both (0)"; _headerB.Text = "Only in B (0)";
        _btnExport.Visible = false;
        _status.Text = "";
    }

    // ── Export ────────────────────────────────────────────────────────────────
    private void OnExport(object? sender, EventArgs e)
    {
        using var d = new SaveFileDialog { Filter = "CSV|*.csv", FileName = $"Compare-Results-{DateTime.Now:yyyy-MM-dd}.csv", Title = "Save Results" };
        if (d.ShowDialog() != DialogResult.OK) return;
        try
        {
            var sb = new StringBuilder();
            sb.AppendLine("Result,Value");
            foreach (var v in _onlyA) sb.AppendLine($"Only in A,\"{v}\"");
            foreach (var v in _both)  sb.AppendLine($"In Both,\"{v}\"");
            foreach (var v in _onlyB) sb.AppendLine($"Only in B,\"{v}\"");
            File.WriteAllText(d.FileName, sb.ToString(), Encoding.UTF8);
            SetStatus($"Exported to {d.FileName}");
        }
        catch (Exception ex) { MessageBox.Show(ex.Message, "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }

    // ── File loading ──────────────────────────────────────────────────────────
    private static List<Dictionary<string, string>> LoadFile(string path) =>
        Path.GetExtension(path).ToLowerInvariant() == ".xml" ? LoadXml(path) : LoadCsv(path);

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
        foreach (var el in doc.Root?.Elements() ?? [])
        {
            var row = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var child in el.Elements())
                row[child.Name.LocalName] = child.Value;
            if (row.Count > 0) rows.Add(row);
        }
        return rows;
    }

    private void SetStatus(string msg) => _status.Text = msg;
}

static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.Run(new MainForm());
    }
}
