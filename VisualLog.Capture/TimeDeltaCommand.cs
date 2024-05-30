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
    internal static ILogger logger = LogManager.GetCurrentClassLogger();

    public TimeDeltaCommand(string name, string description) : base(name, description)
    {
      this.AddOption(new Option<string>(
        aliases: new[] { "--log" },
        description: "Log path"));
      this.Handler = CommandHandler.Create(this.TimeDeltaCommandHandler);
    }

    public int TimeDeltaCommandHandler(string log)
    {
      logger.Debug($"Call TimeDeltaCommandHandler. Path {log}");
      var fileInfo = new FileInfo(log);
      if (!new FileInfo(log).Exists && !new DirectoryInfo(log).Exists)
      {
        logger.Error($"Does not exist. {log}");
        return -1;
      }

      var filePaths = new List<string>();
      if (!fileInfo.Attributes.HasFlag(FileAttributes.Directory))
        filePaths.Add(log);
      else
        filePaths.AddRange(new DirectoryInfo(log).EnumerateFiles().Select(x => x.FullName));

      foreach (var filePath in filePaths)
      {
        var jlog = new JLog(filePath);
        logger.Debug($"Reading {jlog.SourceFilePath}");
        jlog.Read();
        logger.Debug($"Parsing {jlog.SourceFilePath}");
        jlog.Parse();
        logger.Debug($"CalculateTimeDelta {jlog.SourceFilePath}");
        jlog.CalculateTimeDelta();
        logger.Debug($"Save {jlog.SourceFilePath}");
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
