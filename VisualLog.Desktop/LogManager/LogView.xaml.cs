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
    private Dictionary<Guid, double> lastSearchPanelHeight;
    private LogViewModel actualViewModel;

    public LogView()
    {
      this.lastSearchPanelHeight = new Dictionary<Guid, double>();
      InitializeComponent();
    }

    private void UserControl_Loaded(object sender, RoutedEventArgs e)
    {
      if (!this.IsVisible)
        return;

      this.ScrollToBottom();
      this.actualViewModel = this.DataContext as LogViewModel;
      if (this.actualViewModel != null)
      {
        this.actualViewModel.ShowLineRequested += this.ShowLine;
        this.actualViewModel.LogMessages.CollectionChanged += this.LogMessages_CollectionChanged;
      }
    }

    private void LogView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
      if (e.OldValue is LogViewModel oldVm)
      {
        oldVm.ShowLineRequested -= this.ShowLine;
        if (oldVm.LogMessages != null)
          oldVm.LogMessages.CollectionChanged -= this.LogMessages_CollectionChanged;
      }

      if (e.NewValue is not LogViewModel vm)
        return;

      this.actualViewModel = vm;
      this.actualViewModel.ShowLineRequested += this.ShowLine;
      this.actualViewModel.LogMessages.CollectionChanged += this.LogMessages_CollectionChanged;

      this.ScrollToBottom();
    }

    private void LogMessages_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
      if (!this.IsVisible)
        return;
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

      if (!(this.FollowTailCheckBox.IsChecked.GetValueOrDefault()))
        return;

      // Defer until after layout to ensure correct extent
      this.MessagesListView.Dispatcher.BeginInvoke(new Action(() =>
      {
        var count = this.MessagesListView.Items.Count;
        if (count == 0)
          return;

        var lastItem = this.MessagesListView.Items[count - 1];
        this.MessagesListView.ScrollIntoView(lastItem);

        // As a fallback, also nudge the ScrollViewer if available
        var scrollViewer = FindDescendant<ScrollViewer>(this.MessagesListView);
        scrollViewer?.ScrollToBottom();
      }), System.Windows.Threading.DispatcherPriority.Background);
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
      if (this.actualViewModel == null)
        return;

      foreach (var selectedItem in e.AddedItems.Cast<MessageInlineViewModel>())
      {
        var message = selectedItem.Message;
        var messagePanelViewModel = this.actualViewModel.SelectedLogMessages.Messages.FirstOrDefault(x => Equals(x, message));
        if (messagePanelViewModel == null)
          this.actualViewModel.SelectedLogMessages.Messages.Add(message);
      }

      foreach (var unselectedItem in e.RemovedItems.Cast<MessageInlineViewModel>())
      {
        var message = unselectedItem.Message;
        var messagePanelViewModel = this.actualViewModel.SelectedLogMessages.Messages.FirstOrDefault(x => Equals(x, message));
        if (messagePanelViewModel != null)
          this.actualViewModel.SelectedLogMessages.Messages.Remove(messagePanelViewModel);
      }
    }

    private static T FindDescendant<T>(DependencyObject root) where T : DependencyObject
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
