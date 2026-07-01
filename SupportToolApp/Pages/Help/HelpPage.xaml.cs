namespace SupportToolApp.Pages.Help;

public partial class HelpPage : ContentPage
{
    private record HelpTopic(string Title, string Category, string Body);

    private readonly List<HelpTopic> _topics =
    [
        new("System Info", "General",
            "Enter a computer name (or leave blank for this machine) then click Query. Pulls Overview, Hardware, Disks, Network, Software, Security, and Services into separate tabs. Use Export to save a full report as a .txt file.\n\nRecent Logins: On the Overview tab, enter a count and click Load to pull the last N login events from the Security event log."),

        new("PS Runner", "System",
            "Paste any PowerShell script into the editor and click Run. Works against a local or remote machine — enter a target computer name and supply credentials if needed. Output appears in the results panel below."),

        new("Installed Apps", "System",
            "Lists all software installed on the target machine via the registry. Shows name, version, publisher, and install date. Filterable by typing in the search box."),

        new("Services", "System",
            "Lists all Windows services on the target machine. Shows service name, display name, state (Running/Stopped), start mode, and the account it runs under. Filterable by name."),

        new("Updates", "System",
            "Lists installed Windows updates on the target machine including KB number, title, and install date."),

        new("Security Patches", "System",
            "Lists security-specific updates and hotfixes applied to the target machine."),

        new("Processes", "System",
            "Shows currently running processes on the target machine with PID, CPU, and memory usage."),

        new("Disk Space", "System",
            "Reports disk capacity, used space, and free space for all drives on the target machine."),

        new("Scheduled Tasks", "System",
            "Lists scheduled tasks configured on the target machine including status and last run time."),

        new("Activation", "System",
            "Checks Windows activation status on the target machine."),

        new("Temp Cleanup", "System",
            "Clears temporary files from the target machine to free up disk space."),

        new("Startup Programs", "System",
            "Lists programs configured to launch at startup on the target machine."),

        new("Account Lockout", "Users",
            "Check whether an Active Directory user account is locked out. Enter the username and click Check. If locked, click Unlock to clear the lockout."),

        new("RDP Sessions", "Users",
            "Lists active and disconnected Remote Desktop sessions on the target machine. Shows username, session ID, and state."),

        new("Password Expiry", "Users",
            "Shows when an AD user's password is set to expire based on the domain password policy and the account's last password set date."),

        new("User Management", "Users",
            "View and manage local user accounts on the target machine. Lists accounts with enabled/disabled status."),

        new("Local Users & Groups", "Users",
            "Lists local user accounts and group memberships on the target machine."),

        new("Logged On User", "Users",
            "Shows who is currently logged on interactively to the target machine."),

        new("Local Admins", "Users",
            "Lists members of the local Administrators group on the target machine."),

        new("Flush DNS", "Network",
            "Runs ipconfig /flushdns on the target machine to clear the DNS resolver cache. Useful when a machine is resolving a stale or incorrect address."),

        new("NetStat", "Network",
            "Displays active network connections and listening ports (netstat -ano) on the target machine. Shows local/remote address, state, and the owning process ID."),

        new("Traceroute", "Network",
            "Traces the network path from the target machine to a hostname or IP address. Shows each hop and its latency."),

        new("WiFi Profiles", "Network",
            "Lists saved WiFi networks on the target machine including the stored password (cleartext) for each profile."),

        new("Proxy", "Network",
            "Reads the current proxy server configuration from the registry on the target machine."),

        new("Reset NIC", "Network",
            "Disables then re-enables the primary network adapter on the target machine. Use this to recover from a dropped network connection without rebooting."),

        new("Ping", "Network",
            "Pings a hostname or IP from the target machine and reports round-trip latency and packet loss."),

        new("WiFi Setup", "Network",
            "Connects the target machine to a specified WiFi network using a provided SSID and password."),

        new("DNS", "Network",
            "Looks up DNS records for a hostname or IP address."),

        new("Ping Sweep", "Network",
            "Scans a range of IP addresses to find which hosts are online."),

        new("Port Tester", "Network",
            "Tests whether a specific TCP port is open and reachable on a target host."),

        new("Dir Browser", "Remote",
            "Browse the file system on a remote machine by UNC path. Navigate folders and inspect directory contents without opening a full RDP session."),

        new("Process Kill", "Remote",
            "Lists running processes on a remote machine. Select a process and click Kill to terminate it."),

        new("WinRM Toggle", "Remote",
            "Enables or disables Windows Remote Management (WinRM) on the target machine. WinRM must be enabled for most remote tools in this app to function."),

        new("Wake on LAN", "Remote",
            "Sends a magic packet to a machine's MAC address to remotely power it on. The machine must have Wake on LAN enabled in its BIOS/NIC settings."),

        new("Event Log", "Remote",
            "Queries the Windows Event Log on the target machine. Select a log (System, Application, Security) and filter by event type or ID."),

        new("Shared Folders", "Remote",
            "Lists network shares configured on the target machine via Win32_Share. Shows share name, local path, and description."),

        new("Pending Reboot", "Remote",
            "Checks whether the target machine has a pending reboot (from updates, software installs, etc.) without restarting it."),

        new("Remote Reboot", "Remote",
            "Initiates a remote reboot of the target machine. Prompts for confirmation before proceeding."),

        new("GP Update", "Remote",
            "Forces a Group Policy refresh (gpupdate /force) on the target machine."),

        new("Hosts File", "Remote",
            "View, add, and remove entries in the hosts file (C:\\Windows\\System32\\drivers\\etc\\hosts) on the target machine. Accesses via admin share (\\\\target\\C$\\...). Target must have admin shares enabled.\n\nView Hosts File: loads the current contents.\nAdd Entry: appends an IP + hostname line.\nRemove Entry: deletes any line containing the specified hostname or IP."),

        new("Auto Logon", "System",
            "Configures Windows automatic logon by writing credentials to the Winlogon registry key. Enable stores the username, password, and optional logon count. Disable clears the auto-logon settings. Requires elevated rights on the target."),

        new("Default Printer", "System",
            "Load the list of printers on the target machine and set one as the default. Select a printer from the list and click Set as Default."),

        new("Trusted Sites", "System",
            "Adds a domain to Internet Explorer / Edge's Trusted Sites zone in the registry (HKCU). Supports both http:// and https://. Useful for internal web apps that need trusted zone access."),

        new("Quick Access", "System",
            "Pins or unpins a folder from Windows Quick Access (the left-hand panel in File Explorer). Enter the full local folder path on the target machine.\n\nPin: adds the folder to Quick Access.\nUnpin: removes it. Uses the Windows Shell COM object."),

        new("Disk Cleanup", "System",
            "Removes old temporary files from the target machine. You can select which locations to clean: Windows Temp, SoftwareDistribution (update cache), User Temp folders, IIS Logs, and the Recycle Bin. Files newer than the specified number of days are kept.\n\nIMPORTANT: Stops the Windows Update service briefly to clear the SoftwareDistribution folder."),

        new("Keep Alive", "System",
            "Prevents this machine from sleeping or locking by telling Windows the system is in use (SetThreadExecutionState API). Useful during large downloads or remote sessions. Set a duration in hours and click Start. No keystrokes are sent. Click Stop to end early."),

        new("Reset AD Password", "Users",
            "Resets an Active Directory user account password. Enter the SamAccountName and new password. Optionally force a password change at next logon or unlock the account. Requires the ActiveDirectory PowerShell module on this machine and domain connectivity."),

        new("Windows Key", "Deploy",
            "Installs or activates a Windows product key on the target machine using slmgr.vbs.\n\nInstall: runs slmgr.vbs -ipk to install the key.\nActivate: runs slmgr.vbs /ato to activate Windows online."),

        new("WSUS Client", "Deploy",
            "Configures or checks Windows Update Server (WSUS) client settings via registry on the target machine.\n\nApply WSUS: writes the server URL, optional target group, and automatic update option.\nDisable WSUS: clears the UseWUServer flag so the machine uses Microsoft Update directly.\nCheck Status: reads the current registry settings."),

        new("Network Monitor", "General",
            "Live ping dashboard for monitoring multiple hosts simultaneously. Add hostnames or IP addresses to the list, set a polling interval, and click Start Monitoring. Each host displays Online/Offline status, ping response time, and the last check time.\n\nAccess from the left-side flyout menu under Network Monitor."),
    ];

    private List<HelpTopic> _filtered;

    public HelpPage()
    {
        InitializeComponent();
        _filtered = [.. _topics];
        BuildCards(_filtered);
    }

    private void OnSearchChanged(object? sender, TextChangedEventArgs e)
    {
        var q = e.NewTextValue?.Trim() ?? "";
        _filtered = string.IsNullOrEmpty(q)
            ? [.. _topics]
            : [.. _topics.Where(t =>
                t.Title.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                t.Category.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                t.Body.Contains(q, StringComparison.OrdinalIgnoreCase))];
        BuildCards(_filtered);
    }

    private void BuildCards(List<HelpTopic> topics)
    {
        HelpContainer.Children.Clear();

        var groups = topics.GroupBy(t => t.Category)
                           .OrderBy(g => CategoryOrder(g.Key));

        foreach (var group in groups)
        {
            // Category header
            HelpContainer.Children.Add(new Label
            {
                Text = group.Key,
                FontSize = 15,
                FontAttributes = FontAttributes.Bold,
                TextColor = Color.FromArgb("#0078D4"),
                Margin = new Thickness(0, 8, 0, 2)
            });

            foreach (var topic in group)
            {
                var border = new Border
                {
                    Stroke = Color.FromArgb("#DDDDDD"),
                    StrokeThickness = 1,
                    StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 6 },
                    Padding = new Thickness(14, 10),
                    BackgroundColor = Colors.White,
                    Margin = new Thickness(0, 0, 0, 6)
                };

                var stack = new VerticalStackLayout { Spacing = 4 };
                stack.Children.Add(new Label
                {
                    Text = topic.Title,
                    FontAttributes = FontAttributes.Bold,
                    FontSize = 13
                });
                stack.Children.Add(new Label
                {
                    Text = topic.Body,
                    FontSize = 12,
                    TextColor = Color.FromArgb("#444444"),
                    LineBreakMode = LineBreakMode.WordWrap
                });

                border.Content = stack;
                HelpContainer.Children.Add(border);
            }
        }

        if (topics.Count == 0)
        {
            HelpContainer.Children.Add(new Label
            {
                Text = "No topics match your search.",
                TextColor = Colors.Gray,
                FontSize = 13,
                Margin = new Thickness(0, 20)
            });
        }
    }

    private async void OnWhatsNewClicked(object? sender, EventArgs e)
        => await Launcher.OpenAsync("https://docs.google.com/document/d/1RKXF2eR6bdol3OLz554H8gIkVg90BcTehI-y59m3av8/edit?usp=sharing");

    private async void OnMsSupportClicked(object? sender, EventArgs e)
        => await Launcher.OpenAsync("http://caeled.geekgalaxy.com/web/steve/tech/AdditionalSupportResources.html");

    private async void OnInternalKbClicked(object? sender, EventArgs e)
        => await Launcher.OpenAsync("http://caeled.geekgalaxy.com/web/steve/tech/DeepSupportPortal.html");

    private static int CategoryOrder(string cat) => cat switch
    {
        "General"  => 0,
        "System"   => 1,
        "Users"    => 2,
        "Network"  => 3,
        "Remote"   => 4,
        "Deploy"   => 5,
        _          => 99
    };
}
