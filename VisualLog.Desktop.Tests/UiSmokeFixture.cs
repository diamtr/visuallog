using NUnit.Framework;
using System;
using System.IO;
using System.Threading;
using System.Windows;
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
    public void ShowSelectedLines()
    {
      RunInWindow((mvm, view) =>
      {
        mvm.OpenAndShow(Path.GetFullPath("doc\\simpletest.log"));
        Pump(800);
        var logVm = mvm.LogManagerViewModel.ActiveLog;
        // Regression: nothing selected threw ArgumentOutOfRangeException
        log.Info("ShowSelectedLines step: show empty");
        logVm.ShowSelectedLinesCommand.Execute(null);
        Pump(300);
        Assert.That(logVm.State.ShowSelectedMessagesPanel, Is.True);
        logVm.SelectedMessagesViewModel.CloseSelectedMessagesCommand.Execute(null);
        Pump(300);

        log.Info("ShowSelectedLines step: show two lines");
        logVm.AddToSelection(logVm.LogMessages[0].Message);
        logVm.AddToSelection(logVm.LogMessages[2].Message);
        logVm.ShowSelectedLinesCommand.Execute(null);
        Pump(300);
        log.Info("ShowSelectedLines step: done");
        Assert.That(logVm.SelectedMessagesViewModel.Messages.Count, Is.EqualTo(2));
      });
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

        log.Info("SearchInLog step: open as selection");
        search.OpenAllAsSelectionCommand.Execute(null);
        Pump(300);
        Assert.That(logVm.State.ShowSelectedMessagesPanel, Is.True);

        log.Info("SearchInLog step: close selection");
        logVm.SelectedMessagesViewModel.CloseSelectedMessagesCommand.Execute(null);
        Pump(300);

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
