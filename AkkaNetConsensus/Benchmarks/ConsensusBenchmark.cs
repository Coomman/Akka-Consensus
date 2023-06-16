using BenchmarkDotNet.Attributes;

namespace AkkaNetConsensus.Benchmarks;

public class ConsensusBenchmark
{
    [Params(1000, 1500, 2000)]
    public int SystemSize { get; set; }
    
    [Params(500, 1000, 1500)]
    public int LeaderLifetime { get; set; }
    
    [Params(0.0, 0.1, 1.0)]
    public double FailureProbability { get; set; }

    [Benchmark]
     public async Task Consensus()
     {
         await Runner.Consensus(SystemSize, LeaderLifetime, failureProb: 0, logMessages: false);
     }

    [Benchmark]
    public async Task ConsensusWithCrashes()
    {
        await Runner.Consensus(1000, 500, FailureProbability, logMessages: false);
    }
}
