using System;
using System.Windows;

namespace VisualLog.Desktop.LogManager
{
  /// <summary>Moves the details from line to line of the log.</summary>
  public interface ILineNavigator
  {
    /// <summary>The line to go to from a line: the next one (direction &gt; 0) or the previous one; 0 - none.</summary>
    int FindLine(int fromLineNumber, int direction);
    /// <summary>Shows the details of the line.</summary>
    void GoToLine(int lineNumber);
  }

  /// <summary>
  /// Detailed look at one line of the log, shown in the panel beside the log.
  /// The kind of the line picks the subclass, and the subclass picks the look (a DataTemplate by type).
  /// A snapshot: it does not change when the log is scrolled or appended.
  /// </summary>
  public abstract class MessageDetailsViewModel : ViewModelBase
  {
    /// <summary>1-based number of the line in the log.</summary>
    public int LineNumber { get; }
    /// <summary>The line as it is in the log.</summary>
    public string RawText { get; }

    /// <summary>Copies the line as it is shown now.</summary>
    public Command CopyCommand { get; }
    public Command CloseCommand { get; }
    public Command PreviousCommand { get; }
    public Command NextCommand { get; }

    public event Action CloseRequested;

    public Action<string> SetClipboardText { get; set; } = text => Clipboard.SetText(text);

    /// <summary>Where the previous and next lines are; null - nowhere to go.</summary>
    public ILineNavigator Navigator { get; set; }

    protected MessageDetailsViewModel(string rawText, int lineNumber)
    {
      this.RawText = rawText ?? string.Empty;
      this.LineNumber = lineNumber;
      this.CopyCommand = new Command(x => this.SetClipboardText(this.CopyText));
      this.CloseCommand = new Command(x => this.CloseRequested?.Invoke());
      this.PreviousCommand = new Command(x => this.GoTo(-1), x => this.FindLine(-1) > 0);
      this.NextCommand = new Command(x => this.GoTo(1), x => this.FindLine(1) > 0);
    }

    // The lines to go to are looked up every time: the search results may change while the panel is open
    private int FindLine(int direction)
    {
      return this.Navigator?.FindLine(this.LineNumber, direction) ?? 0;
    }

    private void GoTo(int direction)
    {
      var lineNumber = this.FindLine(direction);
      if (lineNumber > 0)
        this.Navigator.GoToLine(lineNumber);
    }

    /// <summary>Text copied by <see cref="CopyCommand"/>.</summary>
    protected virtual string CopyText => this.RawText;

    /// <summary>Details of a line: a JSON object line is shown as its fields, any other line as text.</summary>
    public static MessageDetailsViewModel From(string rawText, int lineNumber)
    {
      var json = JsonMessageDetailsViewModel.TryParse(rawText);
      return json != null
        ? new JsonMessageDetailsViewModel(rawText, lineNumber, json)
        : new TextMessageDetailsViewModel(rawText, lineNumber);
    }
  }

  /// <summary>An unstructured line: shown as text wrapped by words.</summary>
  public class TextMessageDetailsViewModel : MessageDetailsViewModel
  {
    public TextMessageDetailsViewModel(string rawText, int lineNumber) : base(rawText, lineNumber) { }
  }
}
