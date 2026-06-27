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
}
