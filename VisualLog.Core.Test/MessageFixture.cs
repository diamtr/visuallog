using NUnit.Framework;
using System.Collections.Generic;

namespace VisualLog.Core.Test
{
  [TestFixture]
  public class MessageFixture
  {
    [Test]
    public void Equals()
    {
      var m1 = new Message("1234567890");
      var m2 = new JMessage("1234567890");
      Assert.IsTrue(m1.Equals(m2), "Different derevided type messages must be equal");
    }

    [Test]
    public void Distinct()
    {
      var sourceMessages = new List<IMessage>() {
        new Message("1234567890"),
        new Message("1234567890"),
        new Message("0000000000"),
        new JMessage("0987654321"),
        new JMessage("0987654321"),
        new JMessage("0000000000")
      };

      var messages = MessageBase.Distinct(sourceMessages);
      CollectionAssert.AreEquivalent(new List<IMessage>() { new Message("1234567890"),
                                                            new Message("0000000000"),
                                                            new JMessage("0987654321") },
                                     messages);
    }
  }
}
