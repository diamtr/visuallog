using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace VisualLog.Core
{
  public class FileWatcher : IDisposable
  {
    public const int DefaultWatchInterval = 250;
    public event Action FileSizeChanged;
    private bool disposed = false;
    protected FileInfo fileInfo;
    protected System.Timers.Timer timer;
    protected int elapsedEventsCount;
    protected int ticksLimit = -1;
    protected int watchInterval;
    protected long previousFileSize;

    #region IDisposable

    public void Dispose()
    {
      Dispose(disposing: true);
      GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
      if (this.disposed)
        return;

      if (disposing)
      {
        this.fileInfo = null;
        if (this.timer != null)
        {
          this.timer.Stop();
          this.timer.Dispose();
        }
      }

      this.disposed = true;
    }

    #endregion

    public FileWatcher(FileInfo fileInfo, int watchInterval) : this(fileInfo: fileInfo)
    {
      this.watchInterval = watchInterval;
    }

    public FileWatcher(FileInfo fileInfo)
    {
      this.fileInfo = fileInfo;
      this.watchInterval = DefaultWatchInterval;
    }

    public void StartWatch(int ticksLimit = -1)
    {
      this.ResetTimer();
      this.timer.Elapsed += this.TicksControl;
      this.timer.Elapsed += this.FileSizeControl;
      this.ticksLimit = ticksLimit;
      this.timer.Enabled = true;
      Task.Run(() => { Thread.Sleep(ticksLimit > 0 ? ticksLimit * this.watchInterval : int.MaxValue); }).Wait();
    }

    public void StopWatch()
    {
      if (this.timer == null)
        return;
      this.timer.Stop();
      this.timer.Dispose();
    }

    protected void ResetTimer()
    {
      this.StopWatch();
      this.timer = new System.Timers.Timer();
      this.timer.Interval = this.watchInterval;
      this.timer.AutoReset = true;
    }

    protected void TicksControl(object sender, ElapsedEventArgs e)
    {
      if (this.ticksLimit < 0)
        return;
      if (this.ticksLimit > this.elapsedEventsCount)
        return;
      this.StopWatch();
    }

    protected void FileSizeControl(object sender, ElapsedEventArgs e)
    {
      this.elapsedEventsCount++;
      this.fileInfo.Refresh();
      if (fileInfo.Length == this.previousFileSize)
        return;
      this.previousFileSize = fileInfo.Length;

      if (this.FileSizeChanged != null)
        this.FileSizeChanged.Invoke();
    }
  }
}
