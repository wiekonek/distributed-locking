using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DistributedMonitor.Primitives
{
  public class DBuffer<T> : DistributedObject
  {
    private const string ADDED = "added";
    private const string REMOVED = "removed";

    private int _size;
    private int _consumingIndex = 0;
    private int _producingIndex = 0;
    private int _count = 0;
    private T[] _buffer;

    public DBuffer(int size, string name, DistributedEnvironment env) : base(env, name, ADDED, REMOVED)
    {
      _size = size;
      _buffer = new T[size];
    }

    public override string JsonData {
      get => JsonConvert.SerializeObject((_size, _consumingIndex, _producingIndex, _count, _buffer));
      set{
        (_size, _consumingIndex, _producingIndex, _count, _buffer) = 
          JsonConvert.DeserializeObject<(int, int, int, int, T[])>(value);
      }
    }

    public async Task Add(T item)
    {
      await LockAsync();
      while (_count == _size) await WaitAsync(REMOVED);
      _buffer[_producingIndex] = item;
      _producingIndex = (_producingIndex + 1) % _size;
      _count++;
      await PulseAsync(ADDED);
      //await PulseAllAsync(ADDED);
      await UnlockAsync();
      return;
    }

    public async Task<T> Take()
    {
      await LockAsync();
      while (_count == 0) await WaitAsync(ADDED);
      var item = _buffer[_consumingIndex];
      _consumingIndex = (_consumingIndex+1) % _size;
      _count--;
      await PulseAsync(REMOVED);
      await UnlockAsync();
      return item;
    }
  }
}
