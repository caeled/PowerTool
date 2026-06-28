namespace SupportToolApp.Pages.Monitor;

public partial class NetworkMonitorPage : ContentPage
{
    public NetworkMonitorPage()
    {
        InitializeComponent();
        BindingContext = new NetworkMonitorViewModel();
    }
}
