using System;
using VisualLog.Core;
using VisualLog.Desktop.Settings;

namespace VisualLog.Desktop.LogManager
{
  /// <summary>A line of the log: the message and its look by the highlighting rules.</summary>
  public class MessageInlineViewModel : MessageViewModelBase
  {
    /// <summary>Gives the current highlighting rules; null - the line is never highlighted.</summary>
    public Func<LineHighlighter> HighlighterSource { get; set; }

    /// <summary>
    /// Look of the line by the highlighting rules, null if no rule matches.
    /// Computed lazily: with list virtualization only the lines on screen are ever matched.
    /// </summary>
    public LineStyle Highlight
    {
      get
      {
        if (!this.isHighlightComputed)
        {
          this.highlight = this.HighlighterSource?.Invoke()?.Match(this.Message?.RawValue);
          this.isHighlightComputed = true;
        }
        return this.highlight;
      }
    }
    private LineStyle highlight;
    private bool isHighlightComputed;

    /// <summary>Forgets the computed look; it is recomputed when the line is shown next time.</summary>
    public void RefreshHighlight()
    {
      if (!this.isHighlightComputed)
        return;
      this.isHighlightComputed = false;
      this.highlight = null;
      this.OnPropertyChanged(nameof(this.Highlight));
    }

    public MessageInlineViewModel(IMessage message) : base(message) { }
    public MessageInlineViewModel() : base() { }
  }
}
