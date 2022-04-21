using System.Collections.Generic;

namespace VisualLog.Core
{
  public class Message
  {
    public long Number { get; set; }
    public string RawValue { get; set; }
    public IDictionary<string, string> Parts { get; set; }
  }
}
