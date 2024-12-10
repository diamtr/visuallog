using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace VisualLog.Core
{
  public class Log
  {
    public List<Message> Messages { get; set; }
    public Encoding Encoding { get; private set; }
    public Format Format { get; set; }
    public bool ReadingInProcess { get; set; }
    public LogReader LogReader { get; private set; }

    public event Action<Message> CatchNewMessage
    {
      add
      {
        if (catchNewMessage == null || !catchNewMessage.GetInvocationList().Contains(value))
          this.catchNewMessage += value;
      }
      remove
      {
        this.catchNewMessage -= value;
      }
    }

    private string sourceFilePath;
    private FileSystemWatcher sourceFileWatcher;
    private Action<Message> catchNewMessage;

    #region ctors

    public Log(string path)
    {
      this.Messages = new List<Message>();
      this.sourceFilePath = path; // TODO: maybe remove this.sourceFilePath
      this.LogReader = new LogReader(path);
    }

    public Log(string path, Encoding encoding) : this(path)
    {
      this.Encoding = encoding; // TODO: maybe remove this.Encoding
      this.LogReader.SetEncoding(encoding);
    }

    #endregion

    #region Reading

    public void Read()
    {
      this.LogReader.LineReaded += this.NewLineReaded;
      this.LogReader.StartReading();
    }

    public void NewLineReaded(string s)
    {
      var message = new Message(s);
      message.FillParts();
      this.Messages.Add(message);
      if (this.catchNewMessage != null)
        this.catchNewMessage.Invoke(message);
    }

    #endregion

    #region Searching

    /// <summary>
    /// Search entries of the string.
    /// </summary>
    /// <param name="s">String to search.</param>
    /// <param name="additionalOptions">Additional regular expression options.</param>
    /// <returns>String search results.</returns>
    /// <remarks>The entries search is going on through regular expression. It always has the "Compiled" option.</remarks>
    public SearchResults SearchString(string s, RegexOptions? additionalOptions = null)
    {
      var options = RegexOptions.Compiled;
      if (additionalOptions.HasValue)
        options = options | additionalOptions.Value;
      var re = new Regex(s, options);

      var searchResults = new SearchResults();
      var i = 1;
      foreach (var message in this.Messages)
      {
        var searchEntry = this.SearchStringInLine(i, message.RawValue, re);
        if (searchEntry.Matches.Any())
          searchResults.Entries.Add(searchEntry);
        i++;
      }

      return searchResults;
    }

    /// <summary>
    /// Search entries of the string in the log line.
    /// </summary>
    /// <param name="lineNumber">Log line number (1-based).</param>
    /// <param name="line">Log line.</param>
    /// <param name="re">Regex.</param>
    /// <returns>Entries of the string in the log line.</returns>
    public SearchEntry SearchStringInLine(int lineNumber, string line, Regex re)
    {
      var matches = re.Matches(line);
      return new SearchEntry() {
        LineNumber = lineNumber,
        RawString = line,
        Matches = matches.Where(x => x.Success)
          .Select(x => new Match() { Index = x.Index, Length = x.Length })
          .ToList()
      };
    }

    #endregion

    public void ApplyFormat()
    {
      if (this.Format == null)
        return;

      foreach (var message in this.Messages)
        message.Parts = this.Format.Deserialize(message);
    }
  }
}
