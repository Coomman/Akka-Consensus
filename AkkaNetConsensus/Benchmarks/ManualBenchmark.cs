namespace AkkaNetConsensus.Benchmarks;

public static class ManualBenchmark
{
    private const int Iterations = 5;
    private const string LogsDirectory = "../../../../logs";

    static ManualBenchmark()
    {
        Directory.CreateDirectory(LogsDirectory);
    }
    
    public static async Task RunSystemSizeBench()
    {
        await using var sw = new StreamWriter(Path.Combine(LogsDirectory, "bench_1000-2000_500ms.txt"));
        
        for (int systemSize = 1000; systemSize <= 2000; systemSize += 50)
        {
            Console.Write($"{systemSize} - ");
            double total = 0;
            
            for (int i = 0; i < Iterations; i++)
            {
                var result = await Runner.Consensus(systemSize, 500, failureProb: 0, logMessages: false);
                total += result.TimeSpan.TotalMilliseconds;
            }
            
            await sw.WriteLineAsync($"{systemSize} {(int) (total / Iterations)}");
            Console.WriteLine(total / Iterations);
        }
    }
    
    public static async Task RunLeaderLifetimeBench()
    {
        await using var sw = new StreamWriter(Path.Combine(LogsDirectory, "bench_2000_500-2000ms.txt"));
        
        for (int lifetime = 500; lifetime <= 2000; lifetime += 50)
        {
            Console.Write($"{lifetime} - ");
            double total = 0;
            
            for (int i = 0; i < Iterations; i++)
            {
                var result = await Runner.Consensus(2000, lifetime, failureProb: 0, logMessages: false);
                total += result.TimeSpan.TotalMilliseconds;
            }
            
            await sw.WriteLineAsync($"{lifetime} {(int) (total / Iterations)}");
            Console.WriteLine(total / Iterations);
        }
    }
    
    public static async Task RunCrashProbabilityBench()
    {
        await using var sw = new StreamWriter(Path.Combine(LogsDirectory, "bench_1000_500ms_with-crashes.txt"));
        
        for (double crashProb = 0.00; crashProb <= 1.01; crashProb += 0.05)
        {
            double total = 0;
            int messagesTotal = 0;
            int messagesAvg = 0;
            
            for (int i = 0; i < Iterations; i++)
            {
                var result = await Runner.Consensus(2000, 500, crashProb, logMessages: false);
                total += result.TimeSpan.TotalMilliseconds;
                messagesTotal += result.MessagesSent;
                messagesAvg += result.MessagesSent / result.DecidesCount;
            }
            
            var log = $"{crashProb:0.00} {(int) (total / Iterations)} {messagesTotal / Iterations} {messagesAvg / Iterations}";
            
            await sw.WriteLineAsync(log);
            Console.WriteLine(log);
        }
    }
}
