using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace VisualLog.Core
{
  public class Log
  {
    public List<Message> Messages { get; set; }
    public Encoding Encoding { get; private set; }
    public LogFormat Format { get; set; }

    public Log()
    {
      this.Messages = new List<Message>();
    }

    public Log(Encoding encoding) : this()
    {
      this.Encoding = encoding;
    }

    public void Read(string path)
    {
      var lines = new List<string>();
      if (this.Encoding != null)
        lines = File.ReadAllLines(path, this.Encoding).ToList();
      else
        lines = File.ReadAllLines(path).ToList();
      foreach (var line in lines)
        this.Messages.Add(new Message() { Number = lines.IndexOf(line), RawValue = line });
    }

    public void ApplyFormat()
    {
      if (this.Format == null)
        return;

      foreach (var message in this.Messages)
        message.Parts = this.Format.Deserialize(message);
    }
  }
}
