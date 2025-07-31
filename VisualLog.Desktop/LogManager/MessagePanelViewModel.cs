using VisualLog.Core;

namespace VisualLog.Desktop.LogManager
{
  public class MessagePanelViewModel : MessageViewModelBase
  {
    public MessagePanelViewModel(IMessage message) : base(message) { }
    public MessagePanelViewModel() : base() { }
  }
}
