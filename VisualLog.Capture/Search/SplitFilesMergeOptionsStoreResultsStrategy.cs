using NLog;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using VisualLog.Core;

namespace VisualLog.Capture.Search
{
  public class SplitFilesMergeOptionsStoreResultsStrategy : IStoreResultsStrategy
  {
    internal static ILogger log = LogManager.GetCurrentClassLogger();

    public void Store(PathBuilder pathBuilder, IEnumerable<SearchResult> searchResults)
    {
      log.Info("[ Results ]");
      var fileGroups = searchResults.GroupBy(x => x.FilePath);
      foreach (var fileGroup in fileGroups)
      {
        var fileName = Path.GetFileName(fileGroup.Key);
        var prefix = string.Join("-", fileGroup.GroupBy(x => x.SearchOption).Select(x => x.Key.FilePrefix));
        var messages = new List<JMessage>();
        foreach (var groupItem in fileGroup)
          messages.AddRange(groupItem.Result);
        messages = JMessage.Distinct(messages);
        if (!messages.Any())
          continue;

        var outFilePath = Path.Combine(pathBuilder.Output, $"{prefix}-{fileName}");
        File.WriteAllLines(outFilePath, messages.Select(x => x.RawValue));
        log.Info($"{outFilePath}");
      }
       if (!searchResults.SelectMany(x => x.Result).Where(x => !string.IsNullOrWhiteSpace(x.RawValue)).Any())
        log.Info("Not found");
    }
  }
}
