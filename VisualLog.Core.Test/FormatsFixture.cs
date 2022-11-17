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
      Assert.AreEqual("VisualLog.Formats.json", formats.Source);
    }

    [Test]
    public void Read()
    {
      var formats = new Formats();
      
      formats.Source = null;
      Assert.DoesNotThrow(() => formats.Read());
      CollectionAssert.IsEmpty(formats.Read());
      
      formats.Source = @"{867F2E4B-25B8-4751-8234-124382930743}";
      Assert.DoesNotThrow(() => formats.Read());
      CollectionAssert.IsEmpty(formats.Read());

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
        Assert.AreEqual(1, f.Count());
        Assert.AreEqual(1, f.First().Id);
        Assert.AreEqual("Seventy four", f.First().Name);
        Assert.AreEqual(2, f.First().Formatters.Count());

        f = Formats.Read(fileName);
        Assert.AreEqual(1, f.Count());
        Assert.AreEqual(1, f.First().Id);
        Assert.AreEqual("Seventy four", f.First().Name);
        Assert.AreEqual(2, f.First().Formatters.Count());
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
      Assert.AreEqual(1, formatsInFile.Count());
      var formatInFile = formatsInFile.First();
      Assert.AreEqual(1, formatInFile.Id);
      Assert.AreEqual("TestLogFormat", formatInFile.Name);
      Assert.AreEqual(1, formatInFile.Formatters.Count);
      var formatterInFile = formatInFile.Formatters.First();
      Assert.AreEqual("TestMessageFormatter", formatterInFile.Name);
      Assert.AreEqual(0, formatterInFile.Priority);
      Assert.AreEqual("*", formatterInFile.Pattern);

      if (File.Exists(fileName))
        File.Delete(fileName);
      Formats.InitFrom(fileName);
      Formats.Write(format);
      formatsInFile = Formats.Read();
      Assert.AreEqual(1, formatsInFile.Count());
      formatInFile = formatsInFile.First();
      Assert.AreEqual(1, formatInFile.Id);
      Assert.AreEqual("TestLogFormat", formatInFile.Name);
      Assert.AreEqual(1, formatInFile.Formatters.Count);
      formatterInFile = formatInFile.Formatters.First();
      Assert.AreEqual("TestMessageFormatter", formatterInFile.Name);
      Assert.AreEqual(0, formatterInFile.Priority);
      Assert.AreEqual("*", formatterInFile.Pattern);

      if (File.Exists(fileName))
        File.Delete(fileName);
    }
  }
}
