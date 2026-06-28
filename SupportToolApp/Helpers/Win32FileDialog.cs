using System.Runtime.InteropServices;
using System.Text;

namespace SupportToolApp.Helpers;

/// <summary>
/// Win32 file dialogs — required when running elevated (WinRT pickers fail with COMException).
/// </summary>
internal static class Win32FileDialog
{
    public static string? OpenFile(string title, string filter = "All Files\0*.*\0\0")
    {
        var ofn = new OpenFileName
        {
            lStructSize = Marshal.SizeOf<OpenFileName>(),
            lpstrFilter = filter,
            lpstrFile   = new string('\0', 260),
            nMaxFile    = 260,
            lpstrTitle  = title,
            Flags       = OFN_NOCHANGEDIR | OFN_FILEMUSTEXIST
        };
        return GetOpenFileName(ref ofn) ? ofn.lpstrFile.TrimEnd('\0') : null;
    }

    public static string? SaveFile(string title, string suggestedName, string filter = "All Files\0*.*\0\0", string defaultExt = "")
    {
        var ofn = new SaveFileName
        {
            lStructSize = Marshal.SizeOf<SaveFileName>(),
            lpstrFilter = filter,
            lpstrFile   = suggestedName + new string('\0', 260),
            nMaxFile    = 260,
            lpstrTitle  = title,
            lpstrDefExt = defaultExt,
            Flags       = OFN_NOCHANGEDIR | OFN_OVERWRITEPROMPT
        };
        return GetSaveFileName(ref ofn) ? ofn.lpstrFile.TrimEnd('\0') : null;
    }

    private const int OFN_NOCHANGEDIR    = 0x00000008;
    private const int OFN_FILEMUSTEXIST  = 0x00001000;
    private const int OFN_OVERWRITEPROMPT = 0x00000002;

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    private struct OpenFileName
    {
        public int    lStructSize;
        public IntPtr hwndOwner;
        public IntPtr hInstance;
        public string lpstrFilter;
        public string lpstrCustomFilter;
        public int    nMaxCustFilter;
        public int    nFilterIndex;
        public string lpstrFile;
        public int    nMaxFile;
        public string lpstrFileTitle;
        public int    nMaxFileTitle;
        public string lpstrInitialDir;
        public string lpstrTitle;
        public int    Flags;
        public short  nFileOffset;
        public short  nFileExtension;
        public string lpstrDefExt;
        public IntPtr lCustData;
        public IntPtr lpfnHook;
        public string lpTemplateName;
        public IntPtr pvReserved;
        public int    dwReserved;
        public int    flagsEx;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    private struct SaveFileName
    {
        public int    lStructSize;
        public IntPtr hwndOwner;
        public IntPtr hInstance;
        public string lpstrFilter;
        public string lpstrCustomFilter;
        public int    nMaxCustFilter;
        public int    nFilterIndex;
        public string lpstrFile;
        public int    nMaxFile;
        public string lpstrFileTitle;
        public int    nMaxFileTitle;
        public string lpstrInitialDir;
        public string lpstrTitle;
        public int    Flags;
        public short  nFileOffset;
        public short  nFileExtension;
        public string lpstrDefExt;
        public IntPtr lCustData;
        public IntPtr lpfnHook;
        public string lpTemplateName;
        public IntPtr pvReserved;
        public int    dwReserved;
        public int    flagsEx;
    }

    public static string? BrowseFolder(string title)
    {
        var bi = new BrowseInfo { lpszTitle = title, ulFlags = BIF_RETURNONLYFSDIRS | BIF_NEWDIALOGSTYLE };
        var pidl = SHBrowseForFolder(ref bi);
        if (pidl == IntPtr.Zero) return null;
        var path = new StringBuilder(260);
        return SHGetPathFromIDList(pidl, path) ? path.ToString() : null;
    }

    private const int BIF_RETURNONLYFSDIRS = 0x0001;
    private const int BIF_NEWDIALOGSTYLE   = 0x0040;

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    private struct BrowseInfo
    {
        public IntPtr hwndOwner;
        public IntPtr pidlRoot;
        public string pszDisplayName;
        public string lpszTitle;
        public int    ulFlags;
        public IntPtr lpfn;
        public IntPtr lParam;
        public int    iImage;
    }

    [DllImport("shell32.dll", CharSet = CharSet.Auto)]
    private static extern IntPtr SHBrowseForFolder(ref BrowseInfo bi);

    [DllImport("shell32.dll", CharSet = CharSet.Auto)]
    private static extern bool SHGetPathFromIDList(IntPtr pidl, StringBuilder pszPath);

    [DllImport("comdlg32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    private static extern bool GetOpenFileName(ref OpenFileName ofn);

    [DllImport("comdlg32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    private static extern bool GetSaveFileName(ref SaveFileName ofn);
}
