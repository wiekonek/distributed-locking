using Akka.Actor;
using Akka.Configuration;
using DistributedMonitor.Actors;
using DistributedMonitor.Actors.Messages;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

namespace DistributedMonitor
{
  public class DistributedEnvironment
  {
    internal ActorSystem DistributedSystem { get; }

    /// <summary>
    /// Create core system for distirbuted locking library.
    /// </summary>
    /// <param name="systemName">Must match system name in seed declaration inside <paramref name="config"/>.</param>
    /// <param name="config">Remember to set system name in seed path to <paramref name="systemName"/>.</param>
    public static async Task<DistributedEnvironment> Initialize(string systemName, string config)
    {
      var env = new DistributedEnvironment(systemName, config);
      await Task.Delay(TimeSpan.FromSeconds(10));
      return env;
    }


    private DistributedEnvironment(string systemName, string config)
    {
      var conf = ConfigurationFactory.ParseString(config);
      DistributedSystem = ActorSystem.Create(systemName, ConfigurationFactory.ParseString(config));
    }

    public async Task AwaitExecution() => await DistributedSystem.WhenTerminated;

  }
}
