﻿using System.Collections.Generic;
using System.Linq;

namespace VisualLog.Core.Search
{
  public class SearchEntry
  {
    public int LineNumber { get; set; }
    public List<MatchEntry> Matches { get; set; }
    public string RawString { get; set; }

    public SearchEntry()
    {
      this.Matches = new List<MatchEntry>();
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
      if (obj is not SearchEntry)
        return false;

      var entry = obj as SearchEntry;
      return 
        this.LineNumber == entry.LineNumber &&
        this.RawString == entry.RawString &&
        this.Matches.SequenceEqual(entry.Matches);
    }
  }
}
