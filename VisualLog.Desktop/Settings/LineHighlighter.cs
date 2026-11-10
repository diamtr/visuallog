using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Media;

namespace VisualLog.Desktop.Settings
{
  /// <summary>
  /// Compiled, immutable set of <see cref="HighlightRule"/>s. Thread-safe.
  /// Rebuild it (see <see cref="HighlightingService"/>) when the rules change.
  /// </summary>
  public sealed class LineHighlighter
  {
    public static readonly LineHighlighter Empty = new LineHighlighter(null);

    private static readonly TimeSpan RegexTimeout = TimeSpan.FromMilliseconds(100);

    private readonly CompiledRule[] rules;
    // Lines matching the same set of rules share one LineStyle
    private readonly Dictionary<ulong, LineStyle> styleCache = new Dictionary<ulong, LineStyle>();

    public bool IsEmpty => this.rules.Length == 0;

    public LineHighlighter(IEnumerable<HighlightRule> rules)
    {
      this.rules = (rules ?? Enumerable.Empty<HighlightRule>())
        .Where(x => x != null && x.IsEnabled && !string.IsNullOrEmpty(x.Pattern) && Validate(x) == null)
        .Where(HasAnyStyle)
        .Select(x => new CompiledRule(x))
        .ToArray();
    }

    /// <summary>The look of the line, or null if no rule matches.</summary>
    public LineStyle Match(string line)
    {
      if (this.rules.Length == 0 || string.IsNullOrEmpty(line))
        return null;

      ulong key = 0;
      List<CompiledRule> matched = null;
      for (int i = 0; i < this.rules.Length; i++)
      {
        if (!this.rules[i].IsMatch(line))
          continue;
        (matched ??= new List<CompiledRule>()).Add(this.rules[i]);
        if (i < 64)
          key |= 1UL << i;
      }
      if (matched == null)
        return null;

      // More than 64 rules: the key is not unique, don't cache
      var cacheable = this.rules.Length <= 64;
      if (cacheable)
        lock (this.styleCache)
          if (this.styleCache.TryGetValue(key, out var cached))
            return cached;

      var style = Merge(matched);
      if (cacheable)
        lock (this.styleCache)
          this.styleCache[key] = style;
      return style;
    }

    /// <summary>Error message for an invalid rule (bad regular expression), otherwise null.</summary>
    public static string Validate(HighlightRule rule)
    {
      if (rule == null || !rule.IsRegex || string.IsNullOrEmpty(rule.Pattern))
        return null;
      try
      {
        _ = new Regex(rule.Pattern, RegexOptions.None, RegexTimeout);
        return null;
      }
      catch (ArgumentException ex)
      {
        return ex.Message;
      }
    }

    /// <summary>The look a rule gives on its own, e.g. for a preview. Null if the rule sets nothing.</summary>
    public static LineStyle Preview(HighlightRule rule)
    {
      if (rule == null || !HasAnyStyle(rule))
        return null;
      return new LineStyle(LineStyle.ParseColor(rule.Foreground), LineStyle.ParseColor(rule.Background),
                           rule.Bold, rule.Italic, rule.Strikethrough);
    }

    private static bool HasAnyStyle(HighlightRule rule)
    {
      return rule.Bold || rule.Italic || rule.Strikethrough ||
             LineStyle.ParseColor(rule.Foreground).HasValue ||
             LineStyle.ParseColor(rule.Background).HasValue;
    }

    private static LineStyle Merge(List<CompiledRule> matched)
    {
      Color? foreground = matched.Select(x => x.Foreground).FirstOrDefault(x => x.HasValue);
      Color? background = matched.Select(x => x.Background).FirstOrDefault(x => x.HasValue);
      return new LineStyle(foreground, background,
                           matched.Any(x => x.Rule.Bold),
                           matched.Any(x => x.Rule.Italic),
                           matched.Any(x => x.Rule.Strikethrough));
    }

    private sealed class CompiledRule
    {
      public HighlightRule Rule { get; }
      public Color? Foreground { get; }
      public Color? Background { get; }
      private readonly Regex regex;
      private readonly StringComparison comparison;

      public CompiledRule(HighlightRule rule)
      {
        this.Rule = rule.Clone();
        this.Foreground = LineStyle.ParseColor(rule.Foreground);
        this.Background = LineStyle.ParseColor(rule.Background);
        this.comparison = rule.MatchCase ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
        if (rule.IsRegex)
          this.regex = new Regex(rule.Pattern,
                                 RegexOptions.Compiled | RegexOptions.CultureInvariant |
                                 (rule.MatchCase ? RegexOptions.None : RegexOptions.IgnoreCase),
                                 RegexTimeout);
      }

      public bool IsMatch(string line)
      {
        if (this.regex == null)
          return line.IndexOf(this.Rule.Pattern, this.comparison) >= 0;
        try
        {
          return this.regex.IsMatch(line);
        }
        catch (RegexMatchTimeoutException)
        {
          return false;
        }
      }
    }
  }
}
