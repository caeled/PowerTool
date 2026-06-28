using System.Runtime.InteropServices;

namespace SupportToolApp.Helpers;

internal static class KeepAliveHelper
{
#if WINDOWS
    private const uint ES_CONTINUOUS       = 0x80000000;
    private const uint ES_SYSTEM_REQUIRED  = 0x00000001;
    private const uint ES_DISPLAY_REQUIRED = 0x00000002;

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern uint SetThreadExecutionState(uint esFlags);

    public static void Enable()  => SetThreadExecutionState(ES_CONTINUOUS | ES_SYSTEM_REQUIRED | ES_DISPLAY_REQUIRED);
    public static void Disable() => SetThreadExecutionState(ES_CONTINUOUS);

#elif MACCATALYST || IOS
    // IOKit power assertion — prevents display sleep and system idle sleep
    [DllImport("/System/Library/Frameworks/IOKit.framework/IOKit")]
    private static extern int IOPMAssertionCreateWithName(
        string assertionType, int assertionLevel, string assertionName, out uint assertionID);

    [DllImport("/System/Library/Frameworks/IOKit.framework/IOKit")]
    private static extern int IOPMAssertionRelease(uint assertionID);

    private const string kIOPMAssertionTypeNoDisplaySleep = "NoDisplaySleepAssertion";
    private const int kIOPMAssertionLevelOn = 255;
    private static uint _assertionID;

    public static void Enable()
    {
        IOPMAssertionCreateWithName(kIOPMAssertionTypeNoDisplaySleep, kIOPMAssertionLevelOn,
            "SupportTool Keep Alive", out _assertionID);
    }

    public static void Disable()
    {
        if (_assertionID != 0) { IOPMAssertionRelease(_assertionID); _assertionID = 0; }
    }

#else
    // Linux: spawn systemd-inhibit in background; kill it to release
    private static System.Diagnostics.Process? _inhibitProcess;

    public static void Enable()
    {
        _inhibitProcess = System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
        {
            FileName  = "systemd-inhibit",
            Arguments = "--what=idle:sleep --who=SupportTool --why=KeepAlive --mode=block sleep infinity",
            UseShellExecute = false,
            CreateNoWindow  = true
        });
    }

    public static void Disable()
    {
        try { _inhibitProcess?.Kill(); } catch { }
        _inhibitProcess = null;
    }
#endif
}
