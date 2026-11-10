using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace VisualLog.Desktop.LogManager
{
  /// <summary>
  /// Interaction logic for LogManagerView.xaml
  /// </summary>
  public partial class LogManagerView : UserControl
  {
    public LogManagerView()
    {
      InitializeComponent();
    }

    private void TabsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      // Keep the active tab visible when it is switched from the keyboard or the dashboard
      if (this.TabsList.SelectedItem != null)
        this.TabsList.ScrollIntoView(this.TabsList.SelectedItem);
    }

    private void TabsList_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
      // The tab strip scrolls horizontally only, so map the wheel onto it
      var scrollViewer = FindDescendant<ScrollViewer>(this.TabsList);
      if (scrollViewer == null || scrollViewer.ScrollableWidth <= 0)
        return;
      scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset - e.Delta / 2.0);
      e.Handled = true;
    }

    private static T FindDescendant<T>(System.Windows.DependencyObject root) where T : System.Windows.DependencyObject
    {
      if (root == null) return null;
      for (int i = 0; i < VisualTreeHelper.GetChildrenCount(root); i++)
      {
        var child = VisualTreeHelper.GetChild(root, i);
        if (child is T match) return match;
        var nested = FindDescendant<T>(child);
        if (nested != null) return nested;
      }
      return null;
    }
  }
}
