using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using ToolsAvaloniaApp.ViewModels;

namespace ToolsAvaloniaApp;

public partial class MainWindow : Window
{
    public MainWindow() => InitializeComponent();

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
        // Try folder picker first; if user wants a file destination they can type it
        var folders = await StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = "Select Destination Folder"
        });
        if (folders.Count > 0)
            vm.P2 = folders[0].Path.LocalPath + System.IO.Path.DirectorySeparatorChar;
    }
}
