using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using VisualLog.Desktop.Controls;

namespace VisualLog.Desktop.LogManager
{
  /// <summary>
  /// A found line with its matches highlighted. Long lines start shortly before the first match.
  /// </summary>
  public partial class SearchEntryView : UserControl
  {
    /// <summary>Characters kept before the first match of a long line.</summary>
    public const int LeadingContext = 60;

    private static readonly Brush FallbackMatchBrush = CreateFallbackBrush();

    public SearchEntryView()
    {
      InitializeComponent();
    }

    private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
      // Rows are recycled by the virtualizing list: always rebuild from scratch
      var vm = e.NewValue as SearchEntryViewModel;
      var entry = vm?.SearchEntry;
      var segments = MatchSegments.Split(entry?.Message?.RawValue, entry?.Matches, LeadingContext);
      var brush = this.TryFindResource("SearchMatchBrush") as Brush ?? FallbackMatchBrush;
      MatchSegments.Fill(this.SearchEntryTextBlock.Inlines, segments, brush);
    }

    private static Brush CreateFallbackBrush()
    {
      var brush = new SolidColorBrush(Color.FromRgb(0xFF, 0xE7, 0xA3));
      brush.Freeze();
      return brush;
    }
  }
}
