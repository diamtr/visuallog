using NUnit.Framework;
using System.IO;
using System.Linq;
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

    [Test]
    public void ReadFormats()
    {
      var vm = new FormatManagerViewModel();
      var fileName = @"VisualLog.Formats.Test.json";
      var content = "[" +
        "{" +
        "\"Name\":\"Seventy four\"," +
        "\"Formatters\":[" +
        "{" +
        "\"Name\":\"Year\"," +
        "\"Priority\":256," +
        "\"Pattern\":\"\\\\d{4}\"" +
        "}," +
        "{" +
        "\"Name\":\"Message\"," +
        "\"Priority\":512," +
        "\"Pattern\":\"*\"" +
        "}" +
        "]" +
        "}," +
        "]";
      File.WriteAllText(fileName, content);
      Core.Formats.InitFrom(fileName);

      try
      {
        Assert.DoesNotThrow(() => vm.ReadLogFormats());

        Assert.AreEqual(1, vm.Formats.Count);
        var formatVM = vm.Formats.First();
        Assert.AreEqual("Seventy four", formatVM.Format.Name);
        Assert.AreEqual(2, formatVM.Formatters.Count);
        this.CheckFormatterVM(formatVM.Formatters[0], "Year", 256, "\\d{4}");
        this.CheckFormatterVM(formatVM.Formatters[1], "Message", 512, "*");
      }
      catch
      {
        File.Delete(fileName);
        throw;
      }
      finally
      {
        if (File.Exists(fileName))
          File.Delete(fileName);
      }
    }

    private void CheckFormatterVM(MessageFormatterViewModel vm, string name, int priority, string pattern)
    {
      Assert.IsNotNull(vm);
      Assert.AreEqual(name, vm.Formatter.Name);
      Assert.AreEqual(priority, vm.Formatter.Priority);
      Assert.AreEqual(pattern, vm.Formatter.Pattern);
    }
  }
}
