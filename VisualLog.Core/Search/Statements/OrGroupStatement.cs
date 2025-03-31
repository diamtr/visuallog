using System.Collections.Generic;
using System.Linq;

namespace VisualLog.Core.Search.Statements
{
  public class OrGroupStatement : ISearchRequestStatement
  {
    public List<ISearchRequestStatement> Statements { get; set; }

    public OrGroupStatement()
    {
      this.Statements = new List<ISearchRequestStatement>();
    }

    public IEnumerable<System.Text.RegularExpressions.Match> GetMatches(Message message)
    {
      var matches = Enumerable.Empty<System.Text.RegularExpressions.Match>();
      foreach (var statement in this.Statements.Where(x => x != null))
        matches = matches.Union(statement.GetMatches(message));
      return matches;
    }
  }
}
