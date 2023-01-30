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
        new SearchEntry() { Line = 2, StartPosition = 9, EndPosition = 12 },
        new SearchEntry() { Line = 3, StartPosition = 6, EndPosition = 9 },
        new SearchEntry() { Line = 5, StartPosition = 0, EndPosition = 3 }
      };
      
      //CollectionAssert.AreEqual(expectedEntries, results.Entries);
    }
  }
}
