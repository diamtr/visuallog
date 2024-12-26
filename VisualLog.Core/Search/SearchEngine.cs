using System.Linq;

namespace VisualLog.Core.Search
{
  public class SearchEngine
  {
    public static SearchResponse Search(Log log, SearchRequest request)
    {
      var response = new SearchResponse();
      if (log == null)
        return response;

      response.LogPath = log.SourceFilePath;
      foreach (var message in log.Messages)
      {
        var entry = Search(message, request);
        if (entry == null)
          continue;
        entry.LineNumber = log.Messages.IndexOf(message) + 1; // Lines numbering is 1-based.
        response.Entries.Add(entry);
      }

      return response;
    }

    public static SearchEntry Search(Message message, SearchRequest request)
    {
      if (message == null || request == null)
        return null;

      var entry = new SearchEntry() { RawString = message.RawValue };
      foreach (var statement in request.Statements)
      {
        var matches = statement.GetMatches(message);
        if (!matches.Any())
          continue;
        entry.Matches.AddRange(matches.Where(x => x.Success)
          .Select(x => new Match() { Index = x.Index, Length = x.Length })
          .ToList());
      }

      if (entry.Matches.Any())
        return entry;

      return null;
    }
  }
}
