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
    private static readonly NLog.Logger log = NLog.LogManager.GetCurrentClassLogger();

    // Each opened log owns its own LogView (see LogManagerView), so the view is bound
    // to a single LogViewModel for its whole life. Hidden views keep their state.
    private LogViewModel actualViewModel;

    public LogView()
    {
      InitializeComponent();
      this.Unloaded += this.UserControl_Unloaded;
      this.IsVisibleChanged += this.LogView_IsVisibleChanged;
    }

    private void UserControl_Loaded(object sender, RoutedEventArgs e)
    {
      this.Attach(this.DataContext as LogViewModel);
      this.ScrollToBottom();
    }

    private void UserControl_Unloaded(object sender, RoutedEventArgs e)
    {
      this.Detach();
    }

    private void LogView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
      if (!this.IsLoaded)
        return;
      this.Attach(e.NewValue as LogViewModel);
      this.ScrollToBottom();
    }

    private void LogView_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
      // New lines are not scrolled to while the view is hidden; catch up when it is shown.
      if (this.IsVisible)
        this.ScrollToBottom();
    }

    private void Attach(LogViewModel viewModel)
    {
      if (ReferenceEquals(this.actualViewModel, viewModel))
        return;
      this.Detach();
      this.actualViewModel = viewModel;
      if (this.actualViewModel == null)
        return;
      this.actualViewModel.ShowLineRequested += this.ShowLine;
      this.actualViewModel.SearchFocusRequested += this.FocusSearch;
      this.actualViewModel.LogMessages.CollectionChanged += this.LogMessages_CollectionChanged;
      this.actualViewModel.State.PropertyChanged += this.State_PropertyChanged;
      this.SearchBox.EscapeRequested += this.SearchBox_EscapeRequested;
      this.ApplySearchPanelVisibility();
    }

    private void Detach()
    {
      if (this.actualViewModel == null)
        return;
      this.actualViewModel.ShowLineRequested -= this.ShowLine;
      this.actualViewModel.SearchFocusRequested -= this.FocusSearch;
      this.actualViewModel.LogMessages.CollectionChanged -= this.LogMessages_CollectionChanged;
      this.actualViewModel.State.PropertyChanged -= this.State_PropertyChanged;
      this.SearchBox.EscapeRequested -= this.SearchBox_EscapeRequested;
      this.actualViewModel = null;
    }

    #region Search

    private void FocusSearch()
    {
      log.Debug("Search: focus the query box");
      this.SearchBox.FocusQuery();
    }

    private void SearchBox_EscapeRequested()
    {
      if (this.actualViewModel != null)
        this.actualViewModel.State.ShowSearchPanel = false;
      this.MessagesListView.Focus();
    }

    private void State_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
      if (e.PropertyName == nameof(LogViewStateViewModel.ShowSearchPanel))
        this.ApplySearchPanelVisibility();
    }

    /// <summary>The results panel takes its last height when shown and no room when hidden.</summary>
    private void ApplySearchPanelVisibility()
    {
      var state = this.actualViewModel?.State;
      var show = state != null && state.ShowSearchPanel;
      log.Debug($"Search: results panel {(show ? "shown" : "hidden")}, height {state?.SearchPanelHeight}");
      if (show)
      {
        this.SearchSplitterRow.Height = new GridLength(5);
        this.SearchPanelRow.MinHeight = 90;
        this.SearchPanelRow.Height = new GridLength(Math.Max(state.SearchPanelHeight, 90));
      }
      else
      {
        if (state != null && this.SearchPanelRow.ActualHeight > 0)
          state.SearchPanelHeight = this.SearchPanelRow.ActualHeight;
        this.SearchSplitterRow.Height = new GridLength(0);
        this.SearchPanelRow.MinHeight = 0;
        this.SearchPanelRow.Height = new GridLength(0);
      }
      this.SearchSplitter.Visibility = show ? Visibility.Visible : Visibility.Collapsed;
      this.SearchPaneContentControl.Visibility = show ? Visibility.Visible : Visibility.Collapsed;
    }

    private void SearchSplitter_DragCompleted(object sender, DragCompletedEventArgs e)
    {
      if (this.actualViewModel != null)
        this.actualViewModel.State.SearchPanelHeight = this.SearchPanelRow.ActualHeight;
    }

    private ScrollViewer logScrollViewer;

    private void MessagesListView_Loaded(object sender, RoutedEventArgs e)
    {
      if (this.logScrollViewer != null)
        return;
      this.logScrollViewer = FindDescendant<ScrollViewer>(this.MessagesListView);
      if (this.logScrollViewer != null)
        this.logScrollViewer.ScrollChanged += (s, args) => this.PlaceSearchMarkers();
      this.PlaceSearchMarkers();
    }

    /// <summary>Puts the match marks over the track of the vertical scrollbar of the log.</summary>
    private void PlaceSearchMarkers()
    {
      var scrollViewer = this.logScrollViewer;
      if (scrollViewer == null || scrollViewer.ComputedVerticalScrollBarVisibility != Visibility.Visible)
      {
        this.SearchMarkers.Visibility = Visibility.Collapsed;
        return;
      }
      var bottom = scrollViewer.ComputedHorizontalScrollBarVisibility == Visibility.Visible ? SystemParameters.HorizontalScrollBarHeight : 0;
      this.SearchMarkers.Visibility = Visibility.Visible;
      this.SearchMarkers.Width = SystemParameters.VerticalScrollBarWidth;
      this.SearchMarkers.VerticalInset = SystemParameters.VerticalScrollBarButtonHeight;
      this.SearchMarkers.Margin = new Thickness(0, 0, 0, bottom);
    }

    #endregion

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
        this.actualViewModel.AddToSelection(selectedItem.Message);
      }

      foreach (var unselectedItem in e.RemovedItems.Cast<MessageInlineViewModel>())
      {
        this.actualViewModel.RemoveFromSelection(unselectedItem.Message);
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
