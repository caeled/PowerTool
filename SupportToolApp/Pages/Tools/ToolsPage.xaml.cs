using SupportToolApp.ViewModels.Tools;

namespace SupportToolApp.Pages.Tools;

public partial class ToolsPage : ContentPage
{
    private readonly Dictionary<Button, Grid> _tabs;
    private Button? _activeTab;
    private Button? _activeCategory;
    private ToolsViewModel? _vm;

    // Which tool buttons belong to each category
    private readonly Dictionary<Button, Button[]> _categories;

    public ToolsPage(ToolsViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
        _vm = vm;

        _tabs = new()
        {
            { TabApps,            PanelApps },
            { TabServices,        PanelServices },
            { TabLocalUsers,      PanelLocalUsers },
            { TabPrinters,        PanelPrinters },
            { TabReboot,          PanelReboot },
            { TabPorts,           PanelPorts },
            { TabUpdates,         PanelUpdates },
            { TabNetwork,         PanelNetwork },
            { TabLoggedOn,        PanelLoggedOn },
            { TabLocalAdmins,     PanelLocalAdmins },
            { TabPatches,         PanelPatches },
            { TabProcesses,       PanelProcesses },
            { TabRebootAction,    PanelRebootAction },
            { TabPingSweep,       PanelPingSweep },
            { TabRunAs,           PanelRunAs },
            { TabEventLog,        PanelEventLog },
            { TabShares,          PanelShares },
            { TabDeploy,          PanelDeploy },
            { TabDiskSpace,       PanelDiskSpace },
            { TabGroupMembership, PanelGroupMembership },
            { TabDns,             PanelDns },
            { TabRegistry,        PanelRegistry },
            { TabGpUpdate,        PanelGpUpdate },
            { TabCopyFile,        PanelCopyFile },
        };

        _categories = new()
        {
            { CatSystem,  [TabApps, TabServices, TabUpdates, TabPatches, TabProcesses, TabPrinters, TabDiskSpace] },
            { CatUsers,   [TabLocalUsers, TabLoggedOn, TabLocalAdmins, TabRunAs, TabGroupMembership] },
            { CatNetwork, [TabNetwork, TabPorts, TabPingSweep, TabDns] },
            { CatRemote,  [TabReboot, TabRebootAction, TabEventLog, TabShares, TabRegistry, TabGpUpdate] },
            { CatDeploy,  [TabDeploy, TabCopyFile] },
        };

        SelectCategory(CatSystem, TabApps);
    }

    private void OnCategoryClicked(object sender, EventArgs e)
    {
        if (sender is not Button cat || !_categories.TryGetValue(cat, out var tools)) return;
        SelectCategory(cat, tools[0]);
    }

    private void OnTabClicked(object sender, EventArgs e)
    {
        if (sender is Button btn) SelectTab(btn);
    }

    private void SelectCategory(Button cat, Button firstTab)
    {
        // Highlight category buttons
        foreach (var c in _categories.Keys)
            c.BackgroundColor = Color.FromArgb(c == cat ? "#0078D4" : "#555555");
        _activeCategory = cat;

        // Show only this category's tool buttons, hide the rest
        var visible = _categories[cat];
        foreach (var (btn, _) in _tabs)
            btn.IsVisible = visible.Contains(btn);

        SelectTab(firstTab);
    }

    private void SelectTab(Button tab)
    {
        foreach (var (btn, panel) in _tabs)
        {
            var isActive = btn == tab;
            if (btn.IsVisible)
                btn.BackgroundColor = isActive ? Color.FromArgb("#0078D4") : Color.FromArgb("#555555");
            panel.IsVisible = isActive;
        }
        _activeTab = tab;
    }

    private void OnEventLogPreset(object sender, EventArgs e)
    {
        if (_vm == null || sender is not Button btn) return;
        _vm.EventLogName = btn.Text;
    }
}
