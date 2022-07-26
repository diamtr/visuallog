using System.Collections.ObjectModel;
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
    public ObservableCollection<MessageFormatterViewModel> Formatters { get; set; }
    public MessageFormatterViewModel SelectedFormatter
    {
      get { return this.selectedFormatter; }
      set { this.selectedFormatter = value; this.OnPropertyChanged(); }
    }
    public Command AddNewFormatterCommand { get; private set; }

    private LogFormat format;
    private MessageFormatterViewModel selectedFormatter;

    public LogFormatViewModel(LogFormat format) : this()
    {
      this.format = format;
    }

    public LogFormatViewModel()
    {
      this.Formatters = new ObservableCollection<MessageFormatterViewModel>();
      this.InitCommands();
    }

    public void AddNewFormatter()
    {
      var formatterViewModel = new MessageFormatterViewModel();
      this.Formatters.Add(formatterViewModel);
    }

    private void InitCommands()
    {
      this.AddNewFormatterCommand = new Command(
        x => { this.AddNewFormatter(); },
        x => true
        );
    }
  }
}
