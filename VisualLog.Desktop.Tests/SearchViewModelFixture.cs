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
  }
}
