﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace VisualLog.Core
{
  public class Formats
  {
    public const string DefaultSource = @"VisualLog.Formats.json";

    public string Source { get; set; }

    private static Formats instance;

    public Formats()
    {
      this.Source = DefaultSource;
    }

    public static void InitFrom(string path = null)
    {
      if (instance == null)
        instance = new Formats();
      if (!string.IsNullOrWhiteSpace(path))
        instance.Source = path;
    }

    public static IEnumerable<Format> Read(string path = null)
    {
      InitFrom(path);
      return instance.Read();
    }

    public static void Write(Format format, string path = null)
    {
      InitFrom(path);
      instance.Write(format);
    }

    public IEnumerable<Format> Read()
    {
      if (this.Source == null ||
          !File.Exists(this.Source))
        return Enumerable.Empty<Format>();

      return JsonConvert.DeserializeObject<IEnumerable<Format>>(File.ReadAllText(this.Source));
    }

    public void Write(Format format)
    {
      var formats = this.Read().ToList();
      if (format.Id <= 0)
        format.Id = formats.Any() ? formats.Max(x => x.Id) + 1 : 1;
      var formatToRewrite = formats.FirstOrDefault(x => x.Id == format.Id);
      if (formatToRewrite != null)
        formats.Remove(formatToRewrite);
      formats.Add(format);
      File.WriteAllText(this.Source, JsonConvert.SerializeObject(formats, Formatting.Indented));
    }
  }
}
