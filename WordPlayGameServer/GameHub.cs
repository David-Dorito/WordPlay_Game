using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Numerics;
using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using static System.Net.Mime.MediaTypeNames;

namespace WordPlayGameServer
{
    public class GameHub : Hub<IGameClient>
    {
        private Random random { get; } = new Random();
        private HttpClient httpClient { get; } = new HttpClient();

        public static List<Player> Players { get; set; } = new List<Player>();
        public static List<Party> Parties { get; set; } = new List<Party>();
        public static List<GameRound> GameRounds { get; set; } = new List<GameRound>();
        public static int NextPartyId { get; set; } = 0;

        private int[] GetWaitingPlayerAndPartyCount()
        {
            int WaitingPlayerCount = 0;
            int WaitingPartyCount = 0;
            foreach (Party p in Parties)
            {
                if (!p.InGame)
                {
                    WaitingPartyCount++;
                    foreach (Player pl in p.Players)
                        WaitingPlayerCount++;
                }
            }
            bool PlayerInParty = false;
            foreach (Player pl in Players)
            {
                foreach (Party p in Parties)
                {
                    if (p.Players.FirstOrDefault(x => x.Username == pl.Username) != null)
                        PlayerInParty = true;
                }
                if (!PlayerInParty)
                    WaitingPlayerCount++;
                PlayerInParty = false;
            }

            return new int[] { WaitingPlayerCount, WaitingPartyCount };
        }

        private void Shuffle<T>(List<T> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = random.Next(0, i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }

        public override Task OnConnectedAsync()
        {
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            var player = Players.SingleOrDefault(p => p.ConnectionId == Context.ConnectionId);
  
            if (player == null) return base.OnDisconnectedAsync(exception);
            
            GameRound RoundToRemove = GameRounds.FirstOrDefault(r => r.Players.Any(p => p.ConnectionId == player.ConnectionId));
            if (RoundToRemove != null)
            {
                foreach (Player pl in RoundToRemove.Players)
                    Clients.Client(pl.ConnectionId).LeaveGame();
                GameRounds.Remove(RoundToRemove);
            }

            foreach (Party party in Parties)
            {
                if (party.Players.Contains(player))
                {
                    party.Players.Remove(player);
                    foreach (Player p in party.Players)
                        Clients.Client(p.ConnectionId).ReceivePartyPlayerList(party.Players);
                    party.InGame = false;
                    break;
                }
            }

            var HostedParties = Parties.Where(p => p.HostName == player.Username).ToList();
            if (HostedParties.Count > 0)
            {
                foreach (Party hostedparty in HostedParties)
                {
                    foreach (Player pl in hostedparty.Players)
                        Clients.Client(pl.ConnectionId).LeaveParty();
                    Parties.Remove(hostedparty);
                }
            }

            Players.Remove(player);

            int WaitingPlayerCount = 0;
            int WaitingPartyCount = 0;
            foreach (Party p in Parties)
            {
                if (!p.InGame)
                {
                    WaitingPartyCount++;
                    foreach (Player pl in p.Players)
                        WaitingPlayerCount++;
                }
            }
            bool PlayerInParty = false;
            foreach (Player pl in Players)
            {
                foreach (Party p in Parties)
                {
                    if (p.Players.FirstOrDefault(x => x.Username == pl.Username) != null)
                        PlayerInParty = true;
                }
                if (!PlayerInParty)
                    WaitingPlayerCount++;
                PlayerInParty = false;
            }
            InformationPackage NewInformationPackage = new InformationPackage() { PartiesList = Parties, TotalPlayerCount = Players.Count, TotalPartyCount = Parties.Count, WaitingPlayerCount = WaitingPlayerCount, WaitingPartyCount = WaitingPartyCount };

            Clients.All.ReceiveInformationPackage(NewInformationPackage);
            return base.OnDisconnectedAsync(exception);
        }

        public async Task<bool> RegisterPlayerAsync(string username)
        {
            var player = Players.FirstOrDefault(p => p.Username == username);
            if (player != null)
                return true;
            if (string.IsNullOrEmpty(username))
                return true;
            Players.Add(new Player() { Username = username, ConnectionId = Context.ConnectionId });

            int[] WaitingPlayerAndPartyCount = GetWaitingPlayerAndPartyCount();
            InformationPackage NewInformationPackage = new InformationPackage() { PartiesList = Parties, TotalPlayerCount = Players.Count, TotalPartyCount = Parties.Count, WaitingPlayerCount = WaitingPlayerAndPartyCount[0], WaitingPartyCount = WaitingPlayerAndPartyCount[1] };

            Clients.All.ReceiveInformationPackage(NewInformationPackage);
            return false;
        }

        public async Task<InformationPackage> GetPartiesAsync()
        {
            int[] WaitingPlayerAndPartyCount = GetWaitingPlayerAndPartyCount();
            InformationPackage NewInformationPackage = new InformationPackage() { PartiesList = Parties, TotalPlayerCount = Players.Count, TotalPartyCount = Parties.Count, WaitingPlayerCount = WaitingPlayerAndPartyCount[0], WaitingPartyCount = WaitingPlayerAndPartyCount[1] };

            return NewInformationPackage;
        }

        public async Task<int> CreatePartyAsync(Party CreatedParty)
        {
            var player = Players.FirstOrDefault(p => p.ConnectionId == Context.ConnectionId);
            if (player != null)
            {
                CreatedParty.Players.Clear();
                CreatedParty.Players.Add(player);
            }
            CreatedParty.Id = NextPartyId;
            NextPartyId++;
            Parties.Add(CreatedParty);

            int[] WaitingPlayerAndPartyCount = GetWaitingPlayerAndPartyCount();
            InformationPackage NewInformationPackage = new InformationPackage() { PartiesList = Parties, TotalPlayerCount = Players.Count, TotalPartyCount = Parties.Count, WaitingPlayerCount = WaitingPlayerAndPartyCount[0], WaitingPartyCount = WaitingPlayerAndPartyCount[1] };

            Clients.All.ReceiveInformationPackage(NewInformationPackage);
            return (int)CreatedParty.Id;
        }

        public async Task<List<Player>> GetPlayersFromPartyAsync(int id)
        {
            Party party = Parties.FirstOrDefault(p => p.Id == id);
            if (party != null)
                return party.Players;
            else
            {
                Player player = Players.FirstOrDefault(p => p.ConnectionId == Context.ConnectionId);
                List<Player> ErrorPlayerList = new List<Player>();
                ErrorPlayerList.Add(player);
                return ErrorPlayerList;
            }
        }
        
        public async Task SendMessageAsync(string message)
        {
            var result = Parties.Select(party => new {
                Party = party,
                Player = party.Players.FirstOrDefault(player => player.ConnectionId == Context.ConnectionId)
            }).FirstOrDefault(x => x.Player != null);

            if (result == null) return;
            
            result.Party.Messages.Add($"{result.Player?.Username}: {message}");
            foreach (Player player in result.Party.Players)
                Clients.Client(player.ConnectionId).ReceivePartyMessageList(result.Party.Messages);
        }

        public async Task DisbandPartyAsync()
        {
            var result = Parties.Select(party => new {
                Party = party,
                Player = party.Players.SingleOrDefault(player => player.ConnectionId == Context.ConnectionId && player.Username == party.HostName)
            }).SingleOrDefault(x => x.Player != null);

            if (result == null) return;

            Parties.Remove(result.Party);

            int[] WaitingPlayerAndPartyCount = GetWaitingPlayerAndPartyCount();
            InformationPackage NewInformationPackage = new InformationPackage() { PartiesList = Parties, TotalPlayerCount = Players.Count, TotalPartyCount = Parties.Count, WaitingPlayerCount = WaitingPlayerAndPartyCount[0], WaitingPartyCount = WaitingPlayerAndPartyCount[1] };

            foreach (Player player in result.Party.Players)
                Clients.Client(player.ConnectionId).LeaveParty();
            Clients.All.ReceiveInformationPackage(NewInformationPackage);
        }
         
        public async Task KickPlayerAsync(string ConnectionId)
        {
            var result = Parties.Select(party => new {
                Party = party,
                Player = party.Players.SingleOrDefault(player => player.ConnectionId == Context.ConnectionId && player.Username == party.HostName)
            }).SingleOrDefault(x => x.Player != null);

            if (result == null) return;
            
            Player PlayerToKick = result.Party.Players.SingleOrDefault(player => player.ConnectionId == ConnectionId);
            if (PlayerToKick == null) return;
            
            result.Party.Players.Remove(PlayerToKick);
            result.Party.PlayerCount = result.Party.Players.Count;
            Clients.Client(PlayerToKick.ConnectionId).LeaveParty();

            int[] WaitingPlayerAndPartyCount = GetWaitingPlayerAndPartyCount();
            InformationPackage NewInformationPackage = new InformationPackage() { PartiesList = Parties, TotalPlayerCount = Players.Count, TotalPartyCount = Parties.Count, WaitingPlayerCount = WaitingPlayerAndPartyCount[0], WaitingPartyCount = WaitingPlayerAndPartyCount[1] };

            foreach (Player player in result.Party.Players)
                Clients.Client(player.ConnectionId).ReceivePartyPlayerList(result.Party.Players);
            Clients.All.ReceiveInformationPackage(NewInformationPackage);
        }

        public async Task<Party> JoinPartyAsync(int id)
        {
            Player player = Players.SingleOrDefault(p => p.ConnectionId == Context.ConnectionId);
            Party party = Parties.SingleOrDefault(p => p.Id == id);

            if (party == null || player == null) return new Party();

            party.Players.Add(player);
            party.PlayerCount = party.Players.Count;

            int[] WaitingPlayerAndPartyCount = GetWaitingPlayerAndPartyCount();
            InformationPackage NewInformationPackage = new InformationPackage() { PartiesList = Parties, TotalPlayerCount = Players.Count, TotalPartyCount = Parties.Count, WaitingPlayerCount = WaitingPlayerAndPartyCount[0], WaitingPartyCount = WaitingPlayerAndPartyCount[1] };

            foreach (Player p in party.Players)
                Clients.Client(p.ConnectionId).ReceivePartyPlayerList(party.Players);
            Clients.All.ReceiveInformationPackage(NewInformationPackage);
            return party;
        }

        public async Task SendGameProperties(int NoImpChance, int OneImpChance, int ManyImpChance, bool EnableHints)
        {
            var result = Parties.Select(party => new {
                Party = party,
                Player = party.Players.SingleOrDefault(player => player.ConnectionId == Context.ConnectionId && player.Username == party.HostName)
            }).SingleOrDefault(x => x.Player != null);

            if (result == null) return;

            foreach (Player player in result.Party.Players)
                if (player.ConnectionId != Context.ConnectionId)
                    Clients.Client(player.ConnectionId).ReceiveGameProperties(NoImpChance, OneImpChance, ManyImpChance, EnableHints);
        }

        public async Task LeavePartyAsync()
        {
            var result = Parties.Select(party => new {
                Party = party,
                Player = party.Players.SingleOrDefault(player => player.ConnectionId == Context.ConnectionId)
            }).SingleOrDefault(x => x.Player != null);

            if (result == null) return;
            
            result.Party.Players.Remove(result.Player);
            result.Party.PlayerCount = result.Party.Players.Count;
            Clients.Client(result.Player.ConnectionId).LeaveParty();

            int[] WaitingPlayerAndPartyCount = GetWaitingPlayerAndPartyCount();
            InformationPackage NewInformationPackage = new InformationPackage() { PartiesList = Parties, TotalPlayerCount = Players.Count, TotalPartyCount = Parties.Count, WaitingPlayerCount = WaitingPlayerAndPartyCount[0], WaitingPartyCount = WaitingPlayerAndPartyCount[1] };

            foreach (Player player in result.Party.Players)
                Clients.Client(player.ConnectionId).ReceivePartyPlayerList(result.Party.Players);
            Clients.All.ReceiveInformationPackage(NewInformationPackage);
        }

        public async Task StartGameAsync(bool HintsEnabled, int NoImpChance, int OneImpChance, int ManyImpChance)
        {
            var result = Parties.Select(party => new {
                Party = party,
                Player = party.Players.SingleOrDefault(player => player.ConnectionId == Context.ConnectionId && player.Username == party.HostName)
            }).SingleOrDefault(x => x.Player != null);

            if (result == null) return;
            
            int CategoryDeciderNumber = random.Next(1, 101);
            string[] SelectedCategory = new string[] { };
            if (HintsEnabled) SelectedCategory = (CategoryDeciderNumber <= 70) ? GameWords.ValidNouns : GameWords.Adjectives;
            else
            {
                if (CategoryDeciderNumber <= 60) SelectedCategory = GameWords.ValidNouns;
                else if (CategoryDeciderNumber <= 80) SelectedCategory = GameWords.InvalidNouns;
                else SelectedCategory = GameWords.Adjectives;
            }
            string SelectedWord = SelectedCategory[random.Next(0, SelectedCategory.Length)];
            GameRound NewGameRound = new GameRound() { HintsEnabled = HintsEnabled, Players = new List<Player>(result.Party.Players), SecretWord = SelectedWord, Id = (int)result.Party.Id, RemainingPlayers = new List<Player>(result.Party.Players) };

            int RoleDeciderNumber = random.Next(0, NoImpChance + OneImpChance + ManyImpChance + 1);
            if (RoleDeciderNumber <= NoImpChance)
            {
                NewGameRound.ThoseWhoKnow = new List<Player>(result.Party.Players); 
            }
            else if (RoleDeciderNumber <= NoImpChance + OneImpChance)
            {
                NewGameRound.Imposters.Add(result.Party.Players[random.Next(0, result.Party.Players.Count)]);
                foreach (Player player in NewGameRound.Players)
                    if (!NewGameRound.Imposters.Contains(player))
                        NewGameRound.ThoseWhoKnow.Add(player);
            }
            else
            {
                foreach (Player player in NewGameRound.Players)
                    if (random.Next(1, 3) == 1) NewGameRound.ThoseWhoKnow.Add(player);
                    else NewGameRound.Imposters.Add(player);
            }

            if (HintsEnabled)
            {
                var ApiReturnRaw = await httpClient.GetStringAsync($"https://api.datamuse.com/words?rel_jjb={NewGameRound.SecretWord}&max=10");
                WordResult[] ApiReturnJson = JsonSerializer.Deserialize<WordResult[]>(ApiReturnRaw);
                if (ApiReturnJson.Length > 0) NewGameRound.SelectedHint = ApiReturnJson[random.Next(0, ApiReturnJson.Length)].word;
            }

            NewGameRound.TurnOrder = new List<Player>(NewGameRound.RemainingPlayers);
            Shuffle(NewGameRound.TurnOrder);
            GameRounds.Add(NewGameRound);
            result.Party.InGame = true;
            foreach (Player player in NewGameRound.Players)
                Clients.Client(player.ConnectionId).GameStarting(NewGameRound);

            int[] WaitingPlayerAndPartyCount = GetWaitingPlayerAndPartyCount();
            InformationPackage NewInformationPackage = new InformationPackage() { PartiesList = Parties, TotalPlayerCount = Players.Count, TotalPartyCount = Parties.Count, WaitingPlayerCount = WaitingPlayerAndPartyCount[0], WaitingPartyCount = WaitingPlayerAndPartyCount[1] };

            Clients.All.ReceiveInformationPackage(NewInformationPackage);
        }

        public async Task SendGameMessageAsync(string message)
        {
            var result = GameRounds.Select(round => new {
                Round = round,
                Player = round.Players.SingleOrDefault(player => player.ConnectionId == Context.ConnectionId)
            }).SingleOrDefault(x => x.Player != null);

            if (result == null) return;
            
            result.Round.GameMessages.Add($"{result.Player?.Username}: {message}");
            foreach (Player player in result.Round.Players)
                Clients.Client(player.ConnectionId).ReceiveGameMessageList(result.Round.GameMessages);
        }

        public async Task SubmitGameHintAsync(string hint)
        {
            var result = GameRounds.Select(round => new {
                Round = round,
                Player = round.Players.SingleOrDefault(player => player.ConnectionId == Context.ConnectionId)
            }).SingleOrDefault(x => x.Player != null);

            if (result == null) return;
            else if (!result.Round.TurnOrder.Any(p => p.ConnectionId == result.Player.ConnectionId)) return;

            result.Round.TurnOrder.Remove(result.Round.TurnOrder.FirstOrDefault(p => p.ConnectionId == result.Player.ConnectionId));
            result.Round.GameMessages.Add($"{result.Player?.Username} GAVE THE HINT: {hint}");
            foreach (Player player in result.Round.Players)
                Clients.Client(player.ConnectionId).ReceiveGameRound(result.Round);
        }

        public async Task SubmitGameGuessAsync(string guess)
        {
            
            var result = GameRounds.Select(round => new {
                Round = round,
                Player = round.Imposters.SingleOrDefault(player => player.ConnectionId == Context.ConnectionId && round.RemainingPlayers.Any(p => p.ConnectionId == Context.ConnectionId))
            }).SingleOrDefault(x => x.Player != null);

            if (result == null) return;
            
            result.Round.Votes.Clear();
            result.Round.VotePlayerList.Clear();

            bool IsRoundFinished = false;
            if (guess.ToLower() == result.Round.SecretWord.ToLower())
            {
                result.Round.GameMessages.Add($"{result.Player.Username} CORRECTLY GUESSED THE WORD {result.Round.SecretWord}, IMPOSTERS WIN\n---------------GAME END---------------");
                result.Round.RemainingPlayers.Clear();
                result.Round.TurnOrder.Clear();
                IsRoundFinished = true;
            }
            else
            {
                result.Round.GameMessages.Add($"{result.Player.Username} INCORRECTLY GUESSED {guess}\n--------------NEXT ROUND--------------");
                result.Round.RemainingPlayers.Remove(result.Round.RemainingPlayers.SingleOrDefault(p => p.ConnectionId == Context.ConnectionId));
                result.Round.TurnOrder = new List<Player>(result.Round.RemainingPlayers);
                Shuffle(result.Round.TurnOrder);
            }
            foreach (Player player in result.Round.Players)
                Clients.Client(player.ConnectionId).ReceiveGameRound(result.Round);

            if (IsRoundFinished)
            {
                foreach (Player player in result.Round.Players)
                    Clients.Client(player.ConnectionId).FinishGame();
                GameRounds.Remove(result.Round);
            }
        }

        public async Task VotePlayerAsync(string ConnectionId)
        {
            var result = GameRounds.Select(round => new {
                Round = round,
                Player = round.RemainingPlayers.SingleOrDefault(player => player.ConnectionId == Context.ConnectionId && round.TurnOrder.Count == 0 && !round.VotePlayerList.Any(p => p.ConnectionId == Context.ConnectionId))
            }).SingleOrDefault(x => x.Player != null);

            if (result == null) return;
            else if (string.IsNullOrEmpty(ConnectionId)) return;
            else if (result.Round.Players.FirstOrDefault(p => p.ConnectionId == ConnectionId) == null && ConnectionId.ToLower() != "veto" && ConnectionId.ToLower() != "allfound") return;

            if (ConnectionId.ToLower() == "veto")
            {
                result.Round.Votes.Add("veto");
                result.Round.GameMessages.Add($"{result.Player.Username} VOTED FOR VETO ({result.Round.Votes.Count(p => p == "veto")} votes)");
                result.Round.VotePlayerList.Add(result.Player);
            }
            else if (ConnectionId.ToLower() == "allfound")
            {
                result.Round.Votes.Add("allfound");
                result.Round.GameMessages.Add($"{result.Player.Username} VOTED FOR ALL FOUND ({result.Round.Votes.Count(p => p == "allfound")} votes)");
                result.Round.VotePlayerList.Add(result.Player);
            }
            else if (result.Round.Players.Any(p => p.ConnectionId == ConnectionId))
            {
                result.Round.Votes.Add(ConnectionId);
                result.Round.GameMessages.Add($"{result.Player.Username} VOTED FOR {result.Round.Players.FirstOrDefault(p => p.ConnectionId == ConnectionId)?.Username} ({result.Round.Votes.Count(p => p == ConnectionId)} votes)");
                result.Round.VotePlayerList.Add(result.Player);
            }

            bool EndRound = false;
            if (result.Round.Votes.Count == result.Round.RemainingPlayers.Count)
            {
                string MostVotedName = "";
                int MostVotedCount = 0;
                foreach (string vote in result.Round.Votes)
                {
                    if (result.Round.Votes.Count(p => p == vote) > MostVotedCount)
                    {
                        MostVotedCount = result.Round.Votes.Count(p => p == vote);
                        MostVotedName = vote;
                    }
                }
                foreach (string vote in result.Round.Votes)
                {
                    if (result.Round.Votes.Count(p => p == vote) == MostVotedCount && vote != MostVotedName)
                    {
                        MostVotedName = "";
                        MostVotedCount = 0;
                        break;
                    }
                }

                if (MostVotedName == "" && MostVotedCount == 0) 
                {
                    result.Round.GameMessages.Add("DRAW IN VOTES\n--------------NEXT ROUND--------------");
                    result.Round.TurnOrder = new List<Player>(result.Round.RemainingPlayers);
                    Shuffle(result.Round.TurnOrder);
                    result.Round.VotePlayerList.Clear();
                    result.Round.Votes.Clear();
                }
                else
                {
                    result.Round.GameMessages.Add($"MOST VOTED: {MostVotedName} ({MostVotedCount} votes)");
                    if (MostVotedName == "veto") 
                    {
                        result.Round.GameMessages.Add("--------------NEXT ROUND--------------");
                        result.Round.TurnOrder = new List<Player>(result.Round.RemainingPlayers);
                        Shuffle(result.Round.TurnOrder);
                        result.Round.VotePlayerList.Clear();
                        result.Round.Votes.Clear();
                    }
                    else if (MostVotedName == "allfound")
                    {
                        bool AllImpostersCaught = true;
                        foreach (Player player in result.Round.RemainingPlayers)
                            if (result.Round.Imposters.Any(p => p.Username == player.Username))
                                AllImpostersCaught = false;
                        if (AllImpostersCaught)
                            result.Round.GameMessages.Add($"ALL {result.Round.Imposters.Count} IMPOSTERS WERE CAUGHT, NORMIES WIN\n---------------GAME END---------------"); 
                        else result.Round.GameMessages.Add($"NOT ALL {result.Round.Imposters.Count} IMPOSTERS WERE CAUGHT, IMPOSTERS WIN\n---------------GAME END---------------"); 

                        EndRound = true;
                    }
                    else if (result.Round.Imposters.Any(p => p.ConnectionId == MostVotedName)) 
                    {
                        result.Round.GameMessages.Add($"{result.Round.Players.FirstOrDefault(p => p.ConnectionId == MostVotedName)?.Username} WAS AN IMPOSTER\n--------------NEXT ROUND--------------");
                        result.Round.RemainingPlayers.Remove(result.Round.RemainingPlayers.SingleOrDefault(p => p.ConnectionId == MostVotedName));
                        result.Round.TurnOrder = new List<Player>(result.Round.RemainingPlayers);
                        Shuffle(result.Round.TurnOrder);
                        result.Round.VotePlayerList.Clear();
                        result.Round.Votes.Clear();
                    }
                    else
                    {
                        result.Round.GameMessages.Add($"{result.Round.Players.FirstOrDefault(p => p.ConnectionId == MostVotedName)?.Username} WAS NOT AN IMPOSTER\n---------------GAME END---------------");

                        EndRound = true;
                    }
                }
            }
            foreach (Player player in result.Round.Players)
                Clients.Client(player.ConnectionId).ReceiveGameRound(result.Round);

            if (EndRound)
            {
                foreach (Player player in result.Round.Players)
                    Clients.Client(player.ConnectionId).FinishGame();
                GameRounds.Remove(result.Round);
            }
        }
    }
}
