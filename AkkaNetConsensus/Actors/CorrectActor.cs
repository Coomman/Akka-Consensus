using Akka.Actor;
using Akka.Event;

namespace AkkaNetConsensus.Actors;

public class CorrectActor: SynodActor
{
    public CorrectActor(int i, int n, bool logMessages)
        : base(i, n, logMessages) { }
    
    public static Props Props(int i, int n, bool logMessages)
    {
        return Akka.Actor.Props.Create(() => new CorrectActor(i, n, logMessages));
    }

    protected override bool BeforeReceive<T>(T message)
    {
        if (LogMessages)
        {
            Logger.Info("Received {0}", message);
        }

        return true;
    }
}
