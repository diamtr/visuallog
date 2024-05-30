using System.Collections.Generic;
using System.Linq;

namespace VisualLog.Core
{
  public abstract class MessageBase : IMessage
  {
    public string RawValue { get; set; }

    public MessageBase(string rawValue)
    {
      this.RawValue = rawValue;
    }

    #region Equals

    public override bool Equals(object obj)
    {
      if (obj == null || !typeof(MessageBase).IsInstanceOfType(obj))
        return false;

      return this.RawValue == ((MessageBase)obj).RawValue;
    }

    public override int GetHashCode()
    {
      return this.RawValue.GetHashCode();
    }

    #endregion

    public static List<IMessage> Distinct(IEnumerable<IMessage> sourceMessages)
    {
      var messages = new List<IMessage>();
      foreach (var message in sourceMessages)
        if (!messages.Any(x => x.RawValue == message.RawValue))
          messages.Add(message);
      return messages;
    }
  }
}
