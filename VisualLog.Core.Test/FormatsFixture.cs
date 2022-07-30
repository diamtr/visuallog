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

      formats.Source = @"VisualLog.Formats.Test.json";
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
      File.WriteAllText(formats.Source, content);
      try
      {
        Assert.DoesNotThrow(() => formats.Read());
        var f = formats.Read();
        Assert.AreEqual(1, f.Count());
        Assert.AreEqual("Seventy four", f.First().Name);
        Assert.AreEqual(2, f.First().Formatters.Count());
      }
      catch
      {
        File.Delete(formats.Source);
        Assert.Fail();
      }
      finally
      {
        if (File.Exists(formats.Source))
          File.Delete(formats.Source);
      }
    }
  }
}
