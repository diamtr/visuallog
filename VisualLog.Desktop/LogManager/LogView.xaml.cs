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
    private Dictionary<int, GridLength> lastSelectedLogMessagesWidth;
    private Dictionary<int, GridLength> lastLogWidth;

    public LogView()
    {
      this.lastSelectedLogMessagesWidth = new Dictionary<int, GridLength>();
      this.lastLogWidth = new Dictionary<int, GridLength>();
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
      var logViewModel = this.DataContext as LogViewModel;
      if (logViewModel != null)
        logViewModel.ShowLineRequested += this.ShowLine;
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

    private void ShowLine(double position)
    {
      if (position < 0)
        return;

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
        scrollViewer.ScrollToVerticalOffset(position);
    }

    private void MessagesListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      var listView = sender as ListView;
      if (listView == null)
        return;

      var dataCondext = listView.DataContext as LogViewModel;
      if (dataCondext == null)
        return;

      foreach (var selectedItem in e.AddedItems.Cast<MessageInlineViewModel>())
      {
        var message = selectedItem.Message;
        var messagePanelViewModel = dataCondext.SelectedLogMessages.Messages.FirstOrDefault(x => Equals(x.Message, message));
        if (messagePanelViewModel == null)
          dataCondext.SelectedLogMessages.Messages.Add(new MessagePanelViewModel(message));
      }

      foreach (var unselectedItem in e.RemovedItems.Cast<MessageInlineViewModel>())
      {
        var message = unselectedItem.Message;
        var messagePanelViewModel = dataCondext.SelectedLogMessages.Messages.FirstOrDefault(x => Equals(x.Message, message));
        if (messagePanelViewModel != null)
          dataCondext.SelectedLogMessages.Messages.Remove(messagePanelViewModel);
      }
    }

    private void SelectedLogMessages_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
      var border = sender as Border;
      if (border == null)
        return;

      var hash = border.DataContext.GetHashCode();

      if (border.Visibility == Visibility.Collapsed)
      {
        var leftWidth = this.LogAndSelectedMessagesGrid.ColumnDefinitions[0].Width;
        this.lastSelectedLogMessagesWidth[hash] = leftWidth;
        var rightWidth = this.LogAndSelectedMessagesGrid.ColumnDefinitions[2].Width;
        this.lastLogWidth[hash] = rightWidth;
        this.LogAndSelectedMessagesGrid.ColumnDefinitions[0].Width = new GridLength(0);
        this.LogAndSelectedMessagesGrid.ColumnDefinitions[1].Width = new GridLength(0);
        this.LogAndSelectedMessagesGrid.ColumnDefinitions[2].Width = new GridLength(1, GridUnitType.Star);
      }
      else
      {
        var leftWidth = this.lastSelectedLogMessagesWidth.ContainsKey(hash) ?
          this.lastSelectedLogMessagesWidth[hash] :
          new GridLength(1, GridUnitType.Star);
        this.LogAndSelectedMessagesGrid.ColumnDefinitions[0].Width = leftWidth;
        this.LogAndSelectedMessagesGrid.ColumnDefinitions[1].Width = new GridLength(3);
        var rightWidth = this.lastLogWidth.ContainsKey(hash) ?
          this.lastLogWidth[hash] :
          new GridLength(2, GridUnitType.Star);
        this.LogAndSelectedMessagesGrid.ColumnDefinitions[2].Width = rightWidth;
      }
    }

    private void LogView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
      var hash = e.NewValue.GetHashCode();
      if (this.SelectedLogMessages.Visibility != Visibility.Collapsed)
      {
        var leftWidth = this.lastSelectedLogMessagesWidth.ContainsKey(hash) ?
          this.lastSelectedLogMessagesWidth[hash] :
          new GridLength(1, GridUnitType.Star);
        this.LogAndSelectedMessagesGrid.ColumnDefinitions[0].Width = leftWidth;
        this.LogAndSelectedMessagesGrid.ColumnDefinitions[1].Width = new GridLength(3);
        var rightWidth = this.lastLogWidth.ContainsKey(hash) ?
          this.lastLogWidth[hash] :
          new GridLength(2, GridUnitType.Star);
        this.LogAndSelectedMessagesGrid.ColumnDefinitions[2].Width = rightWidth;
      }
    }
  }
}
