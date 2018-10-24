using Akka.Actor;
using DistributedMonitor.Actors;
using DistributedMonitor.Actors.Messages;
using System.Linq;
using System.Threading.Tasks;

namespace DistributedMonitor
{

  public abstract class DistributedObject
  {
    private readonly DistributedEnvironment _env;
    private readonly IActorRef _actor;
    public bool _locked = false;
    public string Name { get; }
    public string[] Conditionals { get; }

    protected DistributedObject(DistributedEnvironment env, string name, params string[] conditionals)
    {
      Name = name;
      Conditionals = conditionals;
      _env = env;
      _actor = _env.DistributedSystem.ActorOf<DistributedObjectActor>(name);
      _actor.Tell(new InternalMessages.Init(this, conditionals));
    }

    public abstract string JsonData { get; set; }

    protected async Task LockAsync()
    {
      await _actor.Ask<Empty>(new InternalMessages.AskLock());
      _locked = true;
      return;
    }

    protected async Task UnlockAsync()
    {
      await UpdateAsync();
      await _actor.Ask<Empty>(new InternalMessages.AskUnlock());
      _locked = false;
      return;
    }

    protected async Task WaitAsync(string conditional)
    {
      if (!_locked)
      {
        throw new DistributedLockingException("WaitAsync() can be called only when object is locked (use LockAsync() first)");
      }
      if (!Conditionals.Contains(conditional))
      {
        throw new DistributedLockingException($"Conditional [{conditional}] not defined");
      }
      await _actor.Ask(new InternalMessages.AskWait(conditional));
      return;
    }

    protected async Task PulseAsync(string conditional)
    {
      if (!_locked)
      {
        throw new DistributedLockingException("WaitAsync() can be called only when object is locked (use LockAsync() first)");
      }
      if (!Conditionals.Contains(conditional))
      {
        throw new DistributedLockingException($"Conditional [{conditional}] not defined");
      }
      await _actor.Ask(new InternalMessages.AskPulse(conditional));
      await UpdateAsync();
      return;
    }

    protected async Task PulseAllAsync(string conditional)
    {
      if (!_locked)
      {
        throw new DistributedLockingException("WaitAsync() can be called only when object is locked (use LockAsync() first)");
      }
      if (!Conditionals.Contains(conditional))
      {
        throw new DistributedLockingException($"Conditional [{conditional}] not defined");
      }

      await UpdateAsync();
      return;
    }

    private async Task UpdateAsync()
    {
      await _actor.Ask<Empty>(new InternalMessages.AskUpdateObject(JsonData));
      return;
    }
  }
}
