using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using VisualLog.Core;
using VisualLog.Core.Search;

namespace VisualLog.Desktop.LogManager
{
  public enum SearchStatus
  {
    /// <summary>Nothing to search for.</summary>
    Idle,
    Searching,
    Found,
    NoMatches,
    InvalidRegex,
  }

  /// <summary>
  /// Line numbers of search results as a live collection (for <see cref="Controls.ScrollMarkersBar"/>).
  /// </summary>
  public sealed class LineNumbersView : IEnumerable<int>, INotifyCollectionChanged
  {
    private readonly ObservableCollection<SearchEntryViewModel> entries;

    public LineNumbersView(ObservableCollection<SearchEntryViewModel> entries)
    {
      this.entries = entries;
      // The owner of the collection outlives this view: forward without keeping anything else alive
      this.entries.CollectionChanged += (s, e) =>
        this.CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }

    public event NotifyCollectionChangedEventHandler CollectionChanged;

    public IEnumerator<int> GetEnumerator()
    {
      foreach (var entry in this.entries)
        if (entry?.SearchEntry != null)
          yield return entry.SearchEntry.LineNumber;
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => this.GetEnumerator();
  }

  /// <summary>
  /// Search in one opened log: the query box in the log toolbar and the results panel under the log.
  /// </summary>
  public class SearchViewModel : ViewModelBase
  {
    private static readonly NLog.Logger log = NLog.LogManager.GetCurrentClassLogger();

    /// <summary>Above this many lines the search runs only on Enter, not while typing.</summary>
    public const int SearchAsYouTypeLineLimit = 300_000;
    public const int HistoryLimit = 15;
    private const int TypingDelayMs = 300;

    /// <summary>Recent queries, shared by all opened logs.</summary>
    public static ObservableCollection<string> SharedHistory { get; } = new ObservableCollection<string>();

    public Command HideSearchPanelCommand { get; private set; }
    public Command ShowSearchPanelCommand { get; private set; }
    public Command SearchCommand { get; private set; }
    public Command NextCommand { get; private set; }
    public Command PreviousCommand { get; private set; }
    public Command ClearCommand { get; private set; }
    public Command GoToEntryCommand { get; private set; }
    public Command UseHistoryCommand { get; private set; }
    public Command CopyAllCommand { get; private set; }
    public Command CopySelectedCommand { get; private set; }
    public Command CopySelectedWithLineNumbersCommand { get; private set; }
    public Command OpenAllAsSelectionCommand { get; private set; }
    public Command OpenSelectedAsSelectionCommand { get; private set; }
    public Command ExportCommand { get; private set; }

    /// <summary>Puts text to the clipboard; replaceable in tests.</summary>
    public Action<string> SetClipboardText { get; set; } = text => Clipboard.SetText(text);

    /// <summary>Asks where to export the results; null - cancelled. Replaceable in tests.</summary>
    public Func<string, string> AskExportPath { get; set; } = AskExportPathWithDialog;

    public ObservableCollection<string> History => SharedHistory;

    public ObservableCollection<SearchEntryViewModel> SearchEntries
    {
      get { return this.searchEntries; }
      private set
      {
        this.searchEntries = value;
        this.MatchLineNumbers = new LineNumbersView(value);
        this.OnPropertyChanged();
        this.OnPropertyChanged(nameof(this.HasResults));
      }
    }
    private ObservableCollection<SearchEntryViewModel> searchEntries;

    /// <summary>Line numbers of the results, for the marks along the log scrollbar.</summary>
    public LineNumbersView MatchLineNumbers
    {
      get { return this.matchLineNumbers; }
      private set
      {
        this.matchLineNumbers = value;
        this.OnPropertyChanged();
      }
    }
    private LineNumbersView matchLineNumbers;

    /// <summary>Entries selected in the results list (set by the view).</summary>
    public IList<SearchEntryViewModel> SelectedEntries
    {
      get { return this.selectedEntries; }
      set
      {
        this.selectedEntries = value ?? new List<SearchEntryViewModel>();
        this.OnPropertyChanged();
        this.OnPropertyChanged(nameof(this.SelectedCount));
      }
    }
    private IList<SearchEntryViewModel> selectedEntries = new List<SearchEntryViewModel>();

    public int SelectedCount => this.selectedEntries.Count;

    public string StringToSearch
    {
      get { return this.stringToSearch; }
      set
      {
        if (this.stringToSearch == value)
          return;
        this.stringToSearch = value;
        this.OnPropertyChanged();
        this.OnPropertyChanged(nameof(this.HasQuery));
        this.ScheduleTypingSearch();
      }
    }
    private string stringToSearch;

    public bool HasQuery => !string.IsNullOrEmpty(this.stringToSearch);

    public bool UseRegex
    {
      get { return this.useRegex; }
      set
      {
        if (this.useRegex == value)
          return;
        this.useRegex = value;
        this.OnPropertyChanged();
        this.SearchAgainIfActive();
      }
    }
    private bool useRegex;

    public bool MatchCase
    {
      get { return this.matchCase; }
      set
      {
        if (this.matchCase == value)
          return;
        this.matchCase = value;
        this.OnPropertyChanged();
        this.SearchAgainIfActive();
      }
    }
    private bool matchCase;

    /// <summary>New lines of the log are searched as they arrive.</summary>
    public bool IsLive
    {
      get { return this.isLive; }
      set
      {
        if (this.isLive == value)
          return;
        this.isLive = value;
        this.OnPropertyChanged();
        if (this.isLive)
          this.SearchNewLines();
      }
    }
    private bool isLive;

    public SearchStatus Status
    {
      get { return this.status; }
      private set
      {
        this.status = value;
        this.OnPropertyChanged();
        this.OnPropertyChanged(nameof(this.HasRegexError));
        this.OnPropertyChanged(nameof(this.HasResults));
        this.OnPropertyChanged(nameof(this.SummaryText));
      }
    }
    private SearchStatus status;

    public bool HasRegexError => this.status == SearchStatus.InvalidRegex;
    public bool HasResults => this.SearchEntries.Count > 0;

    public string RegexError
    {
      get { return this.regexError; }
      private set
      {
        this.regexError = value;
        this.OnPropertyChanged();
      }
    }
    private string regexError;

    /// <summary>Total matches; one line may hold several.</summary>
    public int MatchCount
    {
      get { return this.matchCount; }
      private set
      {
        this.matchCount = value;
        this.OnPropertyChanged();
        this.OnPropertyChanged(nameof(this.SummaryText));
      }
    }
    private int matchCount;

    public long ElapsedMilliseconds { get; private set; }

    private DateTime? lastSearchDateTime;
    public DateTime? LastSearchDateTime
    {
      get { return this.lastSearchDateTime; }
      protected set
      {
        this.lastSearchDateTime = value;
        this.OnPropertyChanged();
      }
    }

    public string SummaryText
    {
      get
      {
        switch (this.status)
        {
          case SearchStatus.Searching:
            return "Searching…";
          case SearchStatus.NoMatches:
            return "No matches";
          case SearchStatus.InvalidRegex:
            return "Invalid regular expression";
          case SearchStatus.Found:
            var lines = this.SearchEntries.Count;
            return $"{this.matchCount:N0} {Plural(this.matchCount, "match", "matches")} in {lines:N0} {Plural(lines, "line", "lines")} · {this.ElapsedMilliseconds:N0} ms";
          default:
            return string.Empty;
        }
      }
    }

    /// <summary>The result the log was last moved to.</summary>
    public SearchEntryViewModel CurrentEntry
    {
      get { return this.currentEntry; }
      set
      {
        if (ReferenceEquals(this.currentEntry, value))
          return;
        this.currentEntry = value;
        this.currentIndex = value == null ? -1 : this.SearchEntries.IndexOf(value);
        this.OnPropertyChanged();
        this.OnPropertyChanged(nameof(this.PositionText));
        this.OnPropertyChanged(nameof(this.CurrentLineNumber));
        if (value != null)
          this.LogViewModel?.ShowLogLine(value);
      }
    }
    private SearchEntryViewModel currentEntry;
    private int currentIndex = -1;

    /// <summary>1-based line of the current result, 0 - none.</summary>
    public int CurrentLineNumber => this.currentEntry?.SearchEntry?.LineNumber ?? 0;

    /// <summary>"3 / 97"; "– / 97" when no result is current; "No results"; empty while there is nothing to show.</summary>
    public string PositionText
    {
      get
      {
        var count = this.SearchEntries.Count;
        if (count == 0)
          return this.status == SearchStatus.NoMatches ? "No results" : string.Empty;
        return this.currentIndex >= 0 ? $"{this.currentIndex + 1:N0} / {count:N0}" : $"– / {count:N0}";
      }
    }

    private LogViewModel logViewModel;
    public LogViewModel LogViewModel
    {
      get { return this.logViewModel; }
      protected set
      {
        if (this.logViewModel != null)
          this.logViewModel.LogMessages.CollectionChanged -= this.LogMessages_CollectionChanged;
        this.logViewModel = value;
        if (this.logViewModel != null)
          this.logViewModel.LogMessages.CollectionChanged += this.LogMessages_CollectionChanged;
        this.OnPropertyChanged();
      }
    }

    // The request the current results were found with; null - no results to keep up to date
    private SearchRequest activeRequest;
    private string activeQuery;
    // Lines of the log [0, searchedLineCount) are covered by the current results
    private int searchedLineCount;
    private CancellationTokenSource searchCancellation;
    private DispatcherTimer typingTimer;

    public SearchViewModel()
    {
      this.searchEntries = new ObservableCollection<SearchEntryViewModel>();
      this.useRegex = true;
      this.isLive = true;
      this.InitCommands();
    }

    public SearchViewModel(LogViewModel logViewModel) : this()
    {
      this.LogViewModel = logViewModel;
    }

    #region Search

    /// <summary>Searches the lines of the log on a background thread; a newer search cancels this one.</summary>
    public async Task SearchAsync()
    {
      this.StopTypingTimer();
      this.searchCancellation?.Cancel();
      this.searchCancellation = null;

      var query = this.StringToSearch;
      if (string.IsNullOrEmpty(query) || this.LogViewModel == null)
      {
        this.ResetResults(SearchStatus.Idle);
        return;
      }

      var statement = new TextStatement() { UseRegex = this.UseRegex, MatchCase = this.MatchCase, Text = query };
      if (statement.RegexError != null)
      {
        log.Debug($"Search: '{query}' is not a valid regex: {statement.RegexError}");
        this.ResetResults(SearchStatus.InvalidRegex);
        this.RegexError = statement.RegexError;
        return;
      }
      this.RegexError = null;

      var request = new SearchRequest();
      request.Statements.Add(statement);

      // The view model collection is read on this (UI) thread; the messages themselves are immutable
      var messages = this.LogViewModel.LogMessages.Select(x => x.Message as Message).ToArray();
      var cancellation = new CancellationTokenSource();
      this.searchCancellation = cancellation;
      log.Debug($"Search: '{query}' (regex {this.UseRegex}, case {this.MatchCase}) in {messages.Length:N0} lines");
      this.Status = SearchStatus.Searching;
      var watch = Stopwatch.StartNew();
      // Results must be applied on the UI thread when the search was started there. 'await' alone
      // returns there only when a SynchronizationContext is set, which is not the case outside of
      // dispatcher processing. Started elsewhere (no running UI loop): results are applied in place.
      var dispatcher = Application.Current?.Dispatcher;
      if (dispatcher != null && !dispatcher.CheckAccess())
        dispatcher = null;

      SearchResponse response = null;
      Exception error = null;
      try
      {
        response = await Task.Run(() => SearchEngine.Search(messages, request, cancellation.Token), cancellation.Token);
      }
      catch (OperationCanceledException)
      {
        log.Debug($"Search: '{query}' cancelled");
        return;
      }
      catch (Exception ex)
      {
        error = ex;
      }

      if (dispatcher != null && !dispatcher.CheckAccess())
        await dispatcher.InvokeAsync(() => this.ApplyResults(query, request, messages.Length, watch, cancellation, response, error));
      else
        this.ApplyResults(query, request, messages.Length, watch, cancellation, response, error);
    }

    private void ApplyResults(string query, SearchRequest request, int searchedLines, Stopwatch watch,
      CancellationTokenSource cancellation, SearchResponse response, Exception error)
    {
      if (cancellation.IsCancellationRequested)
        return;
      this.searchCancellation = null;
      if (error != null)
      {
        log.Error(error, $"Search of '{query}' failed");
        this.ResetResults(SearchStatus.NoMatches);
        return;
      }
      this.ElapsedMilliseconds = watch.ElapsedMilliseconds;
      this.activeRequest = request;
      this.activeQuery = query;
      this.searchedLineCount = searchedLines;
      this.currentEntry = null;
      this.currentIndex = -1;
      this.SearchEntries = new ObservableCollection<SearchEntryViewModel>(response.Entries.Select(x => new SearchEntryViewModel(x)));
      this.MatchCount = response.Entries.Sum(x => x.Matches.Count);
      this.LastSearchDateTime = DateTime.Now;
      log.Debug($"Search: '{query}' found {response.Entries.Count:N0} lines in {this.ElapsedMilliseconds} ms");
      // Lines that came while searching
      if (this.IsLive)
        this.SearchNewLines();
      this.UpdateStatusByResults();
      this.OnPropertyChanged(nameof(this.CurrentEntry));
      this.OnPropertyChanged(nameof(this.CurrentLineNumber));
      this.OnPropertyChanged(nameof(this.PositionText));
      if (this.LogViewModel != null)
        this.LogViewModel.State.ShowSearchPanel = true;
    }

    private void SearchAgainIfActive()
    {
      if (this.HasQuery)
        _ = this.SearchAsync();
    }

    private void ResetResults(SearchStatus newStatus)
    {
      this.activeRequest = null;
      this.activeQuery = null;
      this.searchedLineCount = 0;
      this.currentEntry = null;
      this.currentIndex = -1;
      this.RegexError = null;
      this.SearchEntries = new ObservableCollection<SearchEntryViewModel>();
      this.SelectedEntries = null;
      this.MatchCount = 0;
      this.Status = newStatus;
      this.OnPropertyChanged(nameof(this.CurrentEntry));
      this.OnPropertyChanged(nameof(this.CurrentLineNumber));
      this.OnPropertyChanged(nameof(this.PositionText));
    }

    private void UpdateStatusByResults()
    {
      this.Status = this.SearchEntries.Count > 0 ? SearchStatus.Found : SearchStatus.NoMatches;
    }

    private void ScheduleTypingSearch()
    {
      if (this.LogViewModel == null)
        return;
      if (string.IsNullOrEmpty(this.stringToSearch))
      {
        // Emptied box: drop the results at once
        _ = this.SearchAsync();
        return;
      }
      if (this.LogViewModel.LogMessages.Count > SearchAsYouTypeLineLimit)
        return;
      if (Application.Current == null)
        return; // no dispatcher loop (tests): search explicitly

      if (this.typingTimer == null)
      {
        this.typingTimer = new DispatcherTimer(DispatcherPriority.Background) { Interval = TimeSpan.FromMilliseconds(TypingDelayMs) };
        this.typingTimer.Tick += (s, e) => { this.StopTypingTimer(); _ = this.SearchAsync(); };
      }
      this.typingTimer.Stop();
      this.typingTimer.Start();
    }

    private void StopTypingTimer()
    {
      this.typingTimer?.Stop();
    }

    #endregion

    #region Live

    private void LogMessages_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
      if (e.Action == NotifyCollectionChangedAction.Add)
      {
        if (this.IsLive)
          this.SearchNewLines();
        return;
      }

      // The log was re-read (e.g. another encoding): old results point to lines that are gone.
      // Live search finds them again as the lines are added back.
      var request = this.activeRequest;
      var query = this.activeQuery;
      var wasActive = request != null;
      this.ResetResults(SearchStatus.Idle);
      if (wasActive && this.IsLive)
      {
        this.activeRequest = request;
        this.activeQuery = query;
        this.Status = SearchStatus.NoMatches;
        this.SearchNewLines();
      }
    }

    /// <summary>Searches the lines added to the log after the current results were found.</summary>
    private void SearchNewLines()
    {
      if (this.activeRequest == null || this.LogViewModel == null || this.status == SearchStatus.Searching)
        return;

      var lines = this.LogViewModel.LogMessages;
      if (this.searchedLineCount >= lines.Count)
        return;

      var added = 0;
      for (var index = this.searchedLineCount; index < lines.Count; index++)
      {
        var entry = SearchEngine.Search(lines[index].Message as Message, this.activeRequest);
        if (entry == null)
          continue;
        entry.LineNumber = index + 1;
        this.SearchEntries.Add(new SearchEntryViewModel(entry));
        this.matchCount += entry.Matches.Count;
        added++;
      }
      this.searchedLineCount = lines.Count;
      if (added == 0)
        return;
      this.OnPropertyChanged(nameof(this.MatchCount));
      this.OnPropertyChanged(nameof(this.PositionText));
      this.OnPropertyChanged(nameof(this.HasResults));
      this.UpdateStatusByResults();
    }

    #endregion

    #region Navigation

    public void GoToNext()
    {
      this.Move(+1);
    }

    public void GoToPrevious()
    {
      this.Move(-1);
    }

    private void Move(int step)
    {
      var count = this.SearchEntries.Count;
      if (count == 0)
        return;
      this.AddToHistory(this.activeQuery);
      int index;
      if (this.currentIndex < 0)
        index = step > 0 ? 0 : count - 1;
      else
        index = (this.currentIndex + step + count) % count;
      log.Debug($"Search: go to result {index + 1} of {count}, line {this.SearchEntries[index].SearchEntry?.LineNumber}");
      this.CurrentEntry = this.SearchEntries[index];
    }

    /// <summary>Enter in the query box: search a new query, or go to the next result of the current one.</summary>
    private async void Submit(bool backwards)
    {
      if (!this.HasQuery)
        return;
      var resultsAreCurrent = this.activeRequest != null && this.activeQuery == this.StringToSearch && this.status != SearchStatus.Searching;
      if (!resultsAreCurrent)
      {
        await this.SearchAsync();
        this.AddToHistory(this.activeQuery);
        if (this.LogViewModel != null && this.activeQuery != null)
          this.LogViewModel.State.ShowSearchPanel = true;
        return;
      }
      if (this.LogViewModel != null)
        this.LogViewModel.State.ShowSearchPanel = true;
      this.Move(backwards ? -1 : +1);
    }

    public void ShowSerchEntryLine(SearchEntryViewModel searchEntryViewModel)
    {
      this.LogViewModel?.ShowLogLine(searchEntryViewModel);
    }

    #endregion

    #region History

    public void AddToHistory(string query)
    {
      if (string.IsNullOrWhiteSpace(query))
        return;
      var existing = SharedHistory.IndexOf(query);
      if (existing == 0)
        return;
      if (existing > 0)
        SharedHistory.RemoveAt(existing);
      SharedHistory.Insert(0, query);
      while (SharedHistory.Count > HistoryLimit)
        SharedHistory.RemoveAt(SharedHistory.Count - 1);
    }

    #endregion

    #region Operations

    public void ViewSelected(IEnumerable<SearchEntryViewModel> searchEntries)
    {
      var entries = searchEntries?.ToList();
      if (entries == null || entries.Count == 0)
        return;
      this.AddToHistory(this.activeQuery);
      this.LogViewModel?.SelectLogLines(entries);
    }

    public void CopySearchEntriesToClipboard()
    {
      this.CopySearchEntriesToClipboard(this.SearchEntries.Select(x => x.SearchEntry));
    }

    public void CopySearchEntriesToClipboard(IEnumerable<SearchEntry> searchEntries, bool withLineNumbers = false)
    {
      var text = FormatEntries(searchEntries, withLineNumbers);
      if (text.Length == 0)
        return;
      this.AddToHistory(this.activeQuery);
      this.SetClipboardText(text);
    }

    public bool Export()
    {
      if (this.SearchEntries.Count == 0)
        return false;
      var logName = Path.GetFileNameWithoutExtension(this.LogViewModel?.LogPath ?? "log");
      var path = this.AskExportPath($"{logName}-search.log");
      if (string.IsNullOrEmpty(path))
        return false;
      this.AddToHistory(this.activeQuery);
      File.WriteAllText(path, FormatEntries(this.SearchEntries.Select(x => x.SearchEntry), false));
      return true;
    }

    public static string FormatEntries(IEnumerable<SearchEntry> searchEntries, bool withLineNumbers)
    {
      var sb = new StringBuilder();
      foreach (var entry in searchEntries.Where(x => x?.Message?.RawValue != null).OrderBy(x => x.LineNumber))
      {
        if (withLineNumbers)
          sb.Append(entry.LineNumber).Append('\t');
        sb.AppendLine(entry.Message.RawValue);
      }
      return sb.ToString();
    }

    private static string AskExportPathWithDialog(string defaultFileName)
    {
      var dialog = new Microsoft.Win32.SaveFileDialog()
      {
        Title = "Export search results",
        FileName = defaultFileName,
        DefaultExt = ".log",
        Filter = "Log files (*.log)|*.log|Text files (*.txt)|*.txt|All files (*.*)|*.*",
      };
      return dialog.ShowDialog() == true ? dialog.FileName : null;
    }

    #endregion

    private static string Plural(int count, string one, string many) => count == 1 ? one : many;

    private static IEnumerable<SearchEntryViewModel> AsEntries(object parameter)
    {
      return (parameter as System.Collections.IEnumerable)?.OfType<SearchEntryViewModel>() ?? Enumerable.Empty<SearchEntryViewModel>();
    }

    private void InitCommands()
    {
      this.HideSearchPanelCommand = new Command(x => {
        if (this.LogViewModel != null)
          this.LogViewModel.State.ShowSearchPanel = false;
      });
      this.ShowSearchPanelCommand = new Command(x => {
        if (this.LogViewModel != null)
          this.LogViewModel.State.ShowSearchPanel = true;
      });
      // Parameter: "back" - Shift+Enter
      this.SearchCommand = new Command(x => this.Submit(x as string == "back"));
      this.NextCommand = new Command(x => this.GoToNext(), x => this.SearchEntries.Count > 0);
      this.PreviousCommand = new Command(x => this.GoToPrevious(), x => this.SearchEntries.Count > 0);
      this.ClearCommand = new Command(x => {
        this.StringToSearch = string.Empty;
        this.ResetResults(SearchStatus.Idle);
        if (this.LogViewModel != null)
          this.LogViewModel.State.ShowSearchPanel = false;
      });
      this.GoToEntryCommand = new Command(x => {
        if (x is SearchEntryViewModel entry)
        {
          if (ReferenceEquals(entry, this.currentEntry))
            this.ShowSerchEntryLine(entry);
          else
            this.CurrentEntry = entry;
        }
      });
      this.UseHistoryCommand = new Command(x => {
        if (x is string query && query.Length > 0)
        {
          this.StringToSearch = query;
          _ = this.SearchAsync();
        }
      });
      this.CopyAllCommand = new Command(x => this.CopySearchEntriesToClipboard(), x => this.SearchEntries.Count > 0);
      this.CopySelectedCommand = new Command(
        x => this.CopySearchEntriesToClipboard(this.TargetSelection(x).Select(e => e.SearchEntry)),
        x => this.TargetSelection(x).Any());
      this.CopySelectedWithLineNumbersCommand = new Command(
        x => this.CopySearchEntriesToClipboard(this.TargetSelection(x).Select(e => e.SearchEntry), true),
        x => this.TargetSelection(x).Any());
      this.OpenAllAsSelectionCommand = new Command(x => this.ViewSelected(this.SearchEntries), x => this.SearchEntries.Count > 0);
      this.OpenSelectedAsSelectionCommand = new Command(
        x => this.ViewSelected(this.TargetSelection(x)),
        x => this.TargetSelection(x).Any());
      this.ExportCommand = new Command(x => this.Export(), x => this.SearchEntries.Count > 0);
    }

    /// <summary>Entries passed as the command parameter, otherwise the selected ones.</summary>
    private IEnumerable<SearchEntryViewModel> TargetSelection(object parameter)
    {
      var entries = AsEntries(parameter).ToList();
      return entries.Count > 0 ? entries : this.selectedEntries;
    }
  }
}
