using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DistributedMonitor.Primitives
{
  public class DBuffor<T> : DistributedObject
  {
    private int _size;
    private int _consumingIndex;
    private int _producingIndex;
    private T[] _buffor;

    public DBuffor(int size, string name, DistributedEnvironment env) : base(env, name)
    {
      _size = size;
    }

    public override string JsonData {
      get => JsonConvert.SerializeObject((_size, _consumingIndex, _producingIndex, _buffor));
      set{
        var data = JsonConvert.DeserializeObject<(int, int, int, T[])>(value);
        _size = data.Item1;
        _consumingIndex = data.Item2;
        _producingIndex = data.Item3;
        _buffor = data.Item4;
      }
    }

    public async Task Add(T item)
    {
      return;
    }

    public async Task<T> Take()
    {
      return default(T);
    }
  }
}
