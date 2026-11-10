using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace VisualLog.Desktop.LogManager
{
  /// <summary>A line that is a JSON object by itself: shown as a tree of its fields or as indented JSON.</summary>
  public class JsonMessageDetailsViewModel : MessageDetailsViewModel
  {
    /// <summary>Fields of the object, in the order of the line.</summary>
    public IReadOnlyList<JsonNodeViewModel> Fields { get; }
    /// <summary>The object as indented JSON.</summary>
    public string IndentedText { get; }

    /// <summary>The fields are shown as a tree; otherwise as indented JSON.</summary>
    public bool IsTreeView
    {
      get { return this.isTreeView; }
      set
      {
        if (this.isTreeView == value)
          return;
        this.isTreeView = value;
        this.OnPropertyChanged();
        this.OnPropertyChanged(nameof(this.IsTextView));
      }
    }
    private bool isTreeView = true;

    public bool IsTextView
    {
      get { return !this.IsTreeView; }
      set { this.IsTreeView = !value; }
    }

    public JsonMessageDetailsViewModel(string rawText, int lineNumber, JObject json) : base(rawText, lineNumber)
    {
      Action<string> copy = text => this.SetClipboardText(text);
      this.Fields = json.Properties().Select(x => JsonNodeViewModel.From(x.Name, x.Value, copy)).ToList();
      this.IndentedText = json.ToString(Formatting.Indented);
    }

    protected override string CopyText => this.IsTextView ? this.IndentedText : this.RawText;

    /// <summary>The line as a JSON object; null if the line is not exactly one JSON object.</summary>
    public static JObject TryParse(string text)
    {
      if (string.IsNullOrWhiteSpace(text) || text.TrimStart()[0] != '{')
        return null;
      // Decimal keeps numbers as written (1.10 stays 1.10); a number out of its range is read as double
      return TryParse(text, FloatParseHandling.Decimal) ?? TryParse(text, FloatParseHandling.Double);
    }

    private static JObject TryParse(string text, FloatParseHandling floatParseHandling)
    {
      try
      {
        using var reader = new JsonTextReader(new StringReader(text))
        {
          // Dates stay strings as written
          DateParseHandling = DateParseHandling.None,
          FloatParseHandling = floatParseHandling,
        };
        var json = JObject.Load(reader);
        // Anything after the object: not a JSON line
        return reader.Read() ? null : json;
      }
      catch (Exception ex) when (ex is JsonException || ex is OverflowException || ex is FormatException)
      {
        return null;
      }
    }
  }
}
