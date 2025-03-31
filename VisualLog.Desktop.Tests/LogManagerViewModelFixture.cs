using NUnit.Framework;
using System.Linq;
using VisualLog.Desktop.LogManager;

namespace VisualLog.Desktop.Tests
{
  [TestFixture]
  public class LogManagerViewModelFixture
  {
    [Test]
    public void OpenLogsCommand()
    {
      var lmvm = new LogManagerViewModel();

      Assert.That(lmvm.OpenLogsCommand.CanExecute(null), Is.True);
      Assert.DoesNotThrow(() => { lmvm.OpenLogsCommand.Execute("doc\\simpletest.log"); });
    }

    [Test]
    public void OpenLogs()
    {
      var lmvm = new LogManagerViewModel();

      // Without parameters
      Assert.DoesNotThrow(() => { lmvm.OpenLogs(); });
      Assert.That(lmvm.Logs, Is.Empty);
      Assert.That(lmvm.ActiveLog, Is.Null);

      // Parameter is null
      Assert.DoesNotThrow(() => { lmvm.OpenLogs(null); });
      Assert.That(lmvm.Logs, Is.Empty);
      Assert.That(lmvm.ActiveLog, Is.Null);

      // Path does not exist
      Assert.DoesNotThrow(() => { lmvm.OpenLogs("thepathdoesnotexist"); });
      Assert.That(lmvm.Logs, Is.Empty);
      Assert.That(lmvm.ActiveLog, Is.Null);

      // Ordinary. Single Path
      Assert.DoesNotThrow(() => { lmvm.OpenLogs("doc\\simpletest.log"); });
      Assert.That(lmvm.Logs, Is.Not.Empty);
      Assert.That(lmvm.Logs, Is.All.Not.Null);
      Assert.That(lmvm.Logs.Count, Is.EqualTo(1));
      var log = lmvm.Logs.First();
      Assert.That(log.DisplayName, Is.EqualTo("simpletest.log"));
      Assert.That(log.LogMessages, Is.Not.Empty);
      Assert.That(log.LogMessages.Count, Is.EqualTo(3));
      Assert.That(lmvm.ActiveLog, Is.Not.Null);
      Assert.That(lmvm.ActiveLog.DisplayName, Is.EqualTo("simpletest.log"));

      // Ordinary. Open the log again
      Assert.DoesNotThrow(() => { lmvm.OpenLogs("doc\\simpletest.log"); });
      Assert.That(lmvm.Logs, Is.Not.Empty);
      Assert.That(lmvm.Logs, Is.All.Not.Null);
      Assert.That(lmvm.Logs.Count, Is.EqualTo(1));
      log = lmvm.Logs.First();
      Assert.That(log.DisplayName, Is.EqualTo("simpletest.log"));
      Assert.That(lmvm.ActiveLog, Is.Not.Null);
      Assert.That(lmvm.ActiveLog.DisplayName, Is.EqualTo("simpletest.log"));

      // Trash call
      Assert.DoesNotThrow(() => { lmvm.OpenLogs("thepathdoesnotexist"); });
      Assert.That(lmvm.Logs, Is.Not.Empty);
      Assert.That(lmvm.Logs, Is.All.Not.Null);
      Assert.That(lmvm.Logs.Count, Is.EqualTo(1));
      Assert.That(lmvm.ActiveLog, Is.Not.Null);
      Assert.That(lmvm.ActiveLog.DisplayName, Is.EqualTo("simpletest.log"));
    }
  }
}
