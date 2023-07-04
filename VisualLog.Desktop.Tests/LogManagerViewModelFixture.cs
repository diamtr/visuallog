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

      Assert.IsTrue(lmvm.OpenLogsCommand.CanExecute(null));
      Assert.DoesNotThrow(() => { lmvm.OpenLogsCommand.Execute(null); });
    }

    [Test]
    public void OpenLogs()
    {
      var lmvm = new LogManagerViewModel();

      // Without parameters
      Assert.DoesNotThrow(() => { lmvm.OpenLogs(); });
      CollectionAssert.IsEmpty(lmvm.Logs);
      Assert.IsNull(lmvm.ActiveLog);

      // Parameter is null
      Assert.DoesNotThrow(() => { lmvm.OpenLogs(null); });
      CollectionAssert.IsEmpty(lmvm.Logs);
      Assert.IsNull(lmvm.ActiveLog);

      // Path does not exist
      Assert.DoesNotThrow(() => { lmvm.OpenLogs("thepathdoesnotexist"); });
      CollectionAssert.IsEmpty(lmvm.Logs);
      Assert.IsNull(lmvm.ActiveLog);

      // Ordinary. Single Path
      Assert.DoesNotThrow(() => { lmvm.OpenLogs("doc\\simpletest.log"); });
      CollectionAssert.IsNotEmpty(lmvm.Logs);
      CollectionAssert.AllItemsAreNotNull(lmvm.Logs);
      Assert.AreEqual(1, lmvm.Logs.Count);
      var log = lmvm.Logs.First();
      Assert.AreEqual("simpletest.log", log.DisplayName);
      CollectionAssert.IsNotEmpty(log.LogMessages);
      Assert.AreEqual(3, log.LogMessages.Count);
      Assert.IsNotNull(lmvm.ActiveLog);
      Assert.AreEqual("simpletest.log", lmvm.ActiveLog.DisplayName);

      // Ordinary. Open the log again
      Assert.DoesNotThrow(() => { lmvm.OpenLogs("doc\\simpletest.log"); });
      CollectionAssert.IsNotEmpty(lmvm.Logs);
      CollectionAssert.AllItemsAreNotNull(lmvm.Logs);
      Assert.AreEqual(1, lmvm.Logs.Count);
      log = lmvm.Logs.First();
      Assert.AreEqual("simpletest.log", log.DisplayName);
      Assert.IsNotNull(lmvm.ActiveLog);
      Assert.AreEqual("simpletest.log", lmvm.ActiveLog.DisplayName);

      // Trash call
      Assert.DoesNotThrow(() => { lmvm.OpenLogs("thepathdoesnotexist"); });
      CollectionAssert.IsNotEmpty(lmvm.Logs);
      CollectionAssert.AllItemsAreNotNull(lmvm.Logs);
      Assert.AreEqual(1, lmvm.Logs.Count);
      Assert.IsNotNull(lmvm.ActiveLog);
      Assert.AreEqual("simpletest.log", lmvm.ActiveLog.DisplayName);
    }
  }
}
