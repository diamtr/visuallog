using NUnit.Framework;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using VisualLog.Core;
using VisualLog.Core.Search;
using VisualLog.Desktop.LogManager;

namespace VisualLog.Desktop.Tests
{
  [TestFixture]
  public class SearchViewModelFixture
  {
    private static readonly string[] Lines =
    {
      "INFO  started",                    // 1
      "ERROR connect timeout",            // 2
      "INFO  working",                    // 3
      "error read Timeout, timeout again" // 4
    };

    private static LogViewModel CreateLog(params string[] lines)
    {
      var log = new LogViewModel();
      foreach (var line in lines)
        log.OnNewLogMessageCatched(new Message(line));
      log.FlushPendingMessages();
      return log;
    }

    private static void AddLine(LogViewModel log, string line)
    {
      log.OnNewLogMessageCatched(new Message(line));
      log.FlushPendingMessages();
    }

    [SetUp]
    public void SetUp()
    {
      SearchViewModel.SharedHistory.Clear();
    }

    [Test]
    public void NewSearchViewModel()
    {
      var vm = new SearchViewModel();
      Assert.That(vm.StringToSearch, Is.Null);
      Assert.That(vm.UseRegex, Is.True);
      Assert.That(vm.MatchCase, Is.False);
      Assert.That(vm.IsLive, Is.True);
      Assert.That(vm.Status, Is.EqualTo(SearchStatus.Idle));
      Assert.That(vm.PositionText, Is.Empty);
    }

    [Test]
    public async Task SearchIgnoresCaseByDefault()
    {
      var log = CreateLog(Lines);
      var vm = log.SearchViewModel;
      vm.StringToSearch = "timeout";
      await vm.SearchAsync();

      Assert.That(vm.Status, Is.EqualTo(SearchStatus.Found));
      Assert.That(vm.SearchEntries.Select(x => x.SearchEntry.LineNumber), Is.EqualTo(new[] { 2, 4 }));
      Assert.That(vm.MatchCount, Is.EqualTo(3));
      Assert.That(vm.SummaryText, Does.StartWith("3 matches in 2 lines"));
      Assert.That(vm.PositionText, Is.EqualTo("– / 2"));
      Assert.That(log.State.ShowSearchPanel, Is.True);
      Assert.That(vm.MatchLineNumbers, Is.EqualTo(new[] { 2, 4 }));
    }

    [Test]
    public async Task MatchCase()
    {
      var log = CreateLog(Lines);
      var vm = log.SearchViewModel;
      vm.StringToSearch = "Timeout";
      vm.MatchCase = true;
      await vm.SearchAsync();

      Assert.That(vm.SearchEntries.Select(x => x.SearchEntry.LineNumber), Is.EqualTo(new[] { 4 }));
      Assert.That(vm.MatchCount, Is.EqualTo(1));
      Assert.That(vm.SummaryText, Does.StartWith("1 match in 1 line"));
    }

    [Test]
    public async Task PlainTextIsNotRegex()
    {
      var log = CreateLog("a.c", "abc");
      var vm = log.SearchViewModel;
      vm.UseRegex = false;
      vm.StringToSearch = "a.c";
      await vm.SearchAsync();
      Assert.That(vm.SearchEntries.Select(x => x.SearchEntry.LineNumber), Is.EqualTo(new[] { 1 }));
    }

    [Test]
    public async Task InvalidRegex()
    {
      var log = CreateLog(Lines);
      var vm = log.SearchViewModel;
      vm.StringToSearch = "time(out";
      await vm.SearchAsync();

      Assert.That(vm.Status, Is.EqualTo(SearchStatus.InvalidRegex));
      Assert.That(vm.HasRegexError, Is.True);
      Assert.That(vm.RegexError, Is.Not.Empty);
      Assert.That(vm.SearchEntries, Is.Empty);

      vm.StringToSearch = "time(out)";
      await vm.SearchAsync();
      Assert.That(vm.Status, Is.EqualTo(SearchStatus.Found));
      Assert.That(vm.RegexError, Is.Null);
    }

    [Test]
    public async Task NoMatches()
    {
      var log = CreateLog(Lines);
      var vm = log.SearchViewModel;
      vm.StringToSearch = "missing";
      await vm.SearchAsync();
      Assert.That(vm.Status, Is.EqualTo(SearchStatus.NoMatches));
      Assert.That(vm.PositionText, Is.EqualTo("No results"));
      Assert.That(vm.HasResults, Is.False);
    }

    [Test]
    public async Task EmptyQueryResets()
    {
      var log = CreateLog(Lines);
      var vm = log.SearchViewModel;
      vm.StringToSearch = "timeout";
      await vm.SearchAsync();
      vm.StringToSearch = string.Empty;
      await vm.SearchAsync();
      Assert.That(vm.Status, Is.EqualTo(SearchStatus.Idle));
      Assert.That(vm.SearchEntries, Is.Empty);
    }

    [Test]
    public async Task LiveSearchesNewLines()
    {
      var log = CreateLog(Lines);
      var vm = log.SearchViewModel;
      vm.StringToSearch = "timeout";
      await vm.SearchAsync();

      AddLine(log, "WARN slow");
      AddLine(log, "ERROR write timeout");

      Assert.That(vm.SearchEntries.Select(x => x.SearchEntry.LineNumber), Is.EqualTo(new[] { 2, 4, 6 }));
      Assert.That(vm.MatchCount, Is.EqualTo(4));
    }

    [Test]
    public async Task NotLiveKeepsResults()
    {
      var log = CreateLog(Lines);
      var vm = log.SearchViewModel;
      vm.IsLive = false;
      vm.StringToSearch = "timeout";
      await vm.SearchAsync();

      AddLine(log, "ERROR write timeout");
      Assert.That(vm.SearchEntries.Count, Is.EqualTo(2));

      // Turning live on catches up
      vm.IsLive = true;
      Assert.That(vm.SearchEntries.Select(x => x.SearchEntry.LineNumber), Is.EqualTo(new[] { 2, 4, 5 }));
    }

    [Test]
    public async Task RereadLogFindsAgain()
    {
      var log = CreateLog(Lines);
      var vm = log.SearchViewModel;
      vm.StringToSearch = "timeout";
      await vm.SearchAsync();

      log.LogMessages.Clear();
      Assert.That(vm.SearchEntries, Is.Empty);
      AddLine(log, "timeout first");
      Assert.That(vm.SearchEntries.Select(x => x.SearchEntry.LineNumber), Is.EqualTo(new[] { 1 }));
      Assert.That(vm.Status, Is.EqualTo(SearchStatus.Found));
    }

    [Test]
    public async Task NavigationWrapsAndShowsLine()
    {
      var log = CreateLog(Lines);
      var vm = log.SearchViewModel;
      vm.StringToSearch = "timeout";
      await vm.SearchAsync();

      vm.GoToNext();
      Assert.That(vm.PositionText, Is.EqualTo("1 / 2"));
      Assert.That(vm.CurrentLineNumber, Is.EqualTo(2));
      Assert.That(log.ShownLine.Message.RawValue, Is.EqualTo(Lines[1]));

      vm.GoToNext();
      Assert.That(vm.PositionText, Is.EqualTo("2 / 2"));
      Assert.That(log.ShownLine.Message.RawValue, Is.EqualTo(Lines[3]));

      vm.GoToNext();
      Assert.That(vm.PositionText, Is.EqualTo("1 / 2"));

      vm.GoToPrevious();
      Assert.That(vm.PositionText, Is.EqualTo("2 / 2"));
      Assert.That(SearchViewModel.SharedHistory, Is.EqualTo(new[] { "timeout" }));
    }

    [Test]
    public async Task ShowLineRequestCarriesTheFoundEntry()
    {
      // Regression (with the list): the line after the found one was shown; the last line threw
      var log = CreateLog(Lines);
      var vm = log.SearchViewModel;
      SearchEntry shown = null;
      log.ShowLineRequested += entry => shown = entry;
      vm.StringToSearch = "again";
      await vm.SearchAsync();

      vm.GoToNext();
      Assert.That(shown.LineNumber, Is.EqualTo(4));
      Assert.That(shown.Matches, Is.Not.Empty);
      Assert.That(log.ShownLine.Message.RawValue, Is.EqualTo(Lines[3]));
      Assert.That(log.State.FollowTail, Is.False);
    }

    [TestCase("timeout", true)]
    [TestCase("a b", true)]
    [TestCase("", false)]
    [TestCase(null, false)]
    [TestCase("one\ntwo", false)]
    [TestCase("one\r\n", false)]
    public void SearchableText(string text, bool expected)
    {
      Assert.That(SearchViewModel.IsSearchableText(text), Is.EqualTo(expected));
    }

    [Test]
    public void EscapeRegexKeepsSpacesReadable()
    {
      Assert.That(SearchViewModel.EscapeRegex("a.b (c) [d] # e"), Is.EqualTo(@"a\.b \(c\) \[d\] # e"));
    }

    [Test]
    public async Task SearchForSelectedTextFindsItLiterally()
    {
      var log = CreateLog(new[] { "value (1) ok", "value 1 ok", "other" });
      var vm = log.SearchViewModel;
      Assert.That(vm.UseRegex, Is.True);

      await vm.SearchForTextAsync("(1)");
      Assert.That(vm.StringToSearch, Is.EqualTo(@"\(1\)"));
      Assert.That(vm.SearchEntries.Select(x => x.SearchEntry.LineNumber), Is.EqualTo(new[] { 1 }));
      Assert.That(log.State.ShowSearchPanel, Is.True);
      Assert.That(SearchViewModel.SharedHistory.First(), Is.EqualTo(@"\(1\)"));

      vm.UseRegex = false;
      await vm.SearchForTextAsync("(1)");
      Assert.That(vm.StringToSearch, Is.EqualTo("(1)"));
      Assert.That(vm.SearchEntries.Select(x => x.SearchEntry.LineNumber), Is.EqualTo(new[] { 1 }));
    }

    [Test]
    public async Task MultilineSelectionIsNotSearched()
    {
      var log = CreateLog(Lines);
      var vm = log.SearchViewModel;
      await vm.SearchForTextAsync("timeout\nagain");
      Assert.That(vm.HasQuery, Is.False);
      Assert.That(log.SearchTextCommand.CanExecute("timeout\nagain"), Is.False);
      Assert.That(log.SearchTextCommand.CanExecute("timeout"), Is.True);
    }

    [Test]
    public async Task CopyAllAndSelected()
    {
      var log = CreateLog(Lines);
      var vm = log.SearchViewModel;
      string clipboard = null;
      vm.SetClipboardText = text => clipboard = text;
      vm.StringToSearch = "timeout";
      await vm.SearchAsync();

      vm.CopyAllCommand.Execute(null);
      Assert.That(clipboard, Is.EqualTo(Lines[1] + "\r\n" + Lines[3] + "\r\n").Or.EqualTo(Lines[1] + "\n" + Lines[3] + "\n"));

      Assert.That(vm.CopySelectedCommand.CanExecute(null), Is.False);
      vm.SelectedEntries = new[] { vm.SearchEntries[1] };
      vm.CopySelectedWithLineNumbersCommand.Execute(null);
      Assert.That(clipboard.TrimEnd(), Is.EqualTo("4\t" + Lines[3]));
    }

    [Test]
    public async Task Export()
    {
      var log = CreateLog(Lines);
      var vm = log.SearchViewModel;
      var path = Path.Combine(Path.GetTempPath(), $"visuallog-export-{System.Guid.NewGuid():N}.log");
      vm.AskExportPath = name => path;
      vm.StringToSearch = "timeout";
      await vm.SearchAsync();
      try
      {
        Assert.That(vm.Export(), Is.True);
        Assert.That(File.ReadAllLines(path), Is.EqualTo(new[] { Lines[1], Lines[3] }));
      }
      finally
      {
        File.Delete(path);
      }

      vm.AskExportPath = name => null;
      Assert.That(vm.Export(), Is.False);
    }

    [Test]
    public void HistoryKeepsRecentUniqueQueries()
    {
      var vm = new SearchViewModel();
      for (var i = 0; i < SearchViewModel.HistoryLimit + 3; i++)
        vm.AddToHistory($"q{i}");
      vm.AddToHistory("q5");
      vm.AddToHistory(" ");

      Assert.That(SearchViewModel.SharedHistory.Count, Is.EqualTo(SearchViewModel.HistoryLimit));
      Assert.That(SearchViewModel.SharedHistory[0], Is.EqualTo("q5"));
      Assert.That(SearchViewModel.SharedHistory.Count(x => x == "q5"), Is.EqualTo(1));
      Assert.That(SearchViewModel.SharedHistory, Does.Not.Contain("q0"));
    }

    [Test]
    public async Task ClearHidesPanel()
    {
      var log = CreateLog(Lines);
      var vm = log.SearchViewModel;
      vm.StringToSearch = "timeout";
      await vm.SearchAsync();
      vm.ClearCommand.Execute(null);
      Assert.That(vm.StringToSearch, Is.Empty);
      Assert.That(vm.SearchEntries, Is.Empty);
      Assert.That(log.State.ShowSearchPanel, Is.False);
    }
  }
}
