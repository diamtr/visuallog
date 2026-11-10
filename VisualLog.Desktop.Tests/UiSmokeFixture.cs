using ICSharpCode.AvalonEdit;
using NUnit.Framework;
using System;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using VisualLog.Desktop.Settings;

namespace VisualLog.Desktop.Tests
{
  /// <summary>
  /// Runs the real main window (placed off-screen) through the main scenarios.
  /// Catches XAML errors that only show up when templates are applied, e.g. an unresolved
  /// StaticResource in a log line template that crashed the app on opening a log.
  /// </summary>
  [TestFixture]
  public class UiSmokeFixture
  {
    private static readonly NLog.Logger log = NLog.LogManager.GetLogger("UiSmoke");

    [Test]
    public void OpenLogSwitchPagesClose()
    {
      RunInWindow((mvm, view) =>
      {
        mvm.Highlighting.Update(new[] { new HighlightRule { Pattern = "line", Background = "#FFF4CE", Bold = true } });

        mvm.OpenAndShow(Path.GetFullPath("doc\\simpletest.log"));
        Pump(800);
        Assert.That(mvm.IsLogManagerActive, Is.True);

        mvm.ShowSettingsCommand.Execute(null);
        Pump(300);
        mvm.SettingsViewModel.AddRule();
        Pump(300);

        mvm.ShowDashboardCommand.Execute(null);
        Pump(300);

        mvm.ShowLogsCommand.Execute(null);
        mvm.LogManagerViewModel.CloseActiveLogCommand.Execute(null);
        Pump(300);
        Assert.That(mvm.IsDashboardActive, Is.True);
      });
    }

    [Test]
    public void LogAsTextSelectionSearch()
    {
      RunInWindow((mvm, view) =>
      {
        mvm.OpenAndShow(Path.GetFullPath("doc\\simpletest.log"));
        Pump(800);
        var logVm = mvm.LogManagerViewModel.ActiveLog;
        var search = logVm.SearchViewModel;
        var editor = FindDescendant<TextEditor>(view, x => x.IsVisible);
        Assert.That(editor, Is.Not.Null);

        log.Info("LogAsText step: text matches the lines");
        var document = editor.Document;
        Assert.That(document.LineCount, Is.EqualTo(logVm.LogMessages.Count));
        Assert.That(document.GetText(document.GetLineByNumber(2)), Is.EqualTo(logVm.LogMessages[1].Message.RawValue));
        Assert.That(editor.IsReadOnly, Is.True);

        log.Info("LogAsText step: line numbers setting");
        Assert.That(editor.ShowLineNumbers, Is.True);
        mvm.SettingsViewModel.ShowLineNumbers = false;
        Pump(100);
        Assert.That(editor.ShowLineNumbers, Is.False);

        log.Info("LogAsText step: Ctrl+F with a selection");
        var line = document.GetLineByNumber(1);
        var start = document.GetText(line).IndexOf("logging");
        editor.Select(line.Offset + start, "logging".Length);
        logVm.ShowSearchPanelCommand.Execute(editor.SelectedText);
        Pump(500);
        Assert.That(search.StringToSearch, Is.EqualTo("logging"));
        Assert.That(search.SearchEntries.Count, Is.EqualTo(2));
        Assert.That(logVm.State.ShowSearchPanel, Is.True);

        log.Info("LogAsText step: next result selects the match");
        editor.Select(0, 0);
        search.NextCommand.Execute(null);
        search.NextCommand.Execute(null);
        Pump(300);
        Assert.That(editor.SelectedText, Is.EqualTo("logging"));
        Assert.That(editor.TextArea.Caret.Line, Is.EqualTo(3));

        log.Info("LogAsText step: new lines are appended");
        var count = logVm.LogMessages.Count;
        logVm.OnNewLogMessageCatched(new Core.Message("appended one"));
        logVm.OnNewLogMessageCatched(new Core.Message("appended two"));
        logVm.FlushPendingMessages();
        Pump(300);
        Assert.That(document.LineCount, Is.EqualTo(count + 2));
        Assert.That(document.GetText(document.GetLineByNumber(count + 2)), Is.EqualTo("appended two"));

        log.Info("LogAsText step: re-read rebuilds the text");
        logVm.ReadLog();
        Pump(300);
        Assert.That(document.LineCount, Is.EqualTo(count));
        log.Info("LogAsText step: done");
      });
    }

    [Test]
    public void LineDetails()
    {
      RunInWindow((mvm, view) =>
      {
        mvm.OpenAndShow(Path.GetFullPath("doc\\simpletest.log"));
        Pump(800);
        var logVm = mvm.LogManagerViewModel.ActiveLog;
        var longValue = string.Join(" ", System.Linq.Enumerable.Repeat("at Some.Namespace.Method()", 60));
        logVm.OnNewLogMessageCatched(new Core.Message($"{{\"level\":\"Error\",\"stack\":\"{longValue}\",\"ex\":{{\"type\":\"IOException\"}}}}"));
        logVm.FlushPendingMessages();
        Pump(300);
        var pane = FindDescendant<LogManager.MessageDetailsView>(view, x => true);
        Assert.That(pane, Is.Not.Null);
        Assert.That(pane.IsVisible, Is.False);

        log.Info("LineDetails step: text line");
        logVm.ShowDetails(1);
        Pump(300);
        Assert.That(pane.IsVisible, Is.True);
        var text = FindDescendant<System.Windows.Controls.TextBox>(pane, x => x.IsVisible);
        Assert.That(text.Text, Is.EqualTo(logVm.LogMessages[0].Message.RawValue));

        log.Info("LineDetails step: next line");
        logVm.Details.NextCommand.Execute(null);
        Pump(300);
        text = FindDescendant<System.Windows.Controls.TextBox>(pane, x => x.IsVisible);
        Assert.That(text.Text, Is.EqualTo(logVm.LogMessages[1].Message.RawValue));
        var editor = FindDescendant<TextEditor>(view, x => x.IsVisible);
        Assert.That(editor.TextArea.Caret.Line, Is.EqualTo(2));

        log.Info("LineDetails step: JSON line");
        logVm.ShowDetails(logVm.LogMessages.Count);
        Pump(300);
        var json = (LogManager.JsonMessageDetailsViewModel)logVm.Details;
        var tree = FindDescendant<System.Windows.Controls.TreeView>(pane, x => x.IsVisible);
        Assert.That(tree, Is.Not.Null);
        Assert.That(tree.Items.Count, Is.EqualTo(3));
        // A long value wraps within the panel
        var wrapped = FindDescendant<System.Windows.Controls.TextBox>(tree, x => x.Text == longValue);
        Assert.That(wrapped, Is.Not.Null);
        Assert.That(wrapped.ActualWidth, Is.LessThanOrEqualTo(pane.ActualWidth));
        Assert.That(wrapped.ActualHeight, Is.GreaterThan(wrapped.FontSize * 2));

        log.Info("LineDetails step: copy a value from the menu of the node");
        string clipboard = null;
        json.SetClipboardText = x => clipboard = x;
        var menu = wrapped.ContextMenu;
        menu.PlacementTarget = wrapped;
        menu.IsOpen = true;
        Pump(200);
        Assert.That(menu.DataContext, Is.SameAs(json.Fields[1]));
        var copyValue = FindMenuItem(menu, "Copy value");
        Assert.That(copyValue.IsEnabled, Is.True);
        copyValue.Command.Execute(copyValue.CommandParameter);
        Assert.That(clipboard, Is.EqualTo(longValue));
        menu.IsOpen = false;
        // Every node has its own menu
        var levelMenu = FindDescendant<System.Windows.Controls.TextBox>(tree, x => x.Text == "Error").ContextMenu;
        Assert.That(levelMenu, Is.Not.SameAs(menu));

        log.Info("LineDetails step: expand, text view");
        json.Fields[2].IsExpanded = true;
        Pump(200);
        Assert.That(FindDescendant<System.Windows.Controls.TextBox>(tree, x => x.Text == "IOException" && x.IsVisible), Is.Not.Null);
        json.IsTextView = true;
        Pump(200);
        Assert.That(tree.IsVisible, Is.False);
        text = FindDescendant<System.Windows.Controls.TextBox>(pane, x => x.IsVisible);
        Assert.That(text.Text, Is.EqualTo(json.IndentedText));

        log.Info("LineDetails step: menu of the read-only text has no Cut and Paste");
        text.Focus();
        text.Select(0, 5);
        menu = text.ContextMenu;
        Assert.That(menu, Is.Not.Null);
        menu.PlacementTarget = text;
        menu.IsOpen = true;
        Pump(200);
        Assert.That(System.Linq.Enumerable.Select(System.Linq.Enumerable.OfType<System.Windows.Controls.MenuItem>(menu.Items), x => x.Header),
                    Is.EqualTo(new[] { "Copy", "Select all" }));
        Assert.That(FindMenuItem(menu, "Copy").IsEnabled, Is.True);
        menu.IsOpen = false;

        log.Info("LineDetails step: close");
        json.CloseCommand.Execute(null);
        Pump(200);
        Assert.That(pane.IsVisible, Is.False);
        log.Info("LineDetails step: done");
      });
    }

    private static System.Windows.Controls.MenuItem FindMenuItem(System.Windows.Controls.ContextMenu menu, string header)
    {
      foreach (var item in menu.Items)
        if (item is System.Windows.Controls.MenuItem menuItem && Equals(menuItem.Header, header))
          return menuItem;
      return null;
    }

    private static T FindDescendant<T>(DependencyObject root, Func<T, bool> condition) where T : DependencyObject
    {
      for (var i = 0; i < VisualTreeHelper.GetChildrenCount(root); i++)
      {
        var child = VisualTreeHelper.GetChild(root, i);
        if (child is T match && condition(match))
          return match;
        var nested = FindDescendant(child, condition);
        if (nested != null)
          return nested;
      }
      return null;
    }

    [Test]
    public void SearchInLog()
    {
      RunInWindow((mvm, view) =>
      {
        mvm.OpenAndShow(Path.GetFullPath("doc\\simpletest.log"));
        Pump(800);
        var logVm = mvm.LogManagerViewModel.ActiveLog;
        var search = logVm.SearchViewModel;

        // Ctrl+F, then typing: the search runs after a pause and opens the results panel
        log.Info("SearchInLog step: Ctrl+F");
        logVm.ShowSearchPanelCommand.Execute(null);
        log.Info("SearchInLog step: typing");
        search.StringToSearch = "logging";
        Pump(800);
        Assert.That(search.Status, Is.EqualTo(LogManager.SearchStatus.Found));
        Assert.That(search.SearchEntries.Count, Is.EqualTo(2));
        Assert.That(logVm.State.ShowSearchPanel, Is.True);

        log.Info("SearchInLog step: next");
        search.NextCommand.Execute(null);
        search.NextCommand.Execute(null);
        Pump(300);
        Assert.That(search.PositionText, Is.EqualTo("2 / 2"));

        log.Info("SearchInLog step: invalid regex");
        search.StringToSearch = "log(";
        Pump(800);
        Assert.That(search.Status, Is.EqualTo(LogManager.SearchStatus.InvalidRegex));

        log.Info("SearchInLog step: history");
        search.UseHistoryCommand.Execute("logging");
        Pump(500);
        Assert.That(search.SearchEntries.Count, Is.EqualTo(2));

        log.Info("SearchInLog step: hide panel");
        search.HideSearchPanelCommand.Execute(null);
        Pump(300);

        log.Info("SearchInLog step: empty query");
        search.StringToSearch = string.Empty;
        Pump(300);

        log.Info("SearchInLog step: clear");
        search.ClearCommand.Execute(null);
        Pump(300);
        Assert.That(logVm.State.ShowSearchPanel, Is.False);
        log.Info("SearchInLog step: done");
      });
    }

    /// <summary>Creates the main window on a separate STA thread; fails on any UI thread exception.</summary>
    private static void RunInWindow(Action<MainViewModel, MainView> scenario)
    {
      var dispatcher = EnsureUiThread();
      Exception error = null;
      currentError = null;
      dispatcher.Invoke(() =>
      {
        try
        {
          var mvm = new MainViewModel();
          mvm.DashboardViewModel.RecentLogsViewModel.StoragePath = TempFile("recent");
          mvm.SettingsViewModel.StoragePath = TempFile("settings");
          var view = new MainView
          {
            DataContext = mvm,
            WindowStartupLocation = WindowStartupLocation.Manual,
            WindowState = WindowState.Normal,
            Left = -20000,
            Top = -20000,
            ShowActivated = false,
            ShowInTaskbar = false
          };
          view.Show();
          Pump(500);
          try
          {
            if (currentError == null)
              scenario(mvm, view);
          }
          finally
          {
            view.Close();
            Pump(100);
            File.Delete(mvm.DashboardViewModel.RecentLogsViewModel.StoragePath);
            File.Delete(mvm.SettingsViewModel.StoragePath);
          }
        }
        catch (Exception ex)
        {
          error = ex;
        }
      });

      error ??= currentError;
      if (error != null)
        Assert.Fail(error.ToString());
    }

    // WPF allows one Application per process, bound to the dispatcher of the thread that created it.
    // All window scenarios therefore run on one long-lived STA thread.
    private static Dispatcher uiDispatcher;
    private static Exception currentError;
    private static readonly object uiThreadLock = new object();

    private static Dispatcher EnsureUiThread()
    {
      lock (uiThreadLock)
      {
        if (uiDispatcher != null)
          return uiDispatcher;

        using var ready = new ManualResetEventSlim();
        var thread = new Thread(() =>
        {
          new Application { ShutdownMode = ShutdownMode.OnExplicitShutdown };
          uiDispatcher = Dispatcher.CurrentDispatcher;
          uiDispatcher.UnhandledException += (s, e) =>
          {
            currentError ??= e.Exception;
            e.Handled = true;
          };
          ready.Set();
          Dispatcher.Run();
        });
        thread.IsBackground = true;
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        ready.Wait();
        return uiDispatcher;
      }
    }

    private static string TempFile(string name) =>
      Path.Combine(Path.GetTempPath(), $"visuallog_{name}_{Guid.NewGuid():N}.json");

    /// <summary>Lets the dispatcher do layout, rendering and timers for a while.</summary>
    private static void Pump(int milliseconds)
    {
      var end = DateTime.Now.AddMilliseconds(milliseconds);
      while (DateTime.Now < end)
      {
        var frame = new DispatcherFrame();
        Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.ContextIdle, new Action(() => frame.Continue = false));
        Dispatcher.PushFrame(frame);
        Thread.Sleep(20);
      }
    }
  }
}
