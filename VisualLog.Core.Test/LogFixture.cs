using NUnit.Framework;
using System.Collections.Generic;
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
      var path = Path.Combine(TestLogsDirName, "0000.log");
      var log = new Log(path);
      log.Read();

      Assert.AreEqual(3, log.Messages.Count);
      Assert.AreEqual("line0", log.Messages[0].RawValue);
      Assert.AreEqual("line1", log.Messages[1].RawValue);
      Assert.AreEqual("line2", log.Messages[2].RawValue);
    }

    [Test]
    public void ReadEncoding()
    {
      var path = Path.Combine(TestLogsDirName, "0001_ru_utf8.log");

      var log = new Log(path);
      log.Read();

      Assert.AreEqual(3, log.Messages.Count);
      Assert.AreEqual("line0", log.Messages[0].RawValue);
      Assert.AreEqual("линия1", log.Messages[1].RawValue);
      Assert.AreEqual("line2", log.Messages[2].RawValue);

      log = new Log(path, Encoding.UTF8);
      log.Read();

      Assert.AreEqual(3, log.Messages.Count);
      Assert.AreEqual("line0", log.Messages[0].RawValue);
      Assert.AreEqual("линия1", log.Messages[1].RawValue);
      Assert.AreEqual("line2", log.Messages[2].RawValue);

      log = new Log(path, Encoding.ASCII);
      log.Read();

      Assert.AreEqual(3, log.Messages.Count);
      Assert.AreEqual("line0", log.Messages[0].RawValue);
      Assert.AreEqual("??????????1", log.Messages[1].RawValue);
      var s = log.Messages[1].RawValue;
      Assert.AreEqual("line2", log.Messages[2].RawValue);


      path = Path.Combine(TestLogsDirName, "0002_ru_ascii.log");

      log = new Log(path);
      log.Read();
      Assert.AreEqual(3, log.Messages.Count);
      Assert.AreEqual("line0", log.Messages[0].RawValue);
      Assert.AreEqual("?????1", log.Messages[1].RawValue);
      Assert.AreEqual("line2", log.Messages[2].RawValue);

      log = new Log(path, Encoding.UTF8);
      log.Read();
      Assert.AreEqual(3, log.Messages.Count);
      Assert.AreEqual("line0", log.Messages[0].RawValue);
      Assert.AreEqual("?????1", log.Messages[1].RawValue);
      Assert.AreEqual("line2", log.Messages[2].RawValue);
    }

    [Test]
    public void ApplyFormatNull()
    {
      var log = new Log(null, Encoding.UTF8);
      log.Messages.Add(new Message("line1"));
      log.Messages.Add(new Message("line2"));
      Assert.DoesNotThrow(() => log.ApplyFormat());
    }

    [Test]
    public void ApplyFormatSimpleText()
    {
      var log = new Log(null, Encoding.UTF8);
      log.Messages.Add(new Message("line1"));
      log.Messages.Add(new Message("line2"));
      var format = new Format();
      format.Formatters.Add(new MessageFormatter() { Name = "Line", Priority = 0, Pattern = @"^.*" });
      log.Format = format;
      Assert.DoesNotThrow(() => log.ApplyFormat());

      CollectionAssert.IsNotEmpty(log.Messages[0].Parts);
      var expected = new Dictionary<string, string>();
      expected.Add("Line", "line1");
      CollectionAssert.AreEquivalent(expected, log.Messages[0].Parts);

      CollectionAssert.IsNotEmpty(log.Messages[1].Parts);
      expected = new Dictionary<string, string>();
      expected.Add("Line", "line2");
      CollectionAssert.AreEquivalent(expected, log.Messages[1].Parts);
    }

    [Test]
    public void ApplyFormatDateTimeAndText()
    {
      var log = new Log(null, Encoding.UTF8);
      log.Messages.Add(new Message(" 09.09.2009 09:09:09 line1"));
      log.Messages.Add(new Message("09.09.2009 09:09:09\tline2 "));
      log.Messages.Add(new Message("\tline3 "));
      var format = new Format();
      format.Formatters.Add(new MessageFormatter() { Name = "Text", Priority = 2, Pattern = @"^.*" });
      format.Formatters.Add(new MessageFormatter() { Name = "DateTime", Priority = 1, Pattern = @"^\d{2}\.\d{2}\.\d{4}\s\d{2}\:\d{2}\:\d{2}" });
      log.Format = format;
      Assert.DoesNotThrow(() => log.ApplyFormat());

      CollectionAssert.IsNotEmpty(log.Messages[0].Parts);
      var expected = new Dictionary<string, string>();
      expected.Add("DateTime", "09.09.2009 09:09:09");
      expected.Add("Text", "line1");
      CollectionAssert.AreEquivalent(expected, log.Messages[0].Parts);

      CollectionAssert.IsNotEmpty(log.Messages[1].Parts);
      expected = new Dictionary<string, string>();
      expected.Add("DateTime", "09.09.2009 09:09:09");
      expected.Add("Text", "line2");
      CollectionAssert.AreEquivalent(expected, log.Messages[1].Parts);

      CollectionAssert.IsNotEmpty(log.Messages[2].Parts);
      expected = new Dictionary<string, string>();
      expected.Add("Text", "line3");
      CollectionAssert.AreEquivalent(expected, log.Messages[2].Parts);
    }

    [Test]
    public void StartFollowTail()
    {
      var path = Path.Combine(TestLogsDirName, "followtail.log");
      File.WriteAllText(path, null);
      var log = new Log(path);
      var newMessagesCount = 0;
      log.CatchNewMessage += (Message message) => { newMessagesCount++; };
      CollectionAssert.IsEmpty(log.Messages);

      Assert.DoesNotThrow(() => { log.StartFollowTail(); });

      File.AppendAllLines(path, new List<string>() { "xx.xx.xxxx xx:xx:xx.xxxx Trace First update followtail.log" });
      System.Threading.Thread.Sleep(System.TimeSpan.FromMilliseconds(5));
      CollectionAssert.IsNotEmpty(log.Messages);
      Assert.AreEqual(1, log.Messages.Count);
      Assert.AreEqual("xx.xx.xxxx xx:xx:xx.xxxx Trace First update followtail.log", log.Messages[0].RawValue);
      Assert.AreEqual(1, newMessagesCount);

      File.AppendAllLines(path, new List<string>() { "xx.xx.xxxx xx:xx:xx.xxxx Trace Second update followtail.log" });
      System.Threading.Thread.Sleep(System.TimeSpan.FromMilliseconds(5));
      Assert.AreEqual(2, log.Messages.Count);
      Assert.AreEqual("xx.xx.xxxx xx:xx:xx.xxxx Trace First update followtail.log", log.Messages[0].RawValue);
      Assert.AreEqual("xx.xx.xxxx xx:xx:xx.xxxx Trace Second update followtail.log", log.Messages[1].RawValue);
      Assert.AreEqual(2, newMessagesCount);

      File.WriteAllText(path, null);
    }

    [Test]
    public void ReadLogUpdates()
    {
      var path = Path.Combine(TestLogsDirName, "followtail.log");
      File.WriteAllText(path, null);
      var log = new Log(path);
      var newMessagesCount = 0;
      log.CatchNewMessage += (Message message) => { newMessagesCount++; };
      CollectionAssert.IsEmpty(log.Messages);

      File.AppendAllLines(path, new List<string>() { "xx.xx.xxxx xx:xx:xx.xxxx Trace First update followtail.log" });
      System.Threading.Thread.Sleep(System.TimeSpan.FromMilliseconds(5));

      Assert.DoesNotThrow(() => { log.ReadLogUpdates(null, new FileSystemEventArgs(WatcherChangeTypes.Changed, TestLogsDirName, "followtail.log")); });
      CollectionAssert.IsNotEmpty(log.Messages);
      Assert.AreEqual(1, log.Messages.Count);
      Assert.AreEqual("xx.xx.xxxx xx:xx:xx.xxxx Trace First update followtail.log", log.Messages[0].RawValue);
      Assert.AreEqual(1, newMessagesCount);

      File.WriteAllText(path, null);
    }

    [Test]
    public void ReadingInProcess()
    {
      var path = Path.Combine(TestLogsDirName, "followtail.log");
      File.WriteAllText(path, null);
      var log = new Log(path);
      CollectionAssert.IsEmpty(log.Messages);

      log.ReadingInProcess = true;

      File.AppendAllLines(path, new List<string>() { "xx.xx.xxxx xx:xx:xx.xxxx Trace First update followtail.log" });
      System.Threading.Thread.Sleep(System.TimeSpan.FromMilliseconds(5));
      Assert.DoesNotThrow(() => { log.ReadLogUpdates(null, new FileSystemEventArgs(WatcherChangeTypes.Changed, TestLogsDirName, "followtail.log")); });
      CollectionAssert.IsEmpty(log.Messages);

      File.AppendAllLines(path, new List<string>() { "xx.xx.xxxx xx:xx:xx.xxxx Trace Second update followtail.log" });
      System.Threading.Thread.Sleep(System.TimeSpan.FromMilliseconds(5));
      Assert.DoesNotThrow(() => { log.ReadLogUpdates(null, new FileSystemEventArgs(WatcherChangeTypes.Changed, TestLogsDirName, "followtail.log")); });
      CollectionAssert.IsEmpty(log.Messages);

      log.ReadingInProcess = false;

      Assert.DoesNotThrow(() => { log.ReadLogUpdates(null, new FileSystemEventArgs(WatcherChangeTypes.Changed, TestLogsDirName, "followtail.log")); });
      Assert.AreEqual(2, log.Messages.Count);
      Assert.AreEqual("xx.xx.xxxx xx:xx:xx.xxxx Trace First update followtail.log", log.Messages[0].RawValue);
      Assert.AreEqual("xx.xx.xxxx xx:xx:xx.xxxx Trace Second update followtail.log", log.Messages[1].RawValue);

      File.WriteAllText(path, null);
    }
  }
}
