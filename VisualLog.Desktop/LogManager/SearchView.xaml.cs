using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
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
  /// Interaction logic for SearchView.xaml
  /// </summary>
  public partial class SearchView : UserControl
  {
    public SearchView()
    {
      InitializeComponent();
    }

    private void ListViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
      var listViewItem = sender as ListViewItem;
      if (listViewItem == null)
        return;
      var searchEntryViewModel = listViewItem.DataContext as SearchEntryViewModel;
      if (searchEntryViewModel == null)
        return;
      var searchViewModel = this.DataContext as SearchViewModel;
      if (searchViewModel == null)
        return;
      searchViewModel.ShowSerchEntryLine(searchEntryViewModel);
    }
  }
}
