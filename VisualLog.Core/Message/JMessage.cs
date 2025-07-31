using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Globalization;
using System.Linq;

namespace VisualLog.Core
{
  public class JMessage : MessageBase
  {
    public JObject JsonObject { get; set; }
    public DateTimeOffset? DateTime { get; set; }
    public string Trace { get; set; }
    public bool Parsed { get; private set; }

    public JMessage(string rawValue) : base(rawValue) { }

    public override void Read()
    {
      var settings = new JsonSerializerSettings() { DateParseHandling = DateParseHandling.None };
      this.JsonObject = JsonConvert.DeserializeObject<JObject>(RawValue, settings);
      this.Items = this.JsonObject.Properties().ToDictionary(x => x.Name, x => (object)x.Value);
      this.FillDateTime();
      this.FillTrace();
    }

    public bool TryParse()
    {
      try
      {
        this.Read();
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

    public void AddTimeDeltaPropertyFirst()
    {
      if (this.JsonObject.ContainsKey("td"))
        return;

      this.JsonObject.AddFirst(new JProperty("td", 0));
    }

    public void CalculateTimeDeltaFrom(JMessage message)
    {
      var timedelta = this.DateTime - message.DateTime;
      var token = this.JsonObject.SelectToken("td", false);
      if (token != null && timedelta.HasValue)
      {
        var prop = token.Parent as JProperty;
        if (prop != null)
          prop.Value = (int)timedelta.Value.TotalMilliseconds;
      }
    }
  }
}
