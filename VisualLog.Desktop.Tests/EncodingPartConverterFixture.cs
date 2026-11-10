using NUnit.Framework;
using System.Text;
using VisualLog.Desktop.Converters;
using VisualLog.Desktop.LogManager;

namespace VisualLog.Desktop.Tests
{
  [TestFixture]
  public class EncodingPartConverterFixture
  {
    [Test]
    public void Format()
    {
      var utf8 = new LogViewModel().GetEncodingDisplayName(Encoding.UTF8);

      Assert.That(EncodingPartConverter.Format(utf8, null), Is.EqualTo("UTF-8"));
      Assert.That(EncodingPartConverter.Format(utf8, "Name"), Is.EqualTo(Encoding.UTF8.EncodingName));
      Assert.That(EncodingPartConverter.Format(utf8, "Details"), Is.EqualTo("65001 · utf-8"));

      Assert.That(EncodingPartConverter.Format("1251 windows-1251 Cyrillic (Windows)", null), Is.EqualTo("windows-1251"));
    }

    [Test]
    public void FormatUnexpected()
    {
      Assert.That(EncodingPartConverter.Format(null, null), Is.Empty);
      Assert.That(EncodingPartConverter.Format("", "Name"), Is.Empty);
      Assert.That(EncodingPartConverter.Format("garbage", "Name"), Is.EqualTo("garbage"));
    }
  }
}
