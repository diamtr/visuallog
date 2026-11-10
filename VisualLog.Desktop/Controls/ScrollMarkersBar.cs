using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace VisualLog.Desktop.Controls
{
  /// <summary>
  /// Draws match tick marks along a vertical scrollbar (VS Code-style).
  /// Meant to be overlaid (IsHitTestVisible=false) on the right edge of a scrollbar.
  /// Render cost is bounded by pixel height, not by the number of matches.
  /// </summary>
  public class ScrollMarkersBar : FrameworkElement
  {
    private static readonly Brush DefaultMarkerBrush = Freeze(new SolidColorBrush(Color.FromRgb(0xE0, 0xA8, 0x00)));
    private static readonly Brush DefaultCurrentMarkerBrush = Freeze(new SolidColorBrush(Color.FromRgb(0x0F, 0x6C, 0xBD)));

    private static Brush Freeze(Brush brush)
    {
      brush.Freeze();
      return brush;
    }

    public static readonly DependencyProperty LineNumbersProperty = DependencyProperty.Register(
      nameof(LineNumbers), typeof(IEnumerable<int>), typeof(ScrollMarkersBar),
      new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender, OnLineNumbersChanged));

    public static readonly DependencyProperty TotalLinesProperty = DependencyProperty.Register(
      nameof(TotalLines), typeof(int), typeof(ScrollMarkersBar),
      new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty CurrentLineNumberProperty = DependencyProperty.Register(
      nameof(CurrentLineNumber), typeof(int), typeof(ScrollMarkersBar),
      new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty MarkerBrushProperty = DependencyProperty.Register(
      nameof(MarkerBrush), typeof(Brush), typeof(ScrollMarkersBar),
      new FrameworkPropertyMetadata(DefaultMarkerBrush, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty CurrentMarkerBrushProperty = DependencyProperty.Register(
      nameof(CurrentMarkerBrush), typeof(Brush), typeof(ScrollMarkersBar),
      new FrameworkPropertyMetadata(DefaultCurrentMarkerBrush, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty MarkerHeightProperty = DependencyProperty.Register(
      nameof(MarkerHeight), typeof(double), typeof(ScrollMarkersBar),
      new FrameworkPropertyMetadata(2.0, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty VerticalInsetProperty = DependencyProperty.Register(
      nameof(VerticalInset), typeof(double), typeof(ScrollMarkersBar),
      new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>1-based line numbers of matches. INotifyCollectionChanged sources are tracked.</summary>
    public IEnumerable<int> LineNumbers
    {
      get => (IEnumerable<int>)GetValue(LineNumbersProperty);
      set => SetValue(LineNumbersProperty, value);
    }

    public int TotalLines
    {
      get => (int)GetValue(TotalLinesProperty);
      set => SetValue(TotalLinesProperty, value);
    }

    /// <summary>1-based line number of the current match; 0 = none.</summary>
    public int CurrentLineNumber
    {
      get => (int)GetValue(CurrentLineNumberProperty);
      set => SetValue(CurrentLineNumberProperty, value);
    }

    public Brush MarkerBrush
    {
      get => (Brush)GetValue(MarkerBrushProperty);
      set => SetValue(MarkerBrushProperty, value);
    }

    public Brush CurrentMarkerBrush
    {
      get => (Brush)GetValue(CurrentMarkerBrushProperty);
      set => SetValue(CurrentMarkerBrushProperty, value);
    }

    public double MarkerHeight
    {
      get => (double)GetValue(MarkerHeightProperty);
      set => SetValue(MarkerHeightProperty, value);
    }

    /// <summary>Space left at the top and bottom (e.g. for scrollbar arrow buttons).</summary>
    public double VerticalInset
    {
      get => (double)GetValue(VerticalInsetProperty);
      set => SetValue(VerticalInsetProperty, value);
    }

    private bool invalidatePending;

    public ScrollMarkersBar()
    {
      IsHitTestVisible = false;
      SnapsToDevicePixels = true;
    }

    private static void OnLineNumbersChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
      var bar = (ScrollMarkersBar)d;
      if (e.OldValue is INotifyCollectionChanged oldCollection)
        CollectionChangedEventManager.RemoveHandler(oldCollection, bar.OnCollectionChanged);
      if (e.NewValue is INotifyCollectionChanged newCollection)
        CollectionChangedEventManager.AddHandler(newCollection, bar.OnCollectionChanged);
    }

    private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
      // Coalesce bursts of changes (thousands of matches may be added) into one render
      if (invalidatePending)
        return;
      invalidatePending = true;
      Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
      {
        invalidatePending = false;
        InvalidateVisual();
      }));
    }

    /// <summary>
    /// Computes distinct, sorted, rounded pixel rows for markers.
    /// y = inset + (line - 1) / max(totalLines, 1) * (height - 2 * inset).
    /// Line numbers below 1 are ignored; those beyond totalLines are clamped to the track end.
    /// </summary>
    public static IReadOnlyList<double> ComputeMarkerOffsets(IEnumerable<int> lineNumbers, int totalLines, double height, double verticalInset)
    {
      if (lineNumbers == null || double.IsNaN(height) || double.IsInfinity(height) || height <= 0)
        return Array.Empty<double>();
      if (double.IsNaN(verticalInset) || verticalInset < 0)
        verticalInset = 0;
      var track = height - 2 * verticalInset;
      if (track <= 0)
        return Array.Empty<double>();

      var total = Math.Max(totalLines, 1);
      var maxRow = (int)Math.Round(verticalInset + track);
      var rows = new HashSet<int>();
      foreach (var line in lineNumbers)
      {
        if (line < 1)
          continue;
        var fraction = Math.Min((double)(line - 1) / total, 1.0);
        var row = (int)Math.Round(verticalInset + fraction * track);
        rows.Add(Math.Min(row, maxRow));
      }

      var result = new List<double>(rows.Count);
      foreach (var row in rows)
        result.Add(row);
      result.Sort();
      return result;
    }

    private static IEnumerable<int> Snapshot(IEnumerable<int> source)
    {
      // The source may be modified on the UI thread only, but guard against enumeration races anyway
      if (source is ICollection collection)
      {
        var copy = new int[collection.Count];
        try
        {
          var i = 0;
          foreach (var item in source)
          {
            if (i >= copy.Length)
              break;
            copy[i++] = item;
          }
          return i == copy.Length ? copy : copy[..i];
        }
        catch (InvalidOperationException)
        {
          return Array.Empty<int>();
        }
      }
      return source;
    }

    protected override void OnRender(DrawingContext drawingContext)
    {
      base.OnRender(drawingContext);
      var height = ActualHeight;
      var width = ActualWidth;
      if (height <= 0 || width <= 0)
        return;

      var inset = VerticalInset;
      var markerHeight = Math.Max(MarkerHeight, 1);
      var lines = LineNumbers;
      if (lines != null)
      {
        var brush = MarkerBrush;
        if (brush != null)
        {
          IReadOnlyList<double> offsets;
          try
          {
            offsets = ComputeMarkerOffsets(Snapshot(lines), TotalLines, height, inset);
          }
          catch (InvalidOperationException)
          {
            offsets = Array.Empty<double>();
          }
          foreach (var y in offsets)
            drawingContext.DrawRectangle(brush, null, new Rect(0, MarkerTop(y, markerHeight, height), width, markerHeight));
        }
      }

      var current = CurrentLineNumber;
      var currentBrush = CurrentMarkerBrush;
      if (current > 0 && currentBrush != null)
      {
        var offsets = ComputeMarkerOffsets(new[] { current }, TotalLines, height, inset);
        if (offsets.Count > 0)
        {
          var currentHeight = markerHeight + 2;
          drawingContext.DrawRectangle(currentBrush, null, new Rect(0, MarkerTop(offsets[0], currentHeight, height), width, currentHeight));
        }
      }
    }

    private static double MarkerTop(double y, double markerHeight, double height)
    {
      var top = y - markerHeight / 2;
      return Math.Max(0, Math.Min(top, height - markerHeight));
    }
  }
}
