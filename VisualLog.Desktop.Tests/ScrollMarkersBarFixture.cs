using NUnit.Framework;
using System.Linq;
using VisualLog.Desktop.Controls;

namespace VisualLog.Desktop.Tests
{
  [TestFixture]
  public class ScrollMarkersBarFixture
  {
    [Test]
    public void BasicPositions()
    {
      // y = (line - 1) / total * height
      var offsets = ScrollMarkersBar.ComputeMarkerOffsets(new[] { 1, 51, 100 }, 100, 200, 0);
      Assert.That(offsets, Is.EqualTo(new[] { 0.0, 100.0, 198.0 }));
    }

    [Test]
    public void SortedAndDistinct()
    {
      var offsets = ScrollMarkersBar.ComputeMarkerOffsets(new[] { 80, 10, 10, 50, 11 }, 100, 100, 0);
      // 10 -> 9, 11 -> 10, 50 -> 49, 80 -> 79
      Assert.That(offsets, Is.EqualTo(new[] { 9.0, 10.0, 49.0, 79.0 }));
    }

    [Test]
    public void ManyMatchesAreBoundedByPixelHeight()
    {
      var lines = Enumerable.Range(1, 100_000);
      var offsets = ScrollMarkersBar.ComputeMarkerOffsets(lines, 100_000, 300, 0);
      Assert.That(offsets.Count, Is.LessThanOrEqualTo(301));
      Assert.That(offsets, Is.Ordered);
      Assert.That(offsets, Is.Unique);
      Assert.That(offsets.First(), Is.EqualTo(0.0));
      Assert.That(offsets.Last(), Is.EqualTo(300.0));
    }

    [Test]
    public void TotalLinesZeroIsTreatedAsOne()
    {
      Assert.That(ScrollMarkersBar.ComputeMarkerOffsets(new[] { 1 }, 0, 100, 0), Is.EqualTo(new[] { 0.0 }));
      // Lines beyond the total are clamped to the end of the track
      Assert.That(ScrollMarkersBar.ComputeMarkerOffsets(new[] { 5 }, 0, 100, 0), Is.EqualTo(new[] { 100.0 }));
    }

    [Test]
    public void VerticalInset()
    {
      // track = 100 - 2*10 = 80
      var offsets = ScrollMarkersBar.ComputeMarkerOffsets(new[] { 1, 51, 101 }, 100, 100, 10);
      Assert.That(offsets, Is.EqualTo(new[] { 10.0, 50.0, 90.0 }));
    }

    [Test]
    public void DegenerateInput()
    {
      Assert.That(ScrollMarkersBar.ComputeMarkerOffsets(null, 100, 100, 0), Is.Empty);
      Assert.That(ScrollMarkersBar.ComputeMarkerOffsets(new int[0], 100, 100, 0), Is.Empty);
      Assert.That(ScrollMarkersBar.ComputeMarkerOffsets(new[] { 1 }, 100, 0, 0), Is.Empty);
      Assert.That(ScrollMarkersBar.ComputeMarkerOffsets(new[] { 1 }, 100, double.NaN, 0), Is.Empty);
      Assert.That(ScrollMarkersBar.ComputeMarkerOffsets(new[] { 1 }, 100, 20, 10), Is.Empty, "no room left by the inset");
      Assert.That(ScrollMarkersBar.ComputeMarkerOffsets(new[] { 0, -3 }, 100, 100, 0), Is.Empty, "line numbers are 1-based");
    }
  }
}
