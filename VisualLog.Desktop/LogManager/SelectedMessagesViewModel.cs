using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using VisualLog.Core;

namespace VisualLog.Desktop.LogManager
{
  public class SelectedMessagesViewModel : ViewModelBase
  {
    public Command ShowPreviousMessageCommand { get; private set; }
    public Command ShowNextMessageCommand { get; private set; }

    public Command CopyAllMessagesCommand { get; private set; }
    public Command CloseSelectedMessagesCommand { get; private set; }

    public event Action CloseRequested;

    public List<IMessage> Messages { get; protected set; }
    public ObservableCollection<MessageInlineViewModel> MessagesViewModels { get; protected set; }

    public int Index
    {
      get { return this.index; }
      set
      {
        this.index = value;
        this.OnPropertyChanged();
        this.OnPropertyChanged(nameof(this.Position));
        this.SetMessagePanelViewModel();
      }
    }
    private int index;
    public int Position {
      get { return this.Index + 1; }
    }

    public MessagePanelViewModel MessagePanelViewModel
    {
      get { return this.messagePanelViewModel; }
      set {
        this.messagePanelViewModel = value;
        this.OnPropertyChanged();
      }
    }
    private MessagePanelViewModel messagePanelViewModel;

    public SelectedMessagesViewModel(IEnumerable<IMessage> messages) : this()
    {
      this.Messages.AddRange(messages);
      foreach (var message in this.Messages)
        this.MessagesViewModels.Add(new MessageInlineViewModel(message));
      this.MessagePanelViewModel = new MessagePanelViewModel(this.Messages[this.Index]);
    }

    public SelectedMessagesViewModel()
    {
      this.Messages = new List<IMessage>();
      this.MessagesViewModels = new ObservableCollection<MessageInlineViewModel>();
      this.Index = 0;
      this.InitCommands();
    }

    private void InitCommands()
    {
      this.CloseSelectedMessagesCommand = new Command(
        x => { this.CloseRequested?.Invoke(); },
        x => true
        );
      this.ShowPreviousMessageCommand = new Command(
        x => { this.ShowPreviousMessage(); },
        x => true
        );
      this.ShowNextMessageCommand = new Command(
        x => { this.ShowNextMessage(); },
        x => true
        );
      this.CopyAllMessagesCommand = new Command(
        x => { this.CopyAllMessagesToClipboard(); },
        x => true
        );
    }

    private void SetMessagePanelViewModel()
    {
      if (!this.IndexInRange())
        return;

      this.MessagePanelViewModel = new MessagePanelViewModel(this.Messages[this.Index]);
    }

    private bool IndexInRange()
    {
      return this.Index >= 0 && this.Index <= this.Messages.Count - 1;
    }

    private void ShowPreviousMessage()
    {
      if (this.Index <= 0)
        return;
      this.Index--;
    }

    private void ShowNextMessage()
    {
      if (this.Index >= this.Messages.Count - 1)
        return;
      this.Index++;
    }

    private void CopyAllMessagesToClipboard()
    {
      var messages = new StringBuilder();
      foreach (var message in this.Messages.Select(x => x.RawValue))
        messages.AppendLine(message);
      Clipboard.SetText(messages.ToString());
    }
  }
}
