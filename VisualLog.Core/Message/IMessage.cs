using System.Collections.Generic;

namespace VisualLog.Core
{
  public interface IMessage
  {
    public string RawValue { get; set; }
    public IDictionary<string, object> Items { get; set; }
  }
}
