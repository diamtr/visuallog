namespace VisualLog.Core
{
  public class Message
  {
    public virtual Log Log { get; set; }
    public virtual long Number { get; set; }
    public virtual string RawValue { get; set; }
  }
}
