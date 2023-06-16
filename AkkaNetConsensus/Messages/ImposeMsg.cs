namespace AkkaNetConsensus.Messages;

public record ImposeMsg(int Ballot, int? Value);
