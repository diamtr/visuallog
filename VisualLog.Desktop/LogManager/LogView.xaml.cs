using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using System.Security.Policy;
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

    private void UserControl_Loaded(object sender, RoutedEventArgs e)
    {
      this.ScrollToBottom();
      var logViewModel = this.DataContext as LogViewModel;
      if (logViewModel != null)
      {
        logViewModel.ShowLineRequested += this.ShowLine;
        logViewModel.LogMessages.CollectionChanged += this.LogMessages_CollectionChanged;
      }
    }

    private void LogView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
      if (e.NewValue == null)
        return;
      var viewModel = e.NewValue as LogViewModel;
      if (viewModel != null)
      {
        if (viewModel.State.ShowSelectedMessageVertical)
          this.ShowLogAndSelectedMessagesGridLeftPart(e.NewValue.GetHashCode());
        else
          this.HideLogAndSelectedMessagesGridLeftPart(e.NewValue.GetHashCode());
      }
      this.ScrollToBottom();
    }

    private void LogMessages_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
      this.ScrollToBottom();
    }

    private void FollowTailCheckBox_Checked(object sender, RoutedEventArgs e)
    {
      this.ScrollToBottom();
    }

    private void ScrollToBottom()
    {
      if (!this.IsLoaded)
        return;

      var scrollViewer = VisualTreeHelper.GetChild(VisualTreeHelper.GetChild(this.MessagesListView, 0), 0) as ScrollViewer;
      if (scrollViewer == null)
        return;

      if (this.FollowTailCheckBox.IsChecked.GetValueOrDefault())
        scrollViewer.ScrollToBottom();
    }

    private void ShowLine(double position)
    {
      if (position < 0)
        return;

      var scrollViewer = VisualTreeHelper.GetChild(VisualTreeHelper.GetChild(this.MessagesListView, 0), 0) as ScrollViewer;
      if (scrollViewer == null)
        return;

      this.FollowTailCheckBox.IsChecked = false;
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

      if (border.DataContext != null)
      {
        if (border.Visibility == Visibility.Collapsed)
          this.HideLogAndSelectedMessagesGridLeftPart(border.DataContext.GetHashCode());
        else
          this.ShowLogAndSelectedMessagesGridLeftPart(border.DataContext.GetHashCode());
      }
    }

    private void ShowLogAndSelectedMessagesGridLeftPart(int hash)
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

    private void HideLogAndSelectedMessagesGridLeftPart(int hash)
    {
      var leftWidth = this.LogAndSelectedMessagesGrid.ColumnDefinitions[0].Width;
      this.lastSelectedLogMessagesWidth[hash] = leftWidth;
      var rightWidth = this.LogAndSelectedMessagesGrid.ColumnDefinitions[2].Width;
      this.lastLogWidth[hash] = rightWidth;
      this.LogAndSelectedMessagesGrid.ColumnDefinitions[0].Width = new GridLength(0);
      this.LogAndSelectedMessagesGrid.ColumnDefinitions[1].Width = new GridLength(0);
      this.LogAndSelectedMessagesGrid.ColumnDefinitions[2].Width = new GridLength(1, GridUnitType.Star);
    }
  }
}
