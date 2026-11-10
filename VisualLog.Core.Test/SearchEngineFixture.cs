using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using VisualLog.Core.Search;

namespace VisualLog.Core.Test
{
  [TestFixture]
  public class SearchEngineFixture
  {
    private const string TestLogsDirName = @"test_logs";

    private static SearchRequest Request(params ISearchRequestStatement[] statements)
    {
      var request = new SearchRequest();
      request.Statements.AddRange(statements);
      return request;
    }

    [Test]
    public void MatchCaseIsTrueByDefault()
    {
      Assert.That(new TextStatement().MatchCase, Is.True);
    }

    [Test]
    public void MatchCaseTrueFindsOnlyExactCase()
    {
      var statement = new TextStatement() { Text = "test", MatchCase = true };
      var matches = statement.GetMatches(new Message("test Test TEST")).ToList();

      Assert.That(matches.Select(x => x.Index), Is.EqualTo(new[] { 0 }));
    }

    [Test]
    public void MatchCaseFalseIgnoresCase()
    {
      var statement = new TextStatement() { Text = "test", MatchCase = false };
      var matches = statement.GetMatches(new Message("test Test TEST")).ToList();

      Assert.That(matches.Select(x => x.Index), Is.EqualTo(new[] { 0, 5, 10 }));
    }

    [Test]
    public void MatchCaseChangeRecompilesRegex()
    {
      var statement = new TextStatement() { Text = "test", MatchCase = false };
      Assert.That(statement.GetMatches(new Message("TEST")).Count(), Is.EqualTo(1));

      statement.MatchCase = true;
      Assert.That(statement.GetMatches(new Message("TEST")).Count(), Is.EqualTo(0));
    }

    [Test]
    public void MatchCaseFalseWithLiteralText()
    {
      var statement = new TextStatement() { Text = "a.b", UseRegex = false, MatchCase = false };
      var matches = statement.GetMatches(new Message("A.B axb")).ToList();

      Assert.That(matches.Select(x => x.Index), Is.EqualTo(new[] { 0 }));
    }

    [Test]
    public void RegexOptionsSetterKeepsWorkingWithMatchCase()
    {
      var statement = new TextStatement() { Text = "test", RegexOptions = RegexOptions.IgnoreCase };

      Assert.That(statement.RegexOptions.HasFlag(RegexOptions.IgnoreCase), Is.True);
      Assert.That(statement.GetMatches(new Message("TEST")).Count(), Is.EqualTo(1));
    }

    [Test]
    public void RegexErrorForInvalidPattern()
    {
      var statement = new TextStatement() { Text = "a(b" };

      Assert.That(statement.RegexError, Is.Not.Null.And.Not.Empty);
    }

    [Test]
    public void InvalidPatternFallsBackToLiteralSearch()
    {
      var statement = new TextStatement() { Text = "a(b" };
      var matches = statement.GetMatches(new Message("xx a(b ab")).ToList();

      Assert.That(matches.Select(x => x.Index), Is.EqualTo(new[] { 3 }));
    }

    [Test]
    public void RegexErrorIsNullForValidPattern()
    {
      var statement = new TextStatement() { Text = @"a\d+b" };

      Assert.That(statement.RegexError, Is.Null);
    }

    [Test]
    public void RegexErrorIsNullWhenUseRegexIsFalse()
    {
      var statement = new TextStatement() { Text = "a(b", UseRegex = false };

      Assert.That(statement.RegexError, Is.Null);
    }

    [Test]
    public void RegexErrorIsResetWhenPatternBecomesValid()
    {
      var statement = new TextStatement() { Text = "a(b" };
      Assert.That(statement.RegexError, Is.Not.Null);

      statement.Text = "a(b)";
      Assert.That(statement.RegexError, Is.Null);

      statement.Text = "[";
      Assert.That(statement.RegexError, Is.Not.Null);

      statement.Text = string.Empty;
      Assert.That(statement.RegexError, Is.Null);
    }

    [Test]
    public void SearchMessagesNumbersLinesByIndexAndSkipsNulls()
    {
      var messages = new List<Message>()
      {
        new Message("foo"),
        null,
        new Message("bar"),
        new Message("foo bar"),
        null,
      };

      var response = SearchEngine.Search(messages, Request(new TextStatement() { Text = "bar" }));

      Assert.That(response.LogPath, Is.Null);
      Assert.That(response.Entries.Select(x => x.LineNumber), Is.EqualTo(new[] { 3, 4 }));
      Assert.That(response.Entries[0].Message, Is.SameAs(messages[2]));
      Assert.That(response.Entries[1].Matches, Is.EqualTo(new[] { new MatchEntry() { Index = 4, Length = 3 } }));
    }

    [Test]
    public void SearchMessagesWithPreCancelledTokenThrows()
    {
      var messages = new List<Message>() { new Message("foo") };
      using var cts = new CancellationTokenSource();
      cts.Cancel();

      Assert.That(() => SearchEngine.Search(messages, Request(new TextStatement() { Text = "foo" }), cts.Token),
        Throws.InstanceOf<OperationCanceledException>());
    }

    [Test]
    public void SearchMessagesWithNullArgumentsReturnsEmptyResponse()
    {
      Assert.That(SearchEngine.Search((IReadOnlyList<Message>)null, new SearchRequest()).Entries, Is.Empty);
      Assert.That(SearchEngine.Search(new List<Message>(), null).Entries, Is.Empty);
    }

    [Test]
    public void SearchLogDelegatesAndSetsLogPath()
    {
      var path = Path.Combine(TestLogsDirName, "searchstringstatic.log");
      var log = new Log(path);
      log.Read();

      var request = Request(new TextStatement() { Text = "test" });
      var fromLog = SearchEngine.Search(log, request);
      var fromList = SearchEngine.Search(log.Messages, request);

      Assert.That(fromLog.LogPath, Is.EqualTo(log.SourceFilePath));
      Assert.That(fromLog.Entries, Is.EqualTo(fromList.Entries));
      Assert.That(fromLog.Entries.Select(x => x.LineNumber), Is.EqualTo(new[] { 2, 3, 5 }));
    }

    [Test]
    public void MatchesFromMultipleStatementsAreSortedByIndex()
    {
      var request = Request(
        new TextStatement() { Text = "ccc" },
        new TextStatement() { Text = "aaa" },
        new TextStatement() { Text = "bbb" });

      var entry = SearchEngine.Search(new Message("aaa bbb ccc aaa"), request);

      Assert.That(entry, Is.Not.Null);
      Assert.That(entry.Matches.Select(x => x.Index), Is.EqualTo(new[] { 0, 4, 8, 12 }));
    }

    [Test]
    public void OverlappingMatchesAreKeptAndSorted()
    {
      var request = Request(
        new TextStatement() { Text = "bc" },
        new TextStatement() { Text = "abc" });

      var entry = SearchEngine.Search(new Message("abcd"), request);

      Assert.That(entry.Matches, Is.EqualTo(new[]
      {
        new MatchEntry() { Index = 0, Length = 3 },
        new MatchEntry() { Index = 1, Length = 2 },
      }));
    }
  }
}
