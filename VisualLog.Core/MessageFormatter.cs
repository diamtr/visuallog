using System.Text.RegularExpressions;

namespace VisualLog.Core
{
  public class MessageFormatter
  {
    public string Name { get; set; }
    public int Priority { get; set; }
    public string Pattern { get; set; }
    public Match Match(string input)
    {
      return Regex.Match(input, this.Pattern);
    }
  }
}
