﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace VisualLog.Core
{
  public class Formats
  {
    public string Source { get; set; }

    public Formats()
    {
      this.Source = "VisualLog.Formats.json";
    }

    public IEnumerable<LogFormat> Read()
    {
      if (this.Source == null ||
          !File.Exists(this.Source))
        return Enumerable.Empty<LogFormat>();

      return JsonConvert.DeserializeObject<IEnumerable<LogFormat>>(File.ReadAllText(this.Source));
    }
  }
}
