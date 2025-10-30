using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using Microsoft.AspNetCore.SignalR.Client;
using WordPlayGameClient.Model;
using WordPlayGameClient.MVVM;

namespace WordPlayGameClient.ViewModel.MainViewModel
{
    internal class PartyLobbyAdminViewModel : ViewModelBase
    {
        public RelayCommand SendMessageCommand { get; }
        public RelayCommand DisbandPartyCommand { get; }
        public RelayCommand KickPlayerCommand { get; }
        public RelayCommand StartGameCommand { get; }

        private Party currentParty;
        public Party CurrentParty
        {
            get { return currentParty; }
            set
            {
                currentParty = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<Player> players;
        public ObservableCollection<Player> Players
        {
            get { return players; }
            set
            {
                players = value;
                OnPropertyChanged();
            }
        }

        private int playerCount;
        public int PlayerCount
        {
            get { return playerCount;}
            set
            {
                playerCount = value;
                OnPropertyChanged();
            }
        }

        private string messageInputText;
        public string MessageInputText
        {
            get { return messageInputText; }
            set
            {
                messageInputText = value;
                OnPropertyChanged();
            }
        }

        private string gameManyImpostersChanceString = "0";
        public string GameManyImpostersChanceString
        {
            get { return gameManyImpostersChanceString; }
            set
            {
                try
                {
                    if (string.IsNullOrEmpty(value))
                    {
                        GameManyImpostersChance = 0;
                        gameManyImpostersChanceString = "0";
                    }
                    else if (Convert.ToInt16(value) > 100)
                    {
                        GameManyImpostersChance = 100;
                        gameManyImpostersChanceString = "100";
                    }
                    else
                    {
                        GameManyImpostersChance = Convert.ToInt16(value);
                        int TempVar = Convert.ToInt16(value);
                        gameManyImpostersChanceString = Convert.ToString(TempVar);
                    }
                    GenericReferences.References.MainWindowVM.Connection.SendAsync("SendGameProperties", GameNoImpostersChance, GameOneImpostersChance, GameManyImpostersChance, GameEnableHints);
                }
                catch { gameManyImpostersChanceString = Convert.ToString(GameNoImpostersChance); }
                OnPropertyChanged();
            }
        }

        private int gameManyImpostersChance = 0;
        public int GameManyImpostersChance
        {
            get { return gameManyImpostersChance; }
            set
            {
                gameManyImpostersChance = value;
                OnPropertyChanged();
                StartGameCommand.RaiseCanExecuteChanged();
            }
        }

        private string gameOneImpostersChanceString = "0";
        public string GameOneImpostersChanceString
        {
            get { return gameOneImpostersChanceString; }
            set
            {
                try
                {
                    if (string.IsNullOrEmpty(value))
                    {
                        GameOneImpostersChance = 0;
                        gameOneImpostersChanceString = "0";
                    }
                    else if (Convert.ToInt16(value) > 100)
                    {
                        GameOneImpostersChance = 100;
                        gameOneImpostersChanceString = "100";
                    }
                    else
                    {
                        GameOneImpostersChance = Convert.ToInt16(value);
                        int TempVar = Convert.ToInt16(value);
                        gameOneImpostersChanceString = Convert.ToString(TempVar);
                    }
                    GenericReferences.References.MainWindowVM.Connection.SendAsync("SendGameProperties", GameNoImpostersChance, GameOneImpostersChance, GameManyImpostersChance, GameEnableHints);
                }
                catch { gameOneImpostersChanceString = Convert.ToString(GameNoImpostersChance); }
                OnPropertyChanged();
            }
        }

        private int gameOneImpostersChance = 0;
        public int GameOneImpostersChance
        {
            get { return gameOneImpostersChance; }
            set
            {
                gameOneImpostersChance = value;
                OnPropertyChanged();
                StartGameCommand.RaiseCanExecuteChanged();
            }
        }

        private string gameNoImpostersChanceString = "0";
        public string GameNoImpostersChanceString
        {
            get { return gameNoImpostersChanceString; }
            set
            {
                try
                {
                    if (string.IsNullOrEmpty(value))
                    {
                        GameNoImpostersChance = 0;
                        gameNoImpostersChanceString = "0";
                    }
                    else if (Convert.ToInt16(value) > 100)
                    {
                        GameNoImpostersChance = 100;
                        gameNoImpostersChanceString = "100";
                    }
                    else
                    {
                        GameNoImpostersChance = Convert.ToInt16(value);
                        int TempVar = Convert.ToInt16(value);
                        gameNoImpostersChanceString = Convert.ToString(TempVar);
                    }
                    GenericReferences.References.MainWindowVM.Connection.SendAsync("SendGameProperties", GameNoImpostersChance, GameOneImpostersChance, GameManyImpostersChance, GameEnableHints);
                }
                catch { gameNoImpostersChanceString = Convert.ToString(GameNoImpostersChance); }
                OnPropertyChanged();
            }
        }

        private int gameNoImpostersChance = 0;
        public int GameNoImpostersChance
        {
            get { return gameNoImpostersChance; }
            set
            {
                gameNoImpostersChance = value;
                OnPropertyChanged();
                StartGameCommand.RaiseCanExecuteChanged();
            }
        }

        private bool gameEnableHints = false;
        public bool GameEnableHints
        {
            get { return gameEnableHints; }
            set
            {
                gameEnableHints = value;
                OnPropertyChanged();
                GenericReferences.References.MainWindowVM.Connection.SendAsync("SendGameProperties", GameNoImpostersChance, GameOneImpostersChance, GameManyImpostersChance, value);
            }
        }

        public event EventHandler PartyCreated;

        public PartyLobbyAdminViewModel()
        {
            SettingsService.GlobalSettings.PropertyChanged += SettingsChangedEventHandler;
            this.PropertyChanged += PropertyChangedHandler;
            PartyCreated += PartySetupHandlerAsync;
            SendMessageCommand = new RelayCommand(execute => SendMessageAsync());
            DisbandPartyCommand = new RelayCommand(execute => GenericReferences.References.MainWindowVM.Connection.SendAsync("DisbandPartyAsync"));
            KickPlayerCommand = new RelayCommand(execute => { if (execute is string PlayerConnectionId) GenericReferences.References.MainWindowVM.Connection.SendAsync("KickPlayerAsync", PlayerConnectionId); else Debug.WriteLine($"Failed to kick: {execute.ToString()}"); }, canExecute => (canExecute is string ConnectionId)? !(ConnectionId == GenericReferences.References.MainWindowVM.Connection.ConnectionId) : false);
            StartGameCommand = new RelayCommand(execute => StartGameAsync(), canExecute => GameNoImpostersChance + GameOneImpostersChance + GameManyImpostersChance == 100 && CurrentParty.PlayerCount >= 3);
        }

        private void PropertyChangedHandler(object caller, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(CurrentParty))
            {
                if (CurrentParty != null)
                {
                    PlayerCount = CurrentParty.Players.Count();
                    Players = new ObservableCollection<Player>(CurrentParty.Players);
                }
            }
        }

        public void RaisePartyCreated()
        {
            PartyCreated?.Invoke(this, EventArgs.Empty);
        }

        public async void PartySetupHandlerAsync(object sender, EventArgs e)
        {
            CurrentParty.Players = await GenericReferences.References.MainWindowVM.Connection.InvokeAsync<List<Player>>("GetPlayersFromPartyAsync", CurrentParty.Id);
        }

        public async Task SendMessageAsync()  
        {
            await GenericReferences.References.MainWindowVM.Connection.SendAsync("SendMessageAsync", MessageInputText);
            MessageInputText = "";
        }

        public async Task StartGameAsync()
        {
            await GenericReferences.References.MainWindowVM.Connection.SendAsync("StartGameAsync", GameEnableHints, GameNoImpostersChance, GameOneImpostersChance, GameManyImpostersChance);
            GameEnableHints = false;
            GameNoImpostersChanceString = "0";
            GameOneImpostersChanceString = "0";
            GameManyImpostersChanceString = "0";
        }
    }
}