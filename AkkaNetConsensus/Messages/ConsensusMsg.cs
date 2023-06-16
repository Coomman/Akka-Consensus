namespace AkkaNetConsensus.Messages;

public record ConsensusMsg(bool ConsensusReached, TimeSpan Elapsed);
