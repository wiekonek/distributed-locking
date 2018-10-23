
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace DistributedMonitor.Primitives
{
  public class DInt : DistributedObject
  {
    private int _value = 0;

    public DInt(string name, DistributedEnvironment env) : base(env, name)
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
      await UnlockAsync();
      return _value;
    }

  }
}
