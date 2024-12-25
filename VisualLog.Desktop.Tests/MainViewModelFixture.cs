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

      Assert.IsNotNull(mvm.LogManagerViewModel);
      Assert.IsNotNull(mvm.FormatManagerViewModel);
    }

    [Test]
    public void SetActiveVM()
    {
      var mvm = new MainViewModel();
      mvm.SetAsActive(mvm.FormatManagerViewModel);
      Assert.AreEqual(mvm.FormatManagerViewModel, mvm.ActiveViewModel);
    }

    [Test]
    public void OnWindowLoaded()
    {
      var mvm = new MainViewModel();
      mvm.OnWindowLoaded();
      Assert.AreEqual(mvm.DashboardViewModel, mvm.ActiveViewModel);
    }

    [Test]
    public void ShowLogManagerCommand()
    {
      var mvm = new MainViewModel();
      mvm.ShowLogManagerCommand.Execute(null);
      Assert.AreEqual(mvm.LogManagerViewModel, mvm.ActiveViewModel);
    }

    [Test]
    public void ShowFormatManagerCommand()
    {
      var mvm = new MainViewModel();
      mvm.ShowFormatManagerCommand.Execute(null);
      Assert.AreEqual(mvm.FormatManagerViewModel, mvm.ActiveViewModel);
    }
  }
}
