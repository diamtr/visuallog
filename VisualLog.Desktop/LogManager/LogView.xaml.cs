using ICSharpCode.AvalonEdit.Rendering;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using VisualLog.Core.Search;
using VisualLog.Desktop.Settings;

namespace VisualLog.Desktop.LogManager
{
  /// <summary>
  /// One opened log: the toolbar, the log as read-only text and the search results panel.
  /// The text is kept in sync with <see cref="LogViewModel.LogMessages"/>: line N of the text is message N - 1.
  /// </summary>
  public partial class LogView : UserControl
  {
    private static readonly NLog.Logger log = NLog.LogManager.GetCurrentClassLogger();

    private static readonly Brush SelectionBrush = Freeze(new SolidColorBrush(Color.FromArgb(0x60, 0x3A, 0x96, 0xDD)));

    // Each opened log owns its own LogView (see LogManagerView), so the view is bound
    // to a single LogViewModel for its whole life. Hidden views keep their state.
    private LogViewModel actualViewModel;

    // Lines of the log [0, syncedLineCount) are in the text
    private LogViewModel syncedViewModel;
    private int syncedLineCount;
    private bool rebuildRequested;
    private bool syncScheduled;

    private SearchViewModel observedSearch;
    private INotifyCollectionChanged observedSearchEntries;

    // Text the context menu was opened for
    private string contextMenuText;
    // Line the context menu was opened on (1-based), 0 - none
    private int contextMenuLineNumber;

    public LogView()
    {
      InitializeComponent();
      this.SetUpEditor();
      this.Unloaded += this.UserControl_Unloaded;
      this.IsVisibleChanged += this.LogView_IsVisibleChanged;
    }

    private void SetUpEditor()
    {
      var editor = this.LogEditor;
      editor.Document.UndoStack.SizeLimit = 0;

      var options = editor.Options;
      options.EnableHyperlinks = false;
      options.EnableEmailHyperlinks = false;
      options.EnableTextDragDrop = false;
      options.EnableRectangularSelection = true;
      options.HighlightCurrentLine = false;
      options.AllowScrollBelowDocument = false;

      // Selection as in a text editor: just a background, no frame, the text keeps its colors
      var area = editor.TextArea;
      area.SelectionBrush = SelectionBrush;
      area.SelectionBorder = null;
      area.SelectionCornerRadius = 0;
      area.SelectionForeground = null;

      var textView = area.TextView;
      textView.LineTransformers.Add(new LineStyleColorizer(this.StyleOfLine));
      textView.BackgroundRenderers.Add(new LineBackgroundRenderer(this.StyleOfLine));
      textView.BackgroundRenderers.Add(new SearchMatchRenderer(
        () => this.actualViewModel?.SearchViewModel?.SearchEntries,
        () => this.actualViewModel?.SearchViewModel?.CurrentEntry?.SearchEntry));
    }

    private LineStyle StyleOfLine(int lineNumber)
    {
      return this.actualViewModel?.GetLogLine(lineNumber)?.Highlight;
    }

    private void UserControl_Loaded(object sender, RoutedEventArgs e)
    {
      this.Attach(this.DataContext as LogViewModel);
      this.ScrollToBottom();
    }

    private void UserControl_Unloaded(object sender, RoutedEventArgs e)
    {
      this.Detach();
    }

    private void LogView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
      if (!this.IsLoaded)
        return;
      this.Attach(e.NewValue as LogViewModel);
      this.ScrollToBottom();
    }

    private void LogView_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
      // New lines are not scrolled to while the view is hidden; catch up when it is shown.
      if (this.IsVisible)
        this.ScrollToBottom();
    }

    private void Attach(LogViewModel viewModel)
    {
      if (ReferenceEquals(this.actualViewModel, viewModel))
        return;
      this.Detach();
      this.actualViewModel = viewModel;
      if (this.actualViewModel == null)
        return;
      this.actualViewModel.ShowLineRequested += this.ShowLine;
      this.actualViewModel.SearchFocusRequested += this.FocusSearch;
      this.actualViewModel.LinesRestyled += this.Redraw;
      this.actualViewModel.LogMessages.CollectionChanged += this.LogMessages_CollectionChanged;
      this.actualViewModel.State.PropertyChanged += this.State_PropertyChanged;
      this.actualViewModel.PropertyChanged += this.ViewModel_PropertyChanged;
      this.SearchBox.EscapeRequested += this.SearchBox_EscapeRequested;
      this.ObserveSearch(this.actualViewModel.SearchViewModel);
      this.ApplySearchPanelVisibility();
      this.ApplyDetailsPanelVisibility();

      // Lines may have come while the view was detached
      if (!this.IsInSync(this.actualViewModel))
        this.rebuildRequested = true;
      this.SyncText();
    }

    private void Detach()
    {
      if (this.actualViewModel == null)
        return;
      this.actualViewModel.ShowLineRequested -= this.ShowLine;
      this.actualViewModel.SearchFocusRequested -= this.FocusSearch;
      this.actualViewModel.LinesRestyled -= this.Redraw;
      this.actualViewModel.LogMessages.CollectionChanged -= this.LogMessages_CollectionChanged;
      this.actualViewModel.State.PropertyChanged -= this.State_PropertyChanged;
      this.actualViewModel.PropertyChanged -= this.ViewModel_PropertyChanged;
      this.SearchBox.EscapeRequested -= this.SearchBox_EscapeRequested;
      this.ObserveSearch(null);
      this.actualViewModel = null;
    }

    #region Text of the log

    private void LogMessages_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
      // Lines are only appended while the log is followed; anything else (re-read) rebuilds the text
      if (e.Action != NotifyCollectionChangedAction.Add || e.NewStartingIndex < this.syncedLineCount)
        this.rebuildRequested = true;
      // Lines come in batches of single adds: put the whole batch into the text at once
      if (this.syncScheduled)
        return;
      this.syncScheduled = true;
      this.Dispatcher.BeginInvoke(new Action(this.SyncText), DispatcherPriority.DataBind);
    }

    /// <summary>The text holds the lines the view model has had so far (text appended since then is checked by the last line).</summary>
    private bool IsInSync(LogViewModel viewModel)
    {
      if (!ReferenceEquals(this.syncedViewModel, viewModel))
        return false;
      var lines = viewModel.LogMessages;
      if (this.syncedLineCount > lines.Count)
        return false;
      if (this.syncedLineCount == 0)
        return this.LogEditor.Document.TextLength == 0;
      var document = this.LogEditor.Document;
      return document.LineCount == this.syncedLineCount &&
             document.GetText(document.GetLineByNumber(this.syncedLineCount)) == lines[this.syncedLineCount - 1].Message?.RawValue;
    }

    private void SyncText()
    {
      this.syncScheduled = false;
      var viewModel = this.actualViewModel;
      if (viewModel == null)
        return;
      var lines = viewModel.LogMessages;
      var document = this.LogEditor.Document;
      if (this.rebuildRequested || this.syncedLineCount > lines.Count)
      {
        this.rebuildRequested = false;
        var watch = System.Diagnostics.Stopwatch.StartNew();
        document.Text = JoinLines(lines, 0, lines.Count);
        this.syncedLineCount = lines.Count;
        this.syncedViewModel = viewModel;
        log.Debug($"Text: rebuilt, {lines.Count:N0} lines in {watch.ElapsedMilliseconds} ms");
      }
      else if (this.syncedLineCount < lines.Count)
      {
        var text = JoinLines(lines, this.syncedLineCount, lines.Count);
        document.Insert(document.TextLength, this.syncedLineCount == 0 ? text : "\n" + text);
        this.syncedLineCount = lines.Count;
      }
      else
      {
        return;
      }
      if (this.IsVisible)
        this.ScrollToBottom();
    }

    private static string JoinLines(IList<MessageInlineViewModel> lines, int from, int to)
    {
      var sb = new StringBuilder();
      for (var index = from; index < to; index++)
      {
        if (index > from)
          sb.Append('\n');
        sb.Append(lines[index].Message?.RawValue);
      }
      return sb.ToString();
    }

    private void Redraw()
    {
      this.LogEditor.TextArea.TextView.Redraw();
    }

    #endregion

    #region Selection: copy, search

    /// <summary>The selected text, null if nothing is selected.</summary>
    private string SelectedText()
    {
      return this.LogEditor.TextArea.Selection.IsEmpty ? null : this.LogEditor.SelectedText;
    }

    private void LogEditor_PreviewKeyDown(object sender, KeyEventArgs e)
    {
      // Ctrl+F with a part of a line selected searches for it
      if (e.Key == Key.F && Keyboard.Modifiers == ModifierKeys.Control && this.actualViewModel != null)
      {
        this.actualViewModel.ShowSearchPanelCommand.Execute(this.SelectedText());
        e.Handled = true;
      }
    }

    private void LogEditor_ContextMenuOpening(object sender, ContextMenuEventArgs e)
    {
      // By the mouse: the line under it; by the keyboard (no cursor position): the line of the caret
      this.contextMenuLineNumber = e.CursorLeft < 0
        ? this.LogEditor.TextArea.Caret.Line
        : this.LineNumberAt(Mouse.GetPosition(this.LogEditor));
      this.ShowDetailsMenuItem.IsEnabled = this.actualViewModel?.GetLogLine(this.contextMenuLineNumber) != null;

      var text = this.SelectedText();
      var searchable = SearchViewModel.IsSearchableText(text);
      this.contextMenuText = searchable ? text : null;
      this.CopyMenuItem.IsEnabled = text != null;
      this.SearchSelectionMenuItem.IsEnabled = searchable;
      this.SearchSelectionMenuHeader.Text = searchable ? $"Search “{Shorten(text, 40)}”" : "Search selected text";
    }

    private void CopyMenuItem_Click(object sender, RoutedEventArgs e)
    {
      this.LogEditor.Copy();
    }

    private void SearchSelectionMenuItem_Click(object sender, RoutedEventArgs e)
    {
      if (this.contextMenuText != null)
        this.actualViewModel?.SearchTextCommand.Execute(this.contextMenuText);
    }

    private void SelectAllMenuItem_Click(object sender, RoutedEventArgs e)
    {
      this.LogEditor.SelectAll();
    }

    private static string Shorten(string text, int maxLength)
    {
      return text.Length <= maxLength ? text : text.Substring(0, maxLength - 1) + "…";
    }

    #endregion

    #region Details of a line

    private void ShowDetailsMenuItem_Click(object sender, RoutedEventArgs e)
    {
      this.actualViewModel?.ShowDetails(this.contextMenuLineNumber);
    }

    private void LogEditor_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
      // Only on the text (and its margins), not on the scrollbars
      if (e.ChangedButton != MouseButton.Left || !IsWithin(e.OriginalSource as DependencyObject, this.LogEditor.TextArea))
        return;
      var lineNumber = this.LineNumberAt(e.GetPosition(this.LogEditor));
      if (lineNumber > 0)
        this.actualViewModel?.ShowDetails(lineNumber);
    }

    /// <summary>1-based number of the line at a point of the editor, 0 if there is no line (below the last one).</summary>
    private int LineNumberAt(Point point)
    {
      return this.LogEditor.GetPositionFromPoint(point)?.Line ?? 0;
    }

    private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      if (e.PropertyName == nameof(LogViewModel.Details))
        this.ApplyDetailsPanelVisibility();
    }

    /// <summary>The details panel takes its last width when shown and no room when hidden.</summary>
    private void ApplyDetailsPanelVisibility()
    {
      var state = this.actualViewModel?.State;
      var show = this.actualViewModel?.Details != null;
      var shown = this.DetailsPane.Visibility == Visibility.Visible;
      if (show == shown)
        return;
      log.Debug($"Details: panel {(show ? "shown" : "hidden")}, width {state?.DetailsPanelWidth}");
      if (show)
      {
        this.DetailsSplitterColumn.Width = new GridLength(5);
        this.DetailsPanelColumn.MinWidth = 200;
        this.DetailsPanelColumn.Width = new GridLength(Math.Max(state.DetailsPanelWidth, 200));
      }
      else
      {
        if (state != null && this.DetailsPanelColumn.ActualWidth > 0)
          state.DetailsPanelWidth = this.DetailsPanelColumn.ActualWidth;
        this.DetailsSplitterColumn.Width = new GridLength(0);
        this.DetailsPanelColumn.MinWidth = 0;
        this.DetailsPanelColumn.Width = new GridLength(0);
        // Esc in the panel closes it: the keyboard goes back to the log
        if (this.DetailsPane.IsKeyboardFocusWithin)
          this.LogEditor.TextArea.Focus();
      }
      this.DetailsSplitter.Visibility = show ? Visibility.Visible : Visibility.Collapsed;
      this.DetailsPane.Visibility = show ? Visibility.Visible : Visibility.Collapsed;
    }

    private void DetailsSplitter_DragCompleted(object sender, DragCompletedEventArgs e)
    {
      if (this.actualViewModel != null)
        this.actualViewModel.State.DetailsPanelWidth = this.DetailsPanelColumn.ActualWidth;
    }

    private static bool IsWithin(DependencyObject node, DependencyObject ancestor)
    {
      while (node != null)
      {
        if (ReferenceEquals(node, ancestor))
          return true;
        node = node is Visual || node is System.Windows.Media.Media3D.Visual3D
          ? VisualTreeHelper.GetParent(node)
          : LogicalTreeHelper.GetParent(node);
      }
      return false;
    }

    #endregion

    #region Search

    private void FocusSearch()
    {
      log.Debug("Search: focus the query box");
      this.SearchBox.FocusQuery();
    }

    private void SearchBox_EscapeRequested()
    {
      if (this.actualViewModel != null)
        this.actualViewModel.State.ShowSearchPanel = false;
      this.LogEditor.TextArea.Focus();
    }

    private void State_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      if (e.PropertyName == nameof(LogViewStateViewModel.ShowSearchPanel))
        this.ApplySearchPanelVisibility();
    }

    /// <summary>Redraws the marked matches when the results or the current result change.</summary>
    private void ObserveSearch(SearchViewModel search)
    {
      if (this.observedSearch != null)
        this.observedSearch.PropertyChanged -= this.Search_PropertyChanged;
      this.ObserveSearchEntries(null);
      this.observedSearch = search;
      if (search == null)
        return;
      search.PropertyChanged += this.Search_PropertyChanged;
      this.ObserveSearchEntries(search.SearchEntries);
    }

    private void ObserveSearchEntries(INotifyCollectionChanged entries)
    {
      if (this.observedSearchEntries != null)
        this.observedSearchEntries.CollectionChanged -= this.SearchEntries_CollectionChanged;
      this.observedSearchEntries = entries;
      if (entries != null)
        entries.CollectionChanged += this.SearchEntries_CollectionChanged;
    }

    private void Search_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
      if (e.PropertyName == nameof(SearchViewModel.SearchEntries))
      {
        this.ObserveSearchEntries(this.observedSearch?.SearchEntries);
        this.RedrawMatches();
      }
      else if (e.PropertyName == nameof(SearchViewModel.CurrentEntry))
      {
        this.RedrawMatches();
      }
    }

    private void SearchEntries_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
      this.RedrawMatches();
    }

    private void RedrawMatches()
    {
      this.LogEditor.TextArea.TextView.InvalidateLayer(KnownLayer.Background);
    }

    /// <summary>The results panel takes its last height when shown and no room when hidden.</summary>
    private void ApplySearchPanelVisibility()
    {
      var state = this.actualViewModel?.State;
      var show = state != null && state.ShowSearchPanel;
      log.Debug($"Search: results panel {(show ? "shown" : "hidden")}, height {state?.SearchPanelHeight}");
      if (show)
      {
        this.SearchSplitterRow.Height = new GridLength(5);
        this.SearchPanelRow.MinHeight = 90;
        this.SearchPanelRow.Height = new GridLength(Math.Max(state.SearchPanelHeight, 90));
      }
      else
      {
        if (state != null && this.SearchPanelRow.ActualHeight > 0)
          state.SearchPanelHeight = this.SearchPanelRow.ActualHeight;
        this.SearchSplitterRow.Height = new GridLength(0);
        this.SearchPanelRow.MinHeight = 0;
        this.SearchPanelRow.Height = new GridLength(0);
      }
      this.SearchSplitter.Visibility = show ? Visibility.Visible : Visibility.Collapsed;
      this.SearchPaneContentControl.Visibility = show ? Visibility.Visible : Visibility.Collapsed;
    }

    private void SearchSplitter_DragCompleted(object sender, DragCompletedEventArgs e)
    {
      if (this.actualViewModel != null)
        this.actualViewModel.State.SearchPanelHeight = this.SearchPanelRow.ActualHeight;
    }

    private ScrollViewer logScrollViewer;

    private void LogEditor_Loaded(object sender, RoutedEventArgs e)
    {
      if (this.logScrollViewer != null)
        return;
      this.logScrollViewer = FindDescendant<ScrollViewer>(this.LogEditor);
      if (this.logScrollViewer != null)
        this.logScrollViewer.ScrollChanged += (s, args) => this.PlaceSearchMarkers();
      this.PlaceSearchMarkers();
    }

    /// <summary>Puts the match marks over the track of the vertical scrollbar of the log.</summary>
    private void PlaceSearchMarkers()
    {
      var scrollViewer = this.logScrollViewer;
      if (scrollViewer == null || scrollViewer.ComputedVerticalScrollBarVisibility != Visibility.Visible)
      {
        this.SearchMarkers.Visibility = Visibility.Collapsed;
        return;
      }
      var bottom = scrollViewer.ComputedHorizontalScrollBarVisibility == Visibility.Visible ? SystemParameters.HorizontalScrollBarHeight : 0;
      this.SearchMarkers.Visibility = Visibility.Visible;
      this.SearchMarkers.Width = SystemParameters.VerticalScrollBarWidth;
      this.SearchMarkers.VerticalInset = SystemParameters.VerticalScrollBarButtonHeight;
      this.SearchMarkers.Margin = new Thickness(0, 0, 0, bottom);
    }

    #endregion

    #region Scrolling

    private void FollowTailCheckBox_Checked(object sender, RoutedEventArgs e)
    {
      this.ScrollToBottom();
    }

    private void ScrollToBottom()
    {
      if (!this.IsLoaded)
        return;
      if (!this.FollowTailCheckBox.IsChecked.GetValueOrDefault())
        return;
      // After layout, so that the extent includes the new lines
      this.Dispatcher.BeginInvoke(new Action(() => this.LogEditor.ScrollToEnd()), DispatcherPriority.Background);
    }

    /// <summary>Scrolls to the line of a search result and selects its first match.</summary>
    private void ShowLine(SearchEntry entry)
    {
      if (entry == null)
        return;
      var document = this.LogEditor.Document;
      if (entry.LineNumber > document.LineCount || this.rebuildRequested)
        this.SyncText();
      if (entry.LineNumber < 1 || entry.LineNumber > document.LineCount)
        return;

      this.FollowTailCheckBox.IsChecked = false;
      var line = document.GetLineByNumber(entry.LineNumber);
      var column = 0;
      if (entry.Matches.Count > 0)
      {
        var match = entry.Matches[0];
        column = Math.Clamp(match.Index, 0, line.Length);
        var length = Math.Clamp(match.Length, 0, line.Length - column);
        this.LogEditor.Select(line.Offset + column, length);
      }
      else
      {
        this.LogEditor.Select(line.Offset, 0);
      }
      this.LogEditor.ScrollTo(entry.LineNumber, column + 1);
    }

    #endregion

    private static T FindDescendant<T>(DependencyObject root) where T : DependencyObject
    {
      if (root == null) return null;
      for (int i = 0; i < VisualTreeHelper.GetChildrenCount(root); i++)
      {
        var child = VisualTreeHelper.GetChild(root, i);
        if (child is T match) return match;
        var nested = FindDescendant<T>(child);
        if (nested != null) return nested;
      }
      return null;
    }

    private static Brush Freeze(Brush brush)
    {
      brush.Freeze();
      return brush;
    }
  }
}
