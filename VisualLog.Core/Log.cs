using System;
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
    public Format Format { get; set; }
    public bool ReadingInProcess { get; set; }
    public LogReader LogReader { get; private set; }
    public string SourceFilePath { get; private set; }

    public event Action<Message> CatchNewMessage
    {
      add
      {
        if (catchNewMessage == null || !catchNewMessage.GetInvocationList().Contains(value))
          this.catchNewMessage += value;
      }
      remove
      {
        this.catchNewMessage -= value;
      }
    }
    
    private FileSystemWatcher sourceFileWatcher;
    private Action<Message> catchNewMessage;

    #region ctors

    public Log(string path)
    {
      this.Messages = new List<Message>();
      this.SourceFilePath = path;
      this.LogReader = new LogReader(path);
    }

    public Log(string path, Encoding encoding) : this(path)
    {
      this.Encoding = encoding; // TODO: maybe remove this.Encoding
      this.LogReader.SetEncoding(encoding);
    }

    #endregion

    #region Reading

    public void Read()
    {
      this.LogReader.LineReaded += this.NewLineReaded;
      this.LogReader.StartReading();
    }

    public void NewLineReaded(string s)
    {
      var message = new Message(s);
      message.FillParts();
      this.Messages.Add(message);
      if (this.catchNewMessage != null)
        this.catchNewMessage.Invoke(message);
    }

    #endregion

    public void ApplyFormat()
    {
      if (this.Format == null)
        return;

      foreach (var message in this.Messages)
        message.Parts = this.Format.Deserialize(message);
    }
  }
}
