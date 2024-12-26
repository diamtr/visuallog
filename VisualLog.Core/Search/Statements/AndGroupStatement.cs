using System.Collections.Generic;
using System.Linq;

namespace VisualLog.Core.Search.Statements
{
  public class AndGroupStatement : ISearchRequestStatement
  {
    public List<ISearchRequestStatement> Statements { get; set; }

    public AndGroupStatement()
    {
      this.Statements = new List<ISearchRequestStatement>();
    }

    public IEnumerable<System.Text.RegularExpressions.Match> GetMatches(Message message)
    {
      var matches = Enumerable.Empty<System.Text.RegularExpressions.Match>();
      foreach (var statement in this.Statements)
      {
        var statementMatches = statement.GetMatches(message);
        if (!statementMatches.Any())
          return Enumerable.Empty<System.Text.RegularExpressions.Match>();
        matches = matches.Union(statementMatches);
      }
      return matches;
    }
  }
}
