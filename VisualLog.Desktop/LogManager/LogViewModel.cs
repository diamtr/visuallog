using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Threading;
using VisualLog.Core;
using VisualLog.Core.Search;
using VisualLog.Desktop.Settings;

namespace VisualLog.Desktop.LogManager
{
  public class LogViewModel : ViewModelBase, IDisposable, ILineNavigator
  {
    private static readonly NLog.Logger log = NLog.LogManager.GetCurrentClassLogger();

    /// <summary>Ctrl+F. Parameter: text selected in the log - searched for if it is a part of one line.</summary>
    public Command ShowSearchPanelCommand { get; private set; }
    /// <summary>Searches for the text given as the parameter (a part of one line).</summary>
    public Command SearchTextCommand { get; private set; }
    public Command CloseCommand { get; private set; }
    public Command ShowCommand { get; private set; }
    public Command OpenInExplorerCommand { get; private set; }

    public event Action<LogViewModel> CloseRequested;
    public event Action<LogViewModel> ShowRequested;
    /// <summary>The view scrolls to the line of the entry and selects its first match.</summary>
    public event Action<SearchEntry> ShowLineRequested;
    public event Action SearchFocusRequested;
    /// <summary>The look of the lines changed (highlighting rules): the view redraws them.</summary>
    public event Action LinesRestyled;

    public ObservableCollection<MessageInlineViewModel> LogMessages { get; set; }

    private readonly List<MessageInlineViewModel> pendingMessages = new List<MessageInlineViewModel>();
    private readonly DispatcherTimer batchTimer;
    private const int BatchSize = 1000;
    private const int BatchTimeoutMs = 500;

    /// <summary>The line the log was last moved to by the search.</summary>
    public MessageInlineViewModel ShownLine
    {
      get { return this.shownLine; }
      private set
      {
        this.shownLine = value;
        this.OnPropertyChanged();
      }
    }
    private MessageInlineViewModel shownLine;

    /// <summary>Details of the line chosen in the log, shown in the panel beside it; null - the panel is closed.</summary>
    public MessageDetailsViewModel Details
    {
      get { return this.details; }
      private set
      {
        if (this.details is JsonMessageDetailsViewModel shownJson)
          this.jsonDetailsAsTree = shownJson.IsTreeView;
        if (this.details != null)
          this.details.CloseRequested -= this.HideDetails;
        this.details = value;
        if (this.details != null)
        {
          this.details.CloseRequested += this.HideDetails;
          this.details.Navigator = this;
        }
        // The chosen look of JSON lines (tree or text) is kept from line to line
        if (this.details is JsonMessageDetailsViewModel json)
          json.IsTreeView = this.jsonDetailsAsTree;
        this.OnPropertyChanged();
      }
    }
    private MessageDetailsViewModel details;
    private bool jsonDetailsAsTree = true;

    /// <summary>Application-wide look of the logs (line numbers...).</summary>
    public DisplaySettings Display
    {
      get { return this.display; }
      set
      {
        this.display = value;
        this.OnPropertyChanged();
      }
    }
    private DisplaySettings display = new DisplaySettings();

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
      // Ctrl+F: the view moves the focus to the query box; the results panel opens if there are results.
      // With a part of a line selected in the log, that text is searched for.
      this.ShowSearchPanelCommand = new Command(
        x => {
          if (SearchViewModel.IsSearchableText(x as string))
            _ = this.SearchViewModel.SearchForTextAsync((string)x);
          else if (this.SearchViewModel.HasQuery)
            this.State.ShowSearchPanel = true;
          this.SearchFocusRequested?.Invoke();
        },
        x => true
      );
      this.SearchTextCommand = new Command(
        x => _ = this.SearchViewModel.SearchForTextAsync(x as string),
        x => SearchViewModel.IsSearchableText(x as string)
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
      this.LinesRestyled?.Invoke();
    }

    public void Dispose()
    {
      this.FlushPendingMessages();
      this.batchTimer?.Stop();
      // Don't let the application-wide service keep a closed log alive
      this.Highlighting = null;
    }

    /// <summary>Line of the log by its 1-based number, null if there is no such line.</summary>
    public MessageInlineViewModel GetLogLine(int lineNumber)
    {
      var index = lineNumber - 1;
      return index >= 0 && index < this.LogMessages.Count ? this.LogMessages[index] : null;
    }

    /// <summary>Opens the details panel for the line by its 1-based number; nothing if there is no such line.</summary>
    public void ShowDetails(int lineNumber)
    {
      var line = this.GetLogLine(lineNumber);
      if (line == null)
        return;
      this.Details = MessageDetailsViewModel.From(line.Message?.RawValue, lineNumber);
    }

    public void HideDetails()
    {
      this.Details = null;
    }

    /// <summary>
    /// The line the details go to from a line: by the search results if there are any, otherwise by all the lines.
    /// 0 - there is no line in that direction.
    /// </summary>
    public int FindLine(int fromLineNumber, int direction)
    {
      var entries = this.SearchViewModel.SearchEntries;
      if (entries.Count == 0)
      {
        var lineNumber = fromLineNumber + Math.Sign(direction);
        return this.GetLogLine(lineNumber) != null ? lineNumber : 0;
      }
      var index = NeighbourEntryIndex(entries, fromLineNumber, direction);
      return index >= 0 ? LineNumberOf(entries[index]) : 0;
    }

    /// <summary>Shows the details of the line and moves the log (and the current search result, if it is one) to it.</summary>
    public void GoToLine(int lineNumber)
    {
      var line = this.GetLogLine(lineNumber);
      if (line == null)
        return;
      this.ShowDetails(lineNumber);
      var entries = this.SearchViewModel.SearchEntries;
      var index = entries.Count == 0 ? -1 : NeighbourEntryIndex(entries, lineNumber - 1, 1);
      if (index >= 0 && LineNumberOf(entries[index]) == lineNumber)
      {
        // Moves the log to the line too
        this.SearchViewModel.CurrentEntry = entries[index];
        return;
      }
      this.State.FollowTail = false;
      this.ShownLine = line;
      this.ShowLineRequested?.Invoke(new SearchEntry { LineNumber = lineNumber });
    }

    /// <summary>
    /// Index of the first result after the line (direction &gt; 0) or of the last one before it; -1 - none.
    /// The results go by line number: the log is searched from the start, new lines are appended.
    /// </summary>
    private static int NeighbourEntryIndex(IList<SearchEntryViewModel> entries, int lineNumber, int direction)
    {
      // The first result after the line
      int low = 0, high = entries.Count;
      while (low < high)
      {
        var middle = low + (high - low) / 2;
        if (LineNumberOf(entries[middle]) <= lineNumber)
          low = middle + 1;
        else
          high = middle;
      }
      if (direction > 0)
        return low < entries.Count ? low : -1;
      var before = low - 1;
      while (before >= 0 && LineNumberOf(entries[before]) >= lineNumber)
        before--;
      return before;
    }

    private static int LineNumberOf(SearchEntryViewModel entry)
    {
      return entry?.SearchEntry?.LineNumber ?? 0;
    }

    public void ShowLogLine(SearchEntryViewModel searchEntryViewModel)
    {
      var entry = searchEntryViewModel?.SearchEntry;
      var line = entry == null ? null : this.GetLogLine(entry.LineNumber);
      if (line == null)
        return;
      this.State.FollowTail = false;
      this.ShownLine = line;
      this.ShowLineRequested?.Invoke(entry);
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
