using System.Linq;
using VisualLog.Core.Search;

namespace VisualLog.Desktop.Search
{
  public class SearchViewModel : ViewModelBase
  {
    private MainViewModel mainViewModel;
    public MainViewModel MainViewModel
    {
      get { return this.mainViewModel; }
      set
      {
        this.mainViewModel = value;
        this.OnPropertyChanged();
      }
    }

    private SearchRequestViewModel searchRequestViewModel;
    public SearchRequestViewModel SearchRequestViewModel
    {
      get { return this.searchRequestViewModel; }
      set
      {
        this.searchRequestViewModel = value;
        this.OnPropertyChanged();
      }
    }

    private SearchResponseViewModel searchResponseViewModel;
    public SearchResponseViewModel SearchResponseViewModel
    {
      get { return this.searchResponseViewModel; }
      set 
      {
        this.searchResponseViewModel = value;
        this.OnPropertyChanged();
      }
    }

    public SearchViewModel(MainViewModel mainViewModel) : this()
    {
      this.MainViewModel = mainViewModel;
    }

    public SearchViewModel()
    {
      this.SearchRequestViewModel = new SearchRequestViewModel();
      this.SearchRequestViewModel.SearchRequested += this.OnSearchRequested;
      this.SearchResponseViewModel = new SearchResponseViewModel();
    }

    private void OnSearchRequested(SearchRequest searchRequest)
    {
      this.SearchResponseViewModel = new SearchResponseViewModel();
      foreach (var logViewModel in this.MainViewModel.Logs)
      {
        var searchResponse = SearchEngine.Search(logViewModel.Log, searchRequest);
        if (searchResponse.Entries.Any())
        {
          var searchResultsViewModel = new LogSearchResultsViewModel(logViewModel, searchResponse);
          searchResultsViewModel.UpdateSearchEntries();
          this.SearchResponseViewModel.LogSearchResults.Add(searchResultsViewModel);
        }
      }
    }
  }
}
