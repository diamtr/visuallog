using System.Text.RegularExpressions;

namespace VisualLog.Core.Search
{
  public interface ISearchRequestStatement
  {
    public int Order { get; set; }
    public MatchCollection GetMatches(Message message);
  }
}
