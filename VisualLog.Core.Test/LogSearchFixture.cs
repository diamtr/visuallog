using NUnit.Framework;
using System.Collections.Generic;
using System.IO;

namespace VisualLog.Core.Test
{
  [TestFixture]
  public class LogSearchFixture
  {
    private const string TestLogsDirName = @"test_logs";

    [Test]
    public void SearchStringInStaticLog()
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
  }
}
