using DistributedMonitor;

namespace ExampleScenarios
{
  class Program
  {
    static void Main(string[] args)
    {
      var systemName = "distributed-environment-name";
      var conf = Config.GetDockerConfigString(systemName);
      new DistributedEnvironment(systemName, conf);
      while (true) { }
    }
  }
}
