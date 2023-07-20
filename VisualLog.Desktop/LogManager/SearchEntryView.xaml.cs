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
using VisualLog.Core;

namespace VisualLog.Desktop.LogManager
{
  /// <summary>
  /// Interaction logic for SearchEntryView.xaml
  /// </summary>
  public partial class SearchEntryView : UserControl
  {
    public SearchEntryView()
    {
      InitializeComponent();
    }

    private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
      var vm = e.NewValue as SearchEntryViewModel;
      if (vm == null || vm.SearchEntry == null || vm.SearchEntry.Matches == null)
        return;

      var rawString = vm.SearchEntry.RawString;
      var stringParts = new List<Tuple<bool, string>>();
      var matches = new Queue<Match>(vm.SearchEntry.Matches.OrderBy(x => x.Index));
      var index = 0;
      while (matches.Any())
      {
        var match = matches.Dequeue();
        var offset = match.Index - index;
        if (offset > 0)
          stringParts.Add(new Tuple<bool, string>(false, rawString.Substring(index, offset)));
        stringParts.Add(new Tuple<bool, string>(true, rawString.Substring(match.Index, match.Length)));
        index = match.Index + match.Length;
      }
      if (index < rawString.Length - 1)
        stringParts.Add(new Tuple<bool, string>(false, rawString.Substring(index)));

      foreach (var part in stringParts)
        if (part.Item1)
          this.SearchEntryTextBlock.Inlines.Add(new Run(part.Item2) { Background = Brushes.Yellow });
        else
          this.SearchEntryTextBlock.Inlines.Add(part.Item2);
    }
  }
}
