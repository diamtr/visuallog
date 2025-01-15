using NUnit.Framework;
using System.IO;
using System.Linq;

namespace VisualLog.Core.Test
{
  [TestFixture]
  public class FormatsFixture
  {
    [Test]
    public void CreateNewFormats()
    {
      var formats = new Formats();
      Assert.That(formats.Source, Is.EqualTo("VisualLog.Formats.json"));
    }

    [Test]
    public void Read()
    {
      var formats = new Formats();
      
      formats.Source = null;
      Assert.DoesNotThrow(() => formats.Read());
      Assert.That(formats.Read(), Is.Empty);
      
      formats.Source = @"{867F2E4B-25B8-4751-8234-124382930743}";
      Assert.DoesNotThrow(() => formats.Read());
      Assert.That(formats.Read(), Is.Empty);

      var fileName = @"VisualLog.Formats.Test.json";
      var content = "[" +
        "{" +
        "\"Id\":1," +
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
      formats.Source = fileName;

      try
      {
        Assert.DoesNotThrow(() => formats.Read());
        var f = formats.Read();
        Assert.That(f.Count(), Is.EqualTo(1));
        Assert.That(f.First().Id, Is.EqualTo(1));
        Assert.That(f.First().Name, Is.EqualTo("Seventy four"));
        Assert.That(f.First().Formatters.Count(), Is.EqualTo(2));

        f = Formats.Read(fileName);
        Assert.That(f.Count(), Is.EqualTo(1));
        Assert.That(f.First().Id, Is.EqualTo(1));
        Assert.That(f.First().Name, Is.EqualTo("Seventy four"));
        Assert.That(f.First().Formatters.Count(), Is.EqualTo(2));
      }
      catch
      {
        File.Delete(formats.Source);
        throw;
      }
      finally
      {
        if (File.Exists(formats.Source))
          File.Delete(formats.Source);
      }
    }

    [Test]
    public void Write()
    {
      var fileName = @"VisualLog.Formats.Test.json";
      if (File.Exists(fileName))
        File.Delete(fileName);

      var format = new Format();
      format.Name = "TestLogFormat";
      var formatter = new MessageFormatter();
      formatter.Name = "TestMessageFormatter";
      formatter.Priority = 0;
      formatter.Pattern = "*";
      format.Formatters.Add(formatter);

      var formats = new Formats();
      formats.Source = fileName;
      formats.Write(format);

      var formatsInFile = formats.Read();
      Assert.That(formatsInFile.Count(), Is.EqualTo(1));
      var formatInFile = formatsInFile.First();
      Assert.That(formatInFile.Id, Is.EqualTo(1));
      Assert.That(formatInFile.Name, Is.EqualTo("TestLogFormat"));
      Assert.That(formatInFile.Formatters.Count, Is.EqualTo(1));
      var formatterInFile = formatInFile.Formatters.First();
      Assert.That(formatterInFile.Name, Is.EqualTo("TestMessageFormatter"));
      Assert.That(formatterInFile.Priority, Is.EqualTo(0));
      Assert.That(formatterInFile.Pattern, Is.EqualTo("*"));

      if (File.Exists(fileName))
        File.Delete(fileName);
      Formats.InitFrom(fileName);
      Formats.Write(format);
      formatsInFile = Formats.Read();
      Assert.That(formatsInFile.Count(), Is.EqualTo(1));
      formatInFile = formatsInFile.First();
      Assert.That(formatInFile.Id, Is.EqualTo(1));
      Assert.That(formatInFile.Name, Is.EqualTo("TestLogFormat"));
      Assert.That(formatInFile.Formatters.Count, Is.EqualTo(1));
      formatterInFile = formatInFile.Formatters.First();
      Assert.That(formatterInFile.Name, Is.EqualTo("TestMessageFormatter"));
      Assert.That(formatterInFile.Priority, Is.EqualTo(0));
      Assert.That(formatterInFile.Pattern, Is.EqualTo("*"));

      if (File.Exists(fileName))
        File.Delete(fileName);
    }
  }
}
