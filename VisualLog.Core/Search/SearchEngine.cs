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

      var messagesCount = log.Messages.Count;
      for (var index = 0; index < messagesCount; index++)
      {
        var message = log.Messages[index];
        var entry = Search(message, request);
        if (entry == null)
          continue;
        entry.LineNumber = index + 1;
        response.Entries.Add(entry);
      }

      return response;
    }

    public static SearchEntry Search(Message message, SearchRequest request)
    {
      if (message == null || request == null)
        return null;

      var entry = new SearchEntry(message);
      foreach (var statement in request.Statements)
      {
        var matches = statement.GetMatches(message);
        if (!matches.Any())
          continue;
        entry.Matches.AddRange(matches.Where(x => x.Success)
          .Select(x => new MatchEntry() { Index = x.Index, Length = x.Length })
          .ToList());
      }

      if (entry.Matches.Any())
        return entry;

      return null;
    }
  }
}
