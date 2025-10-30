using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using Microsoft.AspNetCore.SignalR.Client;
using WordPlayGameClient.Model;
using WordPlayGameClient.MVVM;

namespace WordPlayGameClient.ViewModel.MainViewModel
{
    internal class PartyLobbyViewModel : ViewModelBase
    {
        public RelayCommand LeavePartyCommand { get; }
        public RelayCommand SendMessageCommand { get; }

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
            get { return playerCount; }
            set
            {
                playerCount = value;
                OnPropertyChanged();
            }
        }

        private List<string> messages;
        public List<string> Messages
        {
            get { return messages; }
            set
            {
                messages = value;
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

        private int gameManyImpostersChance = 0;
        public int GameManyImpostersChance
        {
            get { return gameManyImpostersChance; }
            set
            {
                gameManyImpostersChance = value;
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
            }
        }

        private string gameEnableHints = "Hints disabled";
        public string GameEnableHints
        {
            get { return gameEnableHints; }
            set
            {
                gameEnableHints = value;
                OnPropertyChanged();
            }
        }

        public event EventHandler PartyJoined;

        public PartyLobbyViewModel()
        {
            SettingsService.GlobalSettings.PropertyChanged += SettingsChangedEventHandler;
            this.PropertyChanged += PropertyChangedHandler;
            PartyJoined += PartySetupHandlerAsync;

            SendMessageCommand = new RelayCommand(execute => SendMessageAsync());
            LeavePartyCommand = new RelayCommand(execute => GenericReferences.References.MainWindowVM.Connection.SendAsync("LeavePartyAsync"));
        }

        private void PropertyChangedHandler(object caller, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(CurrentParty))
            {
                if (CurrentParty != null)
                {
                    PlayerCount = CurrentParty.Players.Count();
                    Players = new ObservableCollection<Player>(CurrentParty.Players);
                    Messages = CurrentParty.Messages;
                }
            }
        }

        public void RaisePartyJoined()
        {
            PartyJoined?.Invoke(this, EventArgs.Empty);
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
    }
}