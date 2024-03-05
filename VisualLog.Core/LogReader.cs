using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VisualLog.Core
{
  public class LogReader
  {
    public string Path { get; private set; }
    public Encoding Encoding { get; private set; }
    public long ReadedBytes { get; private set; }
    public bool ReadingInProcess { get; private set; }
    public event Action<string> LineReaded
    {
      add
      {
        if (this.lineReaded == null || !this.lineReaded.GetInvocationList().Contains(value))
          this.lineReaded += value;
      }
      remove
      {
        this.lineReaded -= value;
      }
    }

    private Action<string> lineReaded;
    private FileSystemWatcher fileWatcher;

    public LogReader(string path)
    {
      this.Path = path;
      this.Encoding = Encoding.Default;
    }

    public void SetEncoding(Encoding encoding)
    {
      this.Encoding = encoding;
    }

    public void StartReading()
    {
      this.ReadingInProcess = true;
      this.ReadedBytes = 0;
      try
      {
        this.ReadFromOffset(0);
        this.SubscribeUpdates();
      }
      catch (Exception ex)
      {
        // TODO: !!!
      }
      finally
      {
        this.ReadingInProcess = false;
      }
    }

    public void SubscribeUpdates()
    {
      var fileInfo = new FileInfo(this.Path);
      Task.Run(() =>
      {
        using (var fileWatcher = new FileWatcher(fileInfo))
        {
          fileWatcher.FileSizeChanged += this.ReadUpdates;
          fileWatcher.StartWatch();
        }
      });
    }

    public void ReadUpdates()
    {
      if (this.ReadingInProcess)
        return;

      this.ReadingInProcess = true;
      try
      {
        this.ReadFromOffset(this.ReadedBytes);
      }
      catch (Exception ex)
      {
        // TODO: !!!
      }
      finally
      {
        this.ReadingInProcess = false;
      }
    }

    public void ReadFromOffset(long offset)
    {
      using (var fileStream = new FileStream(this.Path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
      using (var streamReader = new StreamReader(fileStream, this.Encoding))
      {
        streamReader.BaseStream.Seek(offset, SeekOrigin.Begin);
        while (streamReader.Peek() >= 0)
        {
          var line = streamReader.ReadLine();
          if (string.IsNullOrEmpty(line))
            continue;
          if (this.lineReaded != null)
            this.lineReaded.Invoke(line);
        }
        this.ReadedBytes = streamReader.BaseStream.Length;
      }
    }
  }
}
