using NUnit.Framework;
using System.Linq;
using System.Threading.Tasks;
using VisualLog.Core;
using VisualLog.Core.Search;
using VisualLog.Desktop.LogManager;

namespace VisualLog.Desktop.Tests
{
  [TestFixture]
  public class MessageDetailsFixture
  {
    [TestCase("2026-11-10 12:00:00 ERROR Something failed")]
    [TestCase("2026-11-10 ERROR {\"msg\":\"prefix before JSON\"}")]
    [TestCase("{\"a\":1} tail after the object")]
    [TestCase("{\"a\":1}{\"b\":2}")]
    [TestCase("[1, 2, 3]")]
    [TestCase("{\"a\":")]
    [TestCase("")]
    [TestCase(null)]
    public void NotJsonObjectLineIsText(string line)
    {
      var details = MessageDetailsViewModel.From(line, 7);

      Assert.That(details, Is.TypeOf<TextMessageDetailsViewModel>());
      Assert.That(details.LineNumber, Is.EqualTo(7));
      Assert.That(details.RawText, Is.EqualTo(line ?? string.Empty));
    }

    [TestCase("{\"a\":1}")]
    [TestCase("  {\"a\":1}  ")]
    [TestCase("{}")]
    public void JsonObjectLineIsJson(string line)
    {
      Assert.That(MessageDetailsViewModel.From(line, 1), Is.TypeOf<JsonMessageDetailsViewModel>());
    }

    [Test]
    public void FieldsOfJsonLine()
    {
      var line = "{\"t\":\"2026-11-10 12:00:00.123+03:00\",\"level\":\"Error\",\"n\":1.10,\"ok\":true,\"none\":null," +
                 "\"stack\":\"line 1\\nline 2\",\"ex\":{\"type\":\"IOException\",\"inner\":{}},\"tags\":[\"a\",2]}";

      var details = (JsonMessageDetailsViewModel)MessageDetailsViewModel.From(line, 1);
      var fields = details.Fields.ToDictionary(x => x.Name);

      Assert.That(details.Fields.Select(x => x.Name), Is.EqualTo(new[] { "t", "level", "n", "ok", "none", "stack", "ex", "tags" }));
      // Dates and numbers stay as written
      Assert.That(fields["t"].Kind, Is.EqualTo(JsonNodeKind.String));
      Assert.That(fields["t"].Value, Is.EqualTo("2026-11-10 12:00:00.123+03:00"));
      Assert.That(fields["n"].Kind, Is.EqualTo(JsonNodeKind.Number));
      Assert.That(fields["n"].Value, Is.EqualTo("1.10"));
      Assert.That(fields["ok"].Kind, Is.EqualTo(JsonNodeKind.Boolean));
      Assert.That(fields["ok"].Value, Is.EqualTo("true"));
      Assert.That(fields["none"].Kind, Is.EqualTo(JsonNodeKind.Null));
      Assert.That(fields["none"].Value, Is.EqualTo("null"));
      // A string is shown without quotes and escapes: a stack trace is multi-line
      Assert.That(fields["stack"].Value, Is.EqualTo("line 1\nline 2"));
      Assert.That(fields["stack"].Children, Is.Empty);

      var ex = fields["ex"];
      Assert.That(ex.Kind, Is.EqualTo(JsonNodeKind.Object));
      Assert.That(ex.Value, Is.EqualTo("2 fields"));
      Assert.That(ex.IsExpanded, Is.False);
      Assert.That(ex.Children.Select(x => x.Name), Is.EqualTo(new[] { "type", "inner" }));
      Assert.That(ex.Children[1].Value, Is.EqualTo("{ }"));

      var tags = fields["tags"];
      Assert.That(tags.Kind, Is.EqualTo(JsonNodeKind.Array));
      Assert.That(tags.Value, Is.EqualTo("2 items"));
      Assert.That(tags.Children.Select(x => x.Name), Is.EqualTo(new[] { "[0]", "[1]" }));
      Assert.That(tags.Children.Select(x => x.Kind), Is.EqualTo(new[] { JsonNodeKind.String, JsonNodeKind.Number }));
    }

    [Test]
    public void CopyLineAsShown()
    {
      var line = "{\"a\":1,\"b\":{\"c\":\"x\"}}";
      var details = (JsonMessageDetailsViewModel)MessageDetailsViewModel.From(line, 1);
      string clipboard = null;
      details.SetClipboardText = text => clipboard = text;

      Assert.That(details.IsTreeView, Is.True);
      details.CopyCommand.Execute(null);
      Assert.That(clipboard, Is.EqualTo(line));

      details.IsTextView = true;
      Assert.That(details.IsTreeView, Is.False);
      details.CopyCommand.Execute(null);
      Assert.That(clipboard, Is.EqualTo(details.IndentedText));
      Assert.That(clipboard, Does.Contain("\n"));
    }

    [Test]
    public void CopyNode()
    {
      var details = (JsonMessageDetailsViewModel)MessageDetailsViewModel.From("{\"msg\":\"a\\tb\",\"b\":{\"c\":1}}", 1);
      string clipboard = null;
      details.SetClipboardText = text => clipboard = text;

      details.Fields[0].CopyValueCommand.Execute(null);
      Assert.That(clipboard, Is.EqualTo("a\tb"));
      details.Fields[0].CopyNameCommand.Execute(null);
      Assert.That(clipboard, Is.EqualTo("msg"));
      details.Fields[1].CopyValueCommand.Execute(null);
      Assert.That(clipboard, Does.Contain("\"c\": 1"));
    }

    [Test]
    public void ShowAndHideDetailsOfLogLine()
    {
      var vm = new LogViewModel();
      vm.OnNewLogMessageCatched(new Message("plain text line"));
      vm.OnNewLogMessageCatched(new Message("{\"a\":1}"));
      vm.FlushPendingMessages();

      vm.ShowDetails(0);
      vm.ShowDetails(3);
      Assert.That(vm.Details, Is.Null);

      vm.ShowDetails(1);
      Assert.That(vm.Details, Is.TypeOf<TextMessageDetailsViewModel>());
      Assert.That(vm.Details.RawText, Is.EqualTo("plain text line"));

      vm.ShowDetails(2);
      Assert.That(vm.Details, Is.TypeOf<JsonMessageDetailsViewModel>());
      Assert.That(vm.Details.LineNumber, Is.EqualTo(2));

      vm.Details.CloseCommand.Execute(null);
      Assert.That(vm.Details, Is.Null);
    }

    private static LogViewModel CreateLog(params string[] lines)
    {
      var vm = new LogViewModel();
      foreach (var line in lines)
        vm.OnNewLogMessageCatched(new Message(line));
      vm.FlushPendingMessages();
      return vm;
    }

    [Test]
    public void NavigateByAllLines()
    {
      var vm = CreateLog("one", "two", "three");
      SearchEntry shown = null;
      vm.ShowLineRequested += x => shown = x;

      vm.ShowDetails(1);
      Assert.That(vm.Details.PreviousCommand.CanExecute(null), Is.False);
      Assert.That(vm.Details.NextCommand.CanExecute(null), Is.True);

      vm.Details.NextCommand.Execute(null);
      Assert.That(vm.Details.LineNumber, Is.EqualTo(2));
      Assert.That(vm.Details.RawText, Is.EqualTo("two"));
      // The log goes to the line too
      Assert.That(shown.LineNumber, Is.EqualTo(2));
      Assert.That(vm.State.FollowTail, Is.False);

      vm.Details.NextCommand.Execute(null);
      Assert.That(vm.Details.LineNumber, Is.EqualTo(3));
      Assert.That(vm.Details.NextCommand.CanExecute(null), Is.False);

      vm.Details.PreviousCommand.Execute(null);
      Assert.That(vm.Details.LineNumber, Is.EqualTo(2));
    }

    [Test]
    public async Task NavigateBySearchResults()
    {
      var vm = CreateLog("error 1", "info", "error 2", "info", "info", "error 3");
      await vm.SearchViewModel.SearchForTextAsync("error");
      Assert.That(vm.SearchViewModel.SearchEntries.Count, Is.EqualTo(3));

      // Opened on a line that is not a result: goes to the nearest results around it
      vm.ShowDetails(4);
      Assert.That(vm.FindLine(4, -1), Is.EqualTo(3));
      Assert.That(vm.FindLine(4, 1), Is.EqualTo(6));

      vm.Details.PreviousCommand.Execute(null);
      Assert.That(vm.Details.LineNumber, Is.EqualTo(3));
      Assert.That(vm.SearchViewModel.CurrentLineNumber, Is.EqualTo(3));

      vm.Details.PreviousCommand.Execute(null);
      Assert.That(vm.Details.LineNumber, Is.EqualTo(1));
      Assert.That(vm.Details.PreviousCommand.CanExecute(null), Is.False);

      vm.Details.NextCommand.Execute(null);
      vm.Details.NextCommand.Execute(null);
      Assert.That(vm.Details.LineNumber, Is.EqualTo(6));
      Assert.That(vm.SearchViewModel.CurrentLineNumber, Is.EqualTo(6));
      Assert.That(vm.Details.NextCommand.CanExecute(null), Is.False);

      // No results: by all the lines again
      vm.SearchViewModel.ClearCommand.Execute(null);
      Assert.That(vm.SearchViewModel.SearchEntries, Is.Empty);
      Assert.That(vm.Details.PreviousCommand.CanExecute(null), Is.True);
      vm.Details.PreviousCommand.Execute(null);
      Assert.That(vm.Details.LineNumber, Is.EqualTo(5));
    }

    [Test]
    public void JsonLookIsKeptFromLineToLine()
    {
      var vm = new LogViewModel();
      vm.OnNewLogMessageCatched(new Message("{\"a\":1}"));
      vm.OnNewLogMessageCatched(new Message("plain text line"));
      vm.OnNewLogMessageCatched(new Message("{\"b\":2}"));
      vm.FlushPendingMessages();

      vm.ShowDetails(1);
      ((JsonMessageDetailsViewModel)vm.Details).IsTextView = true;
      vm.ShowDetails(2);
      vm.ShowDetails(3);

      Assert.That(((JsonMessageDetailsViewModel)vm.Details).IsTextView, Is.True);
    }
  }
}
