using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Globalization;

namespace VisualLog.Core
{
  public class JMessage
  {
    public JObject JsonObject { get; set; }
    public DateTimeOffset? DateTime { get; set; }
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
        var settings = new JsonSerializerSettings() { DateParseHandling = DateParseHandling.None };
        this.JsonObject = JsonConvert.DeserializeObject<JObject>(RawValue, settings);
        this.Parsed = true;
        var tokenName = "t";
        var token = this.JsonObject.SelectToken(tokenName, false);
        if (token != null)
        {
          DateTimeOffset dt;
          if (DateTimeOffset.TryParseExact(token.Value<string>(), "yyyy-MM-dd HH:mm:ss.fffzzz", CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
            this.DateTime = dt;
        }
      }
      catch
      {
        this.Parsed = false;
      }

      return this.Parsed;
    }

    public bool MessageDateTimeBetween(DateTime from, DateTime to)
    {
      if (!this.Parsed || !this.DateTime.HasValue)
        return false;
      return this.DateTime.Value.DateTime >= from && this.DateTime.Value.DateTime <= to;
    }

    public void AddLogNamePropertyFirst(string logName)
    {
      if (this.JsonObject.ContainsKey("logName"))
        return;

      this.JsonObject.AddFirst(new JProperty("logName", logName));
    }
  }
}
