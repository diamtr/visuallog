using System.Collections.Generic;
using System.Linq;
using VisualLog.Core.Search;
using VisualLog.Desktop.LogManager;

namespace VisualLog.Desktop.Search
{
  public class SearchViewModel : ViewModelBase
  {
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

    public List<LogViewModel> Logs { get; set; }

    public SearchViewModel()
    {
      this.Logs = new List<LogViewModel>();
      this.SearchRequestViewModel = new SearchRequestViewModel();
      this.SearchRequestViewModel.SearchRequested += this.OnSearchRequested;
      this.SearchResponseViewModel = new SearchResponseViewModel();
    }

    private void OnSearchRequested(SearchRequest searchRequest)
    {
      this.SearchResponseViewModel = new SearchResponseViewModel();
      foreach (var logViewModel in this.Logs)
      {
        var searchResponse = SearchEngine.Search(logViewModel.Log, searchRequest);
        if (searchResponse.Entries.Any())
          this.SearchResponseViewModel.LogSearchResults.Add(new LogSearchResultsViewModel(logViewModel, searchResponse));
      }
    }
  }
}
