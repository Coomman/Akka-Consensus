using BenchmarkDotNet.Running;
using AkkaNetConsensus.Benchmarks;

//BenchmarkRunner.Run<ConsensusBenchmark>();

//await ManualBenchmark.RunSystemSizeBench();
//await ManualBenchmark.RunLeaderLifetimeBench();
await ManualBenchmark.RunCrashProbabilityBench();
