using System.Collections.Generic;

namespace VisualLog.Core
{
  public class Message
  {
    public string RawValue { get; set; }
    public IDictionary<string, string> Parts { get; set; }

    public Message(string rawValue)
    {
      this.RawValue = rawValue;
      this.Parts = new Dictionary<string, string>();
    }

    public override bool Equals(object obj)
    {
      if (obj == null || !this.GetType().Equals(obj.GetType()))
        return false;

      return this.RawValue == ((Message)obj).RawValue;
    }

    public override int GetHashCode()
    {
      return this.RawValue.GetHashCode();
    }
  }
}
