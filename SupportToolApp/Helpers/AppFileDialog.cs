namespace SupportToolApp.Helpers;

/// <summary>
/// Cross-platform file/folder dialogs.
/// On Windows (elevated) uses Win32 P/Invoke because WinRT pickers fail under requireAdministrator.
/// On Mac/Linux uses MAUI FilePicker/FolderPicker natively.
/// </summary>
internal static class AppFileDialog
{
    public static async Task<string?> OpenFileAsync(string title, params string[] extensions)
    {
#if WINDOWS
        var filter = extensions.Length > 0
            ? $"{title}\0{string.Join(";", extensions.Select(e => $"*{e}"))}\0All Files\0*.*\0\0"
            : "All Files\0*.*\0\0";
        return await Task.Run(() => Win32FileDialog.OpenFile(title, filter));
#else
        var options = new PickOptions
        {
            PickerTitle = title,
            FileTypes   = extensions.Length > 0
                ? new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
                    {
                        { DevicePlatform.macOS,  extensions },
                        { DevicePlatform.iOS,    extensions },
                        { DevicePlatform.Android,extensions },
                        { DevicePlatform.WinUI,  extensions },
                    })
                : null
        };
        var result = await FilePicker.Default.PickAsync(options);
        return result?.FullPath;
#endif
    }

    public static async Task<string?> SaveFileAsync(string title, string suggestedName, string extension)
    {
#if WINDOWS
        var filter = $"{title}\0*{extension}\0All Files\0*.*\0\0";
        return await Task.Run(() => Win32FileDialog.SaveFile(title, suggestedName, filter, extension.TrimStart('.')));
#else
        // MAUI has no native save dialog on all platforms yet — prompt user for path via entry
        // For now fall back to Documents folder with suggested name
        var docsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        return Path.Combine(docsPath, suggestedName);
#endif
    }

    public static async Task<string?> BrowseFolderAsync(string title)
    {
#if WINDOWS
        return await Task.FromResult(Win32FileDialog.BrowseFolder(title));
#else
        var result = await FolderPicker.Default.PickAsync(CancellationToken.None);
        return result?.Folder?.Path;
#endif
    }
}
