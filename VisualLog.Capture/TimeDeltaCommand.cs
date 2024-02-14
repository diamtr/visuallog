using NLog;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using System.IO;
using System.Linq;
using VisualLog.Core;

namespace VisualLog.Capture
{
  public class TimeDeltaCommand : Command
  {
    internal static ILogger log = LogManager.GetCurrentClassLogger();

    public TimeDeltaCommand(string name, string description) : base(name, description)
    {
      this.AddOption(new Option<string>(
        aliases: new[] { "--path", "-p" },
        description: "Log path"));
      this.Handler = CommandHandler.Create(this.TimeDeltaCommandHandler);
    }

    public int TimeDeltaCommandHandler(string path)
    {
      log.Debug($"Call TimeDeltaCommandHandler. Path {path}");
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

      foreach (var filePath in filePaths)
      {
        var jlog = new JLog(filePath);
        log.Debug($"Reading {jlog.SourceFilePath}");
        jlog.Read();
        log.Debug($"Parsing {jlog.SourceFilePath}");
        jlog.Parse();
        log.Debug($"CalculateTimeDelta {jlog.SourceFilePath}");
        jlog.CalculateTimeDelta();
        log.Debug($"Save {jlog.SourceFilePath}");
        jlog.Save();
      }

      return 0;
    }

    public static Command GetCommand()
    {
      return new TimeDeltaCommand("timedelta", "Isert field into each log file line indicated how many time goes after previous line in milliseconds");
    }
  }
}
