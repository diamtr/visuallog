using System.Collections.Generic;
using System.IO;

namespace VisualLog.Core.Search
{
  public class SearchResults
  {
    public List<Entry> Entries { get; set; }
    public string LogPath { get; set; }
    public string LogName {
      get 
      {
        if (string.IsNullOrWhiteSpace(this.LogPath))
          return null;
        return Path.GetFileName(this.LogPath); 
      }
    }

    public SearchResults()
    {
      this.Entries = new List<Entry>();
    }
  }
}
