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
    private Dictionary<Guid, GridLength> lastSelectedMessagesPanelWidth;
    private Dictionary<Guid, GridLength> lastLogWidth;
    private Dictionary<Guid, GridLength> lastSearchPanelHeight;
    private Dictionary<Guid, GridLength> lastLogHeight;
    private LogViewModel actualViewModel;

    public LogView()
    {
      this.lastSelectedMessagesPanelWidth = new Dictionary<Guid, GridLength>();
      this.lastLogWidth = new Dictionary<Guid, GridLength>();
      this.lastSearchPanelHeight = new Dictionary<Guid, GridLength>();
      this.lastLogHeight = new Dictionary<Guid, GridLength>();
      InitializeComponent();
    }

    private void UserControl_Loaded(object sender, RoutedEventArgs e)
    {
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
      if (e.NewValue == null)
        return;

      if (this.actualViewModel != null)
      {
        this.RememberSelectedMessagesPanelState(this.actualViewModel.Guid);
        this.RememberSearchPanelState(this.actualViewModel.Guid);
      }
      this.actualViewModel = e.NewValue as LogViewModel;

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
      if (this.actualViewModel == null)
        return;

      foreach (var selectedItem in e.AddedItems.Cast<MessageInlineViewModel>())
      {
        var message = selectedItem.Message;
        var messagePanelViewModel = this.actualViewModel.SelectedLogMessages.Messages.FirstOrDefault(x => Equals(x.Message, message));
        if (messagePanelViewModel == null)
          this.actualViewModel.SelectedLogMessages.Messages.Add(new MessagePanelViewModel(message));
      }

      foreach (var unselectedItem in e.RemovedItems.Cast<MessageInlineViewModel>())
      {
        var message = unselectedItem.Message;
        var messagePanelViewModel = this.actualViewModel.SelectedLogMessages.Messages.FirstOrDefault(x => Equals(x.Message, message));
        if (messagePanelViewModel != null)
          this.actualViewModel.SelectedLogMessages.Messages.Remove(messagePanelViewModel);
      }
    }

    private void SelectedLogMessages_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
      var border = sender as Border;
      if (border == null)
        return;

      if (this.actualViewModel != null)
      {
        if (border.Visibility == Visibility.Collapsed)
          this.HideSelectedMessagesPanel(this.actualViewModel.Guid);
        else
          this.ShowSelectedMessagesPanel(this.actualViewModel.Guid);
      }
    }

    private void SearchPanel_VisibilityChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
      var border = sender as Border;
      if (border == null)
        return;

      if (this.actualViewModel != null)
      {
        if (border.Visibility == Visibility.Collapsed)
          this.HideSearchPanel(this.actualViewModel.Guid);
        else
          this.ShowSearchPanel(this.actualViewModel.Guid);
      }
    }

    private void ShowSelectedMessagesPanel(Guid viewModelGuid)
    {
      var selectedMessagesWidth = this.lastSelectedMessagesPanelWidth.ContainsKey(viewModelGuid) ?
          this.lastSelectedMessagesPanelWidth[viewModelGuid] :
          new GridLength(1, GridUnitType.Star);
      this.LogAndSelectedMessagesGrid.ColumnDefinitions[0].Width = selectedMessagesWidth;
      this.LogAndSelectedMessagesGrid.ColumnDefinitions[1].Width = new GridLength(3);
      var logWidth = this.lastLogWidth.ContainsKey(viewModelGuid) ?
        this.lastLogWidth[viewModelGuid] :
        new GridLength(2, GridUnitType.Star);
      this.LogAndSelectedMessagesGrid.ColumnDefinitions[2].Width = logWidth;
    }

    private void HideSelectedMessagesPanel(Guid viewModelGuid)
    {
      this.LogAndSelectedMessagesGrid.ColumnDefinitions[0].Width = new GridLength(0);
      this.LogAndSelectedMessagesGrid.ColumnDefinitions[1].Width = new GridLength(0);
      this.LogAndSelectedMessagesGrid.ColumnDefinitions[2].Width = new GridLength(1, GridUnitType.Star);
    }

    private void ShowSearchPanel(Guid viewModelGuid)
    {
      var searchHeight = this.lastSearchPanelHeight.ContainsKey(viewModelGuid) ?
          this.lastSearchPanelHeight[viewModelGuid] :
          new GridLength(1, GridUnitType.Star);
      this.LogAndSearchGrid.RowDefinitions[0].Height = searchHeight;
      this.LogAndSearchGrid.RowDefinitions[1].Height = new GridLength(3);
      var logHeight = this.lastLogHeight.ContainsKey(viewModelGuid) ?
        this.lastLogHeight[viewModelGuid] :
        new GridLength(2, GridUnitType.Star);
      this.LogAndSearchGrid.RowDefinitions[2].Height = logHeight;
    }

    private void HideSearchPanel(Guid viewModelGuid)
    {
      this.LogAndSearchGrid.RowDefinitions[0].Height = new GridLength(0);
      this.LogAndSearchGrid.RowDefinitions[1].Height = new GridLength(0);
      this.LogAndSearchGrid.RowDefinitions[2].Height = new GridLength(1, GridUnitType.Star);
    }

    private void RememberSelectedMessagesPanelState(Guid viewModelGuid)
    {
      var panelWidth = this.LogAndSelectedMessagesGrid.ColumnDefinitions[0].Width;
      if (panelWidth.Value > 0)
        this.lastSelectedMessagesPanelWidth[viewModelGuid] = panelWidth;
      var logWidth = this.LogAndSelectedMessagesGrid.ColumnDefinitions[2].Width;
      if (logWidth.Value > 0)
        this.lastLogWidth[viewModelGuid] = logWidth;
    }

    private void RememberSearchPanelState(Guid viewModelGuid)
    {
      var panelHeight = this.LogAndSearchGrid.RowDefinitions[0].Height;
      if (panelHeight.Value > 0)
        this.lastSearchPanelHeight[viewModelGuid] = panelHeight;
      var logHeight = this.LogAndSearchGrid.RowDefinitions[2].Height;
      if (logHeight.Value > 0)
        this.lastLogHeight[viewModelGuid] = logHeight;
    }

    private void SearchGridSplitter_DragCompleted(object sender, DragCompletedEventArgs e)
    {
      if (this.actualViewModel != null)
        this.RememberSearchPanelState(this.actualViewModel.Guid);
    }

    private void SelectedMessagesGridSplitter_DragCompleted(object sender, DragCompletedEventArgs e)
    {
      if (this.actualViewModel != null)
        this.RememberSelectedMessagesPanelState(this.actualViewModel.Guid);
    }
  }
}
