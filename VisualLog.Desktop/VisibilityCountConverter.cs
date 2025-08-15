using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace VisualLog.Desktop
{
  public class VisibilityCountConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (targetType != typeof(Visibility))
        throw new InvalidOperationException("Converter can only convert to value of type Visibility.");

      if (value == null)
        return Visibility.Collapsed;

      var longValue = System.Convert.ToInt64(value, culture);
      if (Reverse)
        longValue = longValue == 0 ? 1 : 0;

      return longValue != 0 ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
      => throw new NotSupportedException();

    public Boolean Reverse { get; set; }
  }
}
