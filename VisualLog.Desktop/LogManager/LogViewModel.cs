using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using VisualLog.Core;

namespace VisualLog.Desktop.LogManager
{
  public class LogViewModel : ViewModelBase
  {
    public Command ShowSearchPanelCommand { get; private set; }
    public Command CloseCommand { get; private set; }
    public event Action<LogViewModel> CloseRequested;
    public event Action<double> ShowLineRequested;
    public ObservableCollection<MessageInlineViewModel> LogMessages { get; set; }
    public SelectedMessagesViewModel SelectedLogMessages
    {
      get { return this.selectedLogMessages; }
      set {
        this.selectedLogMessages = value;
        this.OnPropertyChanged();
      }
    }
    private SelectedMessagesViewModel selectedLogMessages;
    public MessageInlineViewModel SelectedLogMessage
    { 
      get
      {
        return this.selectedLogMessage;
      }
      set
      {
        this.selectedLogMessage = value;
        this.OnPropertyChanged();
      }
    }
    private MessageInlineViewModel selectedLogMessage;
    
    public string DisplayName
    {
      get
      {
        return this.displayName;
      }
      set
      {
        this.displayName = value;
        this.OnPropertyChanged();
      }
    }
    private string displayName;
    public List<string> Encodings { get; set; }
    public string LogPath
    {
      get
      {
        return this.logPath;
      }
      set
      {
        this.logPath = value;
        this.OnPropertyChanged();
        this.DisplayName = Path.GetFileName(this.logPath);
      }
    }
    private string logPath;
    public string SelectedEncoding
    {
      get
      {
        return this.selectedEncoding;
      }
      set
      {
        this.selectedEncoding = value;
        this.OnPropertyChanged();
      }
    }
    private string selectedEncoding;
    public List<Format> LogFormats { get; set; }
    public LogViewStateViewModel State { get; set; }
    public SearchViewModel SearchViewModel { get; set; }
    public Log Log { get; private set; }

    public LogViewModel(string path) : this()
    {
      this.LogPath = path;
    }
    
    public LogViewModel()
    {
      this.LogMessages = new ObservableCollection<MessageInlineViewModel>();
      this.SelectedLogMessages = new SelectedMessagesViewModel();
      this.Encodings = new List<string>();
      this.LogFormats = new List<Format>();
      this.State = new LogViewStateViewModel();
      this.SearchViewModel = new SearchViewModel(this);
      this.InitEncodings();
      this.PropertyChanged += SelectedEncoding_PropertyChanged;
      this.InitCommands();
    }

    private void InitCommands()
    {
      this.ShowSearchPanelCommand = new Command(
        x => this.State.ShowSearchPanel = true,
        x => true
      );
      this.CloseCommand = new Command(
        x => { if (this.CloseRequested != null) this.CloseRequested.Invoke(this); },
        x => true
      );
    }

    public void ReadLog()
    {
      if (string.IsNullOrWhiteSpace(this.logPath) ||
          !File.Exists(this.logPath))
        return;

      this.ReadLog(this.logPath);
    }

    public void ReadLog(string path)
    {
      if (string.IsNullOrWhiteSpace(path) ||
          !File.Exists(path))
        return;

      this.LogMessages.Clear();
      if (this.Log != null)
      {
        this.Log.CatchNewMessage -= this.OnNewLogMessageCatched;
        this.Log = null;
      }
      this.logPath = path;
      var encoding = Encoding.Default;
      if (!string.IsNullOrWhiteSpace(this.SelectedEncoding))
        encoding = Encoding.GetEncoding(int.Parse(this.SelectedEncoding.Split(' ')[0]));
      this.Log = new Log(this.logPath, encoding);
      this.Log.CatchNewMessage += this.OnNewLogMessageCatched;
      this.Log.Read();
    }

    public void FollowTail()
    {
      this.State.FollowTail = true;
      if (this.Log == null)
        return;
    }

    public void OnNewLogMessageCatched(Message message)
    {
      if (message == null)
        return;
      try
      {
        // UI thread safety
        if (Application.Current != null)
          Application.Current.Dispatcher.Invoke(() => { this.LogMessages.Add(new MessageInlineViewModel(message)); });
        else
          this.LogMessages.Add(new MessageInlineViewModel(message));
      }
      catch
      {
      }
    }

    public void ShowLogLine(SearchEntryViewModel searchEntryViewModel)
    {
      var lineNumber = searchEntryViewModel.SearchEntry.LineNumber - 1;
      if (this.ShowLineRequested != null)
        this.ShowLineRequested.Invoke(lineNumber > 10 ? lineNumber - 10 : 0);
      this.SelectedLogMessage = this.LogMessages[lineNumber];
    }

    public void SelectLogLines(IEnumerable<SearchEntryViewModel> searchEntries)
    {
      this.SelectedLogMessage = null;
      var firstEntry = searchEntries.FirstOrDefault();
      this.SelectedLogMessage = firstEntry != null ? this.LogMessages[firstEntry.SearchEntry.LineNumber] : null;
      this.SelectedLogMessages.SetMessages(searchEntries);
    }

    public void InitEncodings()
    {
      var encodings = Encoding.GetEncodings();
      this.Encodings.AddRange(encodings.Select(x => this.GetEncodingDisplayName(x.GetEncoding())));
      this.OnPropertyChanged(nameof(this.Encodings));
      this.SelectedEncoding = this.GetEncodingDisplayName(Encoding.UTF8);
    }

    public string GetEncodingDisplayName(Encoding encoding)
    {
      return $"{encoding.CodePage} {encoding.WebName} {encoding.EncodingName}";
    }

    private void SelectedEncoding_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
      if (sender == null || e.PropertyName != nameof(SelectedEncoding))
        return;

      this.ReadLog();
    }
  }
}
