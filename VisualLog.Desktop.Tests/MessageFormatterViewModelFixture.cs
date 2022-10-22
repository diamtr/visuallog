using NUnit.Framework;
using VisualLog.Core;
using VisualLog.Desktop.FormatManager;

namespace VisualLog.Desktop.Tests
{
  [TestFixture]
  public class MessageFormatterViewModelFixture
  {
    [Test]
    public void CreateNewMessageFormatterViewModel()
    {
      var vm = new MessageFormatterViewModel();
      Assert.IsNull(vm.Formatter);

      var formatter = new MessageFormatter();
      vm = new MessageFormatterViewModel(formatter);
      Assert.AreEqual(formatter, vm.Formatter);
    }
  }
}
