namespace VisualLog.Desktop.Settings
{
  /// <summary>
  /// A conditional formatting rule for log lines, stored in the application settings.
  /// If a line contains <see cref="Pattern"/> the line gets the rule's look.
  /// Several rules may match one line: for every style property the topmost rule that sets it wins,
  /// Bold/Italic/Strikethrough are combined.
  /// </summary>
  public class HighlightRule
  {
    public bool IsEnabled { get; set; } = true;

    /// <summary>Text to find in a line, or a regular expression if <see cref="IsRegex"/>.</summary>
    public string Pattern { get; set; } = string.Empty;

    public bool IsRegex { get; set; }

    public bool MatchCase { get; set; }

    /// <summary>Text color as "#RRGGBB" or "#AARRGGBB"; null keeps the default color.</summary>
    public string Foreground { get; set; }

    /// <summary>Row background as "#RRGGBB" or "#AARRGGBB"; null keeps the default background.</summary>
    public string Background { get; set; }

    public bool Bold { get; set; }

    public bool Italic { get; set; }

    public bool Strikethrough { get; set; }

    public HighlightRule Clone()
    {
      return (HighlightRule)this.MemberwiseClone();
    }
  }
}
