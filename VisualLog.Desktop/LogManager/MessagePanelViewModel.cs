using VisualLog.Core;

namespace VisualLog.Desktop.LogManager
{
  public class MessagePanelViewModel : ViewModelBase
  {
    public Message Message
    {
      get { return this.message; }
      set {
        this.message = value;
        this.OnPropertyChanged();
        this.SetStackTrace();
      }
    }
    private Message message;

    public string StackTrace
    {
      get { return this.stackTrace; }
      set
      {
        this.stackTrace = value;
        this.OnPropertyChanged();
      }
    }
    private string stackTrace;

    public MessagePanelViewModel(Message message) : this()
    {
      this.Message = message;
    }

    public MessagePanelViewModel() { }

    private void SetStackTrace()
    {
      this.StackTrace = message.Parts.ContainsKey("stack") ? message.Parts["stack"] : "stack not found";
    }
  }
}
