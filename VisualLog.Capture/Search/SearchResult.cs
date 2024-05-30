using System.Collections.Generic;
using VisualLog.Core;

namespace VisualLog.Capture.Search
{
  public class SearchResult
  {
    public string FilePath { get; set; }
    public ISearchOption SearchOption { get; set; }
    public IEnumerable<IMessage> Result { get; set; }
  }
}
