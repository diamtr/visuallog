using NUnit.Framework;
using VisualLog.Desktop.LogManager;

namespace VisualLog.Desktop.Tests
{
  [TestFixture]
  public class SearchViewModelFixture
  {
    [Test]
    public void NewSearchViewModel()
    {
      var vm = new SearchViewModel();
      Assert.IsNull(vm.StringToSearch);
    }

    [Test]
    public void HideRequested()
    {
      var flag = false;
      var vm = new SearchViewModel();
      vm.HideSearchPanelRequested += () => flag = true;
      vm.HideSearchPanelCommand.Execute(null);
      Assert.IsTrue(flag);
    }

    [Test]
    public void SearchRequested()
    {
      var flag = false;
      var vm = new SearchViewModel();
      vm.SearchRequested += (sender, e) => flag = true;
      vm.SearchCommand.Execute(null);
      Assert.IsTrue(flag);
    }
  }
}
