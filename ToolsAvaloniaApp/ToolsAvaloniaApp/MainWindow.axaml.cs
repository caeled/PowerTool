using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using ToolsAvaloniaApp.ViewModels;

namespace ToolsAvaloniaApp;

public partial class MainWindow : Window
{
    private Button[] _qlButtons = [];

    public MainWindow()
    {
        InitializeComponent();
        DataContextChanged += (_, _) => WireViewModel();
        WireViewModel();
    }

    private void WireViewModel()
    {
        if (DataContext is not MainViewModel vm) return;
        _qlButtons = [
            this.FindControl<Button>("Ql0")!,
            this.FindControl<Button>("Ql1")!,
            this.FindControl<Button>("Ql2")!,
            this.FindControl<Button>("Ql3")!,
        ];
        vm.OnSettingsRequested += OpenSettings;
        vm.OnRdpRequested      += OpenRdp;
        vm.PropertyChanged     += (_, e) => { if (e.PropertyName == nameof(vm.QuickLinks)) RefreshQuickLinks(vm); };
        RefreshQuickLinks(vm);
    }

    private void RefreshQuickLinks(MainViewModel vm)
    {
        for (int i = 0; i < 4; i++)
        {
            var btn  = _qlButtons[i];
            var link = vm.QuickLinks.Links[i];
            if (link.IsConfigured)
            {
                btn.Content = link.Name;
                btn.Classes.Remove("ql-empty");
                btn.Classes.Add("ql");
                ToolTip.SetTip(btn, link.Url);
            }
            else
            {
                btn.Content = $"Link {i + 1}";
                btn.Classes.Remove("ql");
                btn.Classes.Add("ql-empty");
                ToolTip.SetTip(btn, "Configure in File → Settings");
            }
        }
    }

    private async void OpenSettings()
    {
        if (DataContext is not MainViewModel vm) return;
        try
        {
            var win    = new SettingsWindow();
            var result = await win.ShowDialog<object?>(this);
            if (result != null) vm.ReloadQuickLinks();
        }
        catch (Exception ex)
        {
            await new Window
            {
                Width = 500, Height = 200,
                Content = new Avalonia.Controls.TextBox
                {
                    Text = ex.ToString(), IsReadOnly = true, AcceptsReturn = true, TextWrapping = Avalonia.Media.TextWrapping.Wrap
                }
            }.ShowDialog(this);
        }
    }

    private async void OpenRdp(string defaultHost)
    {
        var win    = new RdpDialog(defaultHost);
        var result = await win.ShowDialog<string?>(this);
        if (string.IsNullOrWhiteSpace(result)) return;

        var host = result.Trim();
        if (OperatingSystem.IsWindows())
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName        = "mstsc.exe",
                Arguments       = $"/v:{host}",
                UseShellExecute = false
            });
        }
        else if (OperatingSystem.IsMacOS())
        {
            var rdpFile = Path.Combine(Path.GetTempPath(), $"connect-{host}.rdp");
            File.WriteAllText(rdpFile, $"full address:s:{host}\nprompt for credentials:i:1\n");
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName        = "open",
                Arguments       = $"\"{rdpFile}\"",
                UseShellExecute = false
            });
        }
        else
        {
            // Linux — escape flatpak sandbox via flatpak-spawn, fall back to direct exec
            var launched = false;
            foreach (var (exe, args) in new[]
            {
                ("flatpak-spawn", $"--host remmina -c rdp://{host}"),
                ("flatpak-spawn", $"--host xdg-open rdp://{host}"),
                ("remmina",       $"-c rdp://{host}"),
                ("xdg-open",      $"rdp://{host}"),
            })
            {
                try
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName        = exe,
                        Arguments       = args,
                        UseShellExecute = false,
                        CreateNoWindow  = true
                    });
                    launched = true;
                    break;
                }
                catch { }
            }
        }
    }

    private async void OnBrowseP1(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not MainViewModel vm) return;
        var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title         = "Select Source File",
            AllowMultiple = false
        });
        if (files.Count > 0)
            vm.P1 = files[0].Path.LocalPath;
    }

    private async void OnBrowseP2(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not MainViewModel vm) return;
        var folders = await StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = "Select Destination Folder"
        });
        if (folders.Count > 0)
            vm.P2 = folders[0].Path.LocalPath + System.IO.Path.DirectorySeparatorChar;
    }
}
