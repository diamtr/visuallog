using NUnit.Framework;
using System;
using VisualLog.Core;
using VisualLog.Desktop.FormatManager;

namespace VisualLog.Desktop.Tests
{
  [TestFixture]
  public class MessageFormatterViewModelFixture
  {
    [Test]
    public void CreateNewMessageFormatterViewModel()
    {
      var vm = new MessageFormatterViewModel();
      Assert.That(vm.Formatter, Is.Null);

      var formatter = new MessageFormatter();
      vm = new MessageFormatterViewModel(formatter);
      Assert.That(vm.Formatter, Is.EqualTo(formatter));
    }
  }
}
