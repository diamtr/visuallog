using System.Collections.Generic;

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

      foreach (var formatter in this.Formatters)
        result.Add(formatter.Name, formatter.Extract(message.RawValue));

      return result;
    }
  }
}
