using NLog;
using System;
using VisualLog.Core;

namespace VisualLog.Capture.Search
{
  public class SearchByDateTimeOption : ISearchOption
  {
    internal static ILogger log = LogManager.GetCurrentClassLogger();

    public string FilePrefix { get; set; }
    public DateTime DateTime { get; set; }
    public bool FixMilliseconds { get; set; }

    public SearchByDateTimeOption(DateTime dateTime)
    {
      this.FilePrefix = "datetime";
      this.DateTime = dateTime;
      this.FixMilliseconds = false;
    }

    public override string ToString()
    {
      if (this.FixMilliseconds)
        return $"DateTime: {this.DateTime}";
      else
        return $"DateTime: [{this.DateTime.ToString("yyyy-MM-dd HH:mm:ss.fff")}, {this.DateTime.AddMilliseconds(999 - this.DateTime.Millisecond).ToString("yyyy-MM-dd HH:mm:ss.fff")}]";
    }

    public SearchResult Search(string filePath)
    {
      var from = this.DateTime;
      var to = this.DateTime;
      if (!this.FixMilliseconds)
        to = to.AddMilliseconds(999 - this.DateTime.Millisecond);

      var jlog = new JLog(filePath);
      jlog.Read();
      jlog.Parse();
      return new SearchResult()
      {
        FilePath = filePath,
        SearchOption = this,
        Result = jlog.GetBetween(from, to)
      };
    }
  }
}
