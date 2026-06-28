using System.Diagnostics;

namespace SoundCombineApp;

public class MainForm : Form
{
    // Controls
    private TabControl _tabs = null!;
    private TabPage _tabSimple = null!, _tabBatch = null!;

    // Simple mode
    private ListBox _simpleList = null!;
    private Button _simpleBtnAdd = null!, _simpleBtnRemove = null!, _simpleBtnUp = null!, _simpleBtnDown = null!;
    private TextBox _simpleOutput = null!;
    private Button _simpleBtnBrowseOutput = null!, _simpleBtnRun = null!;
    private Label _simpleStatus = null!;

    // Batch mode
    private TextBox _batchFolder = null!, _batchAddFile = null!;
    private Button _batchBtnFolder = null!, _batchBtnAddFile = null!, _batchBtnSilence = null!, _batchBtnRun = null!;
    private RadioButton _batchPrepend = null!, _batchAppend = null!;
    private ListBox _batchList = null!;
    private Label _batchStatus = null!;

    private readonly string _exeDir;

    public MainForm()
    {
        _exeDir = AppContext.BaseDirectory;

        Text = "Sound Combine";
        Size = new Size(780, 560);
        MinimumSize = new Size(640, 480);
        StartPosition = FormStartPosition.CenterScreen;
        Font = new Font("Segoe UI", 9f);

        _tabs    = new TabControl { Dock = DockStyle.Fill };
        _tabSimple = new TabPage("Simple Combine");
        _tabBatch  = new TabPage("Batch Prepend / Append");
        _tabs.TabPages.AddRange([_tabSimple, _tabBatch]);

        BuildSimpleTab();
        BuildBatchTab();

        Controls.Add(_tabs);
    }

    // ── Simple tab ────────────────────────────────────────────────────────────
    private void BuildSimpleTab()
    {
        var p = _tabSimple;
        p.Padding = new Padding(8);

        var info = new Label
        {
            Text = "Add WAV files in order. Output is a new combined file — originals untouched.",
            Dock = DockStyle.Top, Height = 28, ForeColor = Color.Gray
        };

        var btnPanel = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 36, FlowDirection = FlowDirection.LeftToRight };
        _simpleBtnAdd    = Btn("+ Add WAV", Color.DarkGreen);
        _simpleBtnRemove = Btn("Remove", Color.DarkRed);
        _simpleBtnUp     = Btn("↑ Up", Color.SteelBlue);
        _simpleBtnDown   = Btn("↓ Down", Color.SteelBlue);
        btnPanel.Controls.AddRange([_simpleBtnAdd, _simpleBtnRemove, _simpleBtnUp, _simpleBtnDown]);

        _simpleList = new ListBox { Dock = DockStyle.Fill, SelectionMode = SelectionMode.One, Font = new Font("Courier New", 9f) };

        var outPanel = new Panel { Dock = DockStyle.Bottom, Height = 32 };
        var outLabel = new Label { Text = "Output:", Width = 55, Left = 0, Top = 6 };
        _simpleOutput = new TextBox { Left = 58, Top = 3, Width = 480, Anchor = AnchorStyles.Left | AnchorStyles.Right };
        _simpleBtnBrowseOutput = new Button { Text = "Browse", Left = 542, Top = 2, Width = 70, Height = 26 };
        _simpleBtnBrowseOutput.Click += (_, _) =>
        {
            using var dlg = new SaveFileDialog { Filter = "WAV files|*.wav", FileName = "combined.wav" };
            if (dlg.ShowDialog() == DialogResult.OK) _simpleOutput.Text = dlg.FileName;
        };
        outPanel.Controls.AddRange([outLabel, _simpleOutput, _simpleBtnBrowseOutput]);

        var runPanel = new Panel { Dock = DockStyle.Bottom, Height = 36 };
        _simpleBtnRun = new Button { Text = "Combine", Width = 100, Left = 0, Top = 4, Height = 28, BackColor = Color.SteelBlue, ForeColor = Color.White };
        _simpleStatus = new Label { Left = 110, Top = 8, AutoSize = true, ForeColor = Color.DarkGreen };
        runPanel.Controls.AddRange([_simpleBtnRun, _simpleStatus]);

        p.Controls.AddRange([_simpleList, outPanel, runPanel, btnPanel, info]);

        _simpleBtnAdd.Click    += (_, _) => { using var d = new OpenFileDialog { Filter = "WAV files|*.wav;*.WAV", Multiselect = true }; if (d.ShowDialog() == DialogResult.OK) foreach (var f in d.FileNames) _simpleList.Items.Add(f); };
        _simpleBtnRemove.Click += (_, _) => { if (_simpleList.SelectedIndex >= 0) _simpleList.Items.RemoveAt(_simpleList.SelectedIndex); };
        _simpleBtnUp.Click     += (_, _) => MoveItem(_simpleList, -1);
        _simpleBtnDown.Click   += (_, _) => MoveItem(_simpleList, +1);
        _simpleBtnRun.Click    += async (_, _) => await RunSimple();
    }

    // ── Batch tab ─────────────────────────────────────────────────────────────
    private void BuildBatchTab()
    {
        var p = _tabBatch;
        p.Padding = new Padding(8);

        var tbl = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 3 };
        tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
        tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));

        _batchFolder  = new TextBox { ReadOnly = true, Dock = DockStyle.Fill };
        _batchAddFile = new TextBox { ReadOnly = true, Dock = DockStyle.Fill };
        _batchBtnFolder  = new Button { Text = "Browse Folder", Dock = DockStyle.Fill };
        _batchBtnAddFile = new Button { Text = "Browse WAV",    Dock = DockStyle.Fill };
        _batchBtnSilence = new Button { Text = "Built-in Silence", Dock = DockStyle.Fill };

        tbl.Controls.Add(new Label { Text = "WAV folder:", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, 0);
        tbl.Controls.Add(_batchFolder, 1, 0);
        tbl.Controls.Add(_batchBtnFolder, 2, 0);

        tbl.Controls.Add(new Label { Text = "Prepend/append:", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, 1);
        tbl.Controls.Add(_batchAddFile, 1, 1);
        var addBtns = new FlowLayoutPanel { Dock = DockStyle.Fill };
        addBtns.Controls.AddRange([_batchBtnAddFile, _batchBtnSilence]);
        tbl.Controls.Add(addBtns, 2, 1);

        _batchPrepend = new RadioButton { Text = "Prepend (add to START)", Checked = true, AutoSize = true };
        _batchAppend  = new RadioButton { Text = "Append (add to END)",    AutoSize = true };
        var radioPanel = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 28 };
        radioPanel.Controls.AddRange([new Label { Text = "Operation:", Width = 70, TextAlign = ContentAlignment.MiddleLeft }, _batchPrepend, _batchAppend]);

        _batchList = new ListBox { Dock = DockStyle.Fill, Font = new Font("Courier New", 9f) };

        var runPanel = new Panel { Dock = DockStyle.Bottom, Height = 36 };
        _batchBtnRun  = new Button { Text = "Run Batch", Width = 100, Left = 0, Top = 4, Height = 28, BackColor = Color.SteelBlue, ForeColor = Color.White };
        _batchStatus  = new Label { Left = 110, Top = 8, AutoSize = true, ForeColor = Color.DarkGreen };
        runPanel.Controls.AddRange([_batchBtnRun, _batchStatus]);

        p.Controls.AddRange([_batchList, runPanel, radioPanel, tbl]);

        _batchBtnFolder.Click  += (_, _) => { using var d = new FolderBrowserDialog(); if (d.ShowDialog() == DialogResult.OK) { _batchFolder.Text = d.SelectedPath; LoadBatchFiles(d.SelectedPath); } };
        _batchBtnAddFile.Click += (_, _) => { using var d = new OpenFileDialog { Filter = "WAV files|*.wav;*.WAV" }; if (d.ShowDialog() == DialogResult.OK) _batchAddFile.Text = d.FileName; };
        _batchBtnSilence.Click += (_, _) => _batchAddFile.Text = Path.Combine(_exeDir, "silence03.WAV");
        _batchBtnRun.Click     += async (_, _) => await RunBatch();
    }

    private void LoadBatchFiles(string folder)
    {
        _batchList.Items.Clear();
        foreach (var f in Directory.GetFiles(folder, "*.wav").Concat(Directory.GetFiles(folder, "*.WAV")).OrderBy(x => x))
            _batchList.Items.Add(Path.GetFileName(f));
    }

    // ── Run Simple ────────────────────────────────────────────────────────────
    private async Task RunSimple()
    {
        if (_simpleList.Items.Count < 2) { _simpleStatus.Text = "Add at least 2 files."; return; }
        if (string.IsNullOrWhiteSpace(_simpleOutput.Text)) { _simpleStatus.Text = "Set an output path."; return; }

        _simpleBtnRun.Enabled = false;
        _simpleStatus.Text = "Working...";
        _simpleStatus.ForeColor = Color.DarkOrange;

        var files  = _simpleList.Items.Cast<string>().ToList();
        var output = _simpleOutput.Text;
        var exe    = Path.Combine(_exeDir, "sndfile-concat.exe");

        try
        {
            await Task.Run(() =>
            {
                var work = Path.Combine(Path.GetTempPath(), $"sc_{Guid.NewGuid():N}");
                Directory.CreateDirectory(work);
                try
                {
                    var copies = files.Select((f, i) =>
                    {
                        var dest = Path.Combine(work, $"{i:D4}_{Path.GetFileName(f)}");
                        File.Copy(f, dest, true);
                        return dest;
                    }).ToList();

                    for (int i = 1; i < copies.Count; i++)
                        RunExe(exe, $"\"{copies[0]}\" \"{copies[i]}\"", work);

                    var outDir = Path.GetDirectoryName(output);
                    if (!string.IsNullOrEmpty(outDir)) Directory.CreateDirectory(outDir);
                    File.Copy(copies[0], output, true);
                }
                finally { Directory.Delete(work, true); }
            });
            _simpleStatus.Text = $"Saved: {Path.GetFileName(output)}";
            _simpleStatus.ForeColor = Color.DarkGreen;
        }
        catch (Exception ex) { _simpleStatus.Text = $"Error: {ex.Message}"; _simpleStatus.ForeColor = Color.DarkRed; }
        finally { _simpleBtnRun.Enabled = true; }
    }

    // ── Run Batch ─────────────────────────────────────────────────────────────
    private async Task RunBatch()
    {
        if (string.IsNullOrWhiteSpace(_batchFolder.Text)) { _batchStatus.Text = "Select a folder."; return; }
        if (string.IsNullOrWhiteSpace(_batchAddFile.Text)) { _batchStatus.Text = "Select a WAV to prepend/append."; return; }

        var wavFiles = Directory.GetFiles(_batchFolder.Text, "*.wav")
                                .Concat(Directory.GetFiles(_batchFolder.Text, "*.WAV"))
                                .OrderBy(x => x).ToList();
        if (wavFiles.Count == 0) { _batchStatus.Text = "No WAV files found."; return; }

        _batchBtnRun.Enabled = false;
        _batchStatus.Text = $"Processing {wavFiles.Count} file(s)...";
        _batchStatus.ForeColor = Color.DarkOrange;

        var exe     = Path.Combine(_exeDir, "sndfile-concat.exe");
        var addFile = _batchAddFile.Text;
        var prepend = _batchPrepend.Checked;
        var outDir  = Path.Combine(_batchFolder.Text, "combined");

        try
        {
            await Task.Run(() =>
            {
                Directory.CreateDirectory(outDir);
                foreach (var src in wavFiles)
                {
                    var outFile = Path.Combine(outDir, Path.GetFileName(src));
                    if (prepend)
                    {
                        File.Copy(addFile, outFile, true);
                        RunExe(exe, $"\"{outFile}\" \"{src}\"", outDir);
                    }
                    else
                    {
                        File.Copy(src, outFile, true);
                        RunExe(exe, $"\"{outFile}\" \"{addFile}\"", outDir);
                    }
                }
            });
            _batchStatus.Text = $"Done — {wavFiles.Count} file(s) in: {outDir}";
            _batchStatus.ForeColor = Color.DarkGreen;
        }
        catch (Exception ex) { _batchStatus.Text = $"Error: {ex.Message}"; _batchStatus.ForeColor = Color.DarkRed; }
        finally { _batchBtnRun.Enabled = true; }
    }

    private static void RunExe(string exe, string args, string workDir)
    {
        var psi = new ProcessStartInfo(exe, args) { WorkingDirectory = workDir, UseShellExecute = false, CreateNoWindow = true };
        using var p = Process.Start(psi) ?? throw new Exception("Failed to start sndfile-concat.exe");
        p.WaitForExit();
        if (p.ExitCode != 0) throw new Exception($"sndfile-concat exited {p.ExitCode}");
    }

    private static void MoveItem(ListBox lb, int direction)
    {
        var i = lb.SelectedIndex;
        if (i < 0) return;
        var newI = i + direction;
        if (newI < 0 || newI >= lb.Items.Count) return;
        var item = lb.Items[i];
        lb.Items.RemoveAt(i);
        lb.Items.Insert(newI, item);
        lb.SelectedIndex = newI;
    }

    private static Button Btn(string text, Color back) =>
        new() { Text = text, AutoSize = true, Height = 28, BackColor = back, ForeColor = Color.White, FlatStyle = FlatStyle.Flat };
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
