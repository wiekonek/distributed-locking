using Akka.Actor;
using Akka.Event;
using DistributedMonitor.Actors.Messages;
using System.Collections.Generic;
using System.Linq;

namespace DistributedMonitor.Actors
{
  internal class DistributedObjectActor : UntypedActor
  {
    private readonly ILoggingAdapter _log = Logging.GetLogger(Context);
    private DistributedObject _obj;
    private IActorRef[] _otherActors = { };
    private Dictionary<ActorPath, bool> _acks = new Dictionary<ActorPath, bool>() { { Context.Self.Path, false } };

    protected override void OnReceive(object message)
    {
      switch(message)
      {
        case InitializeActorMsg init:
          _obj = init.Obj;
          _log.Info($" initialized");
          break;
        case UpdateActorsListMsg update:
          _otherActors = update.ActorRefs.Where(a => a.Path != Self.Path).ToArray();
          var tmpAcks = _acks;
          _acks = update.ActorRefs.ToDictionary(r => r.Path, r => tmpAcks.ContainsKey(r.Path) ? tmpAcks[r.Path] : false);
          _log.Info($" actors updated");
          break;

        case UpdateMeMsg updateMe:
          _log.Info($" update me with [{updateMe.Value}]");
          foreach (var actorRef in _otherActors)
          {
            actorRef.Tell(new UpdateValueMsg(updateMe.Value));
          }
          break;
        case UpdateValueMsg updateValue:
          _log.Info($" Update old [{_obj.JsonData}] with new [{updateValue.Value}] from [{Sender.Path.Name}]");
          _obj.JsonData = updateValue.Value;
          break;
        default:
          throw new DistributedLockingException($"Unknown type of msg: [{message.GetType()}]");
      }
    }
  }
}
