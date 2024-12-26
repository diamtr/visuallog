using System.Text.RegularExpressions;

namespace VisualLog.Core.Search
{
  public class TextStatement : ISearchRequestStatement
  {
    public int Order { get; set; }
    public string Text
    {
      get { return this.text; }
      set
      {
        this.text = value;
        this.compiledRegex = this.regexOptions.HasValue ? new Regex(this.text, this.regexOptions.Value) : null;
      }
    }
    private string text;
    public RegexOptions? RegexOptions
    {
      get { return this.regexOptions; }
      set
      {
        this.regexOptions = System.Text.RegularExpressions.RegexOptions.Compiled;
        if (value.HasValue)
           this.regexOptions = this.regexOptions | value;
        this.compiledRegex = this.regexOptions.HasValue ? new Regex(this.Text, this.regexOptions.Value) : null;
      }
    }
    private RegexOptions? regexOptions;
    private Regex compiledRegex;

    public TextStatement()
    {
      this.regexOptions = System.Text.RegularExpressions.RegexOptions.Compiled;
    }

    public MatchCollection GetMatches(Message message)
    {
      if (this.compiledRegex == null)
        this.compiledRegex = new Regex(this.Text, this.RegexOptions.Value);
      return this.compiledRegex.Matches(message.RawValue);
    }
  }
}
