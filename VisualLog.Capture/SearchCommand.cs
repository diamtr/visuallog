using NLog;
using System;
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using VisualLog.Capture.Search;

namespace VisualLog.Capture
{
  public class SearchCommand : Command
  {
    internal static ILogger logger = LogManager.GetCurrentClassLogger();

    public SearchCommand(string name, string description) : base(name, description)
    {
      this.AddOption(new Option<string[]>(
        aliases: new[] { "--log" },
        description: "Log path (Multiple arguments allowed)")
        { AllowMultipleArgumentsPerToken = true });
      this.AddOption(new Option<string[]>(
        aliases: new[] { "--dir" },
        description: "Logs directory path (Multiple arguments allowed)")
        { AllowMultipleArgumentsPerToken = true });
      this.AddOption(new Option<string>(
        aliases: new[] { "--output" },
        description: "Search result output path"));
      this.AddOption(new Option<string[]>(
        aliases: new[] { "--tr" },
        description: "Search by trace (Multiple arguments allowed)")
        { AllowMultipleArgumentsPerToken = true });
      this.Handler = CommandHandler.Create(this.SearchCommandHandler);
    }

    public int SearchCommandHandler(string[] log, string[] dir, string output, string[] tr)
    {
      var pathBuilder = new PathBuilder();
      pathBuilder.WithWorkingDirectory(Environment.CurrentDirectory);
      pathBuilder.WithFiles(log);
      pathBuilder.WithDirectories(dir);
      pathBuilder.WithOutput(output);
      pathBuilder.Build();

      var searchEngine = new SearchEngine();
      searchEngine.PathBuilder = pathBuilder;
      searchEngine.StoreResultsStrategy = new SplitFilesMergeOptionsStoreResultsStrategy();
      searchEngine.WithOption(new SearchByTracesOption(tr));

      searchEngine.Search();
      searchEngine.StoreSearchResults();

      return 0;
    }

    public static Command GetCommand()
    {
      return new SearchCommand("search", "Search in logs");
    }
  }
}
