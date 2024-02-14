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
    public string Trace { get; set; }
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
        this.FillDateTime();
        this.FillTrace();
        this.Parsed = true;
      }
      catch
      {
        this.Parsed = false;
      }

      return this.Parsed;
    }

    public void FillDateTime()
    {
      var timeTokenName = "t";
      var token = this.JsonObject.SelectToken(timeTokenName, false);
      if (token != null)
      {
        DateTimeOffset dt;
        if (DateTimeOffset.TryParseExact(token.Value<string>(), "yyyy-MM-dd HH:mm:ss.fffzzz", CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
          this.DateTime = dt;
      }
    }

    public void FillTrace()
    {
      var timeTokenName = "tr";
      var token = this.JsonObject.SelectToken(timeTokenName, false);
      if (token != null)
        this.Trace = token.Value<string>();
    }

    public bool MessageDateTimeBetween(DateTime? from, DateTime? to)
    {
      if (!this.Parsed || !this.DateTime.HasValue)
        return false;
      return (!from.HasValue || this.DateTime.Value.DateTime >= from) &&
             (!to.HasValue || this.DateTime.Value.DateTime <= to);
    }

    public void AddLogNamePropertyFirst(string logName)
    {
      if (this.JsonObject.ContainsKey("logName"))
        return;

      this.JsonObject.AddFirst(new JProperty("logName", logName));
    }
  }
}
