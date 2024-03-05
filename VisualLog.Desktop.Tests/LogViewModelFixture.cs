using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using VisualLog.Core;
using VisualLog.Desktop.LogManager;

namespace VisualLog.Desktop.Tests
{
  [TestFixture]
  public class LogViewModelFixture
  {
    [Test]
    public void NewLogViewModel()
    {
      var vm = new LogViewModel();
      CollectionAssert.IsEmpty(vm.LogMessages);
      CollectionAssert.IsNotEmpty(vm.Encodings);
      CollectionAssert.AreEquivalent(Encoding.GetEncodings().Select(x => vm.GetEncodingDisplayName(x.GetEncoding())), vm.Encodings);
      Assert.IsTrue(vm.SelectedEncoding.StartsWith("65001"));
      Assert.IsNotNull(vm.State);
      Assert.IsFalse(vm.State.ShowSelectedMessageHorizontal);
      Assert.IsFalse(vm.State.ShowSelectedMessageVertical);
      Assert.IsTrue(vm.State.FollowTail);
    }

    [Test]
    public void InitEncodings()
    {
      var vm = new LogViewModel();
      vm.Encodings = new List<string>();
      CollectionAssert.IsEmpty(vm.Encodings);
      vm.InitEncodings();
      CollectionAssert.IsNotEmpty(vm.Encodings);
      CollectionAssert.AreEquivalent(Encoding.GetEncodings().Select(x => vm.GetEncodingDisplayName(x.GetEncoding())), vm.Encodings);
    }

    [Test]
    public void ReadLog()
    {
      var vm = new LogViewModel();
      CollectionAssert.IsEmpty(vm.LogMessages);
      vm.ReadLog();
      CollectionAssert.IsEmpty(vm.LogMessages);

      var logLines = new string[]
      {
        "line1",
        "line2",
        "line3"
      };
      File.WriteAllLines("testlog.log", logLines);

      var expectedMessages = new List<Message>();
      expectedMessages.Add(new Message("line1"));
      expectedMessages.Add(new Message("line2"));
      expectedMessages.Add(new Message("line3"));

      vm.ReadLog("testlog.log");
      CollectionAssert.IsNotEmpty(vm.LogMessages);
      CollectionAssert.AreEqual(expectedMessages, vm.LogMessages.Select(x => x.Message));

      vm.ReadLog();
      CollectionAssert.IsNotEmpty(vm.LogMessages);
      CollectionAssert.AreEqual(expectedMessages, vm.LogMessages.Select(x => x.Message));

      File.Delete("testlog.log");
    }

    [Test]
    public void OnNewLogMessageCatched()
    {
      var vm = new LogViewModel();
      var logLines = new string[]
      {
        "line1",
        "line2",
        "line3"
      };
      File.WriteAllLines("testlog.log", logLines);

      CollectionAssert.IsEmpty(vm.LogMessages);

      var expectedMessages = new List<Message>();
      expectedMessages.Add(new Message("line1"));
      expectedMessages.Add(new Message("line2"));
      expectedMessages.Add(new Message("line3"));
      vm.ReadLog("testlog.log");
      CollectionAssert.IsNotEmpty(vm.LogMessages);
      CollectionAssert.AreEqual(expectedMessages, vm.LogMessages.Select(x => x.Message));

      expectedMessages.Add(new Message("line4"));
      Assert.DoesNotThrow(() => { vm.OnNewLogMessageCatched(new Message("line4")); });
      CollectionAssert.IsNotEmpty(vm.LogMessages);
      CollectionAssert.AreEqual(expectedMessages, vm.LogMessages.Select(x => x.Message));
    }

    [Test]
    public void FollowTail()
    {
      var vm = new LogViewModel();
      var logLines = new string[]
      {
        "line1",
        "line2",
        "line3"
      };
      File.WriteAllLines("testlog.log", logLines);

      CollectionAssert.IsEmpty(vm.LogMessages);
      Assert.DoesNotThrow(() => { vm.FollowTail(); });
      CollectionAssert.IsEmpty(vm.LogMessages);

      var expectedMessages = new List<Message>();
      expectedMessages.Add(new Message("line1"));
      expectedMessages.Add(new Message("line2"));
      expectedMessages.Add(new Message("line3"));
      vm.ReadLog("testlog.log");
      CollectionAssert.IsNotEmpty(vm.LogMessages);
      CollectionAssert.AreEqual(expectedMessages, vm.LogMessages.Select(x => x.Message));

      expectedMessages.Add(new Message("line4"));
      File.AppendAllLines("testlog.log", new string[] { "line4" });
      System.Threading.Thread.Sleep(TimeSpan.FromMilliseconds(FileWatcher.DefaultWatchInterval + 100));
      CollectionAssert.IsNotEmpty(vm.LogMessages);
      System.Threading.Thread.Sleep(TimeSpan.FromMilliseconds(FileWatcher.DefaultWatchInterval + 100));
      CollectionAssert.AreEqual(expectedMessages, vm.LogMessages.Select(x => x.Message));
    }
  }
}