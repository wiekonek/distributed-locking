using Akka.Actor;

namespace DistributedMonitor.Actors.Messages
{
  internal class UpdateActorsListMsg
  {
    internal UpdateActorsListMsg(IActorRef[] actorRefs)
    {
      ActorRefs = actorRefs;
    }

    internal IActorRef[] ActorRefs { get; }
  }
}

