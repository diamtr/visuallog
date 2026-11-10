using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace VisualLog.Desktop.Settings
{
  /// <summary>Application level settings, stored as JSON. New settings go here as new properties.</summary>
  public class AppSettings
  {
    private static readonly NLog.Logger log = NLog.LogManager.GetCurrentClassLogger();

    public const string FileName = @"settings.json";

    public List<HighlightRule> HighlightRules { get; set; } = new List<HighlightRule>();

    /// <summary>Default location, next to the application like recent.json.</summary>
    public static string DefaultPath => Path.Combine(AppContext.BaseDirectory, FileName);

    /// <summary>Reads settings; a missing or broken file gives default settings.</summary>
    public static AppSettings Load(string path)
    {
      try
      {
        if (File.Exists(path))
        {
          var settings = JsonConvert.DeserializeObject<AppSettings>(File.ReadAllText(path));
          if (settings != null)
          {
            settings.HighlightRules ??= new List<HighlightRule>();
            settings.HighlightRules.RemoveAll(x => x == null);
            return settings;
          }
        }
      }
      catch (Exception ex)
      {
        log.Error(ex, $"Settings could not be read from '{path}', defaults are used");
      }
      return new AppSettings();
    }

    /// <summary>Writes settings; returns false if the file could not be written.</summary>
    public bool Save(string path)
    {
      try
      {
        File.WriteAllText(path, JsonConvert.SerializeObject(this, Formatting.Indented));
        return true;
      }
      catch (Exception ex)
      {
        log.Error(ex, $"Settings could not be saved to '{path}'");
        return false;
      }
    }
  }
}
