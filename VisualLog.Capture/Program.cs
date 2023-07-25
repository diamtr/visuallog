using System;
using System.IO;
using System.Linq;
using VisualLog.Core;

namespace VisualLog.Capture
{
  class Program
  {
    static void Main(string[] args)
    {
      var from = new DateTime(2023, 1, 13, 0, 0, 2);
      var to = new DateTime(2023, 1, 13, 0, 0, 3);
      var log = new JLog("xxxxxxx");
      Console.WriteLine($"Reading: {log.SourceFilePath}");
      log.Read();
      Console.WriteLine($"Parsing...");
      log.Parse();
      Console.WriteLine($"Get between [{from}; {to}]");
      var messages = log.GetBetween(from, to); 
      Console.WriteLine($"Found: {messages.Count()}");
      var dest = "D:\\between.log";
      Console.WriteLine($"Writing: {dest}");
      File.WriteAllLines(dest, messages.Select(x => x.RawValue));
    }
  }
}
