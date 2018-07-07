using System.Threading.Tasks;
using Akka.Actor;
using DistributedMonitor.Actors.Messages;

namespace DistributedMonitor
{

  public abstract class DistributedObject
  {
    private readonly DistributedEnvironment _env;
    private readonly IActorRef _actor;

    private bool _locked;

    public string Name { get; }

    protected DistributedObject(DistributedEnvironment env, string name)
    {
      Name = name;
      _env = env;
      _actor = _env.Register(this);
    }

    public abstract string JsonData { get; set; }

    protected async Task LockAsync()
    {
      _locked = true;
      await Task.CompletedTask;
    }

    protected void Unlock()
    {
      _locked = false;
      Update();
      return;
    }

    protected async Task WaitAsync()
    {
      if (!_locked)
      {
        throw new DistributedLockingException("WaitAsync() can be called only when object is locked (use LockAsync() first)");
      }
      _locked = false;
      await Task.CompletedTask;
    }

    protected void Notify()
    {
      Update();
      return;
    }

    protected void NotifyAll()
    {
      Update();
      return;
    }

    private void Update()
    {
      _env.UpdateObject(Name, JsonData);
      _actor.Tell(new UpdateMeMsg(JsonData));
      return;
    }
  }
}
