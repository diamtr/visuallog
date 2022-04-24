using System.Collections.Generic;
using System.Linq;

namespace VisualLog.Core
{
  public class LogFormat
  {
    public string Name { get; set; }
    public List<MessageFormatter> Formatters { get; set; }

    public LogFormat()
    {
      this.Formatters = new List<MessageFormatter>();
    }

    public IDictionary<string, string> Deserialize(Message message)
    {
      var result = new Dictionary<string, string>();
      var input = message.RawValue.Trim();

      foreach (var formatter in this.Formatters.OrderBy(x => x.Priority))
      {
        var match = formatter.Match(input);
        if (!match.Success)
          continue;
        input = input.Remove(0, match.Value.Length);
        input = input.Trim();
        result.Add(formatter.Name, match.Value);
      }

      return result;
    }
  }
}
