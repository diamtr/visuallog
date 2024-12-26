using System.Collections.Generic;
using System.IO;

namespace VisualLog.Core.Search
{
  public class Results
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

    public Results()
    {
      this.Entries = new List<Entry>();
    }
  }
}
