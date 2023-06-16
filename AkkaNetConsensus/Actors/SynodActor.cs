﻿using Akka.Actor;
using Akka.Event;
using AkkaNetConsensus.Messages;

namespace AkkaNetConsensus.Actors;

public abstract class SynodActor : ReceiveActor
{
    protected readonly ILoggingAdapter Logger = Context.GetLogger();
    protected readonly bool LogMessages;
    
    private readonly int _i;
    private readonly int _n;
    private readonly int _quorum;
    private readonly int _initialProposal;
    
    private int? _proposal;
    private int? _estimate;
    private int _ballot;
    private int _readBallot;
    private int _imposeBallot;

    private (int? est, int estBallot)[] _states;
    private int _gathersCount;
    private int _acksCount;

    private int? _decidedValue;

    private bool _canPropose = true;

    protected SynodActor(int i, int n, bool logMessages)
    {
        LogMessages = logMessages;
        
        _i = i;
        _n = n;
        _quorum = n / 2 + 1;
        _initialProposal = Random.Shared.Next(0, 2);
        
        _ballot = i - n;
        _proposal = null;
        _estimate = null;
        _readBallot = 0;
        _imposeBallot = i - n;
        _states = Enumerable.Repeat(0, n).Select(_ => ((int?)null, 0)).ToArray();

        Receive<ProposeMsg>(OnPropose);
        Receive<ReadMsg>(OnRead);
        Receive<GatherMsg>(OnGather);
        Receive<ImposeMsg>(OnImpose);
        Receive<AckMsg>(OnAck);
        Receive<DecideMsg>(OnDecide);
        Receive<AbortMsg>(OnAbort);
    }

    private void OnPropose(ProposeMsg message)
    {
        if (_decidedValue is not null)
        {
            Context.Parent.Tell(new DecideMsg(_decidedValue.Value), Self);
            return;
        }

        if (!_canPropose)
            return;

        if (!BeforeReceive(message))
            return;

        _proposal = _initialProposal;
        _ballot += _n;
        _states = Enumerable.Repeat(0, _n).Select(_ => ((int?)null, 0)).ToArray();
        _acksCount = 0;
        _gathersCount = 0;

        Broadcast(new ReadMsg(_ballot));
        _canPropose = false;
    }

    private void OnRead(ReadMsg message)
    {
        if (!BeforeReceive(message))
            return;

        if (_readBallot > message.Ballot || _imposeBallot > message.Ballot)
        {
            Sender.Tell(new AbortMsg(message.Ballot), Self);
        }
        else
        {
            _readBallot = message.Ballot;
            Sender.Tell(new GatherMsg( _i, message.Ballot, _imposeBallot, _estimate), Self);
        }
    }

    private void OnGather(GatherMsg message)
    {
        if (!BeforeReceive(message))
            return;

        if (message.Ballot != _ballot)
            return;

        ++_gathersCount;
        
        _states[message.Index] = (message.Est, message.EstBallot);

        if (++_gathersCount >= _quorum)
        {
            if (_states.Any(x => x.estBallot > 0))
            {
                _proposal = _states.MaxBy(x => x.estBallot).est;
            }

            _states = Enumerable.Repeat(0, _n).Select(_ => ((int?)null, 0)).ToArray();
            Broadcast(new ImposeMsg(message.Ballot, _proposal));
        }
    }

    private void OnImpose(ImposeMsg message)
    {
        if (!BeforeReceive(message))
            return;

        if (_readBallot > message.Ballot || _imposeBallot > message.Ballot)
        {
            Sender.Tell(new AbortMsg(message.Ballot), Self);
        }
        else
        {
            _estimate = message.Value;
            _imposeBallot = message.Ballot;
            Sender.Tell(new AckMsg(message.Ballot), Self);
        }
    }

    private void OnAck(AckMsg message)
    {
        if (!BeforeReceive(message))
            return;
        
        if (message.Ballot != _ballot)
            return;

        if (++_acksCount >= _quorum)
        {
            Broadcast(new DecideMsg(_proposal!.Value));
        }
    }

    private void OnDecide(DecideMsg message)
    {
        if (_decidedValue is not null)
            return;
        
        if (!BeforeReceive(message))
            return;

        _decidedValue = message.Value;
            
        Context.Parent.Tell(message, Self);
        Broadcast(message);
    }

    private void OnAbort(AbortMsg message)
    {
        if (!BeforeReceive(message))
            return;
        
        if (message.Ballot != _ballot)
            return;

        _canPropose = true;
    }

    private void Broadcast<T>(T message)
    {
        Context.ActorSelection("../*").Tell(message, Self);
    }
    
    protected abstract bool BeforeReceive<T>(T message);
}
