using Newtonsoft.Json;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using VisualLog.Core;

namespace VisualLog.Capture
{
  class Program
  {
    static void Main(string[] args)
    {
      var from = DateTime.Parse("2023-07-21 12:03:37.352");
      var to = DateTime.Parse("2023-07-21 12:03:38.218");
      var dirPath = @"xxxxx";
      var dir = new DirectoryInfo(dirPath);
      var result = new JLog();
      foreach (var fileInfo in dir.EnumerateFiles())
      {
        var log = new JLog(fileInfo.FullName);
        Console.WriteLine($"Reading: {log.SourceFilePath}");
        log.Read();
        Console.WriteLine($"Parsing...");
        log.Parse();
        Console.WriteLine($"Get between [{from}; {to}]");
        var messages = log.GetBetween(from, to);
        foreach (var message in messages)
          message.AddLogNamePropertyFirst(fileInfo.Name);
        Console.WriteLine($"Found: {messages.Count()}");
        result.Messages.AddRange(messages);
      }
      result.OrderMessagesByDateTime();
      var dest = "D:\\between.log";
      Console.WriteLine($"Writing: {dest}");
      File.WriteAllLines(dest, result.Messages.Select(x => JsonConvert.SerializeObject(x.JsonObject)));
    }
  }
}
