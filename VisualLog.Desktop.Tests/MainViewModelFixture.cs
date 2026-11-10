using NUnit.Framework;
using System;
using System.IO;
using System.Linq;

namespace VisualLog.Desktop.Tests
{
  [TestFixture]
  public class MainViewModelFixture
  {
    [Test]
    public void Create()
    {
      var mvm = new MainViewModel();

      Assert.That(mvm.LogManagerViewModel, Is.Not.Null);
      Assert.That(mvm.FormatManagerViewModel, Is.Not.Null);
    }

    [Test]
    public void SetActiveVM()
    {
      var mvm = new MainViewModel();
      mvm.SetAsActive(mvm.FormatManagerViewModel);
      Assert.That(mvm.ActiveViewModel, Is.EqualTo(mvm.FormatManagerViewModel));
    }

    [Test]
    public void OnWindowLoaded()
    {
      var mvm = new MainViewModel();
      mvm.OnWindowLoaded();
      Assert.That(mvm.ActiveViewModel, Is.EqualTo(mvm.DashboardViewModel));
    }

    [Test]
    public void ShowFormatManagerCommand()
    {
      var mvm = new MainViewModel();
      mvm.ShowFormatManagerCommand.Execute(null);
      Assert.That(mvm.ActiveViewModel, Is.EqualTo(mvm.FormatManagerViewModel));
    }

    [Test]
    public void OpenAndShow()
    {
      var mvm = CreateIsolated();
      mvm.OnWindowLoaded();

      // Nothing opened: stay on the dashboard
      mvm.OpenAndShow("thepathdoesnotexist");
      Assert.That(mvm.ActiveViewModel, Is.EqualTo(mvm.DashboardViewModel));

      mvm.OpenAndShow("doc\\simpletest.log");
      Assert.That(mvm.ActiveViewModel, Is.EqualTo(mvm.LogManagerViewModel));
      Assert.That(mvm.LogManagerViewModel.ActiveLog.DisplayName, Is.EqualTo("simpletest.log"));
      Assert.That(mvm.DashboardViewModel.RecentLogsViewModel.RecentLogs.Single().LogInfo.DisplayName, Is.EqualTo("simpletest.log"));
    }

    [Test]
    public void NavigationState()
    {
      var mvm = CreateIsolated();
      mvm.OnWindowLoaded();
      Assert.That(mvm.IsDashboardActive, Is.True);
      Assert.That(mvm.IsLogManagerActive, Is.False);

      // "Logs" is unavailable while nothing is opened
      Assert.That(mvm.ShowLogsCommand.CanExecute(null), Is.False);

      mvm.OpenAndShow("doc\\simpletest.log");
      Assert.That(mvm.IsDashboardActive, Is.False);
      Assert.That(mvm.IsLogManagerActive, Is.True);

      mvm.ShowDashboardCommand.Execute(null);
      Assert.That(mvm.IsDashboardActive, Is.True);

      Assert.That(mvm.ShowLogsCommand.CanExecute(null), Is.True);
      mvm.ShowLogsCommand.Execute(null);
      Assert.That(mvm.IsLogManagerActive, Is.True);
    }

    [Test]
    public void OnlyActiveLogIsMarkedActive()
    {
      var mvm = CreateIsolated();
      var first = new LogManager.LogViewModel();
      var second = new LogManager.LogViewModel();
      mvm.Logs.Add(first);
      mvm.Logs.Add(second);

      mvm.LogManagerViewModel.ActiveLog = first;
      Assert.That(first.IsActive, Is.True);
      Assert.That(second.IsActive, Is.False);

      mvm.LogManagerViewModel.ActiveLog = second;
      Assert.That(first.IsActive, Is.False);
      Assert.That(second.IsActive, Is.True);

      mvm.LogManagerViewModel.ActiveLog = null;
      Assert.That(first.IsActive, Is.False);
      Assert.That(second.IsActive, Is.False);
    }

    [Test]
    public void SwitchBetweenLogs()
    {
      var mvm = CreateIsolated();
      var lm = mvm.LogManagerViewModel;
      var first = new LogManager.LogViewModel();
      var second = new LogManager.LogViewModel();
      var third = new LogManager.LogViewModel();

      // Nothing to switch between
      mvm.Logs.Add(first);
      lm.ActiveLog = first;
      Assert.That(lm.NextLogCommand.CanExecute(null), Is.False);
      Assert.That(lm.PreviousLogCommand.CanExecute(null), Is.False);

      mvm.Logs.Add(second);
      mvm.Logs.Add(third);
      Assert.That(lm.NextLogCommand.CanExecute(null), Is.True);

      lm.NextLogCommand.Execute(null);
      Assert.That(lm.ActiveLog, Is.EqualTo(second));
      lm.NextLogCommand.Execute(null);
      Assert.That(lm.ActiveLog, Is.EqualTo(third));

      // Wraps around in both directions
      lm.NextLogCommand.Execute(null);
      Assert.That(lm.ActiveLog, Is.EqualTo(first));
      lm.PreviousLogCommand.Execute(null);
      Assert.That(lm.ActiveLog, Is.EqualTo(third));
      Assert.That(third.IsActive, Is.True);
      Assert.That(first.IsActive, Is.False);
    }

    [Test]
    public void CloseActiveLog()
    {
      var mvm = CreateIsolated();
      var lm = mvm.LogManagerViewModel;
      mvm.OnWindowLoaded();
      Assert.That(lm.CloseActiveLogCommand.CanExecute(null), Is.False);

      mvm.OpenAndShow("doc\\simpletest.log");
      Assert.That(lm.CloseActiveLogCommand.CanExecute(null), Is.True);

      lm.CloseActiveLogCommand.Execute(null);
      Assert.That(mvm.Logs, Is.Empty);
      Assert.That(lm.ActiveLog, Is.Null);
      Assert.That(mvm.ActiveViewModel, Is.EqualTo(mvm.DashboardViewModel));
    }

    /// <summary>MainViewModel whose recent logs history is stored in a temp file.</summary>
    private static MainViewModel CreateIsolated()
    {
      var mvm = new MainViewModel();
      mvm.DashboardViewModel.RecentLogsViewModel.StoragePath = Path.Combine(Path.GetTempPath(), $"visuallog_recent_{Guid.NewGuid():N}.json");
      mvm.SettingsViewModel.StoragePath = Path.Combine(Path.GetTempPath(), $"visuallog_settings_{Guid.NewGuid():N}.json");
      return mvm;
    }

    [Test]
    public void ShowSettings()
    {
      var mvm = CreateIsolated();
      mvm.OnWindowLoaded();
      Assert.That(mvm.SettingsViewModel, Is.Not.Null);
      Assert.That(mvm.IsSettingsActive, Is.False);

      var raised = false;
      mvm.PropertyChanged += (s, e) => raised |= e.PropertyName == nameof(MainViewModel.IsSettingsActive);
      mvm.ShowSettingsCommand.Execute(null);

      Assert.That(raised, Is.True);
      Assert.That(mvm.IsSettingsActive, Is.True);
      Assert.That(mvm.IsDashboardActive, Is.False);
      Assert.That(mvm.ActiveViewModel, Is.EqualTo(mvm.SettingsViewModel));
    }

    [Test]
    public void SettingsDriveHighlighting()
    {
      var mvm = CreateIsolated();
      var rule = mvm.SettingsViewModel.AddRule();
      rule.Pattern = "ERROR";
      rule.Bold = true;
      mvm.SettingsViewModel.ApplyNow();

      Assert.That(mvm.Highlighting.Current.Match("an ERROR line").IsBold, Is.True);
      try { File.Delete(mvm.SettingsViewModel.StoragePath); } catch { }
    }

    [Test]
    public void OpenLogs()
    {
      var mvm = CreateIsolated();

      // Without parameters
      Assert.DoesNotThrow(() => { mvm.Open(); });
      Assert.That(mvm.Logs, Is.Empty);
      Assert.That(mvm.LogManagerViewModel.ActiveLog, Is.Null);

      // Parameter is null
      Assert.DoesNotThrow(() => { mvm.Open(null); });
      Assert.That(mvm.Logs, Is.Empty);
      Assert.That(mvm.LogManagerViewModel.ActiveLog, Is.Null);

      // Path does not exist
      Assert.DoesNotThrow(() => { mvm.Open("thepathdoesnotexist"); });
      Assert.That(mvm.Logs, Is.Empty);
      Assert.That(mvm.LogManagerViewModel.ActiveLog, Is.Null);

      // Ordinary. Single Path
      Assert.DoesNotThrow(() => { mvm.Open("doc\\simpletest.log"); });
      Assert.That(mvm.Logs, Is.Not.Empty);
      Assert.That(mvm.Logs, Is.All.Not.Null);
      Assert.That(mvm.Logs.Count, Is.EqualTo(1));
      var log = mvm.Logs.First();
      Assert.That(log.DisplayName, Is.EqualTo("simpletest.log"));
      Assert.That(log.LogMessages, Is.Not.Empty);
      Assert.That(log.LogMessages.Count, Is.EqualTo(3));
      Assert.That(mvm.LogManagerViewModel.ActiveLog, Is.Not.Null);
      Assert.That(mvm.LogManagerViewModel.ActiveLog.DisplayName, Is.EqualTo("simpletest.log"));

      // Ordinary. Open the log again
      Assert.DoesNotThrow(() => { mvm.Open("doc\\simpletest.log"); });
      Assert.That(mvm.Logs, Is.Not.Empty);
      Assert.That(mvm.Logs, Is.All.Not.Null);
      Assert.That(mvm.Logs.Count, Is.EqualTo(1));
      log = mvm.Logs.First();
      Assert.That(log.DisplayName, Is.EqualTo("simpletest.log"));
      Assert.That(mvm.LogManagerViewModel.ActiveLog, Is.Not.Null);
      Assert.That(mvm.LogManagerViewModel.ActiveLog.DisplayName, Is.EqualTo("simpletest.log"));

      // Trash call
      Assert.DoesNotThrow(() => { mvm.Open("thepathdoesnotexist"); });
      Assert.That(mvm.Logs, Is.Not.Empty);
      Assert.That(mvm.Logs, Is.All.Not.Null);
      Assert.That(mvm.Logs.Count, Is.EqualTo(1));
      Assert.That(mvm.LogManagerViewModel.ActiveLog, Is.Not.Null);
      Assert.That(mvm.LogManagerViewModel.ActiveLog.DisplayName, Is.EqualTo("simpletest.log"));
    }
  }
}
