using NLog;
using System;
using System.Text.RegularExpressions;
using VisualLog.Core;

namespace VisualLog.Capture.Search
{
  public class SearchByDateTimeOption : ISearchOption
  {
    public const string TimeIntervalPattern = @"(?'value'[\+\-]?\d{1,4})([h,m,s,ms])";
    internal static ILogger log = LogManager.GetCurrentClassLogger();

    public string FilePrefix { get; set; }
    public DateTime DateTime { get; set; }
    public string TimeInterval { get; set; }

    private DateTime from;
    private DateTime to;

    public SearchByDateTimeOption(DateTime dateTime, string timeinterval)
    {
      this.FilePrefix = "datetime";
      this.WithDateTime(dateTime);
      this.WithTimeInterval(timeinterval);
    }

    public override string ToString()
    {
      return $"DateTime: [{this.from.ToString("yyyy-MM-dd HH:mm:ss.fff")}, {this.to.ToString("yyyy-MM-dd HH:mm:ss.fff")}]";
    }

    public void WithDateTime(DateTime dateTime)
    {
      this.DateTime = dateTime;
      this.from = this.DateTime;
      this.to = this.DateTime;
    }

    public void WithTimeInterval(string timeinterval)
    {
      if (string.IsNullOrWhiteSpace(timeinterval) || !Regex.IsMatch(timeinterval, TimeIntervalPattern))
      {
        this.to = this.to.AddMilliseconds(999 - this.DateTime.Millisecond);
        return;
      }

      this.TimeInterval = timeinterval;
      var numberGroup = Regex.Match(timeinterval, TimeIntervalPattern).Groups["value"];
      int intervalValue;
      if (!int.TryParse(numberGroup.Value, out intervalValue))
        return;
      var intervalMeasure = timeinterval.Substring(numberGroup.Length);
      switch (intervalMeasure)
      {
        case "h":
          if (intervalValue >= 0)
            this.to = this.to.AddHours(intervalValue);
          else
            this.from = this.from.AddHours(intervalValue);
          break;
        case "m":
          if (intervalValue >= 0)
            this.to = this.to.AddMinutes(intervalValue);
          else
            this.from = this.from.AddMinutes(intervalValue);
          break;
        case "s":
          if (intervalValue >= 0)
            this.to = this.to.AddSeconds(intervalValue);
          else
            this.from = this.from.AddSeconds(intervalValue);
          break;
        case "ms":
          if (intervalValue >= 0)
            this.to = this.to.AddMilliseconds(intervalValue);
          else
            this.from = this.from.AddMilliseconds(intervalValue);
          break;
        default:
          break;
      }
    }

    public SearchResult Search(string filePath)
    {
      var jlog = new JLog(filePath);
      jlog.Read();
      jlog.Parse();
      return new SearchResult()
      {
        FilePath = filePath,
        SearchOption = this,
        Result = jlog.GetBetween(this.from, this.to)
      };
    }
  }
}
