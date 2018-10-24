
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace DistributedMonitor.Primitives
{
  public class DInt : DistributedObject
  {
    private int _value = 0;

    private const string NOT_ENOUGH = "not-enough";

    public DInt(string name, DistributedEnvironment env) : base(env, name, NOT_ENOUGH)
    {
    }

    public override string JsonData
    {
      get => JsonConvert.SerializeObject(_value);
      set => _value = JsonConvert.DeserializeObject<int>(value);
    }

    public int Get()
    {
      return _value;
    }

    public async Task Set(int value)
    {
      await LockAsync();
      _value = value;
      await UnlockAsync();
    }

    public async Task<int> AddAsync(int value)
    {
      await LockAsync();
      _value = _value + value;
      await PulseAsync(NOT_ENOUGH);
      await UnlockAsync();
      return _value;
    }

    public async Task<int> SubtractIfResultIsPositive(int value)
    {
      await LockAsync();
      while (_value < value) await WaitAsync(NOT_ENOUGH);
      _value = _value - value;
      await UnlockAsync();
      return _value;
    }

  }
}
