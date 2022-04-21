using System.Collections.Generic;

namespace VisualLog.Core
{
  public class LogFormat
  {
    public string Name { get; set; }
    public List<IMessageFormatter> Formatters { get; set; }
    public IDictionary<string, string> Deserialize(Message message)
    {
      return new Dictionary<string, string>();
    }
  }
}
