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
      Assert.That(vm.Formats, Is.Empty);
      Assert.That(vm.SelectedFormat, Is.Null);

      vm.CreateFormat();
      Assert.That(vm.Formats, Is.Not.Empty);
      Assert.That(vm.SelectedFormat, Is.Not.Null);
      Assert.That(vm.Formats.Count, Is.EqualTo(1));

      vm.CreateFormatCommand.Execute(null);
      Assert.That(vm.Formats, Is.Not.Empty);
      Assert.That(vm.SelectedFormat, Is.Not.Null);
      Assert.That(vm.Formats.Count, Is.EqualTo(2));
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

        Assert.That(vm.Formats.Count, Is.EqualTo(1));
        var formatVM = vm.Formats.First();
        Assert.That(formatVM.Format.Name, Is.EqualTo("Seventy four"));
        Assert.That(formatVM.Formatters.Count, Is.EqualTo(2));
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
      Assert.That(vm, Is.Not.Null);
      Assert.That(vm.Formatter.Name, Is.EqualTo(name));
      Assert.That(vm.Formatter.Priority, Is.EqualTo(priority));
      Assert.That(vm.Formatter.Pattern, Is.EqualTo(pattern));
    }
  }
}
