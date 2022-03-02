using NUnit.Framework;
using System.IO;

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
  }
}
