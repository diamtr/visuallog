using VisualLog.Core.TextProcessor;

namespace VisualLog.Core
{
  public class Message : MessageBase
  {
    public Message(string rawValue) : base(rawValue) { }

    public override void Read()
    {
      var stackIndex = this.RawValue.IndexOf("\"stack\"");
      if (stackIndex >= 0)
      {
        var sub = this.RawValue.Substring(stackIndex + 9);
        var quationMarkIndex = sub.IndexOf("\"");
        sub = sub.Substring(0, quationMarkIndex);
        this.Items["StackTrace"] = StackTraceBeautifier.Format(sub);
      }
    }
  }
}
