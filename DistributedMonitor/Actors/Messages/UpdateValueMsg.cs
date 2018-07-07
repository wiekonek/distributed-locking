namespace DistributedMonitor.Actors.Messages
{
  internal class UpdateValueMsg
  {

    internal UpdateValueMsg(string value)
    {
      Value = value;
    }

    internal string Value { get; }
  }
}
