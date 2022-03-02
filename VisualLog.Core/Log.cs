using System.Collections.Generic;
using System.Linq;

namespace VisualLog.Core
{
  public class Log
  {
    public List<Message> Messages { get; set; }

    public Log()
    {
      this.Messages = new List<Message>();
    }

    public void Read(string path)
    {
      var lines = System.IO.File.ReadAllLines(path).ToList();
      foreach (var line in lines)
        this.Messages.Add(new Message() { Number = lines.IndexOf(line), RawValue = line });
    }
  }
}
