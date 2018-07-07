namespace DistributedMonitor.Actors.Messages
{
  internal class UpdateMeMsg
  {
    internal UpdateMeMsg(string value)
    {
      Value = value;
    }

    internal string Value { get; }
  }
}
