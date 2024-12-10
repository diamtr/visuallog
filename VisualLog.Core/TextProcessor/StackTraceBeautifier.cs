using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace VisualLog.Core.TextProcessor
{
  public class StackTraceBeautifier
  {
    public const string BreakTheLinePattern = @"(\\r\\n\s*|\\r\s*|\\n\s*)";

    public static string Format(string input)
    {
      if (string.IsNullOrWhiteSpace(input))
        return input;

      var stringBuilder = new StringBuilder();
      var tokens = Regex.Split(input, BreakTheLinePattern)
        .Where(x => !Regex.IsMatch(x, BreakTheLinePattern));
      foreach (var token in tokens)
        stringBuilder.AppendLine(token);

      return stringBuilder.ToString();
    }
  }
}
