using Akka.Actor;
using AkkaNetConsensus.Actors;
using AkkaNetConsensus.Messages;

namespace AkkaNetConsensus.Benchmarks;

public static class Runner
{
    public static async Task<TimeSpan> Consensus(int systemSize, int leaderLifetime, double failureProb, bool logMessages)
    {
        var random = new Random(69);
        
        using var system = ActorSystem.Create("Local");

        var actor = system.ActorOf(ConsensusActor.Props(systemSize, systemSize / 2 - 1, failureProb, logMessages), "Consensus");
        
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
