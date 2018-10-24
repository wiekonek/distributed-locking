using Akka.Actor;
using Akka.Cluster;
using Akka.Configuration;
using Akka.Event;
using DistributedMonitor;
using DistributedMonitor.Primitives;
using System;
using System.Threading.Tasks;

namespace ExampleScenarios
{
  class Program
  {
    static async Task Main(string[] args)
    {
      var systemName = "distributed-environment-name";
      var conf = Config.GetDockerConfigString(systemName);
      var env = await DistributedEnvironment.Initialize(systemName, conf);

      //var dInt = new DInt("int-name", env);
      //await Task.Delay(5000);

      //for (int i = 0; i < 100; i++)
      //{
      //  Console.WriteLine(await dInt.AddAsync(1));
      //  await Task.Delay(100);
      //}


      //if (bool.Parse(Environment.GetEnvironmentVariable("IS_PROD")))
      //{
      //  for (int i = 0; i < 50; i++)
      //  {
      //    await Task.Delay(100);
      //    var count = await dInt.AddAsync(1);
      //    Console.WriteLine($"Current count: {count}");
      //  }
      //}
      //else
      //{
      //  for (int i = 0; i < 100; i++)
      //  {
      //    var whatIGet = await dInt.SubtractIfResultIsPositive(5);
      //    Console.WriteLine($"Count decreased to: {whatIGet}");
      //  }
      //}

      var hostname = Environment.GetEnvironmentVariable("HOSTNAME");
      var rand = new Random();
      var buffer = new DBuffer<string>(5, "buffer-name", env);
      await Task.Delay(5000);
      if (bool.Parse(Environment.GetEnvironmentVariable("IS_PROD")))
      {
        for (int i = 0; i < 50; i++)
        {
          await Task.Delay(TimeSpan.FromMilliseconds(rand.Next() % 500 + 100));
          await buffer.Add($"{hostname} : <{i}>");
          Console.WriteLine($"Producing >>>>>>>> [{hostname} : <{i}>]");
        }
      }
      else
      {
        for (int i = 0; i < 100; i++)
        {
          await Task.Delay(TimeSpan.FromMilliseconds(rand.Next() % 500 + 500));
          var whatIGet = await buffer.Take();
          Console.WriteLine($"Consuming <<<<<<<< [{whatIGet}]");
        }
      }

      await Task.Delay(-1);
    }
  }
}
