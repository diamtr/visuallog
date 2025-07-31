using VisualLog.Core;

namespace VisualLog.Desktop.LogManager
{
  public class MessageViewModelBase : ViewModelBase
  {
    private IMessage message;
    public IMessage Message
    {
      get { return this.message; }
      set
      {
        this.message = value;
        this.OnPropertyChanged();
        this.SetItems();
      }
    }

    private string stackTrace;
    public string StackTrace
    {
      get { return this.stackTrace; }
      set
      {
        this.stackTrace = value;
        this.OnPropertyChanged();
      }
    }
    
    public MessageViewModelBase(IMessage message) : this()
    {
      this.Message = message;
    }
    public MessageViewModelBase() : base() { }

    protected void SetItems()
    {
      this.StackTrace = message.Items.ContainsKey("StackTrace") ? message.Items["StackTrace"].ToString() : "stack not found";
    }
  }
}
