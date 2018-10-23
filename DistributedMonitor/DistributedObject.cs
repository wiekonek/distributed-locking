using System;
using System.Threading.Tasks;
using Akka.Actor;
using DistributedMonitor.Actors;
using DistributedMonitor.Actors.Messages;

namespace DistributedMonitor
{

  public abstract class DistributedObject
  {
    private readonly DistributedEnvironment _env;
    private readonly IActorRef _actor;

    public string Name { get; }

    protected DistributedObject(DistributedEnvironment env, string name)
    {
      Name = name;
      _env = env;
      _actor = _env.DistributedSystem.ActorOf<DistributedObjectActor>(name);
      _actor.Tell(new InternalMessages.Init(this));
    }

    public abstract string JsonData { get; set; }
    
    protected async Task LockAsync()
    {
      await _actor.Ask<Empty>(new InternalMessages.AskLock());
      return;
    }

    protected async Task UnlockAsync()
    {
      await UpdateAsync();
      await _actor.Ask<Empty>(new InternalMessages.AskUnlock());
      return;
    }

    //protected async Task WaitAsync()
    //{
    //  if (!_locked)
    //  {
    //    throw new DistributedLockingException("WaitAsync() can be called only when object is locked (use LockAsync() first)");
    //  }
    //  _locked = false;
    //  await Task.CompletedTask;
    //}

    //protected async Task NotifyAsync()
    //{
    //  await UpdateAsync();
    //  return;
    //}

    //protected async Task NotifyAllAsync()
    //{
    //  await UpdateAsync();
    //  return;
    //}

    private async Task UpdateAsync()
    {
      await _actor.Ask<Empty>(new InternalMessages.AskUpdateObject(JsonData));
      return;
    }
  }
}
