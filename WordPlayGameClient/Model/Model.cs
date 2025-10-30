using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace WordPlayGameClient.Model
{
    public sealed class Player
    {
        public string Username { get; set; }
        public string ConnectionId { get; set; }
    }

    public sealed class Party : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public string? Name { get; set; }

        private string hostName;
        public string HostName
        {
            get { return hostName; }
            set
            {
                hostName = value;
                if (string.IsNullOrEmpty(Name))
                    Name = $"{value}'s Party";
            }
        }

        public string? Description { get; set; } = "";

        public int MaxPlayerCount { get; set; }

        private List<Player> players;
        public List<Player> Players
        {
            get { return players; }
            set
            {
                players = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Players)));
            }
        }

        public int PlayerCount { get; set; } = 0;

        private bool englishOnly;
        public bool EnglishOnly
        {
            get { return englishOnly; }
            set
            {
                englishOnly = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(EnglishOnly)));
            }
        }
        public string EnglishOnlyString { get; set; } = "";

        public int? Id { get; set; }

        private List<string> messages = new List<string>();
        public List<string> Messages
        {
            get { return messages; }
            set
            {
                messages = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Messages)));
            }
        }

        public bool InGame { get; set; } = false;

        public Party()
        {
            this.PropertyChanged += PropertyChangedHandler;
            if (string.IsNullOrEmpty(Name))
                Name = $"{HostName}'s Party";
            if (Players == null)
                Players = new List<Player>();
        }

        private void PropertyChangedHandler(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Players))
                PlayerCount = Players.Count;
            if (e.PropertyName == nameof(EnglishOnly))
                if (EnglishOnly)
                    EnglishOnlyString = "English only";
                else EnglishOnlyString = "";
        }
    }

    public sealed class InformationPackage
    {
        public List<Party> PartiesList { get; set; }

        public int TotalPlayerCount { get; set; }
        public int WaitingPlayerCount { get; set; }
        public int TotalPartyCount { get; set; }
        public int WaitingPartyCount { get; set; }
    }

    public sealed class GameRound : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public int Id { get; set; }

        public List<Player> RemainingPlayers { get; set; }

        private List<Player> players { get; set; } = new List<Player>();
        public List<Player> Players
        {
            get { return players; }
            set
            {
                players = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Players)));
            }
        }
        public List<Player> Imposters { get; set; } = new List<Player>();
        public List<Player> ThoseWhoKnow { get; set; } = new List<Player>();

        private List<Player> turnOrder = new List<Player>();
        public List<Player> TurnOrder
        {
            get { return turnOrder; }
            set
            {
                turnOrder = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TurnOrder)));
            }
        }

        public List<string> Votes { get; set; } = new List<string>();
        public List<Player> VotePlayerList { get; set; } = new List<Player>();

        private string secretWord;
        public string SecretWord
        {
            get { return secretWord; }
            set
            {
                secretWord = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SecretWord)));
            }
        }

        private string selectedHint = "";
        public string SelectedHint
        {
            get { return selectedHint; }
            set
            {
                selectedHint = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedHint)));
            }
        }

        public bool HintsEnabled { get; set; }
        public int PlayerCount { get; set; }

        private List<string> gameMessages = new List<string>();
        public List<string> GameMessages
        {
            get { return gameMessages; }
            set
            {
                gameMessages = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(GameMessages)));
            }
        }

        private ObservableCollection<string> shownMessages = new ObservableCollection<string>();
        public ObservableCollection<string> ShownMessages
        {
            get { return shownMessages; }
            set
            {
                shownMessages = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ShownMessages)));
            }
        }

        public GameRound()
        {
            this.PropertyChanged += PropertyChangedHandler;
            PlayerCount = Players.Count;
            TurnOrder = TurnOrder;
        }

        private void PropertyChangedHandler(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Players))
                PlayerCount = Players.Count;
            if (e.PropertyName == nameof(GameMessages))
                ShownMessages = new ObservableCollection<string>(GameMessages);
        }
    }
}