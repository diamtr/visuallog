using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace VisualLog.Core
{
  public class JMessage
  {
    public JObject JsonObject { get; set; }
    public string RawValue { get; set; }
    public bool Parsed { get; private set; }

    public JMessage(string rawValue)
    {
      this.RawValue = rawValue;
    }

    public bool TryParse()
    {
      try
      {
        var settings = new JsonSerializerSettings()
        { 
          DateParseHandling = DateParseHandling.None,
          DateFormatString = "yyyy-MM-dd HH:mm:ssZ"
        };
        this.JsonObject = JsonConvert.DeserializeObject<JObject>(RawValue, settings);
        this.Parsed = true;
      }
      catch
      {
        this.Parsed = false;
      }

      return this.Parsed;
    }

    public bool MessageDateTimeBetween(DateTime from, DateTime to)
    {
      if (!this.Parsed)
        return false;
      var tokenName = "t";
      var token = this.JsonObject.SelectToken(tokenName, false);
      if (token == null)
        return false;
      var dateTime = token.Value<DateTime>();
      return dateTime >= from && dateTime <= to;
    }
  }
}
