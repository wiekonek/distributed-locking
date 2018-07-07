using Akka.Actor;
using DistributedMonitor.Actors.Messages;
using System;
using System.Collections.Generic;
using System.Text;

enum RegisterUnifierState
{

}

namespace DistributedMonitor.Actors
{
  internal class RegisterUnifier : UntypedActor
  {
    protected override void OnReceive(object message)
    {
      switch (message)
      {
        case UpdateRegisterMsg updateRegister:

          break;
        default:
          throw new DistributedLockingException($"Unknown type of msg: [{message.GetType()}]");
      }
    }
  }
}
