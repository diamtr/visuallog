using NUnit.Framework;
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
  }
}