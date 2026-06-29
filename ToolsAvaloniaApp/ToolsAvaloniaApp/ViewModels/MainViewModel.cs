using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using ToolsAvaloniaApp.Helpers;

namespace ToolsAvaloniaApp.ViewModels;

public class MainViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    private void Set<T>(ref T field, T value, [CallerMemberName] string? name = null)
    {
        if (Equals(field, value)) return;
        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    private string _target = "";
    private string _username = "";
    private string _password = "";
    private string _output = "";
    private bool _loading;

    public string Target   { get => _target;   set => Set(ref _target,   value); }
    public string Username { get => _username;  set => Set(ref _username, value); }
    public string Password { get => _password;  set => Set(ref _password, value); }
    public string Output   { get => _output;    set => Set(ref _output,   value); }
    public bool   Loading  { get => _loading;   set => Set(ref _loading,  value); }

    public ICommand LoadAppsCommand      { get; }
    public ICommand LoadServicesCommand  { get; }
    public ICommand LoadProcessesCommand { get; }
    public ICommand LoadDiskCommand      { get; }
    public ICommand LoadPatchesCommand   { get; }
    public ICommand ClearCommand         { get; }

    public MainViewModel()
    {
        LoadAppsCommand      = new RelayCommand(async () => await Run(AppsScript()));
        LoadServicesCommand  = new RelayCommand(async () => await Run(ServicesScript()));
        LoadProcessesCommand = new RelayCommand(async () => await Run(ProcessesScript()));
        LoadDiskCommand      = new RelayCommand(async () => await Run(DiskScript()));
        LoadPatchesCommand   = new RelayCommand(async () => await Run(PatchesScript()));
        ClearCommand         = new RelayCommand(() => { Target = ""; Username = ""; Password = ""; Output = ""; });
    }

    private async Task Run(string script)
    {
        if (Loading) return;
        Loading = true;
        Output = "Running...";
        try   { Output = await PsRunner.RunAsync(script); }
        catch (Exception ex) { Output = $"Error: {ex.Message}"; }
        finally { Loading = false; }
    }

    private bool HasCreds => !string.IsNullOrWhiteSpace(_username) && !string.IsNullOrWhiteSpace(_password);
    private bool IsRemote => !string.IsNullOrWhiteSpace(_target);

    private string CredBlock()
    {
        if (!HasCreds) return "";
        return $"""
$secpw = ConvertTo-SecureString '{_password.Replace("'", "''")}' -AsPlainText -Force
$cred  = New-Object -TypeName System.Management.Automation.PSCredential -ArgumentList '{_username.Replace("'", "''")}', $secpw

""";
    }

    private string Wrap(string cmd)
    {
        if (!IsRemote) return cmd;
        var cred = HasCreds ? " -Credential $cred" : "";
        return $"{CredBlock()}Invoke-Command -ComputerName '{_target.Trim()}'{cred} -ScriptBlock {{ {cmd} }}";
    }

    private string AppsScript() => Wrap(@"
Get-ItemProperty 'HKLM:\Software\Microsoft\Windows\CurrentVersion\Uninstall\*',
    'HKLM:\Software\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall\*' |
Where-Object { $_.DisplayName } |
Select-Object DisplayName, DisplayVersion, Publisher |
Sort-Object DisplayName |
Format-Table -AutoSize | Out-String -Width 200");

    private string ServicesScript() => Wrap(@"
Get-Service | Sort-Object Status, Name |
Select-Object Name, DisplayName, Status, StartType |
Format-Table -AutoSize | Out-String -Width 200");

    private string ProcessesScript() => Wrap(@"
Get-Process | Sort-Object CPU -Descending | Select-Object -First 50 |
Select-Object Name, Id,
    @{N='CPU';E={[math]::Round($_.CPU,1)}},
    @{N='Mem MB';E={[math]::Round($_.WorkingSet/1MB,1)}} |
Format-Table -AutoSize | Out-String -Width 200");

    private string DiskScript() => Wrap(@"
Get-PSDrive -PSProvider FileSystem |
Select-Object Name,
    @{N='Used GB';E={[math]::Round($_.Used/1GB,2)}},
    @{N='Free GB';E={[math]::Round($_.Free/1GB,2)}},
    @{N='Total GB';E={[math]::Round(($_.Used+$_.Free)/1GB,2)}} |
Format-Table -AutoSize | Out-String -Width 200");

    private string PatchesScript() => Wrap(@"
Get-HotFix | Sort-Object InstalledOn -Descending | Select-Object -First 50 |
Select-Object HotFixID, Description, InstalledOn, InstalledBy |
Format-Table -AutoSize | Out-String -Width 200");
}

public class RelayCommand : ICommand
{
    private readonly Func<Task>? _async;
    private readonly Action?     _sync;
    public event EventHandler? CanExecuteChanged;
    public RelayCommand(Func<Task> a) { _async = a; }
    public RelayCommand(Action a)     { _sync  = a; }
    public bool CanExecute(object? _) => true;
    public void Execute(object? _)
    {
        if (_async != null) _ = _async();
        else _sync?.Invoke();
    }
}