using NUnit.Framework;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using VisualLog.Core.Search;

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

      var expectedEntries = new List<Entry>()
      {
        new Entry() { 
          LineNumber = 2,
          RawString = "line2 - test",
          Matches = new List<Match>() {
            new Match() { Index = 8, Length = 4 }
          } 
        },
        new Entry() { 
          LineNumber = 3,
          RawString = "line3 test line3 Test",
          Matches = new List<Match>() {
            new Match() { Index = 6, Length = 4 }
          } 
        },
        new Entry() {
          LineNumber = 5,
          RawString = "test Test test",
          Matches = new List<Match>() {
            new Match() { Index = 0, Length = 4 },
            new Match() { Index = 10, Length = 4 }
          }
        },
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

      var expectedEntries = new List<Entry>()
      {
        new Entry() {
          LineNumber = 2,
          RawString = "line2 - test",
          Matches = new List<Match>() {
            new Match() { Index = 8, Length = 4 } 
          }
        },
        new Entry() {
          LineNumber = 3,
          RawString = "line3 test line3 Test",
          Matches = new List<Match>() {
            new Match() { Index = 6, Length = 4 },
            new Match() { Index = 17, Length = 4 }
          }
        },
        new Entry() {
          LineNumber = 5,
          RawString = "test Test test",
          Matches = new List<Match>() {
            new Match() { Index = 0, Length = 4 },
            new Match() { Index = 5, Length = 4 },
            new Match() { Index = 10, Length = 4 }
          }
        },
      };

      CollectionAssert.AreEqual(expectedEntries, results.Entries);
    }
  }
}
