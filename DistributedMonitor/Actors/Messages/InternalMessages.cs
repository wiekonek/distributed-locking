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
      public string Conditional { get; set; }

      public AskLock(string conditional = null)
      {
        Conditional = conditional;
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
      public string[] Conditionals { get; set; }

      public Init(DistributedObject distributedObject, string[] conditionals)
      {
        DistributedObject = distributedObject;
        Conditionals = conditionals;
      }
    }

    internal class AskWait : IInternalMessage
    {
      public string Conditional { get; set; }

      public AskWait(string conditional)
      {
        Conditional = conditional;
      }
    }

    internal class AskPulse : IInternalMessage
    {
      public string Conditional { get; set; }

      public AskPulse(string conditional)
      {
        Conditional = conditional;
      }
    }

    internal class AskPulseAll : IInternalMessage
    {
      public string Conditional { get; set; }

      public AskPulseAll(string conditional)
      {
        Conditional = conditional;
      }
    }
  }
}
