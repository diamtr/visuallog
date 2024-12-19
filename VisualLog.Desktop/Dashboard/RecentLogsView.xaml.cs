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

namespace VisualLog.Desktop.Dashboard
{
  /// <summary>
  /// Interaction logic for RecentLogsView.xaml
  /// </summary>
  public partial class RecentLogsView : UserControl
  {
    public RecentLogsView()
    {
      InitializeComponent();
    }

    private void UserControl_Loaded(object sender, RoutedEventArgs e)
    {
      var rlvm = this.DataContext as RecentLogsViewModel;
      if (rlvm == null)
        return;
      rlvm.FillAvailableRecentLogs();
    }
  }
}
