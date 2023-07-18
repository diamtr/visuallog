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

    public Log()
    {
      this.Messages = new List<Message>();
    }

    public Log(string path) : this()
    {
      this.sourceFilePath = path;
    }

    public Log(string path, Encoding encoding) : this(path)
    {
      this.Encoding = encoding;
    }

    #endregion

    #region Reading

    public void Read()
    {
      this.ReadingInProcess = true;
      var lines = new List<string>();
      if (this.Encoding != null)
        lines = File.ReadAllLines(this.sourceFilePath, this.Encoding).ToList();
      else
        lines = File.ReadAllLines(this.sourceFilePath).ToList();
      foreach (var line in lines)
        this.Messages.Add(new Message(line));
      this.ReadingInProcess = false;
    }

    public void StartFollowTail()
    {
      var fileInfo = new FileInfo(this.sourceFilePath);
      this.sourceFileWatcher = new FileSystemWatcher(fileInfo.DirectoryName);
      this.sourceFileWatcher.NotifyFilter = NotifyFilters.LastWrite;
      this.sourceFileWatcher.Filter = Path.GetFileName(fileInfo.Name);
      this.sourceFileWatcher.Changed += this.ReadLogUpdates;
      this.sourceFileWatcher.EnableRaisingEvents = true;
    }

    public void ReadLogUpdates(object sender, FileSystemEventArgs e)
    {
      if (e.ChangeType != WatcherChangeTypes.Changed)
        return;

      if (this.ReadingInProcess)
        return;

      this.ReadingInProcess = true;
      using (var fileStream = new FileStream(this.sourceFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
      using (var streamReader = new StreamReader(fileStream, this.Encoding))
      {
        var linesCount = 0;
        while (streamReader.Peek() >= 0)
        {
          var line = streamReader.ReadLine();
          if (line == null)
            break;
          linesCount++;
          if (linesCount <= this.Messages.Count)
            continue;
          try
          {
            var newMessage = new Message(line);
            this.Messages.Add(newMessage);
            if (this.catchNewMessage != null)
              this.catchNewMessage.Invoke(newMessage);
          }
          catch
          {
            continue;
          }
        }
      }
      this.ReadingInProcess = false;
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
      foreach (var message in this.Messages)
        searchResults.Entries.AddRange(this.SearchStringInLine(this.Messages.IndexOf(message) + 1, message.RawValue, re));

      return searchResults;
    }

    /// <summary>
    /// Search entries of the string in the log line.
    /// </summary>
    /// <param name="lineNumber">Log line number (1-based).</param>
    /// <param name="line">Log line.</param>
    /// <param name="re">Regex.</param>
    /// <returns>Entries of the string in the log line.</returns>
    public List<SearchEntry> SearchStringInLine(int lineNumber, string line, Regex re)
    {
      var matches = re.Matches(line);
      return matches
        .Where(x => x.Success)
        .Select(x => new SearchEntry() { LineNumber = lineNumber, StartPosition = x.Index, RawString = line })
        .ToList();
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
