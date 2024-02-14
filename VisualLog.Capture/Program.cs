using NLog;
using System;
using System.CommandLine;

namespace VisualLog.Capture
{
  class Program
  {
    internal static ILogger log => LogManager.GetCurrentClassLogger();

    static int Main(string[] args)
    {
      var exitCode = 0;
      try
      {
        var rootCommand = new RootCommand();
        rootCommand.Add(MergeLogsCommand.GetCommand());
        rootCommand.Add(SearchCommand.GetCommand());
        rootCommand.Add(TimeDeltaCommand.GetCommand());
        exitCode = rootCommand.Invoke(args);
      }
      catch (Exception ex)
      {
        log.Error(ex, @"Error occured");
        exitCode = -1;
      }
      return exitCode;
    }
  }
}
