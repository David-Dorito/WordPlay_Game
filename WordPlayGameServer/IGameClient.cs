namespace WordPlayGameServer
{
    public interface IGameClient
    {
        Task ReceiveMessageDebug(string message);
        Task ReceiveInformationPackage(InformationPackage InfoPackage);
        Task ReceivePartyMessageList(List<string> MessageList);
        Task LeaveParty();
        Task ReceivePartyPlayerList(List<Player> PlayerList);
        Task ReceiveGameProperties(int NoImpChance, int OneImpChance, int ManyImpChance, bool EnableHints);
        Task GameStarting(GameRound game);
        Task ReceiveGameRound(GameRound game);
        Task ReceiveGameMessageList(List<string> MessageList);
        Task LeaveGame();
        Task FinishGame();
    }
}
