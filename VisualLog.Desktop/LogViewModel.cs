using System.Collections.Generic;
using System.Linq;
using VisualLog.Core;

namespace VisualLog.Desktop
{
  public class LogViewModel : ViewModelBase
  {
    public List<string> LogMessages { get; set; }

    private Log log;

    public LogViewModel()
    {
      this.LogMessages = new List<string>();
    }

    public void ReadLog(string path)
    {
      if (string.IsNullOrWhiteSpace(path) ||
          !System.IO.File.Exists(path))
        return;

      this.log = new Log();
      this.log.Read(path);
      this.LogMessages = this.log.Messages.Select(x => x.RawValue).ToList();
      this.OnPropertyChanged(nameof(this.LogMessages));
    }
  }
}
