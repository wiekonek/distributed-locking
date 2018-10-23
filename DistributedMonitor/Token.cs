using Akka.Actor;
using DistributedMonitor.Actors.Messages;
using System.Collections.Generic;
using System.Linq;

namespace DistributedMonitor
{
  internal class Token
  {
    public Dictionary<Address, int> LastRequestNumber;
    public Queue<Address> Queue;

    public Token(Dictionary<Address, int> lastRequestNumber, Queue<Address> queue)
    {
      LastRequestNumber = lastRequestNumber;
      Queue = queue;
    }

    public ExternalMessages.TokenMsg ToTokenMessage()
    {
      return new ExternalMessages.TokenMsg(
        LastRequestNumber.ToDictionary(pair => pair.Key.ToString(), pair => pair.Value),
        Queue.Select(x => x.ToString()).ToList()
      );
    }
  }
}
