using System.Linq;
using VisualLog.Core;

namespace VisualLog.Desktop.LogManager
{
  public class MessageInlineViewModel : ViewModelBase
  {
    public bool OnlyRaw
    {
      get
      { 
        return this.onlyRaw;
      }
      set
      {
        this.onlyRaw = value;
        this.OnPropertyChanged();
      }
    }
    public bool CopyEnabled
    {
      get
      {
        return this.copyEnabled;
      }
      set
      {
        this.copyEnabled = value;
        this.OnPropertyChanged();
      }
    }

    public string StackTrace
    {
      get
      {
        return this.stackTrace;
      }
      set
      {
        this.stackTrace = value;
        this.OnPropertyChanged();
      }
    }

    public Message Message
    {
      get { return this.message; }
    }

    private Message message;
    private bool onlyRaw;
    private bool copyEnabled;
    private string stackTrace;

    public MessageInlineViewModel(Message message)
    {
      this.message = message;
      this.OnlyRaw = message.Parts == null || !message.Parts.Any();
      this.StackTrace = message.Parts.ContainsKey("stack") ? message.Parts["stack"] : "stack not found";
    }
  }
}
