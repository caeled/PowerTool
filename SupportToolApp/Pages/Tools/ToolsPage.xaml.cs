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
            { TabSchedTasks,      PanelSchedTasks },
            { TabActivation,      PanelActivation },
            { TabTempClean,       PanelTempClean },
            { TabStartup,         PanelStartup },
            { TabGroupMembership, PanelGroupMembership },
            { TabAcctLockout,     PanelAcctLockout },
            { TabRdpSessions,     PanelRdpSessions },
            { TabPwdExpiry,       PanelPwdExpiry },
            { TabUserMgmt,        PanelUserMgmt },
            { TabDns,             PanelDns },
            { TabFlushDns,        PanelFlushDns },
            { TabNetStat,         PanelNetStat },
            { TabTrace,           PanelTrace },
            { TabWifi,            PanelWifi },
            { TabProxy,           PanelProxy },
            { TabResetNic,        PanelResetNic },
            { TabRegistry,        PanelRegistry },
            { TabGpUpdate,        PanelGpUpdate },
            { TabCopyFile,        PanelCopyFile },
            { TabWinget,         PanelWinget },
            { TabRegPush,        PanelRegPush },
            { TabPendingUpdates, PanelPendingUpdates },
            { TabRunUpdates,     PanelRunUpdates },
            { TabUpdateLog,      PanelUpdateLog },
            { TabRemotePsSystem,  PanelRemotePs },
            { TabRemotePsRemote,  PanelRemotePs },
            { TabPsRunSystem,     PanelPsRun },
            { TabPsRunRemote,     PanelPsRun },
            { TabDirBrowser,      PanelDirBrowser },
            { TabProcKill,        PanelProcKill },
            { TabWinRmToggle,     PanelWinRmToggle },
            { TabWakeOnLan,       PanelWakeOnLan },
            { TabPing,            PanelPing },
            { TabWifiSetup,       PanelWifiSetup },
            { TabDhcpStats,       PanelDhcpStats },
            { TabPowerCopy,       PanelPowerCopy },
            { TabAutoLogon,       PanelAutoLogon },
            { TabSetPrinter,      PanelSetPrinter },
            { TabTrustedSites,    PanelTrustedSites },
            { TabQuickAccess,     PanelQuickAccess },
            { TabWinKey,          PanelWinKey },
            { TabWsusClient,      PanelWsusClient },
            { TabHostsFile,       PanelHostsFile },
            { TabAdReset,         PanelAdReset },
            { TabDiskCleanup,     PanelDiskCleanup },
            { TabKeepAlive,       PanelKeepAlive },
        };

        _categories = new()
        {
            { CatSystem,  [TabApps, TabServices, TabUpdates, TabPatches, TabProcesses, TabPrinters, TabDiskSpace, TabSchedTasks, TabActivation, TabTempClean, TabStartup, TabRemotePsSystem, TabPsRunSystem, TabAutoLogon, TabSetPrinter, TabTrustedSites, TabQuickAccess, TabDiskCleanup, TabKeepAlive] },
            { CatUsers,   [TabLocalUsers, TabLoggedOn, TabLocalAdmins, TabRunAs, TabGroupMembership, TabAcctLockout, TabRdpSessions, TabPwdExpiry, TabUserMgmt, TabAdReset] },
            { CatNetwork, [TabNetwork, TabPorts, TabPingSweep, TabDns, TabFlushDns, TabNetStat, TabTrace, TabWifi, TabProxy, TabResetNic, TabPing, TabWifiSetup, TabDhcpStats] },
            { CatRemote,  [TabReboot, TabRebootAction, TabEventLog, TabShares, TabRegistry, TabGpUpdate, TabRemotePsRemote, TabPsRunRemote, TabDirBrowser, TabProcKill, TabWinRmToggle, TabWakeOnLan, TabHostsFile] },
            { CatDeploy,  [TabDeploy, TabCopyFile, TabWinget, TabRegPush, TabPendingUpdates, TabRunUpdates, TabUpdateLog, TabWinKey, TabWsusClient, TabPowerCopy] },
        };

        SelectCategory(CatSystem, TabApps);
    }

    private void OnCategoryClicked(object? sender, EventArgs e)
    {
        if (sender is not Button cat || !_categories.TryGetValue(cat, out var tools)) return;
        SelectCategory(cat, tools[0]);
    }

    private void OnTabClicked(object? sender, EventArgs e)
    {
        if (sender is Button btn) SelectTab(btn);
    }

    private void SelectCategory(Button cat, Button firstTab)
    {
        // Highlight category buttons
        foreach (var c in _categories.Keys)
        {
            var active = c == cat;
            c.BackgroundColor = active ? Color.FromArgb("#0078D4") : Colors.White;
            c.TextColor        = active ? Colors.White : Colors.Black;
        }
        _activeCategory = cat;

        // Show only this category's tool buttons, hide the rest
        var visible = _categories[cat];
        foreach (var (btn, _) in _tabs)
            btn.IsVisible = visible.Contains(btn);

        SelectTab(firstTab);
    }

    private void SelectTab(Button tab)
    {
        // Hide all panels first — handles shared-panel case where two tabs map to the same panel
        foreach (var panel in _tabs.Values.Distinct())
            panel.IsVisible = false;

        foreach (var (btn, panel) in _tabs)
        {
            var isActive = btn == tab;
            if (btn.IsVisible)
            {
                btn.BackgroundColor = isActive ? Color.FromArgb("#0078D4") : Colors.White;
                btn.TextColor        = isActive ? Colors.White : Colors.Black;
            }
            if (isActive) panel.IsVisible = true;
        }
        _activeTab = tab;
    }

    private void OnEventLogPreset(object? sender, EventArgs e)
    {
        if (_vm == null || sender is not Button btn) return;
        _vm.EventLogName = btn.Text;
    }

}
