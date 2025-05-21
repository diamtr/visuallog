using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
      Assert.That(vm.LogMessages, Is.Empty);
      Assert.That(vm.Encodings, Is.Not.Empty);
      Assert.That(vm.Encodings, Is.EquivalentTo(Encoding.GetEncodings().Select(x => vm.GetEncodingDisplayName(x.GetEncoding()))));
      Assert.That(vm.SelectedEncoding, Does.StartWith("65001"));
      Assert.That(vm.State, Is.Not.Null);
      Assert.That(vm.State.ShowSelectedMessagesPanel, Is.False);
      Assert.That(vm.State.FollowTail, Is.True);
    }

    [Test]
    public void InitEncodings()
    {
      var vm = new LogViewModel();
      vm.Encodings = new List<string>();
      Assert.That(vm.Encodings, Is.Empty);
      vm.InitEncodings();
      Assert.That(vm.Encodings, Is.Not.Empty);
      Assert.That(vm.Encodings, Is.EquivalentTo(Encoding.GetEncodings().Select(x => vm.GetEncodingDisplayName(x.GetEncoding()))));
    }

    [Test]
    public void ReadLog()
    {
      var vm = new LogViewModel();
      Assert.That(vm.LogMessages, Is.Empty);
      vm.ReadLog();
      Assert.That(vm.LogMessages, Is.Empty);

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
      Assert.That(vm.LogMessages, Is.Not.Empty);
      Assert.That(vm.LogMessages.Select(x => x.Message), Is.EqualTo(expectedMessages));

      vm.ReadLog();
      Assert.That(vm.LogMessages, Is.Not.Empty);
      Assert.That(vm.LogMessages.Select(x => x.Message), Is.EqualTo(expectedMessages));

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

      Assert.That(vm.LogMessages, Is.Empty);

      var expectedMessages = new List<Message>();
      expectedMessages.Add(new Message("line1"));
      expectedMessages.Add(new Message("line2"));
      expectedMessages.Add(new Message("line3"));
      vm.ReadLog("testlog.log");
      Assert.That(vm.LogMessages, Is.Not.Empty);
      Assert.That(vm.LogMessages.Select(x => x.Message), Is.EqualTo(expectedMessages));

      expectedMessages.Add(new Message("line4"));
      Assert.DoesNotThrow(() => { vm.OnNewLogMessageCatched(new Message("line4")); });
      Assert.That(vm.LogMessages, Is.Not.Empty);
      Assert.That(vm.LogMessages.Select(x => x.Message), Is.EqualTo(expectedMessages));
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

      Assert.That(vm.LogMessages, Is.Empty);
      Assert.DoesNotThrow(() => { vm.FollowTail(); });
      Assert.That(vm.LogMessages, Is.Empty);

      var expectedMessages = new List<Message>();
      expectedMessages.Add(new Message("line1"));
      expectedMessages.Add(new Message("line2"));
      expectedMessages.Add(new Message("line3"));
      vm.ReadLog("testlog.log");
      Assert.That(vm.LogMessages, Is.Not.Empty);
      Assert.That(vm.LogMessages.Select(x => x.Message), Is.EqualTo(expectedMessages));

      expectedMessages.Add(new Message("line4"));
      File.AppendAllLines("testlog.log", new string[] { "line4" });
      Task.Run(() => { System.Threading.Thread.Sleep(TimeSpan.FromMilliseconds(FileWatcher.DefaultWatchInterval + 1300)); }).Wait();
      Assert.That(vm.LogMessages, Is.Not.Empty);
      
      Task.Run(() => { System.Threading.Thread.Sleep(TimeSpan.FromMilliseconds(FileWatcher.DefaultWatchInterval + 1300)); }).Wait();
      Assert.That(vm.LogMessages.Select(x => x.Message), Is.EqualTo(expectedMessages));

      TestContext.WriteLine($"Attempt: {TestContext.CurrentContext.CurrentRepeatCount}");
    }
  }
}