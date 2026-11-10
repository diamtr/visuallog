using System;
using System.Globalization;
using System.Windows.Data;

namespace VisualLog.Desktop.Converters
{
  /// <summary>
  /// Formats a moment in the past the way modern apps do: "Just now", "5 min ago", "Yesterday, 14:30"...
  /// </summary>
  public class RelativeTimeConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (value is not DateTime time || time == default)
        return string.Empty;
      return Format(time, DateTime.Now);
    }

    public static string Format(DateTime time, DateTime now)
    {
      var inv = CultureInfo.InvariantCulture;
      var delta = now - time;

      if (delta < TimeSpan.FromMinutes(1))
        return "Just now";
      if (delta < TimeSpan.FromHours(1))
        return $"{(int)delta.TotalMinutes} min ago";
      if (time.Date == now.Date)
        return "Today, " + time.ToString("HH:mm", inv);
      if (time.Date == now.Date.AddDays(-1))
        return "Yesterday, " + time.ToString("HH:mm", inv);
      if (delta < TimeSpan.FromDays(7))
        return time.ToString("ddd, HH:mm", inv);
      if (time.Year == now.Year)
        return time.ToString("d MMM", inv);
      return time.ToString("d MMM yyyy", inv);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
      => throw new NotSupportedException();
  }
}
