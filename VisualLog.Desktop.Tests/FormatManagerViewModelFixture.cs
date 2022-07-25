using NUnit.Framework;
using VisualLog.Desktop.FormatManager;

namespace VisualLog.Desktop.Tests
{
  [TestFixture]
  public class FormatManagerViewModelFixture
  {
    [Test]
    public void CreateNewFormat()
    {
      var vm = new FormatManagerViewModel();
      CollectionAssert.IsEmpty(vm.Formats);
      Assert.IsNull(vm.SelectedFormat);

      vm.CreateFormat();
      CollectionAssert.IsNotEmpty(vm.Formats);
      Assert.AreEqual(1, vm.Formats.Count);
      Assert.IsNotNull(vm.SelectedFormat);

      vm.CreateFormatCommand.Execute(null);
      CollectionAssert.IsNotEmpty(vm.Formats);
      Assert.AreEqual(2, vm.Formats.Count);
      Assert.IsNotNull(vm.SelectedFormat);
    }
  }
}
