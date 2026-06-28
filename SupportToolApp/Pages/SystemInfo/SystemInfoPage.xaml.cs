using SupportToolApp.ViewModels;

namespace SupportToolApp.Pages.SystemInfo;

public partial class SystemInfoPage : ContentPage
{
    private readonly Dictionary<Button, View> _tabs;

    public SystemInfoPage(SystemInfoViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;

        _tabs = new()
        {
            { TabOverview, PanelOverview },
            { TabHardware, PanelHardware },
            { TabDisks,    PanelDisks    },
            { TabNetwork,  PanelNetwork  },
            { TabSoftware, PanelSoftware },
            { TabSecurity, PanelSecurity },
        };
        SelectTab(TabOverview);
    }

    private void OnTabClicked(object sender, EventArgs e)
    {
        if (sender is Button btn) SelectTab(btn);
    }

    private void SelectTab(Button tab)
    {
        foreach (var (btn, panel) in _tabs)
        {
            btn.BackgroundColor = btn == tab ? Color.FromArgb("#0078D4") : Color.FromArgb("#555555");
            panel.IsVisible = btn == tab;
        }
    }
}
