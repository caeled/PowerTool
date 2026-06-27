using SupportToolApp.ViewModels;

namespace SupportToolApp.Pages.Search;

public partial class SearchPage : ContentPage
{
    public SearchPage(SearchViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
