using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using VisualLog.Core;

namespace VisualLog.Capture
{
  class Program
  {
    static void Main(string[] args)
    {
      if (args.Length < 3)
      {
        Console.WriteLine("Arguments error");
        return;
      }

      var logPath1 = args[0];
      var logPath2 = args[1];
      var resultLog = args[2];
      DateTime? from = null;
      DateTime? to = null;
      List<JMessage> messages1 = new List<JMessage>();
      List<JMessage> messages2 = new List<JMessage>();

      if (args.Length > 3)
        from = DateTime.Parse(args[3]); //"2023-07-21 12:03:37.352");
      if (args.Length > 4)
        to = DateTime.Parse(args[4]); // "2023-07-21 12:03:37.352");

      var result = new JLog();

      var fileName1 = new FileInfo(logPath1).Name;
      var log1 = new JLog(logPath1);
      Console.WriteLine($"Reading: {log1.SourceFilePath}");
      log1.Read();
      Console.WriteLine($"Parsing...");
      log1.Parse();
      if (from.HasValue && to.HasValue)
      {
        Console.WriteLine($"Get between [{from}; {to}]");
        messages1 = log1.GetBetween(from.Value, to.Value).ToList();
      }
      else
      {
        messages1 = log1.Messages;
      }
      foreach (var message in messages1)
        message.AddLogNamePropertyFirst(fileName1);

      var fileName2 = new FileInfo(logPath2).Name;
      var log2 = new JLog(logPath2);
      Console.WriteLine($"Reading: {log2.SourceFilePath}");
      log2.Read();
      Console.WriteLine($"Parsing...");
      log2.Parse();
      if (from.HasValue && to.HasValue)
      {
        Console.WriteLine($"Get between [{from}; {to}]");
        messages2 = log2.GetBetween(from.Value, to.Value).ToList();
      }
      else
      {
        messages2 = log2.Messages;
      }
      foreach (var message in messages2)
        message.AddLogNamePropertyFirst(fileName2);

      result.Messages = messages1.Union(messages2).ToList();
      result.OrderMessagesByDateTime();
      Console.WriteLine($"Writing: {resultLog}");
      File.WriteAllLines(resultLog, result.Messages.Select(x => JsonConvert.SerializeObject(x.JsonObject)));
    }
  }
}
