using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using WordPlayGameClient.Model;
using WordPlayGameClient.MVVM;

namespace WordPlayGameClient.ViewModel.MainViewModel
{
    internal class GameRoundViewModel : ViewModelBase
    {
        public RelayCommand SubmitHintCommand { get; }
        public RelayCommand SendGameMessageCommand { get; }
        public RelayCommand VotePlayerCommand { get; }
        public RelayCommand ToggleVotePopup { get; }
        public RelayCommand SubmitGuessCommand { get; }

        private GameRound currentGameRound;
        public GameRound CurrentGameRound
        {
            get { return currentGameRound; }
            set
            {
                currentGameRound = value;
                OnPropertyChanged();
                SubmitHintCommand.RaiseCanExecuteChanged();
                VotePlayerCommand.RaiseCanExecuteChanged();
                
            }
        }

        private ObservableCollection<Player> players = new ObservableCollection<Player>();
        public ObservableCollection<Player> Players
        {
            get { return players; }
            set
            {
                players = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<Player> waitingPlayers = new ObservableCollection<Player>();
        public ObservableCollection<Player> WaitingPlayers
        {
            get { return waitingPlayers; }
            set
            {
                waitingPlayers = value;
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

        private string hintInputText = "";
        public string HintInputText
        {
            get { return hintInputText; }
            set
            {
                try
                {
                    hintInputText = value;
                    OnPropertyChanged();
                    SubmitHintCommand.RaiseCanExecuteChanged();
                }
                catch { }
            }
        }

        private bool hintInputActive = true;
        public bool HintInputActive
        {
            get { return hintInputActive; }
            set
            {
                hintInputActive = value;
                OnPropertyChanged();
            }
        }

        private string guessInputText = "";
        public string GuessInputText
        {
            get { return guessInputText; }
            set
            {
                try
                {
                    guessInputText = value;
                    OnPropertyChanged();
                    SubmitGuessCommand.RaiseCanExecuteChanged();
                }
                catch { }
            }
        }
        
        private bool guessInputActive = true;
        public bool GuessInputActive
        {
            get { return guessInputActive; }
            set
            {
                guessInputActive = value;
                OnPropertyChanged();
            }
        }

        private string secretWordDisplayText = "";
        public string SecretWordDisplayText
        {
            get { return secretWordDisplayText; }
            set
            {
                secretWordDisplayText = value;
                OnPropertyChanged();
            }
        }

        private bool isVotePopupOpen = false;
        public bool IsVotePopupOpen
        {
            get { return isVotePopupOpen; }
            set
            {
                isVotePopupOpen = value;
                OnPropertyChanged();
            }
        }

        private List<Player> votePopupPlayerList = new List<Player>();
        public List<Player> VotePopupPlayerList
        {
            get { return votePopupPlayerList; }
            set
            {
                votePopupPlayerList = value;
                OnPropertyChanged();
            }
        }

        private bool voteBool = false;
        private bool VoteBool
        {
            get { return voteBool; }
            set
            {
                voteBool = value;
                VotePlayerCommand.RaiseCanExecuteChanged();
            }
        }

        public GameRoundViewModel()
        {
            this.PropertyChanged += PropertyChangedHandler;
            SettingsService.GlobalSettings.PropertyChanged += SettingsChangedEventHandler;
            SendGameMessageCommand = new RelayCommand(execute => SendGameMessageAsync());
            SubmitHintCommand = new RelayCommand(execute => SubmitHintAsync(), canExecute => !string.IsNullOrEmpty(HintInputText));
            ToggleVotePopup = new RelayCommand(execute => IsVotePopupOpen = !IsVotePopupOpen);
            VotePlayerCommand = new RelayCommand(execute => { if (execute is string ConnectionId) GenericReferences.References.MainWindowVM.Connection.SendAsync("VotePlayerAsync", ConnectionId); }, canExecute => VoteBool);
            SubmitGuessCommand = new RelayCommand(execute => SubmitGuessAsync(), canExecute => !string.IsNullOrEmpty(GuessInputText));
        }

        private void PropertyChangedHandler(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(CurrentGameRound))
            {
                if (CurrentGameRound != null)
                {
                    Players = new ObservableCollection<Player>(CurrentGameRound.TurnOrder);
                    WaitingPlayers = new ObservableCollection<Player>(CurrentGameRound.Players.Where(p => !Players.Any(x => x.Username == p.Username)));
                    HintInputActive = (CurrentGameRound.TurnOrder.Any(p => p.Username == GenericReferences.References.Username) ? CurrentGameRound.TurnOrder[0].ConnectionId == GenericReferences.References.MainWindowVM.Connection.ConnectionId : false);
                    GuessInputActive = CurrentGameRound.Imposters.Any(p => p.ConnectionId == GenericReferences.References.MainWindowVM.Connection.ConnectionId) && CurrentGameRound.RemainingPlayers.Any(p => p.ConnectionId == GenericReferences.References.MainWindowVM.Connection.ConnectionId);
                    SecretWordDisplayText = (CurrentGameRound.Imposters.Any(p => p.ConnectionId == GenericReferences.References.MainWindowVM.Connection.ConnectionId) ? "???" : CurrentGameRound.SecretWord);
                    VoteBool = CurrentGameRound.TurnOrder.Count == 0 && CurrentGameRound.RemainingPlayers.Any(p => p.ConnectionId == GenericReferences.References.MainWindowVM.Connection.ConnectionId) && !CurrentGameRound.VotePlayerList.Any(p => p.ConnectionId == GenericReferences.References.MainWindowVM.Connection.ConnectionId);
                    VotePopupPlayerList.Clear();
                    VotePopupPlayerList.Add(new Player() { Username = "All found", ConnectionId = "allfound" });
                    VotePopupPlayerList.Add(new Player() { Username = "Veto", ConnectionId = "veto" });
                    foreach (Player player in CurrentGameRound.RemainingPlayers)
                        VotePopupPlayerList.Add(player);
                    VotePopupPlayerList = VotePopupPlayerList;
                }
            }
        }

        public async Task SendGameMessageAsync()
        {
            await GenericReferences.References.MainWindowVM.Connection.SendAsync("SendGameMessageAsync", MessageInputText);
            MessageInputText = "";
        }

        public async Task SubmitHintAsync()
        {
            await GenericReferences.References.MainWindowVM.Connection.SendAsync("SubmitGameHintAsync", HintInputText);
            HintInputText = "";
        }

        public async Task SubmitGuessAsync()
        {
            await GenericReferences.References.MainWindowVM.Connection.SendAsync("SubmitGameGuessAsync", GuessInputText);
            GuessInputText = "";
        }
    }
}