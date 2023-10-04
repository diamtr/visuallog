using System.Collections.Generic;

namespace VisualLog.Core
{
  public class SearchResults
  {
    public List<SearchEntry> Entries { get; set; }

    public SearchResults()
    {
      this.Entries = new List<SearchEntry>();
    }
  }
}
