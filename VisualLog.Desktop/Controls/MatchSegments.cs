using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using VisualLog.Core.Search;

namespace VisualLog.Desktop.Controls
{
  /// <summary>A piece of text that is either plain or a search match.</summary>
  public readonly record struct TextSegment(string Text, bool IsMatch);

  /// <summary>Splits text into plain/match segments for highlighting search results.</summary>
  public static class MatchSegments
  {
    public const string Ellipsis = "…";

    /// <summary>
    /// Splits <paramref name="text"/> into alternating plain and match segments.
    /// Matches may be unsorted, overlapping (merged) or out of range (clamped/skipped);
    /// zero-length matches are ignored.
    /// </summary>
    /// <param name="maxLeadingContext">
    /// When &gt;= 0 and the first match starts after more than this many characters,
    /// the leading plain text is cut to its last <paramref name="maxLeadingContext"/> chars
    /// and prefixed with "…". -1 disables trimming.
    /// </param>
    public static IReadOnlyList<TextSegment> Split(string text, IEnumerable<MatchEntry> matches, int maxLeadingContext = -1)
    {
      var result = new List<TextSegment>();
      if (string.IsNullOrEmpty(text))
        return result;
      if (matches == null)
      {
        result.Add(new TextSegment(text, false));
        return result;
      }

      // Clamp to the text and drop empty ranges
      var ranges = new List<(int Start, int End)>();
      foreach (var m in matches)
      {
        if (m.Length <= 0)
          continue;
        var start = Math.Max(0, m.Index);
        var end = (int)Math.Min((long)m.Index + m.Length, text.Length);
        if (start >= text.Length || end <= start)
          continue;
        ranges.Add((start, end));
      }
      ranges.Sort((a, b) => a.Start != b.Start ? a.Start.CompareTo(b.Start) : a.End.CompareTo(b.End));

      // Merge overlapping / touching ranges
      var merged = new List<(int Start, int End)>(ranges.Count);
      foreach (var r in ranges)
      {
        if (merged.Count > 0 && r.Start <= merged[merged.Count - 1].End)
        {
          var last = merged[merged.Count - 1];
          if (r.End > last.End)
            merged[merged.Count - 1] = (last.Start, r.End);
        }
        else
          merged.Add(r);
      }

      var index = 0;
      foreach (var r in merged)
      {
        if (r.Start > index)
        {
          var plain = text.Substring(index, r.Start - index);
          if (index == 0 && maxLeadingContext >= 0 && plain.Length > maxLeadingContext)
            plain = Ellipsis + plain.Substring(plain.Length - maxLeadingContext);
          result.Add(new TextSegment(plain, false));
        }
        result.Add(new TextSegment(text.Substring(r.Start, r.End - r.Start), true));
        index = r.End;
      }
      if (index < text.Length)
        result.Add(new TextSegment(text.Substring(index), false));

      return result;
    }

    /// <summary>Clears <paramref name="inlines"/> and fills it with runs for the segments.</summary>
    public static void Fill(InlineCollection inlines, IReadOnlyList<TextSegment> segments, Brush matchBackground, FontWeight? matchWeight = null)
    {
      if (inlines == null)
        throw new ArgumentNullException(nameof(inlines));
      inlines.Clear();
      if (segments == null)
        return;
      foreach (var segment in segments)
      {
        var run = new Run(segment.Text);
        if (segment.IsMatch)
        {
          run.Background = matchBackground;
          if (matchWeight.HasValue)
            run.FontWeight = matchWeight.Value;
        }
        inlines.Add(run);
      }
    }
  }
}
