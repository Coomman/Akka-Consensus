namespace AkkaNetConsensus.Messages;

public record SentMessagesMsg(int MessagesSent, int DecidesCount);
