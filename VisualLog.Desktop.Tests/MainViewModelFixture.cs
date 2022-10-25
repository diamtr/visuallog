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
      Assert.AreEqual(mvm.LogManagerViewModel, mvm.ActiveViewModel);
    }

    [Test]
    public void SetActiveVM()
    {
      var mvm = new MainViewModel();
      mvm.SetAsActive(mvm.FormatManagerViewModel);
      Assert.AreEqual(mvm.FormatManagerViewModel, mvm.ActiveViewModel);
    }
  }
}
