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

namespace VisualLog.Desktop
{
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainView : Window
  {
    public MainView()
    {
      InitializeComponent();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
      var mvm = this.DataContext as MainViewModel;
      if (mvm == null)
        return;
      mvm.OnWindowLoaded();
    }

    private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
      var mvm = this.DataContext as MainViewModel;
      if (mvm == null)
        return;
      mvm.OnWindowClosing();
    }
  }
}
