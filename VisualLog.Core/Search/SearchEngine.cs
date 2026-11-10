using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace VisualLog.Core.Search
{
  public class SearchEngine
  {
    private const int CancellationCheckInterval = 4096;

    public static SearchResponse Search(Log log, SearchRequest request)
    {
      if (log == null)
        return new SearchResponse();

      var response = Search(log.Messages, request);
      response.LogPath = log.SourceFilePath;
      return response;
    }

    /// <summary>
    /// Searches the given messages. Line numbers are index + 1 within <paramref name="messages"/>;
    /// null messages are skipped. Only reads the list, so it can run on a background thread
    /// as long as the list is not modified concurrently.
    /// </summary>
    public static SearchResponse Search(IReadOnlyList<Message> messages, SearchRequest request, CancellationToken cancellationToken = default)
    {
      cancellationToken.ThrowIfCancellationRequested();

      var response = new SearchResponse();
      if (messages == null || request == null)
        return response;

      var messagesCount = messages.Count;
      for (var index = 0; index < messagesCount; index++)
      {
        if (index % CancellationCheckInterval == 0)
          cancellationToken.ThrowIfCancellationRequested();

        var message = messages[index];
        if (message == null)
          continue;

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
        entry.Matches.AddRange(statement.GetMatches(message)
          .Where(x => x.Success)
          .Select(x => new MatchEntry() { Index = x.Index, Length = x.Length }));
      }

      if (entry.Matches.Count == 0)
        return null;

      if (request.Statements.Count > 1)
      {
        // Stable sort so equal indexes keep statement order.
        var sorted = entry.Matches.OrderBy(x => x.Index).ToList();
        entry.Matches.Clear();
        entry.Matches.AddRange(sorted);
      }

      return entry;
    }
  }
}
