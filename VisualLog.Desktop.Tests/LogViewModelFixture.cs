using NUnit.Framework;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

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

      vm.ReadLog("testlog.log");
      CollectionAssert.IsNotEmpty(vm.LogMessages);
      CollectionAssert.AreEqual(logLines, vm.LogMessages);

      vm.LogMessages = new List<string>();

      vm.ReadLog();
      CollectionAssert.IsNotEmpty(vm.LogMessages);
      CollectionAssert.AreEqual(logLines, vm.LogMessages);

      File.Delete("testlog.log");
    }
  }
}