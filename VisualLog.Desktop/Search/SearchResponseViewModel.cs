using System.Collections.ObjectModel;

namespace VisualLog.Desktop.Search
{
  public class SearchResponseViewModel : ViewModelBase
  {
    public ObservableCollection<LogSearchResultsViewModel> LogSearchResults { get; set; }

    public SearchResponseViewModel()
    {
      this.LogSearchResults = new ObservableCollection<LogSearchResultsViewModel>();
    }
  }
}
