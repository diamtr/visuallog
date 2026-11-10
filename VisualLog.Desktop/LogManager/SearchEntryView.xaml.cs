using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using VisualLog.Core.Search;
using VisualLog.Desktop.Controls;

namespace VisualLog.Desktop.LogManager
{
  /// <summary>
  /// A found line with its matches highlighted. The line is shown from its start when the first match
  /// fits the width; otherwise the start is cut ("…") so that the match is visible.
  /// </summary>
  public partial class SearchEntryView : UserControl
  {
    /// <summary>Characters kept visible after the first match when the start of the line is cut.</summary>
    public const int TrailingContext = 12;

    private static readonly Brush FallbackMatchBrush = CreateFallbackBrush();

    private SearchEntry entry;
    // Leading context the text was built with; null - not built yet
    private int? builtLeadingContext;

    public SearchEntryView()
    {
      InitializeComponent();
    }

    private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
      // Rows are recycled by the virtualizing list: always rebuild from scratch
      this.entry = (e.NewValue as SearchEntryViewModel)?.SearchEntry;
      this.builtLeadingContext = null;
      this.Rebuild();
    }

    private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
    {
      if (e.WidthChanged)
        this.Rebuild();
    }

    private void Rebuild()
    {
      var text = this.entry?.Message?.RawValue;
      var leadingContext = LeadingContextFor(text, this.entry?.Matches, this.VisibleCharacters());
      if (this.builtLeadingContext == leadingContext)
        return;
      this.builtLeadingContext = leadingContext;
      var segments = MatchSegments.Split(text, this.entry?.Matches, leadingContext);
      var brush = this.TryFindResource("SearchMatchBrush") as Brush ?? FallbackMatchBrush;
      MatchSegments.Fill(this.SearchEntryTextBlock.Inlines, segments, brush);
    }

    /// <summary>How many characters of the line fit the row; 0 - not measured yet.</summary>
    private int VisibleCharacters()
    {
      var width = this.ActualWidth;
      if (width <= 0)
        return 0;
      var block = this.SearchEntryTextBlock;
      var typeface = new Typeface(block.FontFamily, block.FontStyle, block.FontWeight, block.FontStretch);
      var sample = new FormattedText("0", CultureInfo.InvariantCulture, FlowDirection.LeftToRight, typeface,
        block.FontSize, Brushes.Black, VisualTreeHelper.GetDpi(this).PixelsPerDip);
      return sample.WidthIncludingTrailingWhitespace > 0 ? (int)(width / sample.WidthIncludingTrailingWhitespace) : 0;
    }

    /// <summary>
    /// Characters to keep before the first match (see <see cref="MatchSegments.Split"/>); -1 - the line is shown
    /// from its start: the first match fits, or the width is not known yet.
    /// </summary>
    public static int LeadingContextFor(string text, IEnumerable<MatchEntry> matches, int visibleCharacters)
    {
      if (string.IsNullOrEmpty(text) || matches == null || visibleCharacters <= 0)
        return -1;
      var start = -1;
      var end = -1;
      foreach (var match in matches)
      {
        if (match.Length <= 0 || match.Index < 0 || match.Index >= text.Length)
          continue;
        if (start < 0 || match.Index < start)
        {
          start = match.Index;
          end = Math.Min(text.Length, match.Index + match.Length);
        }
      }
      if (start < 0 || end <= visibleCharacters)
        return -1;
      // "…" + leading context + match + some text after it fill the row
      var trailing = Math.Min(TrailingContext, text.Length - end);
      return Math.Max(0, visibleCharacters - MatchSegments.Ellipsis.Length - (end - start) - trailing);
    }

    private static Brush CreateFallbackBrush()
    {
      var brush = new SolidColorBrush(Color.FromRgb(0xFF, 0xE7, 0xA3));
      brush.Freeze();
      return brush;
    }
  }
}
