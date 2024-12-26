using System.Collections.Generic;

namespace VisualLog.Core.Search
{
  public interface ISearchRequestStatement
  {
    public IEnumerable<System.Text.RegularExpressions.Match> GetMatches(Message message);
  }
}
