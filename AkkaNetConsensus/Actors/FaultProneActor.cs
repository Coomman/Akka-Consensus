using Akka.Actor;
using Akka.Event;

namespace AkkaNetConsensus.Actors;

public class FaultProneActor : SynodActor
{
    private readonly double _failureProb;

    private bool _hasFailed;
    
    public FaultProneActor(int i, int n, double failureProb, bool logMessages) : base(i, n, logMessages)
    {
        _failureProb = failureProb;
    }
    
    public static Props Props(int i, int n, double failureProb, bool logMessages)
    {
        return Akka.Actor.Props.Create(() => new FaultProneActor(i, n, failureProb, logMessages));
    }

    protected override bool BeforeReceive<T>(T message)
    {
        if (_hasFailed)
            return false;

        _hasFailed = Random.Shared.NextDouble() < _failureProb;
        if (_hasFailed)
        {
            Logger.Error("HAS CRASHED");
            return false;
        }

        if (LogMessages)
        {
            Logger.Info("Received {0}", message);
        }

        return true;
    }
}
