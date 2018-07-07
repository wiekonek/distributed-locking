using Akka.Actor;
using Akka.Configuration;
using DistributedMonitor.Actors;
using DistributedMonitor.Actors.Messages;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DistributedMonitor
{
  public class DistributedEnvironment
  {
    private readonly Dictionary<string, DistributedObjectData> _register;

    internal ActorSystem DistributedSystem { get; }

    /// <summary>
    /// Create core system for distirbuted locking library.
    /// </summary>
    /// <param name="systemName">Must match system name in seed declaration inside <paramref name="config"/>.</param>
    /// <param name="config">Remember to set system name in seed path to <paramref name="systemName"/>.</param>
    public DistributedEnvironment(string systemName, string config)
    {
      var conf = ConfigurationFactory.ParseString(config);
      _register = new Dictionary<string, DistributedObjectData>();
      DistributedSystem = ActorSystem.Create(systemName, ConfigurationFactory.ParseString(config));
    }

    public DistributedEnvironment()
    {
    }

    internal IActorRef Register(DistributedObject distributedObject)
    {
      IActorRef actor;
      lock (_register)
      {


        if (!_register.ContainsKey(distributedObject.Name))
        {
          actor = DistributedSystem.ActorOf<DistributedObjectActor>(distributedObject.Name + "-0");
          actor.Tell(new InitializeActorMsg(distributedObject));
          _register.Add(
            distributedObject.Name,
            new DistributedObjectData()
            {
              Object = distributedObject,
              Actors = new[] { actor },
              Count = 1
            }
          );
        }
        else
        {
          var existingObjectData = _register[distributedObject.Name];
          Console.WriteLine($"Distributed object with name [{distributedObject.Name}] already created!");
          if (existingObjectData.Object.GetType() != distributedObject.GetType())
          {
            throw new DistributedLockingException($"Distributed object types mismatch [{existingObjectData.Object.GetType()}] and [{distributedObject.GetType()}]");
          }

          actor = DistributedSystem.ActorOf<DistributedObjectActor>($"{distributedObject.Name}-{existingObjectData.Count++}");
          actor.Tell(new InitializeActorMsg(distributedObject));
          distributedObject.JsonData = _register[distributedObject.Name].Object.JsonData;
          Console.WriteLine($"Set starting value to [{distributedObject.JsonData}]");
          var actorsRef = existingObjectData.Actors.Append(actor).ToArray();
          existingObjectData.Actors = actorsRef;
          foreach (var actorRef in actorsRef)
          {
            actorRef.Tell(new UpdateActorsListMsg(actorsRef));
          }
        }
      }
      return actor;
    }

    internal void UpdateObject(string name, string jsonData)
    {
      _register[name].Object.JsonData = jsonData;
    }

    private class DistributedObjectData
    {
      public DistributedObject Object;
      public IActorRef[] Actors;
      public int Count;
    }
  }
}
