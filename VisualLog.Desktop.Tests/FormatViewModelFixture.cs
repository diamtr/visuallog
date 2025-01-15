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
      Assert.That(vm.Format, Is.Null);
      Assert.That(vm.SelectedFormatter, Is.Null);

      var format = new Format();
      vm = new FormatViewModel(format);
      Assert.That(vm.Format, Is.EqualTo(format));
      Assert.That(vm.SelectedFormatter, Is.Null);

    }

    [Test]
    public void AddNewFormatter()
    {
      var format = new Format();
      var formatVM = new FormatViewModel(format);
      formatVM.AddNewFormatter();
      Assert.That(formatVM.Formatters.Count, Is.EqualTo(1));
      formatVM.AddNewFormatterCommand.Execute(null);
      Assert.That(formatVM.Formatters.Count, Is.EqualTo(2));
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
      Assert.That(formats.Count(), Is.EqualTo(1));
      Assert.That(formats.First().Id, Is.EqualTo(1));
      Assert.That(formats.First().Name, Is.EqualTo("test format"));

      if (File.Exists(fileName))
        File.Delete(fileName);
    }
  }
}
