using System.Collections.Generic;
using VisualLog.Core.TextProcessor;

namespace VisualLog.Core
{
  public class Message : MessageBase
  {
    public IDictionary<string, string> Parts { get; set; }

    public Message(string rawValue) : base(rawValue)
    {
      this.Parts = new Dictionary<string, string>();
    }

    public void FillParts()
    {
      var stackIndex = this.RawValue.IndexOf("\"stack\"");
      if (stackIndex >= 0)
      {
        var sub = this.RawValue.Substring(stackIndex + 9);
        var quationMarkIndex = sub.IndexOf("\"");
        sub = sub.Substring(0, quationMarkIndex);
        this.Parts["stack"] = StackTraceBeautifier.Format(sub);
      }
    }
  }
}
