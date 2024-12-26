using System.Collections.Generic;

namespace VisualLog.Core.Search
{
  public class SearchRequest
  {
    public List<ISearchRequestStatement> Statements { get; set; }

    public SearchRequest()
    {
      this.Statements = new List<ISearchRequestStatement>();
    }
  }
}
