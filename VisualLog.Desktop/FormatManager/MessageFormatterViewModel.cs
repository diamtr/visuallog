using VisualLog.Core;

namespace VisualLog.Desktop.FormatManager
{
  public class MessageFormatterViewModel : ViewModelBase
  {
    public MessageFormatter Formatter
    {
      get { return this.formatter; }
      set { this.formatter = value; this.OnPropertyChanged(); }
    }

    private MessageFormatter formatter;

    public MessageFormatterViewModel(MessageFormatter formatter) : this()
    {
      this.formatter = formatter;
    }

    public MessageFormatterViewModel()
    {

    }
  }
}
