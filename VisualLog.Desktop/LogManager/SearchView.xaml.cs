using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace VisualLog.Desktop.LogManager
{
  /// <summary>
  /// Search results panel under the log.
  /// </summary>
  public partial class SearchView : UserControl
  {
    public SearchView()
    {
      InitializeComponent();
    }

    private void SearchEntriesList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      if (this.DataContext is SearchViewModel vm)
        vm.SelectedEntries = this.SearchEntriesList.SelectedItems.OfType<SearchEntryViewModel>().ToList();
    }

    private void SearchEntriesList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
      // Double click on a row: show its line again, even if it is the current result already
      var item = ItemsControl.ContainerFromElement(this.SearchEntriesList, e.OriginalSource as DependencyObject) as ListViewItem;
      if (item?.DataContext is SearchEntryViewModel entry && this.DataContext is SearchViewModel vm)
        vm.ShowSerchEntryLine(entry);
    }
  }
}
