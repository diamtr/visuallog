using VisualLog.Core;

namespace VisualLog.Desktop.LogManager
{
  public class MessageInlineViewModel : MessageViewModelBase
  {
    public bool CopyEnabled
    {
      get { return this.copyEnabled; }
      set {
        this.copyEnabled = value;
        this.OnPropertyChanged();
      }
    }
    private bool copyEnabled;

    public MessageInlineViewModel(IMessage message) : base(message) { }
    public MessageInlineViewModel() : base() { }
  }
}
