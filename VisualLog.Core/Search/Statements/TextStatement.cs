using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace VisualLog.Core.Search
{
  public class TextStatement : ISearchRequestStatement
  {
    private RegexOptions DefaultRegexOptions => System.Text.RegularExpressions.RegexOptions.Compiled;

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

    private Regex compiledRegex;

    public TextStatement()
    {
      this.useRegex = true;
      this.regexOptions = this.DefaultRegexOptions;
    }

    public IEnumerable<Match> GetMatches(Message message)
    {
      return this.compiledRegex != null ? this.compiledRegex.Matches(message.RawValue) : Enumerable.Empty<Match>();
    }

    private void UpdateCompiledRegex()
    {
      if (string.IsNullOrWhiteSpace(this.Text))
      {
        this.compiledRegex = null;
        return;
      }

      var text = this.UseRegex ? this.Text : Regex.Escape(this.Text);
      this.compiledRegex = new Regex(text, this.RegexOptions);
    }
  }
}
