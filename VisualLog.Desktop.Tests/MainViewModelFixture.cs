using NUnit.Framework;

namespace VisualLog.Desktop.Tests
{
  [TestFixture]
  public class MainViewModelFixture
  {
    [Test]
    public void Create()
    {
      var mvm = new MainViewModel();

      Assert.That(mvm.LogManagerViewModel, Is.Not.Null);
      Assert.That(mvm.FormatManagerViewModel, Is.Not.Null);
    }

    [Test]
    public void SetActiveVM()
    {
      var mvm = new MainViewModel();
      mvm.SetAsActive(mvm.FormatManagerViewModel);
      Assert.That(mvm.ActiveViewModel, Is.EqualTo(mvm.FormatManagerViewModel));
    }

    [Test]
    public void OnWindowLoaded()
    {
      var mvm = new MainViewModel();
      mvm.OnWindowLoaded();
      Assert.That(mvm.ActiveViewModel, Is.EqualTo(mvm.DashboardViewModel));
    }

    [Test]
    public void ShowLogManagerCommand()
    {
      var mvm = new MainViewModel();
      mvm.ShowLogManagerCommand.Execute(null);
      Assert.That(mvm.ActiveViewModel, Is.EqualTo(mvm.LogManagerViewModel));
    }

    [Test]
    public void ShowFormatManagerCommand()
    {
      var mvm = new MainViewModel();
      mvm.ShowFormatManagerCommand.Execute(null);
      Assert.That(mvm.ActiveViewModel, Is.EqualTo(mvm.FormatManagerViewModel));
    }
  }
}
