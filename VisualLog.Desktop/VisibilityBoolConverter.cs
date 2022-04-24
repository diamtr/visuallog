using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace VisualLog.Desktop
{
  public class VisibilityBoolConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (targetType != typeof(Visibility))
        throw new InvalidOperationException("Converter can only convert to value of type Visibility.");

      if (value == null)
        return Visibility.Collapsed;

      var isRunning = System.Convert.ToBoolean(value, culture);
      if (Reverse)
        isRunning = !isRunning;

      return isRunning ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
      throw new InvalidOperationException("Converter cannot convert back.");
    }

    public Boolean Reverse { get; set; }
  }
}
