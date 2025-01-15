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

      var request = new SearchRequest();
      request.Statements.Add(new TextStatement() { Text = "test" });
      var results = SearchEngine.Search(log, request);
      Assert.That(results, Is.Not.Null);
      Assert.That(results.Entries, Is.Not.Null);

      var expectedEntries = new List<SearchEntry>()
      {
        new SearchEntry() { 
          LineNumber = 2,
          RawString = "line2 - test",
          Matches = new List<MatchEntry>() {
            new MatchEntry() { Index = 8, Length = 4 }
          } 
        },
        new SearchEntry() { 
          LineNumber = 3,
          RawString = "line3 test line3 Test",
          Matches = new List<MatchEntry>() {
            new MatchEntry() { Index = 6, Length = 4 }
          } 
        },
        new SearchEntry() {
          LineNumber = 5,
          RawString = "test Test test",
          Matches = new List<MatchEntry>() {
            new MatchEntry() { Index = 0, Length = 4 },
            new MatchEntry() { Index = 10, Length = 4 }
          }
        },
      };

      Assert.That(results.Entries, Is.EqualTo(expectedEntries));
    }

    [Test]
    public void SearchStringInStaticLogCaseInsensitive()
    {
      var path = Path.Combine(TestLogsDirName, "searchstringstatic.log");
      var log = new Log(path);
      log.Read();

      var request = new SearchRequest();
      request.Statements.Add(new TextStatement() { Text = "test", RegexOptions = RegexOptions.IgnoreCase });
      var results = SearchEngine.Search(log, request);

      Assert.That(results, Is.Not.Null);
      Assert.That(results.Entries, Is.Not.Null);

      var expectedEntries = new List<SearchEntry>()
      {
        new SearchEntry() {
          LineNumber = 2,
          RawString = "line2 - test",
          Matches = new List<MatchEntry>() {
            new MatchEntry() { Index = 8, Length = 4 } 
          }
        },
        new SearchEntry() {
          LineNumber = 3,
          RawString = "line3 test line3 Test",
          Matches = new List<MatchEntry>() {
            new MatchEntry() { Index = 6, Length = 4 },
            new MatchEntry() { Index = 17, Length = 4 }
          }
        },
        new SearchEntry() {
          LineNumber = 5,
          RawString = "test Test test",
          Matches = new List<MatchEntry>() {
            new MatchEntry() { Index = 0, Length = 4 },
            new MatchEntry() { Index = 5, Length = 4 },
            new MatchEntry() { Index = 10, Length = 4 }
          }
        },
      };

      Assert.That(results.Entries, Is.EqualTo(expectedEntries));
    }
  }
}
