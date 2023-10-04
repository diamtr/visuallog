using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace VisualLog.Desktop.LogManager
{
  /// <summary>
  /// Interaction logic for LogView.xaml
  /// </summary>
  public partial class LogView : UserControl
  {
    public LogView()
    {
      InitializeComponent();
    }

    private void MessagesListView_SourceUpdated(object sender, NotifyCollectionChangedEventArgs e)
    {
      this.ScrollToBottom();
    }

    private void PART_AutoSrollToBottomButton_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
      this.ScrollToBottom();
    }

    private void PART_AutoSrollToBottomButton_Checked(object sender, RoutedEventArgs e)
    {
      this.ScrollToBottom();
      var sourceCollection = (INotifyCollectionChanged)this.MessagesListView.Items.SourceCollection;
      if (sourceCollection == null)
        return;
      sourceCollection.CollectionChanged += this.MessagesListView_SourceUpdated;
    }

    private void PART_AutoSrollToBottomButton_Unchecked(object sender, RoutedEventArgs e)
    {
      var sourceCollection = (INotifyCollectionChanged)this.MessagesListView.Items.SourceCollection;
      if (sourceCollection == null)
        return;
      sourceCollection.CollectionChanged -= this.MessagesListView_SourceUpdated;
    }

    private void UserControl_Loaded(object sender, RoutedEventArgs e)
    {
      this.ScrollToBottom();
    }

    private void ScrollToBottom()
    {
      if (VisualTreeHelper.GetChildrenCount(this.MessagesListView) <= 0)
        return;
      var border = (Border)VisualTreeHelper.GetChild(this.MessagesListView, 0);
      if (VisualTreeHelper.GetChildrenCount(border) <= 0)
        return;
      var scrollViewer = (ScrollViewer)VisualTreeHelper.GetChild(border, 0);
      if (VisualTreeHelper.GetChildrenCount(scrollViewer) <= 0)
        return;
      var scrollViewerBorder = (Border)VisualTreeHelper.GetChild(scrollViewer, 0);
      if (VisualTreeHelper.GetChildrenCount(scrollViewerBorder) <= 0)
        return;
      var grid = (Grid)VisualTreeHelper.GetChild(scrollViewerBorder, 0);
      if (VisualTreeHelper.GetChildrenCount(grid) <= 0)
        return;
      var toggleButton = (ToggleButton)VisualTreeHelper.GetChild(grid, 3);
      if (toggleButton.IsChecked == true)
        scrollViewer.ScrollToBottom();
    }
  }
}
