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
        this.HideSelectedMessagesPanel();
        this.HideSearchPanel();
      }
    }

    private void LogView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
      if (e.NewValue == null)
        return;

      this.RememberSelectedMessagesPanelState();
      this.RememberSearchPanelState();
      this.actualViewModel = e.NewValue as LogViewModel;
      this.ShowSelectedMessagesPanel();
      this.ShowSearchPanel();

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
          this.HideSelectedMessagesPanel();
        else
          this.ShowSelectedMessagesPanel();
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
          this.HideSearchPanel();
        else
          this.ShowSearchPanel();
      }
    }

    private void ShowSelectedMessagesPanel()
    {
      if (!this.actualViewModel?.State?.ShowSelectedMessagesPanel ?? true)
        return;

      var selectedMessagesWidth = this.lastSelectedMessagesPanelWidth.ContainsKey(this.actualViewModel.Guid) ?
          this.lastSelectedMessagesPanelWidth[this.actualViewModel.Guid] :
          new GridLength(1, GridUnitType.Star);
      this.LogAndSelectedMessagesGrid.ColumnDefinitions[0].Width = selectedMessagesWidth;
      this.LogAndSelectedMessagesGrid.ColumnDefinitions[1].Width = new GridLength(3);
      var logWidth = this.lastLogWidth.ContainsKey(this.actualViewModel.Guid) ?
        this.lastLogWidth[this.actualViewModel.Guid] :
        new GridLength(4, GridUnitType.Star);
      this.LogAndSelectedMessagesGrid.ColumnDefinitions[2].Width = logWidth;
    }

    private void HideSelectedMessagesPanel()
    {
      this.LogAndSelectedMessagesGrid.ColumnDefinitions[0].Width = new GridLength(0);
      this.LogAndSelectedMessagesGrid.ColumnDefinitions[1].Width = new GridLength(0);
      this.LogAndSelectedMessagesGrid.ColumnDefinitions[2].Width = new GridLength(1, GridUnitType.Star);
    }

    private void ShowSearchPanel()
    {
      if (!this.actualViewModel?.State?.ShowSearchPanel ?? true)
        return;

      var searchHeight = this.lastSearchPanelHeight.ContainsKey(this.actualViewModel.Guid) ?
          this.lastSearchPanelHeight[this.actualViewModel.Guid] :
          new GridLength(1, GridUnitType.Star);
      this.LogAndSearchGrid.RowDefinitions[0].Height = searchHeight;
      this.LogAndSearchGrid.RowDefinitions[1].Height = new GridLength(3);
      var logHeight = this.lastLogHeight.ContainsKey(this.actualViewModel.Guid) ?
        this.lastLogHeight[this.actualViewModel.Guid] :
        new GridLength(2, GridUnitType.Star);
      this.LogAndSearchGrid.RowDefinitions[2].Height = logHeight;
    }

    private void HideSearchPanel()
    {
      this.LogAndSearchGrid.RowDefinitions[0].Height = new GridLength(0);
      this.LogAndSearchGrid.RowDefinitions[1].Height = new GridLength(0);
      this.LogAndSearchGrid.RowDefinitions[2].Height = new GridLength(1, GridUnitType.Star);
    }

    private void RememberSelectedMessagesPanelState()
    {
      if (!this.actualViewModel?.State?.ShowSelectedMessagesPanel ?? true)
        return;

      var panelWidth = this.LogAndSelectedMessagesGrid.ColumnDefinitions[0].Width;
      if (panelWidth.Value > 0)
        this.lastSelectedMessagesPanelWidth[this.actualViewModel.Guid] = panelWidth;
      var logWidth = this.LogAndSelectedMessagesGrid.ColumnDefinitions[2].Width;
      if (logWidth.Value > 0)
        this.lastLogWidth[this.actualViewModel.Guid] = logWidth;
    }

    private void RememberSearchPanelState()
    {
      if (!this.actualViewModel?.State?.ShowSearchPanel ?? true)
        return;

      var panelHeight = this.LogAndSearchGrid.RowDefinitions[0].Height;
      if (panelHeight.Value > 0)
        this.lastSearchPanelHeight[this.actualViewModel.Guid] = panelHeight;
      var logHeight = this.LogAndSearchGrid.RowDefinitions[2].Height;
      if (logHeight.Value > 0)
        this.lastLogHeight[this.actualViewModel.Guid] = logHeight;
    }

    private void SearchGridSplitter_DragCompleted(object sender, DragCompletedEventArgs e)
    {
      this.RememberSearchPanelState();
    }

    private void SelectedMessagesGridSplitter_DragCompleted(object sender, DragCompletedEventArgs e)
    {
      this.RememberSelectedMessagesPanelState();
    }
  }
}
