using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Threading;
using VisualLog.Core;
using VisualLog.Desktop.Settings;

namespace VisualLog.Desktop.LogManager
{
  public class LogViewModel : ViewModelBase, IDisposable
  {
    private static readonly NLog.Logger log = NLog.LogManager.GetCurrentClassLogger();

    public Command ShowSearchPanelCommand { get; private set; }
    public Command ShowSelectedLinesCommand { get; private set; }
    public Command CloseCommand { get; private set; }
    public Command ShowCommand { get; private set; }
    public Command OpenInExplorerCommand { get; private set; }

    public event Action<LogViewModel> CloseRequested;
    public event Action<LogViewModel> ShowRequested;
    public event Action<double> ShowLineRequested;
    public event Action SearchFocusRequested;

    public ObservableCollection<MessageInlineViewModel> LogMessages { get; set; }
    
    private readonly List<MessageInlineViewModel> pendingMessages = new List<MessageInlineViewModel>();
    private readonly DispatcherTimer batchTimer;
    private const int BatchSize = 1000;
    private const int BatchTimeoutMs = 500;

    public SelectedMessagesViewModel SelectedMessagesViewModel
    {
      get { return this.selectedMessagesViewModel; }
      set {
        this.selectedMessagesViewModel = value;
        this.OnPropertyChanged();
      }
    }
    private SelectedMessagesViewModel selectedMessagesViewModel;
    protected List<IMessage> selectedMessages;

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
    /// <summary>
    /// True while this log is the one shown in the log manager.
    /// Every opened log has its own view; inactive ones are kept alive but hidden.
    /// </summary>
    public bool IsActive
    {
      get { return this.isActive; }
      set
      {
        if (this.isActive == value)
          return;
        this.isActive = value;
        this.OnPropertyChanged();
      }
    }
    private bool isActive;

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
    public Guid Guid { get; private set; }

    /// <summary>
    /// Application-wide line highlighting. When the rules change, the lines of this log are re-styled.
    /// </summary>
    public HighlightingService Highlighting
    {
      get { return this.highlighting; }
      set
      {
        if (ReferenceEquals(this.highlighting, value))
          return;
        if (this.highlighting != null)
          this.highlighting.Changed -= this.OnHighlightingChanged;
        this.highlighting = value;
        if (this.highlighting != null)
          this.highlighting.Changed += this.OnHighlightingChanged;
        this.OnHighlightingChanged();
      }
    }
    private HighlightingService highlighting;
    private readonly Func<LineHighlighter> highlighterSource;

    public LogViewModel(string path) : this()
    {
      this.LogPath = path;
    }
    
    public LogViewModel()
    {
      this.Guid = Guid.NewGuid();
      this.highlighterSource = () => this.highlighting?.Current;
      this.LogMessages = new ObservableCollection<MessageInlineViewModel>();
      this.selectedMessages = new List<IMessage>();
      this.SelectedMessagesViewModel = new SelectedMessagesViewModel();
      this.Encodings = new List<string>();
      this.LogFormats = new List<Format>();
      this.State = new LogViewStateViewModel();
      this.SearchViewModel = new SearchViewModel(this);
      this.InitEncodings();
      this.PropertyChanged += SelectedEncoding_PropertyChanged;
      this.InitCommands();
      this.batchTimer = new DispatcherTimer(TimeSpan.FromMilliseconds(BatchTimeoutMs), DispatcherPriority.Background, this.ProcessBatch, Application.Current?.Dispatcher ?? Dispatcher.CurrentDispatcher);
      this.batchTimer.Stop();
    }

    private void InitCommands()
    {
      // Ctrl+F: the view moves the focus to the query box; the results panel opens if there are results
      this.ShowSearchPanelCommand = new Command(
        x => {
          if (this.SearchViewModel.HasQuery)
            this.State.ShowSearchPanel = true;
          this.SearchFocusRequested?.Invoke();
        },
        x => true
      );
      this.ShowSelectedLinesCommand = new Command(
        x => {
          this.ShowSelectedMessages(this.selectedMessages);
        },
        x => true
      );
      this.CloseCommand = new Command(
        x => {
          this.CloseRequested?.Invoke(this);
        },
        x => true
      );
      this.ShowCommand = new Command(
        x => { 
          this.ShowRequested?.Invoke(this);
        },
        x => true
      );
      this.OpenInExplorerCommand = new Command(
        x => {
          var fileInfo = new FileInfo(this.LogPath);
          if (fileInfo.Exists)
            System.Diagnostics.Process.Start("explorer.exe", fileInfo.Directory.FullName);
        },
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

      this.FlushPendingMessages();
      this.LogMessages.Clear();
      if (this.Log != null)
      {
        this.Log.MessageCatched -= this.OnNewLogMessageCatched;
        this.Log = null;
      }
      this.logPath = path;
      var encoding = Encoding.Default;
      if (!string.IsNullOrWhiteSpace(this.SelectedEncoding))
        encoding = Encoding.GetEncoding(int.Parse(this.SelectedEncoding.Split(' ')[0]));
      log.Debug($"Read: '{path}', encoding {encoding.WebName}");
      var watch = System.Diagnostics.Stopwatch.StartNew();
      try
      {
        this.Log = new Log(this.logPath, encoding);
        this.Log.MessageCatched += this.OnNewLogMessageCatched;
        this.Log.Read();
        this.FlushPendingMessages();
      }
      catch (Exception ex)
      {
        log.Error(ex, $"Read failed: '{path}'");
        throw;
      }
      log.Info($"Read: '{path}', {this.LogMessages.Count:N0} lines in {watch.ElapsedMilliseconds} ms");
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
        var messageViewModel = new MessageInlineViewModel(message);
        messageViewModel.HighlighterSource = this.highlighterSource;

        // UI thread safety
        if (Application.Current != null)
          Application.Current.Dispatcher.Invoke(() => { this.AddToBatch(messageViewModel); });
        else
          this.AddToBatch(messageViewModel);
      }
      catch (Exception ex)
      {
        // Called from the file watcher thread: an exception here would be lost silently
        log.Error(ex, $"New line of '{this.logPath}' was not added");
      }
    }

    #region Selected messages

    public void AddToSelection(IMessage message)
    {
      this.selectedMessages.Add(message);
    }

    public void RemoveFromSelection(IMessage message)
    {
      this.selectedMessages.Remove(message);
    }

    private void ShowSelectedMessages(IEnumerable<IMessage> messages)
    {
      this.SelectedMessagesViewModel = new SelectedMessagesViewModel(messages);
      this.SelectedMessagesViewModel.CloseRequested += this.OnSelectedLinesCloseRequested;
      this.State.ShowSelectedMessagesPanel = true;
    }

    private void OnSelectedLinesCloseRequested()
    {
      this.State.ShowSelectedMessagesPanel = false;
      if (this.SelectedMessagesViewModel != null)
      {
        this.SelectedMessagesViewModel.CloseRequested -= this.OnSelectedLinesCloseRequested;
        this.SelectedMessagesViewModel = null;
      }
    }


    #endregion

    private void AddToBatch(MessageInlineViewModel messageViewModel)
    {
      this.pendingMessages.Add(messageViewModel);
      if (this.pendingMessages.Count >= BatchSize)
        this.ProcessBatchNow();
      else if (!this.batchTimer.IsEnabled)
        this.batchTimer.Start();
    }

    private void ProcessBatch(object sender, EventArgs e)
    {
      this.batchTimer.Stop();
      this.ProcessBatchNow();
    }

    private void ProcessBatchNow()
    {
      if (this.pendingMessages.Count == 0)
        return;

      var messagesToAdd = this.pendingMessages.ToList();
      this.pendingMessages.Clear();
      foreach (var message in messagesToAdd)
        this.LogMessages.Add(message);
    }

    public void FlushPendingMessages()
    {
      this.batchTimer.Stop();
      this.ProcessBatchNow();
    }

    private void OnHighlightingChanged()
    {
      // Only lines that were already shown hold a computed look; the rest are matched lazily
      foreach (var message in this.LogMessages)
        message.RefreshHighlight();
    }

    public void Dispose()
    {
      this.FlushPendingMessages();
      this.batchTimer?.Stop();
      // Don't let the application-wide service keep a closed log alive
      this.Highlighting = null;
    }

    /// <summary>Line of the log by its 1-based number, null if there is no such line.</summary>
    private MessageInlineViewModel GetLogLine(int lineNumber)
    {
      var index = lineNumber - 1;
      return index >= 0 && index < this.LogMessages.Count ? this.LogMessages[index] : null;
    }

    public void ShowLogLine(SearchEntryViewModel searchEntryViewModel)
    {
      var line = searchEntryViewModel?.SearchEntry == null ? null : this.GetLogLine(searchEntryViewModel.SearchEntry.LineNumber);
      if (line == null)
        return;
      // Scroll so that the line is not glued to the top edge
      var index = searchEntryViewModel.SearchEntry.LineNumber - 1;
      this.ShowLineRequested?.Invoke(index > 10 ? index - 10 : 0);
      this.SelectedLogMessage = line;
    }

    public void SelectLogLines(IEnumerable<SearchEntryViewModel> searchEntries)
    {
      var entries = searchEntries?.Where(x => x?.SearchEntry != null).ToList() ?? new List<SearchEntryViewModel>();
      this.SelectedLogMessage = null;
      var firstEntry = entries.FirstOrDefault();
      this.SelectedLogMessage = firstEntry != null ? this.GetLogLine(firstEntry.SearchEntry.LineNumber) : null;
      this.ShowSelectedMessages(entries.Select(x => x.SearchEntry.Message));
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
