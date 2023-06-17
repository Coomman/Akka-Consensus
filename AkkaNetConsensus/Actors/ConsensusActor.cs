using System.Diagnostics;
using Akka.Actor;
using Akka.Event;
using AkkaNetConsensus.Messages;

namespace AkkaNetConsensus.Actors;

public class ConsensusActor : ReceiveActor
{
    private readonly ILoggingAdapter _logger = Context.GetLogger();

    private readonly IReadOnlyList<IActorRef> _actors;
    private readonly Stopwatch _sw;

    private string _lastLog = string.Empty;
    private readonly List<int> _messagesSent = new();
    
    public ConsensusActor(int totalProcesses, int faultProneProcesses, double failureProb, bool logMessages)
    {
        var correctActors = Enumerable.Range(0, totalProcesses - faultProneProcesses)
            .Select(i => Context.ActorOf(CorrectActor.Props(i, totalProcesses, logMessages), $"Correct-{i}"));
        var faultProneActors = Enumerable.Range(totalProcesses - faultProneProcesses, faultProneProcesses)
            .Select(i => Context.ActorOf(FaultProneActor.Props(i, totalProcesses, failureProb, logMessages), $"Crash-{i}"));

        _actors = correctActors.Concat(faultProneActors).ToArray();

        _sw = Stopwatch.StartNew();

        Receive<LaunchMsg>(OnLaunch);
        Receive<DecideMsg>(OnDecide);
        Receive<SentMessagesMsg>(OnSentMessages);
    }
    
    public static Props Props(int totalProcesses, int faultProneProcesses, double failureProb, bool logMessages = true)
    {
        return Akka.Actor.Props.Create(() => new ConsensusActor(totalProcesses, faultProneProcesses, failureProb, logMessages));
    }

    private void OnLaunch(LaunchMsg message)
    {
        if (!_sw.IsRunning)
        {
            Sender.Tell(new ConsensusMsg(true, _sw.Elapsed));
            return;
        }

        var leader = _actors[message.LeaderIndex];
        //Log($"New leader is {leader.Path.Name}", LogLevel.InfoLevel);
        
        leader.Tell(new ProposeMsg(), Self);
        Sender.Tell(new ConsensusMsg(false, _sw.Elapsed));
    }

    private void OnDecide(DecideMsg message)
    {
        _sw.Stop();
        
        _messagesSent.Add(message.MessagesSent);
        
        //_logger.Warning($"{Sender.Path.Name} decided value {message.Value} in {_sw.Elapsed:g}. Sent {message.MessagesSent} messages");
    }

    private void OnSentMessages(SentMessagesMsg message)
    {
        Sender.Tell(new SentMessagesMsg(_messagesSent.Sum(), _messagesSent.Count));
        
        //_logger.Warning($"Sent {(int) _messagesSent.Average()} messages on {_messagesSent.Count} processes");
    }

    private void Log(string log, LogLevel logLevel)
    {
        if (log == _lastLog)
            return;
        
        _logger.Log(logLevel, log);
        _lastLog = log;
    }
}
