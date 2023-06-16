namespace AkkaNetConsensus.Messages;

public record GatherMsg(int Index, int Ballot, int EstBallot, int? Est);
