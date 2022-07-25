using VisualLog.Core;

namespace VisualLog.Desktop.FormatManager
{
  public class LogFormatViewModel : ViewModelBase
  {
    public LogFormat Format
    {
      get { return this.format; }
      set { this.format = value; this.OnPropertyChanged(); }
    }

    private LogFormat format;
  }
}
