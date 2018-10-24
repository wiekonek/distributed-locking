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
    public Dictionary<string, Queue<Address>> Conditionals;

    public Token(Dictionary<Address, int> lastRequestNumber, Queue<Address> queue, Dictionary<string, Queue<Address>> conditionals = null)
    {
      LastRequestNumber = lastRequestNumber;
      Queue = queue;
      Conditionals = conditionals;
    }

    public ExternalMessages.TokenMsg ToTokenMessage()
    {
      return new ExternalMessages.TokenMsg(
        LastRequestNumber.ToDictionary(pair => pair.Key.ToString(), pair => pair.Value),
        Queue.Select(x => x.ToString()).ToList(),
        Conditionals.ToDictionary(pair => pair.Key, pair => pair.Value.Select(x => x.ToString()).ToList())
      );
    }
  }
}
