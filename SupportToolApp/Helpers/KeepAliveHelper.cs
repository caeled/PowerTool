using System.Runtime.InteropServices;

namespace SupportToolApp.Helpers;

internal static class KeepAliveHelper
{
    private const uint ES_CONTINUOUS       = 0x80000000;
    private const uint ES_SYSTEM_REQUIRED  = 0x00000001;
    private const uint ES_DISPLAY_REQUIRED = 0x00000002;

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern uint SetThreadExecutionState(uint esFlags);

    public static void Enable()  => SetThreadExecutionState(ES_CONTINUOUS | ES_SYSTEM_REQUIRED | ES_DISPLAY_REQUIRED);
    public static void Disable() => SetThreadExecutionState(ES_CONTINUOUS);
}
