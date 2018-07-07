using DistributedMonitor;
using DistributedMonitor.Primitives;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ProdConsCsharp
{
  class Program
  {
    static void Main(string[] args)
    {
      Thread.Sleep(2000);
      var distributedEnv = new DistributedEnvironment(GetConfig());
      var tasks = new List<Task>();
      for (int i = 0; i < 2; i++)
      {
        var task = new Task(() => Run(distributedEnv));
        tasks.Add(task);
        task.Start();
      }
    }

    static async void Run(DistributedEnvironment distributedEnv)
    {
      var rand = new Random(Task.CurrentId.Value);
      await Task.Delay(rand.Next() % 4 * 1000);
      var a = new DInt("test-int", distributedEnv);
      Console.WriteLine($"Get {a.Get()}");
      await a.Add(1);
      Console.WriteLine($"After add {a.Get()}");
      return;
    }

    static string GetConfig()
    {
      return File.ReadAllText("akka.conf");
    }
  }


}
