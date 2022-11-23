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

    private string sourceFilePath;
    private FileSystemWatcher sourceFileWatcher;

    public Log()
    {
      this.Messages = new List<Message>();
    }

    public Log(string path) : this()
    {
      this.sourceFilePath = path;
    }

    public Log(string path, Encoding encoding) : this(path)
    {
      this.Encoding = encoding;
    }

    public void Read()
    {
      this.ReadingInProcess = true;
      var lines = new List<string>();
      if (this.Encoding != null)
        lines = File.ReadAllLines(this.sourceFilePath, this.Encoding).ToList();
      else
        lines = File.ReadAllLines(this.sourceFilePath).ToList();
      foreach (var line in lines)
        this.Messages.Add(new Message(line));
      this.ReadingInProcess = false;
    }

    public void ApplyFormat()
    {
      if (this.Format == null)
        return;

      foreach (var message in this.Messages)
        message.Parts = this.Format.Deserialize(message);
    }

    public void StartFollowTail()
    {
      var fileInfo = new FileInfo(this.sourceFilePath);
      this.sourceFileWatcher = new FileSystemWatcher(fileInfo.DirectoryName);
      this.sourceFileWatcher.NotifyFilter = NotifyFilters.LastWrite;
      this.sourceFileWatcher.Filter = Path.GetFileName(fileInfo.Name);
      this.sourceFileWatcher.Changed += this.ReadLogUpdates;
      this.sourceFileWatcher.EnableRaisingEvents = true;
    }

    public void ReadLogUpdates(object sender, FileSystemEventArgs e)
    {
      if (e.ChangeType != WatcherChangeTypes.Changed)
        return;

      if (this.ReadingInProcess)
        return;

      this.ReadingInProcess = true;
      using (var fileStream = new FileStream(this.sourceFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
      using (var streamReader = new StreamReader(fileStream, this.Encoding))
      {
        var linesCount = 0;
        while (streamReader.Peek() >= 0)
        {
          var line = streamReader.ReadLine();
          if (line == null)
            break;
          linesCount++;
          if (linesCount <= this.Messages.Count)
            continue;
          try
          {
            this.Messages.Add(new Message(line));
          }
          catch
          {
            continue;
          }
        }
      }
      this.ReadingInProcess = false;
    }
  }
}
