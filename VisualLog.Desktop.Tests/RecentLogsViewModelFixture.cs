using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using VisualLog.Desktop.Converters;
using VisualLog.Desktop.Dashboard;

namespace VisualLog.Desktop.Tests
{
  [TestFixture]
  public class RecentLogsViewModelFixture
  {
    private string tempDir;
    private string storagePath;

    [SetUp]
    public void SetUp()
    {
      this.tempDir = Path.Combine(Path.GetTempPath(), "VisualLogTests_" + Guid.NewGuid().ToString("N"));
      Directory.CreateDirectory(this.tempDir);
      this.storagePath = Path.Combine(this.tempDir, RecentLogsViewModel.RecentFileName);
    }

    [TearDown]
    public void TearDown()
    {
      try { Directory.Delete(this.tempDir, true); } catch { }
    }

    private MainViewModel CreateMainViewModel()
    {
      var mvm = new MainViewModel();
      mvm.DashboardViewModel.RecentLogsViewModel.StoragePath = this.storagePath;
      return mvm;
    }

    private string CreateLogFile(string name)
    {
      var path = Path.Combine(this.tempDir, name);
      File.WriteAllText(path, "2024-01-01 00:00:00.0000 [INFO] line" + Environment.NewLine);
      return path;
    }

    [Test]
    public void OpenedLogBecomesRecentAndIsPersistedImmediately()
    {
      var mvm = this.CreateMainViewModel();
      var recent = mvm.DashboardViewModel.RecentLogsViewModel;
      var path = this.CreateLogFile("a.log");

      mvm.Open(path);

      Assert.That(recent.RecentLogs.Count, Is.EqualTo(1));
      Assert.That(recent.RecentLogs[0].LogInfo.Path, Is.EqualTo(path));
      Assert.That(recent.RecentLogs[0].LogInfo.DisplayName, Is.EqualTo("a.log"));
      Assert.That(File.Exists(this.storagePath), Is.True);

      // A fresh instance (next app start) sees the history
      var next = this.CreateMainViewModel();
      next.DashboardViewModel.RecentLogsViewModel.FillAvailableRecentLogs();
      Assert.That(next.DashboardViewModel.RecentLogsViewModel.RecentLogs.Select(x => x.LogInfo.Path), Is.EqualTo(new[] { path }));
    }

    [Test]
    public void ShowsTenMostRecentNewestFirst()
    {
      var recent = this.CreateMainViewModel().DashboardViewModel.RecentLogsViewModel;
      var paths = new List<string>();
      for (var i = 0; i < 12; i++)
      {
        var path = this.CreateLogFile($"{i:00}.log");
        paths.Add(path);
        recent.Remember(path);
        System.Threading.Thread.Sleep(2); // distinct LastOpened
      }

      Assert.That(recent.RecentLogs.Count, Is.EqualTo(RecentLogsViewModel.MaxDisplayed));
      var expected = Enumerable.Reverse(paths).Take(10).ToArray();
      Assert.That(recent.RecentLogs.Select(x => x.LogInfo.Path), Is.EqualTo(expected));

      // Re-opening moves the log to the top without duplicating it
      recent.Remember(paths[5]);
      Assert.That(recent.RecentLogs.Count, Is.EqualTo(10));
      Assert.That(recent.RecentLogs[0].LogInfo.Path, Is.EqualTo(paths[5]));
      Assert.That(recent.RecentLogs.Count(x => x.LogInfo.Path == paths[5]), Is.EqualTo(1));
    }

    [Test]
    public void OpenCommandOpensLogAndSwitchesToLogView()
    {
      var mvm = this.CreateMainViewModel();
      var recent = mvm.DashboardViewModel.RecentLogsViewModel;
      var path = this.CreateLogFile("a.log");
      recent.Remember(path);
      mvm.OnWindowLoaded();

      recent.RecentLogs[0].OpenCommand.Execute(null);

      Assert.That(mvm.Logs.Single().LogPath, Is.EqualTo(path));
      Assert.That(mvm.ActiveViewModel, Is.EqualTo(mvm.LogManagerViewModel));
    }

    [Test]
    public void OpeningMissingFileRemovesItFromRecent()
    {
      var mvm = this.CreateMainViewModel();
      var recent = mvm.DashboardViewModel.RecentLogsViewModel;
      var path = this.CreateLogFile("gone.log");
      recent.Remember(path);
      File.Delete(path);
      mvm.OnWindowLoaded();

      recent.RecentLogs[0].OpenCommand.Execute(null);

      Assert.That(recent.RecentLogs, Is.Empty);
      Assert.That(mvm.Logs, Is.Empty);
      Assert.That(mvm.ActiveViewModel, Is.EqualTo(mvm.DashboardViewModel));
    }

    [Test]
    public void RemoveAndClear()
    {
      var recent = this.CreateMainViewModel().DashboardViewModel.RecentLogsViewModel;
      var a = this.CreateLogFile("a.log");
      var b = this.CreateLogFile("b.log");
      recent.Remember(a);
      recent.Remember(b);

      recent.RecentLogs.Single(x => x.LogInfo.Path == a).RemoveCommand.Execute(null);
      Assert.That(recent.RecentLogs.Select(x => x.LogInfo.Path), Is.EqualTo(new[] { b }));

      recent.ClearCommand.Execute(null);
      Assert.That(recent.RecentLogs, Is.Empty);

      var next = this.CreateMainViewModel().DashboardViewModel.RecentLogsViewModel;
      next.FillAvailableRecentLogs();
      Assert.That(next.RecentLogs, Is.Empty);
    }

    [Test]
    public void LoadSkipsMissingFilesAndKeepsHistoryWhenRememberingBeforeLoad()
    {
      var a = this.CreateLogFile("a.log");
      var b = this.CreateLogFile("b.log");
      var first = this.CreateMainViewModel().DashboardViewModel.RecentLogsViewModel;
      first.Remember(a);
      first.Remember(b);
      File.Delete(b);

      // Remember is called before the dashboard was ever shown: history must not be lost
      var second = this.CreateMainViewModel().DashboardViewModel.RecentLogsViewModel;
      var c = this.CreateLogFile("c.log");
      second.Remember(c);

      Assert.That(second.RecentLogs.Select(x => x.LogInfo.Path), Is.EquivalentTo(new[] { a, c }));
    }

    [Test]
    public void RelativeTime()
    {
      var now = new DateTime(2026, 11, 9, 15, 0, 0);
      Assert.That(RelativeTimeConverter.Format(now.AddSeconds(-20), now), Is.EqualTo("Just now"));
      Assert.That(RelativeTimeConverter.Format(now.AddMinutes(-5), now), Is.EqualTo("5 min ago"));
      Assert.That(RelativeTimeConverter.Format(now.AddHours(-3), now), Is.EqualTo("Today, 12:00"));
      Assert.That(RelativeTimeConverter.Format(now.AddDays(-1), now), Is.EqualTo("Yesterday, 15:00"));
      Assert.That(RelativeTimeConverter.Format(now.AddDays(-3), now), Is.EqualTo("Fri, 15:00"));
      Assert.That(RelativeTimeConverter.Format(now.AddDays(-30), now), Is.EqualTo("10 Oct"));
      Assert.That(RelativeTimeConverter.Format(new DateTime(2024, 3, 1), now), Is.EqualTo("1 Mar 2024"));
    }
  }
}
