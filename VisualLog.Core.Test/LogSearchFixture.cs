using NUnit.Framework;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace VisualLog.Core.Test
{
  [TestFixture]
  public class LogSearchFixture
  {
    private const string TestLogsDirName = @"test_logs";

    [Test]
    public void SearchStringInStaticLogCaseSensitive()
    {
      var path = Path.Combine(TestLogsDirName, "searchstringstatic.log");
      var log = new Log(path);
      log.Read();

      var results = log.SearchString("test");
      Assert.IsNotNull(results);
      Assert.IsNotNull(results.Entries);

      var expectedEntries = new List<SearchEntry>()
      {
        new SearchEntry() { LineNumber = 2, StartPosition = 8 },
        new SearchEntry() { LineNumber = 3, StartPosition = 6 },
        new SearchEntry() { LineNumber = 5, StartPosition = 0 },
        new SearchEntry() { LineNumber = 5, StartPosition = 10 }
      };
      
      CollectionAssert.AreEqual(expectedEntries, results.Entries);
    }

    [Test]
    public void SearchStringInStaticLogCaseInsensitive()
    {
      var path = Path.Combine(TestLogsDirName, "searchstringstatic.log");
      var log = new Log(path);
      log.Read();

      var results = log.SearchString("test", RegexOptions.IgnoreCase);
      Assert.IsNotNull(results);
      Assert.IsNotNull(results.Entries);

      var expectedEntries = new List<SearchEntry>()
      {
        new SearchEntry() { LineNumber = 2, StartPosition = 8 },
        new SearchEntry() { LineNumber = 3, StartPosition = 6 },
        new SearchEntry() { LineNumber = 3, StartPosition = 17 },
        new SearchEntry() { LineNumber = 5, StartPosition = 0 },
        new SearchEntry() { LineNumber = 5, StartPosition = 5 },
        new SearchEntry() { LineNumber = 5, StartPosition = 10 }
      };

      CollectionAssert.AreEqual(expectedEntries, results.Entries);
    }
  }
}
