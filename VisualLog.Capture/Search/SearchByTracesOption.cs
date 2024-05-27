using NLog;
using System.Collections.Generic;
using VisualLog.Core;

namespace VisualLog.Capture.Search
{
  public class SearchByTracesOption : ISearchOption
  {
    internal static ILogger log = LogManager.GetCurrentClassLogger();

    public string FilePrefix { get; set; }
    public List<string> Traces { get; set; }

    public SearchByTracesOption(IEnumerable<string> traces)
    {
      this.FilePrefix = "tr";
      this.Traces = new List<string>();
      this.Traces.AddRange(traces);
    }

    public override string ToString()
    {
      return $"Traces: [{string.Join(", ", this.Traces)}]";
    }

    public SearchResult Search(string filePath)
    {
      var jlog = new JLog(filePath);
      jlog.Read();
      jlog.Parse();
      return new SearchResult() {
        FilePath = filePath,
        SearchOption = this,
        Result = jlog.GetByTrace(this.Traces.ToArray())
      };
    }
  }
}
