using System.Collections.Generic;
using System.Collections.ObjectModel;
using VisualLog.Core;

namespace VisualLog.Desktop.FormatManager
{
  public class FormatManagerViewModel : ViewModelBase
  {
    public ObservableCollection<LogFormatViewModel> Formats { get; private set; }
    public LogFormatViewModel SelectedFormat
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

    private LogFormatViewModel selectedFormat;

    public FormatManagerViewModel()
    {
      this.Formats = new ObservableCollection<LogFormatViewModel>();
      this.InitCommands();
    }

    public void CreateFormat()
    {
      var format = new LogFormat() { Name = "<NEW FORMAT>" };
      var formatVm = new LogFormatViewModel() { Format = format };
      this.Formats.Add(formatVm);
      this.SelectedFormat = formatVm;
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
