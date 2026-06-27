namespace SupportToolApp.Services;

public class CredentialService
{
    public event Action? Changed;

    // User credential (WinRM, file ops, Deploy, Copy, RunAs)
    private string _userDomain = ".";
    private string _userUsername = "";
    private string _userPassword = "";
    public string UserDomain    { get => _userDomain;    set { _userDomain = value;    Changed?.Invoke(); } }
    public string UserUsername  { get => _userUsername;  set { _userUsername = value;  Changed?.Invoke(); } }
    public string UserPassword  { get => _userPassword;  set { _userPassword = value;  Changed?.Invoke(); } }
    public bool HasUserCred => !string.IsNullOrWhiteSpace(UserUsername) && !string.IsNullOrWhiteSpace(UserPassword);

    // Office 365 credential
    private string _o365Username = "";
    private string _o365Password = "";
    public string O365Username { get => _o365Username; set { _o365Username = value; Changed?.Invoke(); } }
    public string O365Password { get => _o365Password; set { _o365Password = value; Changed?.Invoke(); } }
    public bool HasO365Cred => !string.IsNullOrWhiteSpace(O365Username) && !string.IsNullOrWhiteSpace(O365Password);

    public void SetUserCred(string domain, string username, string password)
    {
        _userDomain = domain; _userUsername = username; _userPassword = password;
        Changed?.Invoke();
    }

    public void SetO365Cred(string username, string password)
    {
        _o365Username = username; _o365Password = password;
        Changed?.Invoke();
    }

    // Google / Workspace credential
    private string _googleUsername = "";
    private string _googlePassword = "";
    public string GoogleUsername { get => _googleUsername; set { _googleUsername = value; Changed?.Invoke(); } }
    public string GooglePassword { get => _googlePassword; set { _googlePassword = value; Changed?.Invoke(); } }
    public bool HasGoogleCred => !string.IsNullOrWhiteSpace(GoogleUsername) && !string.IsNullOrWhiteSpace(GooglePassword);

    public void SetGoogleCred(string username, string password)
    {
        _googleUsername = username; _googlePassword = password;
        Changed?.Invoke();
    }

    // Quick Launch buttons (name + URL, stored in Preferences — not sensitive)
    private string _ql1Name = "", _ql1Url = "", _ql2Name = "", _ql2Url = "";
    public string QuickLink1Name { get => _ql1Name; set { _ql1Name = value; Changed?.Invoke(); } }
    public string QuickLink1Url  { get => _ql1Url;  set { _ql1Url  = value; Changed?.Invoke(); } }
    public string QuickLink2Name { get => _ql2Name; set { _ql2Name = value; Changed?.Invoke(); } }
    public string QuickLink2Url  { get => _ql2Url;  set { _ql2Url  = value; Changed?.Invoke(); } }

    private string _ql3Name = "", _ql3Url = "", _ql4Name = "", _ql4Url = "";
    public string QuickLink3Name { get => _ql3Name; set { _ql3Name = value; Changed?.Invoke(); } }
    public string QuickLink3Url  { get => _ql3Url;  set { _ql3Url  = value; Changed?.Invoke(); } }
    public string QuickLink4Name { get => _ql4Name; set { _ql4Name = value; Changed?.Invoke(); } }
    public string QuickLink4Url  { get => _ql4Url;  set { _ql4Url  = value; Changed?.Invoke(); } }

    public void SetQuickLinks(string name1, string url1, string name2, string url2,
                              string name3, string url3, string name4, string url4)
    {
        _ql1Name = name1; _ql1Url = url1; _ql2Name = name2; _ql2Url = url2;
        _ql3Name = name3; _ql3Url = url3; _ql4Name = name4; _ql4Url = url4;
        Preferences.Set("ql1name", name1); Preferences.Set("ql1url", url1);
        Preferences.Set("ql2name", name2); Preferences.Set("ql2url", url2);
        Preferences.Set("ql3name", name3); Preferences.Set("ql3url", url3);
        Preferences.Set("ql4name", name4); Preferences.Set("ql4url", url4);
        Changed?.Invoke();
    }

    public void LoadQuickLinks()
    {
        _ql1Name = Preferences.Get("ql1name", ""); _ql1Url = Preferences.Get("ql1url", "");
        _ql2Name = Preferences.Get("ql2name", ""); _ql2Url = Preferences.Get("ql2url", "");
        _ql3Name = Preferences.Get("ql3name", ""); _ql3Url = Preferences.Get("ql3url", "");
        _ql4Name = Preferences.Get("ql4name", ""); _ql4Url = Preferences.Get("ql4url", "");
        _localFolder  = Preferences.Get("localfolder",  "");
        _folderName   = Preferences.Get("foldername",   "");
        _libraryUrl   = Preferences.Get("libraryurl",   "https://en.wikipedia.org/wiki/Main_Page");
        _libraryName  = Preferences.Get("libraryname",  "");
        _logoPath     = Preferences.Get("logopath",     "");
        _logoInitials = Preferences.Get("logoinitials", "IT");
        _logoColor    = Preferences.Get("logocolor",    "#0078D4");
    }

    // Logo (image path or initials badge)
    private string _logoPath = "", _logoInitials = "IT", _logoColor = "#0078D4";
    public string LogoPath
    {
        get => _logoPath;
        set { _logoPath = value; Preferences.Set("logopath", value); Changed?.Invoke(); }
    }
    public string LogoInitials
    {
        get => _logoInitials;
        set { _logoInitials = value; Preferences.Set("logoinitials", value); Changed?.Invoke(); }
    }
    public string LogoColor
    {
        get => _logoColor;
        set { _logoColor = value; Preferences.Set("logocolor", value); Changed?.Invoke(); }
    }

    // Local folder shortcut
    private string _localFolder = "", _folderName = "";
    public string LocalFolder
    {
        get => _localFolder;
        set { _localFolder = value; Preferences.Set("localfolder", value); Changed?.Invoke(); }
    }
    public string FolderName
    {
        get => _folderName;
        set { _folderName = value; Preferences.Set("foldername", value); Changed?.Invoke(); }
    }

    // Library URL (default: Wikipedia)
    private string _libraryUrl = "https://en.wikipedia.org/wiki/Main_Page", _libraryName = "";
    public string LibraryUrl
    {
        get => _libraryUrl;
        set { _libraryUrl = value; Preferences.Set("libraryurl", value); Changed?.Invoke(); }
    }
    public string LibraryName
    {
        get => _libraryName;
        set { _libraryName = value; Preferences.Set("libraryname", value); Changed?.Invoke(); }
    }
}
