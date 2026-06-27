using System.Collections.ObjectModel;
using System.Windows.Input;
using SupportToolApp.Models;
using SupportToolApp.Services;

namespace SupportToolApp.ViewModels;

public class FileShare { public string Name { get; set; } = ""; public string Path { get; set; } = ""; }

public class SearchViewModel : BaseViewModel
{
    private readonly IAdService _ad;
    private ObservableCollection<AdUser> _adResults = [];

    public ObservableCollection<AdUser> AdResults { get => _adResults; set => SetProperty(ref _adResults, value); }

    public ObservableCollection<FileShare> FileShares { get; } =
    [
        new() { Name = "Home Drives", Path = @"\\fs01\homes" },
        new() { Name = "Shared Data", Path = @"\\fs01\shared" },
        new() { Name = "IT Dept", Path = @"\\fs01\IT" },
        new() { Name = "Software Repo", Path = @"\\fs02\software" },
    ];

    public ICommand SearchAdCommand { get; }
    public ICommand WebSearchCommand { get; }
    public ICommand OpenUrlCommand { get; }
    public ICommand BrowseShareCommand { get; }

    public SearchViewModel(IAdService ad)
    {
        _ad = ad;
        SearchAdCommand = new Command<string>(async q =>
        {
            var r = await _ad.SearchUsersAsync(q ?? "");
            AdResults = new ObservableCollection<AdUser>(r);
        });
        WebSearchCommand = new Command<string>(async baseUrl =>
        {
            // The search term needs to come from the page — simplified here
            await Browser.OpenAsync(baseUrl, BrowserLaunchMode.SystemPreferred);
        });
        OpenUrlCommand = new Command<string>(async url =>
        {
            if (!string.IsNullOrWhiteSpace(url))
                await Browser.OpenAsync(url, BrowserLaunchMode.SystemPreferred);
        });
        BrowseShareCommand = new Command<string>(path =>
        {
            try { System.Diagnostics.Process.Start("explorer.exe", path); } catch { }
        });
    }
}
