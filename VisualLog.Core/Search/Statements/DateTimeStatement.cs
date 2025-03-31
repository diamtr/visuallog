using System;
using System.Collections.Generic;

namespace VisualLog.Core.Search
{
  public class DateTimeStatement : ISearchRequestStatement
  {
    public DateTime DateTime { get; set; }
    public DateTimeStatementCondition Condition { get; set; }

    public IEnumerable<System.Text.RegularExpressions.Match> GetMatches(Message message)
    {
      throw new NotImplementedException("DateTimeStatement.GetMatches not implemented yet");
    }
  }
}
