using System.Collections.Generic;
using System.IO;

namespace VisualLog.Core.Search
{
  public class SearchResponse
  {
    public List<SearchEntry> Entries { get; set; }
    public string LogPath { get; set; }
    public string LogName {
      get 
      {
        if (string.IsNullOrWhiteSpace(this.LogPath))
          return null;
        return Path.GetFileName(this.LogPath); 
      }
    }

    public SearchResponse()
    {
      this.Entries = new List<SearchEntry>();
    }
  }
}
