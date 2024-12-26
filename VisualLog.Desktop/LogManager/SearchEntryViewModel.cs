using VisualLog.Core.Search;

namespace VisualLog.Desktop.LogManager
{
  public class SearchEntryViewModel : ViewModelBase
  {
    public Entry SearchEntry { get; set; }

    public SearchEntryViewModel() { }
    public SearchEntryViewModel(Entry searchEntry)
    {
      this.SearchEntry = searchEntry;
    }
  }
}
