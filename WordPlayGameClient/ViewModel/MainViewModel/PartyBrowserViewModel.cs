using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Microsoft.AspNetCore.SignalR.Client;
using WordPlayGameClient.Model;
using WordPlayGameClient.MVVM;
using System.Diagnostics;

namespace WordPlayGameClient.ViewModel.MainViewModel
{
    internal class PartyBrowserViewModel : ViewModelBase
    {
        public RelayCommand CreatePartyCommand { get; }
        public RelayCommand JoinPartyCommand { get; }

        private ObservableCollection<Party> parties;
        public ObservableCollection<Party> Parties
        {
            get { return parties; }
            set
            {
                parties = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<Party> shownParties;
        public ObservableCollection<Party> ShownParties
        {
            get { return shownParties; }
            set
            {
                shownParties = value;
                OnPropertyChanged();
            }
        }

        private bool createPartyCooldown;
        public bool CreatePartyCooldown
        {
            get { return createPartyCooldown; }
            set
            {
                createPartyCooldown = value;
                OnPropertyChanged();
                CreatePartyCommand.RaiseCanExecuteChanged();
            }
        }

        private bool joinPartyCooldown;
        public bool JoinPartyCooldown
        {
            get { return joinPartyCooldown; }
            set
            {
                joinPartyCooldown = value;
                OnPropertyChanged();
                JoinPartyCommand.RaiseCanExecuteChanged();
            }
        }

        private string nameSearchText = "";
        public string NameSearchText
        {
            get { return nameSearchText; }
            set
            {
                nameSearchText = value;
                NameDisplayText = string.IsNullOrEmpty(value) ? " Party name" : "";
                OnPropertyChanged();
            }
        }

        private string nameDisplayText = " Party name";
        public string NameDisplayText
        {
            get { return nameDisplayText; }
            set
            {
                nameDisplayText = value;
                OnPropertyChanged();
            }
        }

        private string hostNameSearchText = "";
        public string HostNameSearchText
        {
            get { return hostNameSearchText; }
            set
            {
                hostNameSearchText = value;
                HostNameDisplayText = string.IsNullOrEmpty(value) ? " Host name" : "";
                OnPropertyChanged();
            }
        }

        private string hostNameDisplayText = " Host name";
        public string HostNameDisplayText
        {
            get { return hostNameDisplayText; }
            set
            {
                hostNameDisplayText = value;
                OnPropertyChanged();
            }
        }

        private string englishOnlySearchText = "Show all";
        public string EnglishOnlySearchText
        {
            get { return englishOnlySearchText; }
            set
            {
                englishOnlySearchText = value;
                OnPropertyChanged();
            }
        }

        private int totalPlayerCount;
        public int TotalPlayerCount
        {
            get { return totalPlayerCount; }
            set
            {
                totalPlayerCount = value;
                OnPropertyChanged();
            }
        }

        private int waitingPlayerCount;
        public int WaitingPlayerCount
        {
            get { return waitingPlayerCount; }
            set
            {
                waitingPlayerCount = value;
                OnPropertyChanged();
            }
        }

        private int totalPartyCount;
        public int TotalPartyCount
        {
            get { return totalPartyCount; }
            set
            {
                totalPartyCount = value;
                OnPropertyChanged();
            }
        }

        private int waitingPartyCount;
        public int WaitingPartyCount
        {
            get { return waitingPartyCount; }
            set
            {
                waitingPartyCount = value;
                OnPropertyChanged();
            }
        }

        private string partyCreatorName;
        public string PartyCreatorName
        {
            get { return partyCreatorName; }
            set
            {
                partyCreatorName = value;
                OnPropertyChanged();
            }
        }

        private bool partyCreatorEnglishOnly;
        public bool PartyCreatorEnglishOnly
        {
            get { return partyCreatorEnglishOnly; }
            set
            {
                partyCreatorEnglishOnly = value;
                OnPropertyChanged();
            }
        }

        private int partyCreatorMaxPlayerCount = 3;
        public int PartyCreatorMaxPlayerCount
        {
            get { return partyCreatorMaxPlayerCount; }
            set
            {
                if (value > 2)
                {
                    partyCreatorMaxPlayerCount = value;
                }
                else { partyCreatorMaxPlayerCount = 3; }
                OnPropertyChanged();
            }
        }

        private string partyCreatorDescription;
        public string PartyCreatorDescription
        {
            get { return partyCreatorDescription; }
            set
            {
                partyCreatorDescription = value;
                OnPropertyChanged();
            }
        }

        public PartyBrowserViewModel()
        {
            SettingsService.GlobalSettings.PropertyChanged += SettingsChangedEventHandler;
            CreatePartyCommand = new RelayCommand(execute => CreatePartyAsync(), canExecute => !CreatePartyCooldown);
            JoinPartyCommand = new RelayCommand(execute => { if (execute is int id) JoinPartyAsync(id); }, canExecute => !JoinPartyCooldown && ((canExecute is int id) ? Parties.FirstOrDefault(p => p.Id == id) != null : false) ? Parties.FirstOrDefault(p => p.Id == id).MaxPlayerCount > Parties.FirstOrDefault(p => p.Id == id).PlayerCount : false );
            this.PropertyChanged += PropertyChangedHandler;
        }

        private void PropertyChangedHandler(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(NameSearchText) || e.PropertyName == nameof(HostNameSearchText) || e.PropertyName == nameof(EnglishOnlySearchText) || e.PropertyName == nameof(Parties))
            {
                if (Parties.Count > 0)
                {
                    switch (EnglishOnlySearchText)
                    {
                        case "Show all": ShownParties = new ObservableCollection<Party>(Parties.Where(p => p.Name.ToLower().Contains(NameSearchText.ToLower()) && p.HostName.ToLower().Contains(HostNameSearchText.ToLower()) && !p.InGame)); break;
                        case "Show english only": ShownParties = new ObservableCollection<Party>(Parties.Where(p => p.Name.ToLower().Contains(NameSearchText.ToLower()) && p.HostName.ToLower().Contains(HostNameSearchText.ToLower()) && p.EnglishOnly == true && !p.InGame)); break;
                        case "Exclude english only": ShownParties = new ObservableCollection<Party>(Parties.Where(p => p.Name.ToLower().Contains(NameSearchText.ToLower()) && p.HostName.ToLower().Contains(HostNameSearchText.ToLower()) && p.EnglishOnly == false && !p.InGame)); break;
                    }
                }
                else ShownParties = new ObservableCollection<Party>();
            }
        }

        public async Task CreatePartyAsync()
        {
            CreatePartyCooldown = true;
            Party CreatedParty = new Party() { Name = PartyCreatorName, HostName = GenericReferences.References.Username, EnglishOnly = PartyCreatorEnglishOnly, Description = PartyCreatorDescription, MaxPlayerCount = PartyCreatorMaxPlayerCount, Players = new List<Player>() { new Player() { Username = GenericReferences.References.Username, ConnectionId = GenericReferences.References.MainWindowVM.Connection.ConnectionId } } }; 
            CreatedParty.Id = await GenericReferences.References.MainWindowVM.Connection.InvokeAsync<int>("CreatePartyAsync", CreatedParty);
            GenericReferences.References.MainVM.CurrentView = GenericReferences.References.PartyLobbyAdminView;
            GenericReferences.References.PartyLobbyAdminVM.CurrentParty = CreatedParty;
            GenericReferences.References.PartyLobbyAdminVM.RaisePartyCreated();
            CreatePartyCooldown = false;
        }

        public async Task JoinPartyAsync(int id)
        {
            JoinPartyCooldown = true;
            GenericReferences.References.PartyLobbyVM.CurrentParty = await GenericReferences.References.MainWindowVM.Connection.InvokeAsync<Party>("JoinPartyAsync", id);
            GenericReferences.References.MainVM.CurrentView = GenericReferences.References.PartyLobbyView;
            GenericReferences.References.PartyLobbyVM.RaisePartyJoined();
            JoinPartyCooldown = false;
        }
    }
}