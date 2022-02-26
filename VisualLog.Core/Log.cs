using System.Collections.Generic;

namespace VisualLog.Core
{
  public class Log
  {
    public virtual List<Message> Messages { get; set; }

    public Log()
    {
      this.Messages = new List<Message>();
    }
  }
}
