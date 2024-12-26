using VisualLog.Core.Search;

namespace VisualLog.Desktop.LogManager
{
  public class SearchEntryViewModel : ViewModelBase
  {
    public SearchEntry SearchEntry { get; set; }

    public SearchEntryViewModel() { }
    public SearchEntryViewModel(SearchEntry searchEntry)
    {
      this.SearchEntry = searchEntry;
    }
  }
}
