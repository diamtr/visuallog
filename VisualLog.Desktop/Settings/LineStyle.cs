using System.Windows;
using System.Windows.Media;

namespace VisualLog.Desktop.Settings
{
  /// <summary>
  /// The resulting look of a log line. Every property always has a usable value
  /// (unset ones fall back to the defaults), so views can bind to it directly.
  /// Instances are immutable and the brushes are frozen: safe to share between lines and threads.
  /// </summary>
  public sealed class LineStyle
  {
    public static readonly Brush DefaultForeground = Freeze(SystemColors.ControlTextBrush);
    public static readonly Brush DefaultBackground = Brushes.Transparent;

    public Brush Foreground { get; }
    public Brush Background { get; }
    public FontWeight FontWeight { get; }
    public FontStyle FontStyle { get; }
    /// <summary>Strikethrough or null.</summary>
    public TextDecorationCollection TextDecorations { get; }

    public bool IsBold => this.FontWeight == FontWeights.Bold;
    public bool IsItalic => this.FontStyle == FontStyles.Italic;
    public bool IsStrikethrough => this.TextDecorations != null;

    public LineStyle(Color? foreground, Color? background, bool bold, bool italic, bool strikethrough)
    {
      this.Foreground = foreground.HasValue ? Freeze(new SolidColorBrush(foreground.Value)) : DefaultForeground;
      this.Background = background.HasValue ? Freeze(new SolidColorBrush(background.Value)) : DefaultBackground;
      this.FontWeight = bold ? FontWeights.Bold : FontWeights.Normal;
      this.FontStyle = italic ? FontStyles.Italic : FontStyles.Normal;
      this.TextDecorations = strikethrough ? System.Windows.TextDecorations.Strikethrough : null;
    }

    /// <summary>Parses "#RRGGBB" / "#AARRGGBB" (or a named color); null if empty or invalid.</summary>
    public static Color? ParseColor(string value)
    {
      if (string.IsNullOrWhiteSpace(value))
        return null;
      try
      {
        return (Color)ColorConverter.ConvertFromString(value.Trim());
      }
      catch
      {
        return null;
      }
    }

    private static Brush Freeze(Brush brush)
    {
      if (brush.CanFreeze && !brush.IsFrozen)
      {
        brush = brush.Clone();
        brush.Freeze();
      }
      return brush;
    }
  }
}
