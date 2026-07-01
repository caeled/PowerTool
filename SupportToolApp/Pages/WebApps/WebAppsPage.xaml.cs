using SupportToolApp.Config;

namespace SupportToolApp.Pages.WebApps;

public partial class WebAppsPage : ContentPage
{
    private bool _editMode = false;

    // Rotate through these colors when the user adds a new app
    private static readonly string[] _palette =
    [
        "#0078D4", "#4285F4", "#7B2FBE", "#228B22",
        "#D4500A", "#C2185B", "#00796B", "#5D4037"
    ];

    public WebAppsPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        Rebuild();
    }

    // ── Build / rebuild the button grid ──────────────────────────────────────

    private void Rebuild()
    {
        ButtonPanel.Children.Clear();

        foreach (var app in AppConfig.Instance.WebApps)
            ButtonPanel.Children.Add(MakeCard(app));

        if (_editMode)
            ButtonPanel.Children.Add(MakeAddPlaceholder());
    }

    private View MakeCard(WebAppEntry app)
    {
        var btn = new Button
        {
            Text              = app.Name,
            BackgroundColor   = Color.FromArgb(app.Color),
            TextColor         = Colors.White,
            FontAttributes    = FontAttributes.Bold,
            FontSize          = 15,
            Padding           = new Thickness(20, 14),
            Margin            = new Thickness(6),
            CornerRadius      = 10,
            MinimumWidthRequest = 140,
        };
        btn.Clicked += (_, _) => LaunchUrl(app.Url);

        if (!_editMode)
            return btn;

        // In edit mode wrap button + remove X in a grid
        var remove = new Button
        {
            Text            = "✕",
            BackgroundColor = Color.FromArgb("#CC3333"),
            TextColor       = Colors.White,
            FontSize        = 11,
            Padding         = new Thickness(4, 2),
            CornerRadius    = 12,
            WidthRequest    = 24,
            HeightRequest   = 24,
            HorizontalOptions = LayoutOptions.End,
            VerticalOptions   = LayoutOptions.Start,
            Margin            = new Thickness(0, 2, 2, 0),
            ZIndex            = 1,
        };
        remove.Clicked += (_, _) => RemoveApp(app);

        return new Grid
        {
            Margin   = new Thickness(6),
            Children = { btn, remove },
        };
    }

    private View MakeAddPlaceholder()
    {
        var btn = new Button
        {
            Text              = "+ Add",
            BackgroundColor   = Color.FromArgb("#444"),
            TextColor         = Colors.White,
            FontAttributes    = FontAttributes.Bold,
            FontSize          = 15,
            Padding           = new Thickness(20, 14),
            Margin            = new Thickness(6),
            CornerRadius      = 10,
            MinimumWidthRequest = 140,
        };
        btn.Clicked += (_, _) =>
        {
            EntryName.Focus();
        };
        return btn;
    }

    // ── Edit toggle ───────────────────────────────────────────────────────────

    private void OnEditToggle(object? sender, EventArgs e)
    {
        _editMode = !_editMode;
        BtnEdit.Text            = _editMode ? "Done" : "Edit";
        BtnEdit.BackgroundColor = _editMode ? Color.FromArgb("#228B22") : Color.FromArgb("#555");
        AddForm.IsVisible       = _editMode;
        Rebuild();
    }

    // ── Add ──────────────────────────────────────────────────────────────────

    private void OnAddApp(object? sender, EventArgs e)
    {
        var name = EntryName.Text?.Trim();
        var url  = EntryUrl.Text?.Trim();

        if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(url))
            return;

        if (!url.Contains("://"))
            url = "https://" + url;

        var color = _palette[AppConfig.Instance.WebApps.Count % _palette.Length];
        AppConfig.Instance.WebApps.Add(new WebAppEntry { Name = name, Url = url, Color = color });
        AppConfig.Instance.Save();

        EntryName.Text = "";
        EntryUrl.Text  = "";
        Rebuild();
    }

    // ── Remove ───────────────────────────────────────────────────────────────

    private void RemoveApp(WebAppEntry app)
    {
        AppConfig.Instance.WebApps.Remove(app);
        AppConfig.Instance.Save();
        Rebuild();
    }

    // ── Launch ───────────────────────────────────────────────────────────────

    private static void LaunchUrl(string url)
    {
        try { Launcher.Default.OpenAsync(url); }
        catch { }
    }
}
