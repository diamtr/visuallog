using NUnit.Framework;
using System.IO;
using System.Text;

namespace VisualLog.Core.Test
{
  [TestFixture]
  public class LogFixture
  {
    private const string TestLogsDirName = @"test_logs";

    [Test]
    public void Read()
    {
      var log = new Log();
      var path = Path.Combine(TestLogsDirName, "0000.log");
      log.Read(path);

      Assert.AreEqual(3, log.Messages.Count);
      Assert.AreEqual(0, log.Messages[0].Number);
      Assert.AreEqual("line0", log.Messages[0].RawValue);
      Assert.AreEqual(1, log.Messages[1].Number);
      Assert.AreEqual("line1", log.Messages[1].RawValue);
      Assert.AreEqual(2, log.Messages[2].Number);
      Assert.AreEqual("line2", log.Messages[2].RawValue);
    }

    [Test]
    public void ReadEncoding()
    {
      var path = Path.Combine(TestLogsDirName, "0001_ru_utf8.log");

      var log = new Log();
      log.Read(path);

      Assert.AreEqual(3, log.Messages.Count);
      Assert.AreEqual(0, log.Messages[0].Number);
      Assert.AreEqual("line0", log.Messages[0].RawValue);
      Assert.AreEqual(1, log.Messages[1].Number);
      Assert.AreEqual("линия1", log.Messages[1].RawValue);
      Assert.AreEqual(2, log.Messages[2].Number);
      Assert.AreEqual("line2", log.Messages[2].RawValue);

      log = new Log(Encoding.UTF8);
      log.Read(path);

      Assert.AreEqual(3, log.Messages.Count);
      Assert.AreEqual(0, log.Messages[0].Number);
      Assert.AreEqual("line0", log.Messages[0].RawValue);
      Assert.AreEqual(1, log.Messages[1].Number);
      Assert.AreEqual("линия1", log.Messages[1].RawValue);
      Assert.AreEqual(2, log.Messages[2].Number);
      Assert.AreEqual("line2", log.Messages[2].RawValue);

      log = new Log(Encoding.ASCII);
      log.Read(path);

      Assert.AreEqual(3, log.Messages.Count);
      Assert.AreEqual(0, log.Messages[0].Number);
      Assert.AreEqual("line0", log.Messages[0].RawValue);
      Assert.AreEqual(1, log.Messages[1].Number);
      Assert.AreEqual("??????????1", log.Messages[1].RawValue);
      var s = log.Messages[1].RawValue;
      Assert.AreEqual(2, log.Messages[2].Number);
      Assert.AreEqual("line2", log.Messages[2].RawValue);


      path = Path.Combine(TestLogsDirName, "0002_ru_ascii.log");

      log = new Log();
      log.Read(path);
      Assert.AreEqual(3, log.Messages.Count);
      Assert.AreEqual(0, log.Messages[0].Number);
      Assert.AreEqual("line0", log.Messages[0].RawValue);
      Assert.AreEqual(1, log.Messages[1].Number);
      Assert.AreEqual("?????1", log.Messages[1].RawValue);
      Assert.AreEqual(2, log.Messages[2].Number);
      Assert.AreEqual("line2", log.Messages[2].RawValue);

      log = new Log(Encoding.UTF8);
      log.Read(path);
      Assert.AreEqual(3, log.Messages.Count);
      Assert.AreEqual(0, log.Messages[0].Number);
      Assert.AreEqual("line0", log.Messages[0].RawValue);
      Assert.AreEqual(1, log.Messages[1].Number);
      Assert.AreEqual("?????1", log.Messages[1].RawValue);
      Assert.AreEqual(2, log.Messages[2].Number);
      Assert.AreEqual("line2", log.Messages[2].RawValue);
    }
  }
}
