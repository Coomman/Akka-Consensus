using Akka.Actor;
using AkkaNetConsensus.Actors;
using AkkaNetConsensus.Messages;

namespace AkkaNetConsensus.Benchmarks;

public static class Runner
{
    public static async Task<(TimeSpan TimeSpan, int MessagesSent, int DecidesCount)> Consensus(
        int systemSize, int leaderLifetime, double failureProb, bool logMessages)
    {
        var random = new Random(69);

        using var system = ActorSystem.Create("Local");

        var actor = system.ActorOf(
            ConsensusActor.Props(systemSize, systemSize / 2 - 1, failureProb, logMessages),
            "Consensus");

        var timeSpan = await Routine(random, actor, systemSize, leaderLifetime);

        await Task.Delay(5000);
        var (messagesSent, decidesCount) = await actor.Ask<SentMessagesMsg>(new SentMessagesMsg(0, 0));

        return (timeSpan, messagesSent, decidesCount);
    }

    private static async Task<TimeSpan> Routine(Random random, IActorRef actor, int systemSize, int leaderLifetime)
    {
        while (true)
        {
            var newLeader = random.Next(0, systemSize / 2 + 1);

            var leaderElectionTime = DateTimeOffset.Now;
            while(DateTimeOffset.Now - leaderElectionTime < TimeSpan.FromMilliseconds(leaderLifetime))
            {
                var result = await actor.Ask<ConsensusMsg>(new LaunchMsg(newLeader));
        
                if (result.ConsensusReached)
                {
                    return result.Elapsed;
                }

                await Task.Delay(50);
            }
        }
    }
}
