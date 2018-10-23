using System;
using System.Collections.Generic;
using System.Text;

namespace DistributedMonitor.Actors.Messages
{
  internal class InternalMessages
  {
    internal interface IInternalMessage { };

    internal class AskLock : IInternalMessage
    {
      public AskLock()
      {
      }
    }

    internal class AskUnlock : IInternalMessage
    {
      public AskUnlock()
      {
      }
    }

    internal class AskUpdateObject : IInternalMessage
    {
      public string JsonData { get; set; }

      public AskUpdateObject(string jsonData)
      {
        JsonData = jsonData;
      }
    }

    internal class Init : IInternalMessage
    {
      public DistributedObject DistributedObject { get; set; }

      public Init(DistributedObject distributedObject)
      {
        DistributedObject = distributedObject;
      }
    }
  }
}
