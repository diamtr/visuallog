using System.Collections.Generic;

namespace VisualLog.Core
{
  public interface ILogFormat
  {
    public string Name { get; set; }
    public List<IMessageFormatter> Formatters { get; set; }
    public IDictionary<string, string> Deserialize(Message message);
  }
}
