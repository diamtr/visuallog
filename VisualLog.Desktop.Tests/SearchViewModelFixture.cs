using NUnit.Framework;
using System;
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
      Assert.That(vm.StringToSearch, Is.Null);
    }
  }
}
