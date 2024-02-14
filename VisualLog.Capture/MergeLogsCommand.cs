using Newtonsoft.Json;
using NLog;
using System;
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using System.CommandLine.Parsing;
using System.IO;
using System.Linq;
using VisualLog.Core;

namespace VisualLog.Capture
{
  public class MergeLogsCommand : Command
  {
    internal ILogger log = LogManager.GetCurrentClassLogger();

    public MergeLogsCommand(string name, string description) : base(name, description)
    {
      this.AddOption(new Option<string[]>(
        aliases: new[] { "--path", "-p" },
        description: "Log path")
      { AllowMultipleArgumentsPerToken = true });
      this.AddOption(new Option<string>(
        aliases: new[] { "--destination", "-d" },
        description: "Composite log destination path"));
      this.AddOption(new Option<DateTime?>(
        aliases: new[] { "--from", "-f" },
        parseArgument: arg => this.DateTimeArgumentParser(arg),
        description: "From date / datetime"));
      this.AddOption(new Option<DateTime?>(
        aliases: new[] { "--to", "-t" },
        parseArgument: arg => this.DateTimeArgumentParser(arg),
        description: "To date / datetime"));
      this.Handler = CommandHandler.Create(this.MergeCommandHendler);
    }

    /// <summary>
    /// Сконвертировать параметр командной строки в дату.
    /// </summary>
    /// <param name="arg">Параметр командной строки.</param>
    /// <returns>Дата.</returns>
    internal DateTime DateTimeArgumentParser(ArgumentResult arg)
    {
      // Парсер нужен для расширения возможных форматов дат.
      var value = arg.Tokens[0].Value;
      return DateTime.Parse(value);
    }

    internal int MergeCommandHendler(string[] path, string destination, DateTime? from, DateTime? to)
    {
      log.Debug($"Call MergeCommandHendler. Paths: {string.Join(", ", path)} From {from} To {to}");

      var result = new JLog();

      foreach (var logPath in path)
      {
        var fileName = new FileInfo(logPath).Name;
        var jlog = new JLog(logPath);
        log.Debug($"Reading: {jlog.SourceFilePath}");
        jlog.Read();
        log.Debug($"Parsing {jlog.SourceFilePath}");
        jlog.Parse();
        log.Debug($"Search in {jlog.SourceFilePath}");
        var messages = jlog.GetBetween(from, to);
        foreach (var message in messages)
          message.AddLogNamePropertyFirst(fileName);
        result.Messages.AddRange(messages);
      }

      log.Debug("Build composite log");
      result.OrderMessagesByDateTime();
      log.Debug($"Writing: {destination}");
      File.WriteAllLines(destination, result.Messages.Select(x => JsonConvert.SerializeObject(x.JsonObject)));

      return 0;
    }

    public static Command GetCommand()
    {
      return new MergeLogsCommand("merge", "Merge logs");
    }
  }
}
