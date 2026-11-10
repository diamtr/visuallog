using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace VisualLog.Core.Search
{
  public class TextStatement : ISearchRequestStatement
  {
    // RegexOptions.Compiled is intentionally not used: the statement is rebuilt on every
    // keystroke in search-as-you-type scenarios, and compiling to IL costs far more than
    // the interpreter saves on a single pass over a log.
    private RegexOptions DefaultRegexOptions => RegexOptions.None;

    private bool useRegex;
    public bool UseRegex
    {
      get { return this.useRegex; }
      set
      {
        this.useRegex = value;
        this.UpdateCompiledRegex();
      }
    }

    private string text;
    public string Text
    {
      get { return this.text; }
      set
      {
        this.text = value ?? string.Empty;
        this.UpdateCompiledRegex();
      }
    }

    private bool matchCase;
    /// <summary>
    /// When false, the search ignores case (adds <see cref="RegexOptions.IgnoreCase"/>). Default is true.
    /// </summary>
    public bool MatchCase
    {
      get { return this.matchCase; }
      set
      {
        this.matchCase = value;
        this.UpdateCompiledRegex();
      }
    }

    private RegexOptions regexOptions;
    public RegexOptions RegexOptions
    {
      get { return this.regexOptions; }
      set
      {
        this.regexOptions = this.DefaultRegexOptions | value;
        this.UpdateCompiledRegex();
      }
    }

    /// <summary>
    /// Parse error message of the regex pattern; null when the pattern is valid,
    /// <see cref="UseRegex"/> is false or the text is empty.
    /// On error the statement falls back to searching the escaped literal text.
    /// </summary>
    public string RegexError { get; private set; }

    private Regex compiledRegex;

    public TextStatement()
    {
      this.useRegex = true;
      this.matchCase = true;
      this.regexOptions = this.DefaultRegexOptions;
    }

    public IEnumerable<Match> GetMatches(Message message)
    {
      return this.compiledRegex != null ? this.compiledRegex.Matches(message.RawValue) : Enumerable.Empty<Match>();
    }

    private void UpdateCompiledRegex()
    {
      this.RegexError = null;
      if (string.IsNullOrWhiteSpace(this.Text))
      {
        this.compiledRegex = null;
        return;
      }

      var options = this.RegexOptions | (this.MatchCase ? RegexOptions.None : RegexOptions.IgnoreCase);
      var pattern = this.UseRegex ? this.Text : Regex.Escape(this.Text);
      try
      {
        this.compiledRegex = new Regex(pattern, options);
      }
      catch (RegexParseException ex)
      {
        this.RegexError = ex.Message;
        this.compiledRegex = new Regex(Regex.Escape(this.Text), options);
      }
    }
  }
}
