using System;
using System.Collections.Generic;
using System.Text;

namespace DistributedMonitor.Actors.Messages
{
  internal class InitializeActorMsg
  {
    internal InitializeActorMsg(DistributedObject obj)
    {
      Obj = obj;
    }

    internal DistributedObject Obj { get; }
  }
}
