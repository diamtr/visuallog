using NUnit.Framework;
using System.IO;
using System.Linq;
using VisualLog.Core;
using VisualLog.Desktop.FormatManager;

namespace VisualLog.Desktop.Tests
{
  [TestFixture]
  public class FormatViewModelFixture
  {
    [Test]
    public void CreateNewLogFormatViewModel()
    {
      var vm = new FormatViewModel();
      Assert.IsNull(vm.Format);
      Assert.IsNull(vm.SelectedFormatter);

      var format = new Format();
      vm = new FormatViewModel(format);
      Assert.AreEqual(format, vm.Format);
      Assert.IsNull(vm.SelectedFormatter);

    }

    [Test]
    public void AddNewFormatter()
    {
      var format = new Format();
      var formatVM = new FormatViewModel(format);
      formatVM.AddNewFormatter();
      Assert.AreEqual(1, formatVM.Formatters.Count);
      formatVM.AddNewFormatterCommand.Execute(null);
      Assert.AreEqual(2, formatVM.Formatters.Count);
    }

    [Test]
    public void SaveFormat()
    {
      var fileName = @"VisualLog.Formats.Test.json";
      if (File.Exists(fileName))
        File.Delete(fileName);

      Formats.InitFrom(fileName);
      var format = new Format() { Name = "test format" };
      var formatVM = new FormatViewModel(format);
      formatVM.SaveFormat();
      var formats = Formats.Read();
      Assert.AreEqual(1, formats.Count());
      Assert.AreEqual(1, formats.First().Id);
      Assert.AreEqual("test format", formats.First().Name);

      if (File.Exists(fileName))
        File.Delete(fileName);
    }
  }
}
