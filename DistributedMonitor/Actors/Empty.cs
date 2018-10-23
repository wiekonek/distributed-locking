namespace DistributedMonitor.Actors
{
  internal class Empty
  {
    static readonly Empty _default = new Empty();

    public static object Default => _default;
  }
}
