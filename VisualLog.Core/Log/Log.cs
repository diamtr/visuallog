using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VisualLog.Core
{
  public class Log
  {
    public List<Message> Messages { get; private set; }
    public Encoding Encoding { get; private set; }
    public Format Format { get; set; }
    public string SourceFilePath { get; private set; }

    private Action<Message> messageCatched;
    public event Action<Message> MessageCatched
    {
      add
      {
        if (messageCatched == null ||
            !messageCatched.GetInvocationList().Contains(value))
          this.messageCatched += value;
      }
      remove
      {
        this.messageCatched -= value;
      }
    }

    private LogReader reader;

    #region ctors

    public Log(string path)
    {
      this.Messages = new List<Message>();
      this.SourceFilePath = path;
      this.reader = new LogReader(path);
    }

    public Log(string path, Encoding encoding) : this(path)
    {
      this.Encoding = encoding; // TODO: maybe remove this.Encoding
      this.reader.SetEncoding(encoding);
    }

    #endregion

    #region Reading

    public void Read()
    {
      this.reader.LineReaded += this.CatchMessage;
      this.reader.ReadAndSubscribeUpdates();
    }

    public void CatchMessage(string s)
    {
      var message = new Message(s);
      message.Read();
      this.Messages.Add(message);
      this.messageCatched?.Invoke(message);
    }

    #endregion

    public void ApplyFormat()
    {
      if (this.Format == null)
        return;

      foreach (var message in this.Messages)
        message.Items = this.Format.Deserialize(message);
    }
  }
}
