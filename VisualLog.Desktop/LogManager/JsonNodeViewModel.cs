using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace VisualLog.Desktop.LogManager
{
  public enum JsonNodeKind
  {
    Object,
    Array,
    String,
    Number,
    Boolean,
    Null,
    Other,
  }

  /// <summary>A field of a JSON object or an item of a JSON array, with its nested fields or items.</summary>
  public class JsonNodeViewModel : ViewModelBase
  {
    /// <summary>Name of the field; "[index]" for an item of an array.</summary>
    public string Name { get; }
    public JsonNodeKind Kind { get; }
    /// <summary>A simple value as text (a string without quotes and escapes); the size of an object or an array.</summary>
    public string Value { get; }
    /// <summary>Text copied as the value: a simple value as shown, an object or an array as indented JSON.</summary>
    public string ValueText { get; }
    public IReadOnlyList<JsonNodeViewModel> Children { get; }

    public bool IsExpanded
    {
      get { return this.isExpanded; }
      set
      {
        if (this.isExpanded == value)
          return;
        this.isExpanded = value;
        this.OnPropertyChanged();
      }
    }
    private bool isExpanded;

    public Command CopyValueCommand { get; }
    public Command CopyNameCommand { get; }

    private JsonNodeViewModel(string name, JsonNodeKind kind, string value, string valueText,
                              IReadOnlyList<JsonNodeViewModel> children, Action<string> copy)
    {
      this.Name = name;
      this.Kind = kind;
      this.Value = value;
      this.ValueText = valueText;
      this.Children = children;
      this.CopyValueCommand = new Command(x => copy?.Invoke(this.ValueText));
      this.CopyNameCommand = new Command(x => copy?.Invoke(this.Name));
    }

    /// <param name="copy">Puts a text into the clipboard.</param>
    public static JsonNodeViewModel From(string name, JToken token, Action<string> copy)
    {
      var none = Array.Empty<JsonNodeViewModel>();
      switch (token)
      {
        case JObject obj:
          var fields = obj.Properties().Select(x => From(x.Name, x.Value, copy)).ToList();
          return new JsonNodeViewModel(name, JsonNodeKind.Object, Size(fields.Count, "{ }", "field"),
                                       obj.ToString(Formatting.Indented), fields, copy);
        case JArray array:
          var items = array.Select((x, i) => From($"[{i}]", x, copy)).ToList();
          return new JsonNodeViewModel(name, JsonNodeKind.Array, Size(items.Count, "[ ]", "item"),
                                       array.ToString(Formatting.Indented), items, copy);
        default:
          var kind = KindOf(token.Type);
          var value = kind == JsonNodeKind.String ? token.Value<string>() : token.ToString(Formatting.None);
          return new JsonNodeViewModel(name, kind, value, value, none, copy);
      }
    }

    private static string Size(int count, string empty, string unit)
    {
      if (count == 0)
        return empty;
      return $"{count} {unit}{(count == 1 ? "" : "s")}";
    }

    private static JsonNodeKind KindOf(JTokenType type)
    {
      switch (type)
      {
        case JTokenType.String:
        case JTokenType.Date:
        case JTokenType.Guid:
        case JTokenType.Uri:
        case JTokenType.TimeSpan:
          return JsonNodeKind.String;
        case JTokenType.Integer:
        case JTokenType.Float:
          return JsonNodeKind.Number;
        case JTokenType.Boolean:
          return JsonNodeKind.Boolean;
        case JTokenType.Null:
        case JTokenType.Undefined:
          return JsonNodeKind.Null;
        default:
          return JsonNodeKind.Other;
      }
    }
  }
}
