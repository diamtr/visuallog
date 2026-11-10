using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using VisualLog.Core.Search;
using VisualLog.Desktop.Controls;

namespace VisualLog.Desktop.Tests
{
  [TestFixture]
  public class MatchSegmentsFixture
  {
    private static MatchEntry M(int index, int length) => new MatchEntry { Index = index, Length = length };

    private static string Describe(IReadOnlyList<TextSegment> segments) =>
      string.Concat(segments.Select(s => s.IsMatch ? $"[{s.Text}]" : s.Text));

    [Test]
    public void EmptyAndNullInput()
    {
      Assert.That(MatchSegments.Split(null, new[] { M(0, 1) }), Is.Empty);
      Assert.That(MatchSegments.Split("", new[] { M(0, 1) }), Is.Empty);

      var noMatches = MatchSegments.Split("abc", null);
      Assert.That(noMatches, Is.EqualTo(new[] { new TextSegment("abc", false) }));

      Assert.That(MatchSegments.Split("abc", new MatchEntry[0]), Is.EqualTo(new[] { new TextSegment("abc", false) }));
    }

    [Test]
    public void SimpleSplit()
    {
      var segments = MatchSegments.Split("hello error world", new[] { M(6, 5) });
      Assert.That(segments, Is.EqualTo(new[]
      {
        new TextSegment("hello ", false),
        new TextSegment("error", true),
        new TextSegment(" world", false),
      }));
    }

    [Test]
    public void SingleTrailingCharacterIsKept()
    {
      // Regression: the old code used `index < rawString.Length - 1` and dropped the last char
      Assert.That(Describe(MatchSegments.Split("errorX", new[] { M(0, 5) })), Is.EqualTo("[error]X"));
      Assert.That(Describe(MatchSegments.Split("a b", new[] { M(0, 1) })), Is.EqualTo("[a] b"));
      Assert.That(Describe(MatchSegments.Split("ab", new[] { M(0, 1) })), Is.EqualTo("[a]b"));
    }

    [Test]
    public void MatchAtStartAndEnd()
    {
      Assert.That(Describe(MatchSegments.Split("error", new[] { M(0, 5) })), Is.EqualTo("[error]"));
      Assert.That(Describe(MatchSegments.Split("an error", new[] { M(3, 5) })), Is.EqualTo("an [error]"));
    }

    [Test]
    public void UnsortedMatches()
    {
      var segments = MatchSegments.Split("a1b2c3", new[] { M(5, 1), M(1, 1), M(3, 1) });
      Assert.That(Describe(segments), Is.EqualTo("a[1]b[2]c[3]"));
      Assert.That(segments.Select(s => s.IsMatch), Is.EqualTo(new[] { false, true, false, true, false, true }));
    }

    [Test]
    public void OverlappingAndTouchingMatchesAreMerged()
    {
      Assert.That(Describe(MatchSegments.Split("abcdefgh", new[] { M(1, 3), M(2, 4) })), Is.EqualTo("a[bcdef]gh"));
      Assert.That(Describe(MatchSegments.Split("abcdefgh", new[] { M(1, 5), M(2, 1) })), Is.EqualTo("a[bcdef]gh"), "contained");
      Assert.That(Describe(MatchSegments.Split("abcdefgh", new[] { M(1, 2), M(3, 2) })), Is.EqualTo("a[bcde]fgh"), "touching");
      Assert.That(Describe(MatchSegments.Split("abcdefgh", new[] { M(2, 2), M(2, 2) })), Is.EqualTo("ab[cd]efgh"), "duplicate");
    }

    [Test]
    public void OutOfRangeMatchesAreClampedOrSkipped()
    {
      Assert.That(Describe(MatchSegments.Split("abcdef", new[] { M(4, 100) })), Is.EqualTo("abcd[ef]"));
      Assert.That(Describe(MatchSegments.Split("abcdef", new[] { M(-2, 4) })), Is.EqualTo("[ab]cdef"));
      Assert.That(Describe(MatchSegments.Split("abcdef", new[] { M(6, 2), M(50, 1), M(-5, 2) })), Is.EqualTo("abcdef"));
      Assert.That(Describe(MatchSegments.Split("abcdef", new[] { M(int.MaxValue - 1, int.MaxValue) })), Is.EqualTo("abcdef"));
      Assert.That(Describe(MatchSegments.Split("abcdef", new[] { M(2, int.MaxValue) })), Is.EqualTo("ab[cdef]"));
    }

    [Test]
    public void ZeroAndNegativeLengthMatchesAreSkipped()
    {
      Assert.That(Describe(MatchSegments.Split("abcdef", new[] { M(2, 0), M(3, -1), M(4, 1) })), Is.EqualTo("abcd[e]f"));
    }

    [Test]
    public void SegmentsReassembleOriginalText()
    {
      var text = "2024-01-01 ERROR db timeout, ERROR again";
      var segments = MatchSegments.Split(text, new[] { M(29, 5), M(11, 5) });
      Assert.That(string.Concat(segments.Select(s => s.Text)), Is.EqualTo(text));
      Assert.That(segments.Where(s => s.IsMatch).Select(s => s.Text), Is.EqualTo(new[] { "ERROR", "ERROR" }));
    }

    [Test]
    public void LeadingContextTrimming()
    {
      var text = new string('x', 100) + "error tail";
      var segments = MatchSegments.Split(text, new[] { M(100, 5) }, maxLeadingContext: 10);
      Assert.That(segments[0], Is.EqualTo(new TextSegment("…" + new string('x', 10), false)));
      Assert.That(segments[1], Is.EqualTo(new TextSegment("error", true)));
      Assert.That(segments[2], Is.EqualTo(new TextSegment(" tail", false)));

      // Short enough: untouched
      Assert.That(Describe(MatchSegments.Split("abc error", new[] { M(4, 5) }, maxLeadingContext: 4)), Is.EqualTo("abc [error]"));
      // -1: no trimming
      Assert.That(MatchSegments.Split(text, new[] { M(100, 5) })[0].Text, Is.EqualTo(new string('x', 100)));
      // 0: only the ellipsis remains
      Assert.That(Describe(MatchSegments.Split("abc error", new[] { M(4, 5) }, maxLeadingContext: 0)), Is.EqualTo("…[error]"));
      // Match at the start: nothing to trim
      Assert.That(Describe(MatchSegments.Split("error abc", new[] { M(0, 5) }, maxLeadingContext: 0)), Is.EqualTo("[error] abc"));
      // Only the leading text is trimmed, text between matches is kept
      Assert.That(Describe(MatchSegments.Split("aaaa1bbbb2", new[] { M(4, 1), M(9, 1) }, maxLeadingContext: 2)), Is.EqualTo("…aa[1]bbbb[2]"));
      // No matches: no trimming
      Assert.That(Describe(MatchSegments.Split("abcdef", new MatchEntry[0], maxLeadingContext: 2)), Is.EqualTo("abcdef"));
    }
  }
}
