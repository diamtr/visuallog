using System.Collections.Generic;
using System.Collections.ObjectModel;
using VisualLog.Core;

namespace VisualLog.Desktop.FormatManager
{
  public class FormatManagerViewModel : ViewModelBase
  {
    public ObservableCollection<LogFormat> Formats { get; private set; }
    public LogFormat SelectedFormat
    {
      get
      {
        return this.selectedFormat;
      }
      set
      {
        this.selectedFormat = value;
        this.OnPropertyChanged();
      }
    }
    public Command CreateFormatCommand { get; private set; }

    private LogFormat selectedFormat;

    public FormatManagerViewModel()
    {
      this.Formats = new ObservableCollection<LogFormat>();
      this.InitCommands();
    }

    public void CreateFormat()
    {
      var format = new LogFormat() { Name = "<NEW FORMAT>" };
      this.Formats.Add(format);
      this.SelectedFormat = format;
    }

    private void InitCommands()
    {
      this.CreateFormatCommand = new Command(
        x => { this.CreateFormat(); },
        x => true
        );
    }
  }
}
