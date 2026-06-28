using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Input;
using SupportToolApp.Helpers;

namespace SupportToolApp.Pages.SoundCombine;

public class SoundCombineViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    private void SetProperty<T>(ref T field, T value, string name) { if (!Equals(field, value)) { field = value; OnPropertyChanged(name); } }

    // ── Mode ────────────────────────────────────────────────────────────────
    private bool _modeSimple = true, _modeBatch;
    public bool ModeSimple { get => _modeSimple; set { SetProperty(ref _modeSimple, value, nameof(ModeSimple)); OnPropertyChanged(nameof(ShowSimple)); OnPropertyChanged(nameof(ShowBatch)); } }
    public bool ModeBatch  { get => _modeBatch;  set { SetProperty(ref _modeBatch,  value, nameof(ModeBatch));  OnPropertyChanged(nameof(ShowSimple)); OnPropertyChanged(nameof(ShowBatch)); } }
    public bool ShowSimple => ModeSimple;
    public bool ShowBatch  => ModeBatch;

    // ── Simple mode ─────────────────────────────────────────────────────────
    public ObservableCollection<string> SimpleFiles { get; } = [];
    private string _simpleOutput = "", _simpleStatus = "";
    public string SimpleOutput { get => _simpleOutput; set => SetProperty(ref _simpleOutput, value, nameof(SimpleOutput)); }
    public string SimpleStatus { get => _simpleStatus; set => SetProperty(ref _simpleStatus, value, nameof(SimpleStatus)); }

    // ── Batch mode ──────────────────────────────────────────────────────────
    private string _batchFolder = "", _batchAddFile = "", _batchStatus = "";
    private bool _batchPrepend = true, _batchAppend;
    public string BatchFolder  { get => _batchFolder;  set => SetProperty(ref _batchFolder,  value, nameof(BatchFolder)); }
    public string BatchAddFile { get => _batchAddFile; set => SetProperty(ref _batchAddFile, value, nameof(BatchAddFile)); }
    public string BatchStatus  { get => _batchStatus;  set => SetProperty(ref _batchStatus,  value, nameof(BatchStatus)); }
    public bool BatchPrepend   { get => _batchPrepend; set { SetProperty(ref _batchPrepend, value, nameof(BatchPrepend)); SetProperty(ref _batchAppend, !value, nameof(BatchAppend)); } }
    public bool BatchAppend    { get => _batchAppend;  set { SetProperty(ref _batchAppend,  value, nameof(BatchAppend));  SetProperty(ref _batchPrepend, !value, nameof(BatchPrepend)); } }
    public bool BatchUseSilence { get => _batchUseSilence; set => SetProperty(ref _batchUseSilence, value, nameof(BatchUseSilence)); }
    private bool _batchUseSilence;
    public ObservableCollection<string> BatchFiles { get; } = [];

    private bool _loading;
    public bool Loading { get => _loading; set => SetProperty(ref _loading, value, nameof(Loading)); }

    // ── Commands ─────────────────────────────────────────────────────────────
    public ICommand AddSimpleFileCommand    { get; }
    public ICommand RemoveSimpleFileCommand { get; }
    public ICommand MoveUpCommand           { get; }
    public ICommand MoveDownCommand         { get; }
    public ICommand BrowseOutputCommand     { get; }
    public ICommand RunSimpleCommand        { get; }
    public ICommand BrowseBatchFolderCommand { get; }
    public ICommand BrowseBatchAddCommand    { get; }
    public ICommand UseSilenceCommand        { get; }
    public ICommand RunBatchCommand          { get; }
    public ICommand? SwitchSimpleCommand    { get; set; }
    public ICommand? SwitchBatchCommand     { get; set; }

    private string _exeDir = "";

    public SoundCombineViewModel()
    {
        AddSimpleFileCommand = new Command(async () =>
        {
            var path = await AppFileDialog.OpenFileAsync("Select WAV file", ".wav", ".WAV");
            if (!string.IsNullOrEmpty(path)) SimpleFiles.Add(path);
        });

        RemoveSimpleFileCommand = new Command<string>(f => SimpleFiles.Remove(f));

        MoveUpCommand = new Command<string>(f =>
        {
            var i = SimpleFiles.IndexOf(f);
            if (i > 0) { SimpleFiles.RemoveAt(i); SimpleFiles.Insert(i - 1, f); }
        });

        MoveDownCommand = new Command<string>(f =>
        {
            var i = SimpleFiles.IndexOf(f);
            if (i >= 0 && i < SimpleFiles.Count - 1) { SimpleFiles.RemoveAt(i); SimpleFiles.Insert(i + 1, f); }
        });

        BrowseOutputCommand = new Command(async () =>
        {
            var path = await AppFileDialog.SaveFileAsync("Save combined WAV", "combined.wav", ".wav");
            if (!string.IsNullOrEmpty(path)) SimpleOutput = path;
        });

        RunSimpleCommand = new Command(async () => await RunSimple());

        BrowseBatchFolderCommand = new Command(async () =>
        {
            var folder = await AppFileDialog.BrowseFolderAsync("Select folder of WAV files");
            if (!string.IsNullOrEmpty(folder))
            {
                BatchFolder = folder;
                BatchFiles.Clear();
                foreach (var f in Directory.GetFiles(folder, "*.wav", SearchOption.TopDirectoryOnly)
                                           .Concat(Directory.GetFiles(folder, "*.WAV", SearchOption.TopDirectoryOnly))
                                           .OrderBy(x => x))
                    BatchFiles.Add(Path.GetFileName(f));
            }
        });

        BrowseBatchAddCommand = new Command(async () =>
        {
            var path = await AppFileDialog.OpenFileAsync("Select WAV to prepend/append", ".wav", ".WAV");
            if (!string.IsNullOrEmpty(path)) { BatchAddFile = path; BatchUseSilence = false; }
        });

        UseSilenceCommand = new Command(() =>
        {
            BatchAddFile = "(built-in 0.3s silence)";
            BatchUseSilence = true;
        });

        RunBatchCommand = new Command(async () => await RunBatch());
    }

    private async Task RunSimple()
    {
        if (SimpleFiles.Count < 2) { SimpleStatus = "Add at least 2 WAV files."; return; }
        if (string.IsNullOrWhiteSpace(SimpleOutput)) { SimpleStatus = "Set an output file path."; return; }

        Loading = true;
        SimpleStatus = "Working...";
        try
        {
            await Task.Run(() =>
            {
                var exeDir = GetExeDir();
                var exe    = Path.Combine(exeDir, "sndfile-concat.exe");
                var work   = Path.Combine(Path.GetTempPath(), $"sc_{Guid.NewGuid():N}");
                Directory.CreateDirectory(work);
                try
                {
                    // Copy all files into work dir
                    var copies = new List<string>();
                    for (int i = 0; i < SimpleFiles.Count; i++)
                    {
                        var dest = Path.Combine(work, $"{i:D4}_{Path.GetFileName(SimpleFiles[i])}");
                        File.Copy(SimpleFiles[i], dest, true);
                        copies.Add(dest);
                    }

                    // Chain: concat each subsequent file onto the first
                    for (int i = 1; i < copies.Count; i++)
                        RunExe(exe, $"\"{copies[0]}\" \"{copies[i]}\"", work);

                    // Copy result to output
                    var outDir = Path.GetDirectoryName(SimpleOutput);
                    if (!string.IsNullOrEmpty(outDir)) Directory.CreateDirectory(outDir);
                    File.Copy(copies[0], SimpleOutput, true);
                }
                finally { Directory.Delete(work, true); }
            });
            SimpleStatus = $"Done — saved to: {SimpleOutput}";
        }
        catch (Exception ex) { SimpleStatus = $"Error: {ex.Message}"; }
        finally { Loading = false; }
    }

    private async Task RunBatch()
    {
        if (string.IsNullOrWhiteSpace(BatchFolder)) { BatchStatus = "Select a folder of WAV files."; return; }
        if (string.IsNullOrWhiteSpace(BatchAddFile)) { BatchStatus = "Select a WAV to prepend/append (or use built-in silence)."; return; }

        var wavFiles = Directory.GetFiles(BatchFolder, "*.wav", SearchOption.TopDirectoryOnly)
                                .Concat(Directory.GetFiles(BatchFolder, "*.WAV", SearchOption.TopDirectoryOnly))
                                .OrderBy(x => x).ToList();
        if (wavFiles.Count == 0) { BatchStatus = "No WAV files found in that folder."; return; }

        Loading = true;
        BatchStatus = $"Processing {wavFiles.Count} file(s)...";
        var outDir = Path.Combine(BatchFolder, "combined");
        try
        {
            await Task.Run(() =>
            {
                var exeDir = GetExeDir();
                var exe    = Path.Combine(exeDir, "sndfile-concat.exe");
                Directory.CreateDirectory(outDir);

                // Resolve the "add" file
                string addSrc = BatchUseSilence
                    ? Path.Combine(exeDir, "silence03.WAV")
                    : BatchAddFile;

                int done = 0;
                foreach (var src in wavFiles)
                {
                    var outFile = Path.Combine(outDir, Path.GetFileName(src));
                    if (BatchPrepend)
                    {
                        // prepend: copy addSrc → outFile, then concat src onto it
                        File.Copy(addSrc, outFile, true);
                        RunExe(exe, $"\"{outFile}\" \"{src}\"", outDir);
                    }
                    else
                    {
                        // append: copy src → outFile, then concat addSrc onto it
                        File.Copy(src, outFile, true);
                        RunExe(exe, $"\"{outFile}\" \"{addSrc}\"", outDir);
                    }
                    done++;
                }
            });
            BatchStatus = $"Done — {wavFiles.Count} file(s) written to: {outDir}";
            BatchFiles.Clear();
            foreach (var f in wavFiles) BatchFiles.Add(Path.GetFileName(f));
        }
        catch (Exception ex) { BatchStatus = $"Error: {ex.Message}"; }
        finally { Loading = false; }
    }

    private static void RunExe(string exe, string args, string workDir)
    {
        var psi = new ProcessStartInfo(exe, args)
        {
            WorkingDirectory = workDir,
            UseShellExecute  = false,
            CreateNoWindow   = true,
            RedirectStandardError = true
        };
        using var p = Process.Start(psi) ?? throw new Exception("Failed to start sndfile-concat.exe");
        p.WaitForExit();
        if (p.ExitCode != 0)
        {
            var err = p.StandardError.ReadToEnd();
            throw new Exception($"sndfile-concat exited {p.ExitCode}: {err}");
        }
    }

    private string GetExeDir()
    {
        if (!string.IsNullOrEmpty(_exeDir)) return _exeDir;

        // Extract bundled binaries to temp folder on first use
        var dir = Path.Combine(Path.GetTempPath(), "SupportToolSoundCombine");
        Directory.CreateDirectory(dir);
        var files = new[] { "sndfile-concat.exe", "libsndfile.dll", "msvcp71.dll", "msvcr71.dll", "silence03.WAV" };
        foreach (var f in files)
        {
            var dest = Path.Combine(dir, f);
            if (!File.Exists(dest))
            {
                using var stream = FileSystem.OpenAppPackageFileAsync($"SoundCombine/{f}").GetAwaiter().GetResult();
                using var fs = File.Create(dest);
                stream.CopyTo(fs);
            }
        }
        _exeDir = dir;
        return dir;
    }
}
