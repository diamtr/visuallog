using NUnit.Framework;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using VisualLog.Core;
using VisualLog.Desktop.LogManager;
using VisualLog.Desktop.Settings;

namespace VisualLog.Desktop.Tests
{
  [TestFixture]
  public class LineHighlighterFixture
  {
    private static Color ColorOf(Brush brush) => ((SolidColorBrush)brush).Color;

    [Test]
    public void NoRules()
    {
      Assert.That(LineHighlighter.Empty.Match("ERROR something"), Is.Null);
      Assert.That(new LineHighlighter(null).Match("ERROR something"), Is.Null);
    }

    [Test]
    public void PlainText()
    {
      var highlighter = new LineHighlighter(new[]
      {
        new HighlightRule { Pattern = "error", Foreground = "#C50F1F", Background = "#FDE7E9", Bold = true }
      });

      // Case-insensitive by default
      var style = highlighter.Match("2024-01-01 ERROR something failed");
      Assert.That(style, Is.Not.Null);
      Assert.That(ColorOf(style.Foreground), Is.EqualTo((Color)ColorConverter.ConvertFromString("#C50F1F")));
      Assert.That(ColorOf(style.Background), Is.EqualTo((Color)ColorConverter.ConvertFromString("#FDE7E9")));
      Assert.That(style.FontWeight, Is.EqualTo(FontWeights.Bold));
      Assert.That(style.FontStyle, Is.EqualTo(FontStyles.Normal));
      Assert.That(style.TextDecorations, Is.Null);

      Assert.That(highlighter.Match("INFO all good"), Is.Null);
      Assert.That(highlighter.Match(""), Is.Null);
      Assert.That(highlighter.Match(null), Is.Null);
    }

    [Test]
    public void MatchCase()
    {
      var highlighter = new LineHighlighter(new[]
      {
        new HighlightRule { Pattern = "ERROR", MatchCase = true, Italic = true }
      });
      Assert.That(highlighter.Match("ERROR here"), Is.Not.Null);
      Assert.That(highlighter.Match("error here"), Is.Null);
    }

    [Test]
    public void Regex()
    {
      var highlighter = new LineHighlighter(new[]
      {
        new HighlightRule { Pattern = @"\btimeout\s+\d+ms", IsRegex = true, Strikethrough = true }
      });
      var style = highlighter.Match("request TIMEOUT 300ms");
      Assert.That(style, Is.Not.Null);
      Assert.That(style.IsStrikethrough, Is.True);
      Assert.That(highlighter.Match("request timeout soon"), Is.Null);
    }

    [Test]
    public void InvalidRegexIsSkipped()
    {
      var bad = new HighlightRule { Pattern = "([", IsRegex = true, Bold = true };
      Assert.That(LineHighlighter.Validate(bad), Is.Not.Null);
      Assert.That(LineHighlighter.Validate(new HighlightRule { Pattern = "([", Bold = true }), Is.Null, "plain text is never invalid");

      var highlighter = new LineHighlighter(new[] { bad, new HighlightRule { Pattern = "warn", Italic = true } });
      Assert.That(highlighter.Match("([ text"), Is.Null);
      Assert.That(highlighter.Match("WARN text").IsItalic, Is.True);
    }

    [Test]
    public void DisabledEmptyAndStylelessRulesAreSkipped()
    {
      var highlighter = new LineHighlighter(new[]
      {
        new HighlightRule { Pattern = "a", Bold = true, IsEnabled = false },
        new HighlightRule { Pattern = "", Bold = true },
        new HighlightRule { Pattern = "a" },
        new HighlightRule { Pattern = "a", Foreground = "not a color" },
      });
      Assert.That(highlighter.IsEmpty, Is.True);
      Assert.That(highlighter.Match("aaa"), Is.Null);
    }

    [Test]
    public void TopmostRuleWinsColorsFlagsAreCombined()
    {
      var highlighter = new LineHighlighter(new[]
      {
        new HighlightRule { Pattern = "error", Foreground = "#C50F1F" },
        new HighlightRule { Pattern = "db", Foreground = "#0F6CBD", Background = "#DEECF9", Italic = true },
      });

      var both = highlighter.Match("db error");
      Assert.That(ColorOf(both.Foreground), Is.EqualTo((Color)ColorConverter.ConvertFromString("#C50F1F")));
      Assert.That(ColorOf(both.Background), Is.EqualTo((Color)ColorConverter.ConvertFromString("#DEECF9")), "not set by the first rule, taken from the next one");
      Assert.That(both.IsItalic, Is.True);

      var onlyDb = highlighter.Match("db query");
      Assert.That(ColorOf(onlyDb.Foreground), Is.EqualTo((Color)ColorConverter.ConvertFromString("#0F6CBD")));

      // Unset properties keep usable defaults
      var onlyError = highlighter.Match("error");
      Assert.That(onlyError.Background, Is.SameAs(LineStyle.DefaultBackground));
      Assert.That(onlyError.FontWeight, Is.EqualTo(FontWeights.Normal));

      // Lines matching the same rules share the style
      Assert.That(highlighter.Match("another db error"), Is.SameAs(both));
    }

    [Test]
    public void RulesAreCopied()
    {
      var rule = new HighlightRule { Pattern = "error", Bold = true };
      var highlighter = new LineHighlighter(new[] { rule });
      rule.Pattern = "something else";
      Assert.That(highlighter.Match("error"), Is.Not.Null);
    }

    [Test]
    public void LogLinesFollowRuleChanges()
    {
      var service = new HighlightingService();
      var log = new LogViewModel { Highlighting = service };
      log.OnNewLogMessageCatched(new Message("ERROR first"));
      log.OnNewLogMessageCatched(new Message("INFO second"));
      log.FlushPendingMessages();

      var error = log.LogMessages[0];
      var info = log.LogMessages[1];
      Assert.That(error.Highlight, Is.Null);

      var notified = new List<string>();
      error.PropertyChanged += (s, e) => notified.Add(e.PropertyName);

      service.Update(new[] { new HighlightRule { Pattern = "error", Bold = true } });
      Assert.That(notified, Does.Contain(nameof(MessageInlineViewModel.Highlight)));
      Assert.That(error.Highlight.IsBold, Is.True);
      Assert.That(info.Highlight, Is.Null);

      service.Update(new HighlightRule[0]);
      Assert.That(error.Highlight, Is.Null);

      // A closed log no longer listens to the service
      log.Dispose();
      notified.Clear();
      service.Update(new[] { new HighlightRule { Pattern = "error", Bold = true } });
      Assert.That(notified, Is.Empty);
    }

    [Test]
    public void SettingsRoundTrip()
    {
      var path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"visuallog_settings_{System.Guid.NewGuid():N}.json");
      try
      {
        var settings = new AppSettings();
        settings.HighlightRules.Add(new HighlightRule { Pattern = "err", IsRegex = true, Foreground = "#C50F1F", Strikethrough = true });
        Assert.That(settings.Save(path), Is.True);

        var loaded = AppSettings.Load(path);
        Assert.That(loaded.HighlightRules.Count, Is.EqualTo(1));
        var rule = loaded.HighlightRules[0];
        Assert.That(rule.Pattern, Is.EqualTo("err"));
        Assert.That(rule.IsRegex, Is.True);
        Assert.That(rule.Foreground, Is.EqualTo("#C50F1F"));
        Assert.That(rule.Strikethrough, Is.True);
        Assert.That(rule.IsEnabled, Is.True);

        System.IO.File.WriteAllText(path, "{ broken");
        Assert.That(AppSettings.Load(path).HighlightRules, Is.Empty);
        Assert.That(AppSettings.Load(path + ".missing").HighlightRules, Is.Empty);
      }
      finally
      {
        System.IO.File.Delete(path);
      }
    }
  }
}
