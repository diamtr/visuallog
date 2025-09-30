using NUnit.Framework;
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
    public void OpenLogs()
    {
      var mvm = new MainViewModel();

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
