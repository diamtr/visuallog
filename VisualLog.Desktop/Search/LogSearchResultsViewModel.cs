using System.Collections.ObjectModel;
using VisualLog.Core.Search;
using VisualLog.Desktop.LogManager;

namespace VisualLog.Desktop.Search
{
  public class LogSearchResultsViewModel : ViewModelBase
  {
    public LogViewModel LogViewModel { get; private set; }
    public ObservableCollection<SearchEntryViewModel> SearchEntries { get; set; }
    public bool ShowHeader { get; set; }
    public bool ShowEntries { get; set; }

    private SearchResponse searchResponse;

    public LogSearchResultsViewModel(LogViewModel logViewModel, SearchResponse searchResponse) : this()
    {
      this.LogViewModel = logViewModel;
      this.searchResponse = searchResponse;
    }

    public LogSearchResultsViewModel()
    {
      this.SearchEntries = new ObservableCollection<SearchEntryViewModel>();
      this.ShowHeader = true;
    }

    public void UpdateSearchEntries()
    {
      this.SearchEntries.Clear();
      foreach (var entry in this.searchResponse.Entries)
        this.SearchEntries.Add(new SearchEntryViewModel(entry));
    }
  }
}
