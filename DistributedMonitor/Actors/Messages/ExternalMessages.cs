using Akka.Actor;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Linq;

namespace DistributedMonitor.Actors.Messages
{
  public class ExternalMessages
  {
    public interface IExternalMessage { }

    public class HelloWorld : IExternalMessage
    {
      public string Msg { get; set; }

      public HelloWorld(string msg)
      {
        Msg = msg;
      }
    }

    internal class RequestCS : IExternalMessage
    {
      public int SequenceNumber { get; set; }

      public RequestCS(int sn)
      {
        SequenceNumber = sn;
      }
    }

    internal class TokenMsg : IExternalMessage
    {
      public List<string> Queue { get; set; }
      public Dictionary<string, int> LastRequestNumber { get; set; }

      public TokenMsg(Dictionary<string, int> lastRequestNumber, List<string> queue)
      {
        Queue = queue;
        LastRequestNumber = lastRequestNumber;
      }
    }

    internal class UpdateData : IExternalMessage
    {
      public string JsonData { get; set; }

      public UpdateData(string jsonData)
      {
        JsonData = jsonData;
      }
    }
  }
}
