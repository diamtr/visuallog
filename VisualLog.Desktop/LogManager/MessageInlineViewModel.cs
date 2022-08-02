using System.Linq;
using VisualLog.Core;

namespace VisualLog.Desktop
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
    public Message Message
    {
      get { return this.message; }
    }

    private Message message;
    private bool onlyRaw;

    public MessageInlineViewModel(Message message)
    {
      this.message = message;
      this.OnlyRaw = message.Parts == null || !message.Parts.Any();
    }
  }
}
