using System.Collections.Generic;

namespace VisualLog.Core
{
  public class Message : MessageBase
  {
    public IDictionary<string, string> Parts { get; set; }

    public Message(string rawValue) : base(rawValue)
    {
      this.Parts = new Dictionary<string, string>();
    }
  }
}
