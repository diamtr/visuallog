using System.Collections.Generic;
using System.Linq;

namespace VisualLog.Core.Search
{
  public class Entry
  {
    public int LineNumber { get; set; }
    public List<Match> Matches { get; set; }
    public string RawString { get; set; }

    public Entry()
    {
      this.Matches = new List<Match>();
    }

    public override int GetHashCode()
    {
      return
        this.LineNumber.GetHashCode() ^
        this.RawString.GetHashCode() ^
        this.Matches.GetHashCode();
    }

    public override bool Equals(object obj)
    {
      if (obj is not Entry)
        return false;

      var entry = obj as Entry;
      return 
        this.LineNumber == entry.LineNumber &&
        this.RawString == entry.RawString &&
        this.Matches.SequenceEqual(entry.Matches);
    }
  }
}
