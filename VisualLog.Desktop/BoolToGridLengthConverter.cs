using System.Globalization;
using System.Windows.Data;
using System.Windows;
using System;

namespace VisualLog.Desktop
{
  public class BoolToGridLengthConverter : IValueConverter
  {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
      if (targetType != typeof(GridLength))
        throw new InvalidOperationException("Converter can only convert to value of type 'GridLength'.");
      bool isVisible = (bool)value;
      return isVisible ? new GridLength(1, GridUnitType.Star) : new GridLength(0);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
      => throw new NotSupportedException();
  }
}
