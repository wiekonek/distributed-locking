using System;
using System.Linq;
using System.Net;

namespace ExampleScenarios
{
  internal class Config
  {
    /// <summary>
    /// From docker-compose.yml file
    /// </summary>
    private const string HOSTNAME = "HOSTNAME";

    /// <summary>
    /// From docker-compose.yml file
    /// </summary>
    private const string IS_SEED = "IS_AKKA_SEED";

    /// <summary>
    /// From docker-compose.yml file
    /// </summary>
    private const string MASTER_HOSTNAME = "master";

    public static string GetDockerConfigString(string systemName)
    {
      if (string.IsNullOrWhiteSpace(systemName))
      {
        throw new ArgumentException("System name must be not empty string!", "systemName");
      }

      string hostname;
      bool isSeed;
      IPAddress myIp, masterIp;

      try
      {
        hostname = Environment.GetEnvironmentVariable(HOSTNAME);
        isSeed = bool.Parse(Environment.GetEnvironmentVariable(IS_SEED));
      }
      catch (Exception e)
      {
        throw new Exception("Unconfigured environment variables!", e);
      }

      try
      {
        myIp = Dns.GetHostAddresses(hostname).First();
        masterIp = Dns.GetHostAddresses(MASTER_HOSTNAME).First();
      }
      catch (Exception e)
      {
        throw new Exception("Unconfigured network!", e);
      }
      return
        @" 
        akka {
          actor.provider = cluster
          remote {
            dot-netty.tcp {
              port = " + (isSeed ? "8081" : "0") + @"
              hostname = " + myIp + @"
            }
          }
          cluster {
            metrics.enabled = off
            seed-nodes = [""akka.tcp://" + systemName + @"@" + masterIp + @":8081""]
            auto-down-unreachable-after = 5s
          }
        }
        ";
    }

  }
}
