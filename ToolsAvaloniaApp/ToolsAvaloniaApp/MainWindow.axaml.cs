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
        var win    = new SettingsWindow { Owner = this };
        var result = await win.ShowDialog<object?>(this);
        if (result != null) vm.ReloadQuickLinks();
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
