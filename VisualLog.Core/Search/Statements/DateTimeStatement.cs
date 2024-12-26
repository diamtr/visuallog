using System;
using System.Text.RegularExpressions;

namespace VisualLog.Core.Search
{
  public class DateTimeStatement : ISearchRequestStatement
  {
    public int Order { get; set; }
    public DateTime DateTime { get; set; }
    public DateTimeStatementCondition Condition { get; set; }

    public MatchCollection GetMatches(Message message)
    {
      throw new NotImplementedException("DateTimeStatement.GetMatches not implemented yet");
    }
  }
}
