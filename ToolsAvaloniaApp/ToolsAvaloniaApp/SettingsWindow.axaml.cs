using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using ToolsAvaloniaApp.Models;

namespace ToolsAvaloniaApp;

public partial class SettingsWindow : Window
{
    private readonly QuickLinksConfig _config;
    private readonly TextBox[]        _nameBoxes = new TextBox[4];
    private readonly TextBox[]        _urlBoxes  = new TextBox[4];
    private readonly TextBlock[]      _errors    = new TextBlock[4];
    private TextBox?   _rdpBox;
    private CheckBox?  _newWindowBox;

    public SettingsWindow()
    {
        InitializeComponent();
        _config = QuickLinksConfig.Load();
        BuildForm();
    }

    private void BuildForm()
    {
        var panel = this.FindControl<StackPanel>("LinksPanel")!;

        // RDP default
        var rdpRow = new StackPanel { Spacing = 4, Margin = new Avalonia.Thickness(0, 0, 0, 8) };
        rdpRow.Children.Add(new TextBlock { Text = "RDP Default Host", Classes = { "label" } });
        _rdpBox = new TextBox { Text = _config.RdpDefault, Watermark = "hostname or IP" };
        rdpRow.Children.Add(_rdpBox);
        panel.Children.Add(rdpRow);

        // Separator
        panel.Children.Add(new Border
        {
            BorderBrush     = Brushes.Gray,
            BorderThickness = new Avalonia.Thickness(0, 0, 0, 1),
            Margin          = new Avalonia.Thickness(0, 4, 0, 8),
            Opacity         = 0.3
        });

        // Open in new window
        _newWindowBox = new CheckBox
        {
            Content    = "Open links in new window",
            IsChecked  = _config.NewWindow,
            Foreground = Brushes.White,
            Margin     = new Avalonia.Thickness(0, 0, 0, 12)
        };
        panel.Children.Add(_newWindowBox);

        // Separator
        panel.Children.Add(new Border
        {
            BorderBrush     = Brushes.Gray,
            BorderThickness = new Avalonia.Thickness(0, 0, 0, 1),
            Margin          = new Avalonia.Thickness(0, 0, 0, 12),
            Opacity         = 0.3
        });

        // Quick link rows
        for (int i = 0; i < 4; i++)
        {
            var link    = _config.Links[i];
            var nameBox = new TextBox { Text = link.Name, Watermark = "Button name" };
            var urlBox  = new TextBox { Text = link.Url,  Watermark = "https://..." };
            var error   = new TextBlock { Foreground = Brushes.OrangeRed, FontSize = 11, IsVisible = false };

            _nameBoxes[i] = nameBox;
            _urlBoxes[i]  = urlBox;
            _errors[i]    = error;

            var row    = new StackPanel { Spacing = 4 };
            row.Children.Add(new TextBlock { Text = $"Button {i + 1}", Classes = { "label" } });
            var fields = new Grid { ColumnDefinitions = ColumnDefinitions.Parse("*,12,2*") };
            Grid.SetColumn(nameBox, 0);
            Grid.SetColumn(urlBox,  2);
            fields.Children.Add(nameBox);
            fields.Children.Add(urlBox);
            row.Children.Add(fields);
            row.Children.Add(error);
            panel.Children.Add(row);
        }
    }

    private void Save_Click(object? sender, RoutedEventArgs e)
    {
        bool valid = true;
        _config.RdpDefault = _rdpBox?.Text?.Trim() ?? "";
        _config.NewWindow  = _newWindowBox?.IsChecked == true;

        for (int i = 0; i < 4; i++)
        {
            var name = _nameBoxes[i].Text?.Trim() ?? "";
            var url  = _urlBoxes[i].Text?.Trim()  ?? "";
            _errors[i].IsVisible = false;

            if (string.IsNullOrEmpty(name) && string.IsNullOrEmpty(url))
            {
                _config.Links[i] = new QuickLink();
                continue;
            }

            if (!string.IsNullOrEmpty(url) && !url.Contains("://"))
            {
                _errors[i].Text      = "URL must contain :// (e.g. https:// or rdp://)";
                _errors[i].IsVisible = true;
                valid = false;
                continue;
            }

            _config.Links[i] = new QuickLink { Name = name, Url = url };
        }

        if (!valid) return;
        _config.Save();
        Close(_config);
    }

    private void Cancel_Click(object? sender, RoutedEventArgs e) => Close(null);
}
