using SupportToolApp.ViewModels;

namespace SupportToolApp.Pages.Reporting;

public partial class ReportingPage : ContentPage
{
    public ReportingPage(ReportingViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
