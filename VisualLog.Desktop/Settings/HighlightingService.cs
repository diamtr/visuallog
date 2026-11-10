using System;
using System.Collections.Generic;

namespace VisualLog.Desktop.Settings
{
  /// <summary>
  /// Application-wide holder of the current line highlighting.
  /// The settings page calls <see cref="Update"/>, opened logs listen to <see cref="Changed"/>.
  /// </summary>
  public class HighlightingService
  {
    public LineHighlighter Current { get; private set; } = LineHighlighter.Empty;

    /// <summary>Raised on the caller's thread after <see cref="Current"/> was replaced.</summary>
    public event Action Changed;

    public void Update(IEnumerable<HighlightRule> rules)
    {
      this.Current = new LineHighlighter(rules);
      this.Changed?.Invoke();
    }
  }
}
