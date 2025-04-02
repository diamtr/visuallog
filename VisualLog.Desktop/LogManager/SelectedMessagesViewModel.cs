using System.Collections.ObjectModel;
using System.Linq;

namespace VisualLog.Desktop.LogManager
{
  public class SelectedMessagesViewModel : ViewModelBase
  {
    public Command ShowPreviousMessageCommand { get; private set; }
    public Command ShowNextMessageCommand { get; private set; }

    public ObservableCollection<MessagePanelViewModel> Messages { get; set; }
    public int Index
    {
      get { return this.index; }
      set
      {
        this.index = value;
        this.OnPropertyChanged();
        this.SetMessageToShow();
      }
    }
    private int index;
    public int Position {
      get { return this.MessageToShow == null ? 0 : this.Index + 1; }
    }
    public MessagePanelViewModel MessageToShow {
      get { return this.messageToShow; }
      set {
        this.messageToShow = value;
        this.OnPropertyChanged();
        this.OnPropertyChanged(nameof(this.Position));
      }
    }
    private MessagePanelViewModel messageToShow;

    public SelectedMessagesViewModel()
    {
      this.Messages = new ObservableCollection<MessagePanelViewModel>();
      this.Messages.CollectionChanged += Messages_CollectionChanged;
      this.Index = 0;
      this.ShowPreviousMessageCommand = new Command(
        x => { this.ShowPreviousMessage(); },
        x => true
        );
      this.ShowNextMessageCommand = new Command(
        x => { this.ShowNextMessage(); },
        x => true
        );
    }

    private void Messages_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
      if (this.Messages == null)
        return;

      if (!this.Messages.Any())
      {
        this.MessageToShow = null;
        return;
      }

      if (this.MessageToShow != null &&
          this.Messages.Contains(this.MessageToShow))
      {
        this.Index = this.Messages.IndexOf(this.MessageToShow);
        return;
      }

      if (!this.IndexInRange() && this.Index != 0)
      {
        this.Index = 0;
        return;
      }

      if (e.OldItems != null &&
          this.MessageToShow != null &&
          e.OldItems.Contains(this.MessageToShow) &&
          this.IndexInRange())
      {
        this.MessageToShow = this.Messages[this.Index];
        return;
      }

      if (this.MessageToShow == null &&
          this.IndexInRange())
        this.MessageToShow = this.Messages[this.Index];
    }

    private void SetMessageToShow()
    {
      if (!this.IndexInRange())
        return;

      this.MessageToShow = Messages[this.Index];
    }

    private bool IndexInRange()
    {
      return this.Index >= 0 && this.Index <= Messages.Count - 1;
    }

    private void ShowPreviousMessage()
    {
      if (this.Index <= 0)
        return;
      this.Index--;
    }

    private void ShowNextMessage()
    {
      if (this.Index >= Messages.Count - 1)
        return;
      this.Index++;
    }
  }
}
