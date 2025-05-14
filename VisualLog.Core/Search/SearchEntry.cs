using System.Collections.Generic;
using System.Linq;

namespace VisualLog.Core.Search
{
  public class SearchEntry
  {
    public int LineNumber { get; set; }
    public List<MatchEntry> Matches { get; set; }
    public Message Message { get; set; }

    public SearchEntry(Message message) : this()
    {
      this.Message = message;
    }

    public SearchEntry()
    {
      this.Matches = new List<MatchEntry>();
    }

    public override int GetHashCode()
    {
      return this.Message != null ?
        this.LineNumber.GetHashCode() ^ this.Message.GetHashCode() ^ this.Matches.GetHashCode() :
        this.LineNumber.GetHashCode() ^ string.Empty.GetHashCode() ^ this.Matches.GetHashCode();
    }

    public override bool Equals(object obj)
    {
      if (obj is not SearchEntry)
        return false;

      var entry = obj as SearchEntry;
      return 
        this.LineNumber == entry.LineNumber &&
        Equals(this.Message, entry.Message) &&
        this.Matches.SequenceEqual(entry.Matches);
    }
  }
}
