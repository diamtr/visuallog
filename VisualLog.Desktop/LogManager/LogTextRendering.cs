using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Rendering;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using VisualLog.Core.Search;
using VisualLog.Desktop.Settings;

namespace VisualLog.Desktop.LogManager
{
  // Line N of the log text is LogMessages[N - 1]: the reader skips empty lines and a message never holds
  // a line break, so the lines map one to one. Only visible lines are styled, as before with the list.

  /// <summary>Text color, weight, style and strikethrough of a line by the highlighting rules.</summary>
  internal sealed class LineStyleColorizer : DocumentColorizingTransformer
  {
    private readonly Func<int, LineStyle> styleOfLine;

    /// <param name="styleOfLine">Look of a line by its 1-based number, null - default look.</param>
    public LineStyleColorizer(Func<int, LineStyle> styleOfLine)
    {
      this.styleOfLine = styleOfLine;
    }

    protected override void ColorizeLine(DocumentLine line)
    {
      if (line.Length == 0)
        return;
      var style = this.styleOfLine(line.LineNumber);
      if (style == null)
        return;
      this.ChangeLinePart(line.Offset, line.EndOffset, element =>
      {
        var properties = element.TextRunProperties;
        // No text color in the rule: keep the color of the log text
        if (!ReferenceEquals(style.Foreground, LineStyle.DefaultForeground))
          properties.SetForegroundBrush(style.Foreground);
        if (style.IsBold || style.IsItalic)
        {
          var typeface = properties.Typeface;
          properties.SetTypeface(new Typeface(typeface.FontFamily, style.FontStyle, style.FontWeight, typeface.Stretch));
        }
        if (style.TextDecorations != null)
          properties.SetTextDecorations(style.TextDecorations);
      });
    }
  }

  /// <summary>Background of a line by the highlighting rules, across the whole width of the view.</summary>
  internal sealed class LineBackgroundRenderer : IBackgroundRenderer
  {
    private readonly Func<int, LineStyle> styleOfLine;

    public LineBackgroundRenderer(Func<int, LineStyle> styleOfLine)
    {
      this.styleOfLine = styleOfLine;
    }

    public KnownLayer Layer => KnownLayer.Background;

    public void Draw(TextView textView, DrawingContext drawingContext)
    {
      if (!textView.VisualLinesValid)
        return;
      foreach (var visualLine in textView.VisualLines)
      {
        var style = this.styleOfLine(visualLine.FirstDocumentLine.LineNumber);
        if (style == null || ReferenceEquals(style.Background, LineStyle.DefaultBackground))
          continue;
        var top = visualLine.VisualTop - textView.ScrollOffset.Y;
        drawingContext.DrawRectangle(style.Background, null, new Rect(0, top, textView.ActualWidth, visualLine.Height));
      }
    }
  }

  /// <summary>Marks the search matches in the visible lines; the matches of the current result stand out.</summary>
  internal sealed class SearchMatchRenderer : IBackgroundRenderer
  {
    private static readonly Brush MatchBrush = Freeze(new SolidColorBrush(Color.FromArgb(0x90, 0xFF, 0xD5, 0x4F)));
    private static readonly Brush CurrentMatchBrush = Freeze(new SolidColorBrush(Color.FromArgb(0xD0, 0xFF, 0x9E, 0x2C)));

    private readonly Func<IList<SearchEntryViewModel>> entries;
    private readonly Func<SearchEntry> currentEntry;

    /// <param name="entries">Search results ordered by line number.</param>
    /// <param name="currentEntry">The result the log was moved to, null - none.</param>
    public SearchMatchRenderer(Func<IList<SearchEntryViewModel>> entries, Func<SearchEntry> currentEntry)
    {
      this.entries = entries;
      this.currentEntry = currentEntry;
    }

    public KnownLayer Layer => KnownLayer.Background;

    public void Draw(TextView textView, DrawingContext drawingContext)
    {
      var entries = this.entries();
      if (entries == null || entries.Count == 0 || !textView.VisualLinesValid)
        return;
      var current = this.currentEntry();
      var matches = new BackgroundGeometryBuilder { AlignToWholePixels = true, CornerRadius = 2 };
      var currentMatches = new BackgroundGeometryBuilder { AlignToWholePixels = true, CornerRadius = 2 };
      var any = false;
      foreach (var visualLine in textView.VisualLines)
      {
        var line = visualLine.FirstDocumentLine;
        var entry = FindByLine(entries, line.LineNumber);
        if (entry == null)
          continue;
        var builder = current != null && current.LineNumber == entry.LineNumber ? currentMatches : matches;
        foreach (var match in entry.Matches)
        {
          var start = Math.Clamp(match.Index, 0, line.Length);
          var length = Math.Clamp(match.Length, 0, line.Length - start);
          if (length == 0)
            continue;
          builder.AddSegment(textView, new TextSegment { StartOffset = line.Offset + start, Length = length });
          any = true;
        }
      }
      if (!any)
        return;
      var geometry = matches.CreateGeometry();
      if (geometry != null)
        drawingContext.DrawGeometry(MatchBrush, null, geometry);
      geometry = currentMatches.CreateGeometry();
      if (geometry != null)
        drawingContext.DrawGeometry(CurrentMatchBrush, null, geometry);
    }

    /// <summary>Binary search by the line number; the results are kept in the order of the lines.</summary>
    internal static SearchEntry FindByLine(IList<SearchEntryViewModel> entries, int lineNumber)
    {
      int low = 0, high = entries.Count - 1;
      while (low <= high)
      {
        var middle = low + (high - low) / 2;
        var entry = entries[middle]?.SearchEntry;
        if (entry == null)
          return null;
        if (entry.LineNumber == lineNumber)
          return entry;
        if (entry.LineNumber < lineNumber)
          low = middle + 1;
        else
          high = middle - 1;
      }
      return null;
    }

    private static Brush Freeze(Brush brush)
    {
      brush.Freeze();
      return brush;
    }
  }
}
