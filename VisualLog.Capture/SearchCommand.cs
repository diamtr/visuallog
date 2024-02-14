using Newtonsoft.Json;
using NLog;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using System.IO;
using System.Linq;
using VisualLog.Core;

namespace VisualLog.Capture
{
  public class SearchCommand : Command
  {
    internal static ILogger log = LogManager.GetCurrentClassLogger();

    public SearchCommand(string name, string description) : base(name, description)
    {
      this.AddOption(new Option<string>(
        aliases: new[] { "--path", "-p" },
        description: "Log path"));
      this.AddOption(new Option<string>(
        aliases: new[] { "--destination", "-d" },
        description: "Search result log destination path"));
      this.AddOption(new Option<string[]>(
        aliases: new[] { "--trace", "-t" },
        description: "Search trace")
        { AllowMultipleArgumentsPerToken = true });
      this.Handler = CommandHandler.Create(this.SearchCommandHandler);
    }

    public int SearchCommandHandler(string path, string destination, string[] trace)
    {
      log.Debug($"Call SearchCommandHandler. Path: {path} Trace {string.Join(", ", trace)}");

      var fileInfo = new FileInfo(path);

      if (!new FileInfo(path).Exists && !new DirectoryInfo(path).Exists)
      {
        log.Error($"Does not exist. {path}");
        return -1;
      }

      var filePaths = new List<string>();
      if (!fileInfo.Attributes.HasFlag(FileAttributes.Directory))
        filePaths.Add(path);
      else
        filePaths.AddRange(new DirectoryInfo(path).EnumerateFiles().Select(x => x.FullName));

      var foundInFiles = 0;
      foreach (var filePath in filePaths)
      {
        var result = new JLog();
        var jlog = new JLog(filePath);
        log.Debug($"Reading: {jlog.SourceFilePath}");
        jlog.Read();
        log.Debug($"Parsing {jlog.SourceFilePath}");
        jlog.Parse();
        log.Debug($"Search in {jlog.SourceFilePath}");
        result.Messages.AddRange(jlog.GetByTrace(trace));

        if (!result.Messages.Any())
        {
          log.Info($"Not found. Traces [{string.Join(", ", trace)}] Log {filePath}");
          continue;
        }

        var destinationFileInfo = new FileInfo(destination);
        if (destinationFileInfo.Exists)
        {
          var pathTokens = destination.Split('.', System.StringSplitOptions.None);
          var secondLastToken = pathTokens[pathTokens.Length - 2];
          secondLastToken = $"{secondLastToken}_{foundInFiles}";
          pathTokens[pathTokens.Length - 2] = secondLastToken;
          destination = string.Join('.', pathTokens);
        }
        log.Debug($"Writing: {destination}");
        File.WriteAllLines(destination, result.Messages.Select(x => JsonConvert.SerializeObject(x.JsonObject)));

        foundInFiles++;
      }

      return 0;
    }

    public static Command GetCommand()
    {
      return new SearchCommand("search", "Search in log");
    }
  }
}
