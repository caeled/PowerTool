using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace ToolsAvaloniaApp;

public partial class RdpDialog : Window
{
    public RdpDialog(string defaultHost)
    {
        InitializeComponent();
        var box = this.FindControl<TextBox>("HostBox")!;
        box.Text = defaultHost;
        Opened += (_, _) => { box.Focus(); box.SelectAll(); };
    }

    private void Connect_Click(object? sender, RoutedEventArgs e)
    {
        var host = this.FindControl<TextBox>("HostBox")?.Text?.Trim() ?? "";
        if (!string.IsNullOrEmpty(host)) Close(host);
    }

    private void Cancel_Click(object? sender, RoutedEventArgs e) => Close(null);

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter) Connect_Click(null, null!);
        if (e.Key == Key.Escape) Cancel_Click(null, null!);
    }
}
