using System;
using System.Globalization;
using System.Windows.Data;

namespace VisualLog.Desktop.Converters
{
  /// <summary>
  /// Splits an encoding display string "{CodePage} {WebName} {EncodingName}"
  /// (see LogViewModel.GetEncodingDisplayName) into parts for compact UI.
  /// ConverterParameter: "Short" (default) - "UTF-8", "windows-1251";
  /// "Name" - "Unicode (UTF-8)"; "Details" - "65001 · utf-8".
  /// </summary>
  public class EncodingPartConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      return Format(value as string, parameter as string);
    }

    public static string Format(string encoding, string part)
    {
      if (string.IsNullOrWhiteSpace(encoding))
        return string.Empty;

      var parts = encoding.Split(new[] { ' ' }, 3);
      if (parts.Length < 3)
        return encoding;

      var codePage = parts[0];
      var webName = parts[1];
      var name = parts[2];

      switch (part)
      {
        case "Name":
          return name;
        case "Details":
          return $"{codePage} · {webName}";
        default:
          return webName.StartsWith("utf", StringComparison.OrdinalIgnoreCase) ||
                 webName.Equals("us-ascii", StringComparison.OrdinalIgnoreCase)
            ? webName.ToUpperInvariant()
            : webName;
      }
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
      => throw new NotSupportedException();
  }
}
