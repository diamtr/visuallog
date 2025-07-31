using NUnit.Framework;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

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

      Assert.That(log.Messages.Count, Is.EqualTo(3));
      Assert.That(log.Messages[0].RawValue, Is.EqualTo("line0"));
      Assert.That(log.Messages[1].RawValue, Is.EqualTo("line1"));
      Assert.That(log.Messages[2].RawValue, Is.EqualTo("line2"));
    }

    [Test]
    public void ReadEncoding()
    {
      var path = Path.Combine(TestLogsDirName, "0001_ru_utf8.log");

      var log = new Log(path);
      log.Read();

      Assert.That(log.Messages.Count, Is.EqualTo(3));
      Assert.That(log.Messages[0].RawValue, Is.EqualTo("line0"));
      Assert.That(log.Messages[1].RawValue, Is.EqualTo("линия1"));
      Assert.That(log.Messages[2].RawValue, Is.EqualTo("line2"));

      log = new Log(path, Encoding.UTF8);
      log.Read();

      Assert.That(log.Messages.Count, Is.EqualTo(3));
      Assert.That(log.Messages[0].RawValue, Is.EqualTo("line0"));
      Assert.That(log.Messages[1].RawValue, Is.EqualTo("линия1"));
      Assert.That(log.Messages[2].RawValue, Is.EqualTo("line2"));

      log = new Log(path, Encoding.ASCII);
      log.Read();

      Assert.That(log.Messages.Count, Is.EqualTo(3));
      Assert.That(log.Messages[0].RawValue, Is.EqualTo("line0"));
      Assert.That(log.Messages[1].RawValue, Is.EqualTo("??????????1"));
      Assert.That(log.Messages[2].RawValue, Is.EqualTo("line2"));

      path = Path.Combine(TestLogsDirName, "0002_ru_ascii.log");

      log = new Log(path);
      log.Read();
      Assert.That(log.Messages.Count, Is.EqualTo(3));
      Assert.That(log.Messages[0].RawValue, Is.EqualTo("line0"));
      Assert.That(log.Messages[1].RawValue, Is.EqualTo("?????1"));
      Assert.That(log.Messages[2].RawValue, Is.EqualTo("line2"));

      log = new Log(path, Encoding.UTF8);
      log.Read();
      Assert.That(log.Messages.Count, Is.EqualTo(3));
      Assert.That(log.Messages[0].RawValue, Is.EqualTo("line0"));
      Assert.That(log.Messages[1].RawValue, Is.EqualTo("?????1"));
      Assert.That(log.Messages[2].RawValue, Is.EqualTo("line2"));
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

      Assert.That(log.Messages[0].Items, Is.Not.Empty);
      var expected = new Dictionary<string, string>();
      expected.Add("Line", "line1");
      Assert.That(log.Messages[0].Items, Is.EquivalentTo(expected));

      Assert.That(log.Messages[1].Items, Is.Not.Empty);
      expected = new Dictionary<string, string>();
      expected.Add("Line", "line2");
      Assert.That(log.Messages[1].Items, Is.EquivalentTo(expected));
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

      Assert.That(log.Messages[0].Items, Is.Not.Empty);
      var expected = new Dictionary<string, string>();
      expected.Add("DateTime", "09.09.2009 09:09:09");
      expected.Add("Text", "line1");
      Assert.That(log.Messages[0].Items, Is.EquivalentTo(expected));

      Assert.That(log.Messages[1].Items, Is.Not.Empty);
      expected = new Dictionary<string, string>();
      expected.Add("DateTime", "09.09.2009 09:09:09");
      expected.Add("Text", "line2");
      Assert.That(log.Messages[1].Items, Is.EquivalentTo(expected));

      Assert.That(log.Messages[2].Items, Is.Not.Empty);
      expected = new Dictionary<string, string>();
      expected.Add("Text", "line3");
      Assert.That(log.Messages[2].Items, Is.EquivalentTo(expected));
    }

    [Test]
    public void StartFollowTail()
    {
      var path = Path.Combine(TestLogsDirName, "followtail.log");
      File.WriteAllText(path, null);
      var log = new Log(path);
      var newMessagesCount = 0;
      log.MessageCatched += (Message message) => { newMessagesCount++; };
      Assert.That(log.Messages, Is.Empty);

      Assert.DoesNotThrow(() => { log.Read(); });

      File.AppendAllLines(path, new List<string>() { "xx.xx.xxxx xx:xx:xx.xxxx Trace First update followtail.log" });
      Task.Run(() => { System.Threading.Thread.Sleep(System.TimeSpan.FromMilliseconds(FileWatcher.DefaultWatchInterval + 1300)); }).Wait();
      Assert.That(newMessagesCount, Is.EqualTo(1));

      File.AppendAllLines(path, new List<string>() { "xx.xx.xxxx xx:xx:xx.xxxx Trace Second update followtail.log" });
      Task.Run(() => { System.Threading.Thread.Sleep(System.TimeSpan.FromMilliseconds(FileWatcher.DefaultWatchInterval + 1300)); }).Wait();
      Assert.That(newMessagesCount, Is.EqualTo(2));

      File.WriteAllText(path, null);

      TestContext.WriteLine($"Attempt: {TestContext.CurrentContext.CurrentRepeatCount}");
    }
  }
}
