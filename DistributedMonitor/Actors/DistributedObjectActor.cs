using Akka.Actor;
using Akka.Cluster;
using Akka.Event;
using DistributedMonitor.Actors.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DistributedMonitor.Actors
{
  internal class DistributedObjectActor : UntypedActor
  {
    private readonly ILoggingAdapter _log = Logging.GetLogger(Context);
    private Cluster _cluster;
    private List<Member> _nodes;

    private DistributedObject _obj;
    private IActorRef _askLockResponseActor;
    private Dictionary<string, IActorRef> _askWaitResponseActors;

    private Dictionary<Address, int> _requestNumber = new Dictionary<Address, int>();
    private Token _token;
    private bool _inCriticalSection;

    private Address _address => Self.Path.Address
      .WithHost(_cluster.SelfAddress.Host)
      .WithPort(_cluster.SelfAddress.Port)
      .WithProtocol(_cluster.SelfAddress.Protocol);

    protected override void PreStart()
    {
      base.PreStart();
      _askWaitResponseActors = new Dictionary<string, IActorRef>();
      _nodes = new List<Member>();
      _cluster = Cluster.Get(Context.System);
      _cluster.Subscribe(Self, ClusterEvent.InitialStateAsEvents,
        new[]
        {
          typeof(ClusterEvent.IMemberEvent),
          typeof(ClusterEvent.UnreachableMember)
        }
      );
    }

    protected override void PostStop()
    {
      _cluster.Unsubscribe(Self);
    }

    protected override void OnReceive(object message)
    {
      switch (message)
      {
        case InternalMessages.IInternalMessage m:
          HandleInternalMessage(m);
          break;

        case ExternalMessages.IExternalMessage m:
          HandleExternalMessage(m);
          break;

        case ClusterEvent.IClusterDomainEvent e:
          HandleMemberEvents(e);
          break;

        default:
          Unhandled(message);
          break;
      }
    }

    private void HandleExternalMessage(ExternalMessages.IExternalMessage message)
    {
      switch (message)
      {

        case ExternalMessages.RequestCS request:
          //_log.Info("Receive cs request with sn:" + request.SequenceNumber);
          var senderAddr = Sender.Path.Address;
          if (request.SequenceNumber > _requestNumber[senderAddr])
          {
            _requestNumber[senderAddr] = request.SequenceNumber;
          }
          //_log.Info(_requestNumber.Select(pair => $"[{pair.Key}, {pair.Value}]").Aggregate((a, b) => a + " " + b));
          if (!_inCriticalSection &&
            _token != null &&
            _requestNumber[senderAddr] > _token.LastRequestNumber[senderAddr])
          {
            Sender.Tell(_token.ToTokenMessage());
            _token = null;
          }
          break;

        case ExternalMessages.TokenMsg token:
          _token = new Token(
            token.LastRequestNumber.ToDictionary(pair => Address.Parse(pair.Key), pair => pair.Value),
            new Queue<Address>(token.Queue.Select(x => Address.Parse(x))),
            token.Conditionals.ToDictionary(pair => pair.Key, pair => new Queue<Address>(pair.Value.Select(x => Address.Parse(x))))
          );
          if (_askLockResponseActor != null)
          {
            _inCriticalSection = true;
            //_log.Info("Entering cs");
            _askLockResponseActor.Tell(Empty.Default);
          }
          break;

        case ExternalMessages.UpdateData update:
          //_log.Info($"Updating old [{_obj.JsonData}] with new [{update.JsonData}]");
          _obj.JsonData = update.JsonData;
          Sender.Tell(Empty.Default, Self);
          break;


        case ExternalMessages.Pulse pulse:
          var addr = _askWaitResponseActors[pulse.Conditional];
          _askWaitResponseActors.Remove(pulse.Conditional);
          Self.Tell(new InternalMessages.AskLock(pulse.Conditional), addr);
          //if (_token != null)
          //{
          //  _inCriticalSection = true;
          //  Sender.Tell(Empty.Default, Self);
          //}
          //else
          //{
          //  var sn = ++_requestNumber[_address];

          //  foreach (var node in _nodes)
          //  {
          //    if (_address == node.Address)
          //    {
          //      continue;
          //    }

          //    ActorForAddress(node.Address).Tell(new ExternalMessages.RequestCS(sn));
          //  }

          //  _askLockResponseActor = Sender;
          //}


          //_askWaitResponseActors[pulse.Conditional].Tell(Empty.Default, Self);
          break;


        default:
          Unhandled(message);
          break;
      }
    }

    private void HandleInternalMessage(InternalMessages.IInternalMessage message)
    {
      switch (message)
      {
        case InternalMessages.Init init:
          _obj = init.DistributedObject;
          if (bool.Parse(Environment.GetEnvironmentVariable("IS_AKKA_SEED")))
          {
            if (_token == null)
            {
              _token = new Token(new Dictionary<Address, int>(), new Queue<Address>());
            }

            _token.Conditionals = new Dictionary<string, Queue<Address>>();
            foreach (var cond in init.Conditionals ?? Enumerable.Empty<string>())
            {
              _token.Conditionals.Add(cond, new Queue<Address>());
            }
          }
          break;

        case InternalMessages.AskWait wait:
          _log.Info($"Now I'm waiting [{_address}] in [{wait.Conditional}]");

          _token.Conditionals[wait.Conditional].Enqueue(_address);
          _askWaitResponseActors.Add(wait.Conditional, Sender);
          Self.Tell(new InternalMessages.AskUnlock());
          break;

        case InternalMessages.AskPulse pulse:

          var q = _token.Conditionals[pulse.Conditional];
          if (q.Count > 0)
          {
            _log.Info($"Pulsing [{pulse.Conditional}]");
            ActorForAddress(q.Dequeue()).Tell(new ExternalMessages.Pulse(pulse.Conditional));
          }
          Sender.Tell(Empty.Default, Self);
          break;

        case InternalMessages.AskLock l:
          if (_token != null)
          {
            _inCriticalSection = true;
            //_log.Info("Entering cs");
            //if (l == null)
            //{
            //  Sender.Tell(Empty.Default, Self);
            //} else
            //{
            //  _askWaitResponseActors[l.Conditional].Tell(Empty.Default, Self);
            //}
            Sender.Tell(Empty.Default, Self);
          }
          else
          {
            var sn = ++_requestNumber[_address];

            foreach (var node in _nodes)
            {
              if (_address == node.Address)
              {
                continue;
              }

              //_log.Info("Requesting cs with sn: " + sn);
              //_log.Info(_requestNumber.Select(pair => $"[{pair.Key}, {pair.Value}]").Aggregate((a, b) => a + " " + b));
              ActorForAddress(node.Address).Tell(new ExternalMessages.RequestCS(sn));
            }

            //if (l == null)
            //{
            //  _askLockResponseActor = Sender;
            //}
            _askLockResponseActor = Sender;
          }
          break;

        case InternalMessages.AskUnlock _:
          _token.LastRequestNumber[_address] = _requestNumber[_address];
          //_log.Info("Unlocking");
          //_log.Info(_requestNumber.Select(pair => $"[{pair.Value}]").Aggregate((a, b) => a + " " + b));
          //_log.Info(_token.LastRequestNumber.Select(pair => $"[{pair.Value}]").Aggregate((a, b) => a + " " + b));
          foreach (var reqNo in _requestNumber)
          {
            if (!_token.Queue.Contains(reqNo.Key) && reqNo.Value > _token.LastRequestNumber[reqNo.Key])
            {
              //_log.Info("Adding to queue");
              _token.Queue.Enqueue(reqNo.Key);
            }
          }
          if (_token.Queue.Count > 0)
          {
            //_log.Info("Sending token");
            ActorForAddress(_token.Queue.Dequeue())
              .Tell(_token.ToTokenMessage());
            _token = null;
          }
          _inCriticalSection = false;
          //_log.Info("Leaving cs");
          Sender.Tell(Empty.Default);
          break;

        case InternalMessages.AskUpdateObject update:
          var tasks = new List<Task>();
          foreach (var node in _nodes)
          {
            if (_address == node.Address)
            {
              continue;
            }

            tasks.Add(ActorForAddress(node.Address).Ask(new ExternalMessages.UpdateData(update.JsonData)));
          }
          Task.WhenAll(tasks).Wait();
          Sender.Tell(Empty.Default);
          break;

        default:
          Unhandled(message);
          break;
      }
    }

    private ActorSelection ActorForAddress(Address address)
    {
      address = address.WithProtocol(_cluster.SelfAddress.Protocol);
      return Context.ActorSelection($"{address}/user/{Self.Path.Name}");
    }

    private void HandleMemberEvents(ClusterEvent.IClusterDomainEvent e)
    {
      switch (e)
      {
        case ClusterEvent.MemberUp up:
          var member = up.Member;
          _log.Info($">> Member is Up: {member.Address}");

          _nodes.Add(member);
          _requestNumber.Add(member.Address, 0);
          if (bool.Parse(Environment.GetEnvironmentVariable("IS_AKKA_SEED")))
          {
            if (_token == null)
            {
              _token = new Token(new Dictionary<Address, int>(), new Queue<Address>());
            }

            _token.LastRequestNumber.Add(member.Address, 0);
          }

          break;
        case ClusterEvent.UnreachableMember unreachable:
          _log.Warning($">> Member detected as unreachable: {unreachable.Member.Address}");
          break;
        case ClusterEvent.MemberRemoved removed:
          _log.Warning($">> Member is Removed: {removed.Member.Address}");
          break;
      }
    }
  }
}
