using VisualLog.Core;

namespace VisualLog.Desktop.FormatManager
{
  public class MessageFormatterViewModel : ViewModelBase
  {
    public string Name
    {
      get { return this.formatter.Name; }
      set { this.formatter.Name = value; this.OnPropertyChanged(); }
    }

    public string Pattern
    {
      get { return this.formatter.Pattern; }
      set { this.formatter.Pattern = value; this.OnPropertyChanged(); }
    }

    public int Priority
    {
      get { return this.formatter.Priority; }
      set { this.formatter.Priority = value; this.OnPropertyChanged(); }
    }

    public MessageFormatter Formatter
    {
      get { return this.formatter; }
      set { this.formatter = value; this.OnPropertyChanged(); }
    }

    private MessageFormatter formatter;

    public MessageFormatterViewModel(MessageFormatter formatter) : this()
    {
      this.Formatter = formatter;
    }

    public MessageFormatterViewModel()
    {

    }
  }
}
