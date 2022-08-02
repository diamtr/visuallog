using System.Collections.Generic;
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

    public static IEnumerable<LogFormat> Read(string sourcePath = null)
    {
      var instance = new Formats();
      if (!string.IsNullOrEmpty(sourcePath))
        instance.Source = sourcePath;
      return instance.Read();
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
