using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using Microsoft.AspNetCore.SignalR.Client;
using WordPlayGameClient.Model;
using WordPlayGameClient.MVVM;
using WordPlayGameClient.View;
using WordPlayGameClient.View.MainView;
using System.Diagnostics;

namespace WordPlayGameClient.ViewModel
{
    internal class MainWindowViewModel : ViewModelBase
    {
        public RelayCommand ExitAppCommand { get; }
        public RelayCommand ToggleFullscreenAppCommand { get; }
        public RelayCommand MinimizeAppCommand { get; }
        public RelayCommand ShowAboutViewCommand { get; }
        public RelayCommand ShowSettingsViewCommand { get; }
        public RelayCommand ShowMainViewCommand { get; }

        private HubConnection connection { get; set; }
        public HubConnection Connection
        {
            get { return connection; }
            set
            {
                connection = value;
                OnPropertyChanged();
            }
        }

        private bool connectedToserver;
        public bool ConnectedToServer
        {
            get { return connectedToserver; }
            set
            {
                connectedToserver = value;
                OnPropertyChanged();
                GenericReferences.References.StartVM.UsernameConfirmed.RaiseCanExecuteChanged();
            }
        }

        private bool hasInternet;
        public bool HasInternet
        {
            get { return hasInternet; }
            set
            {
                hasInternet = value;
                OnPropertyChanged();
                GenericReferences.References.StartVM.UsernameConfirmed.RaiseCanExecuteChanged();
            }
        }

        private UserControl mainView;
        public UserControl MainView
        {
            get { return mainView; }
            set
            {
                mainView = value;
                OnPropertyChanged();
            }
        }

        private string minimizeIcon;
        public string MinimizeIcon
        {
            get { return minimizeIcon; }
            set
            {
                minimizeIcon = value;
                OnPropertyChanged();
            }
        }

        public MainWindowViewModel()
        {
            Window window = App.Current.MainWindow;
            AboutView _AboutView = new AboutView();
            SettingsView _SettingsView = new SettingsView();
            NoInternetView _NoInternetView = new NoInternetView();
            MainView _MainView = new MainView();
            GenericReferences.References.AboutView = _AboutView;
            GenericReferences.References.SettingsView = _SettingsView;
            GenericReferences.References.NoInternetView = _NoInternetView;
            GenericReferences.References.MainView = _MainView;

            MainView = _MainView;

            ExitAppCommand = new RelayCommand(execute => Application.Current.Shutdown());
            ToggleFullscreenAppCommand = new RelayCommand(execute => ToggleFullscreenAppCommandMethod());
            MinimizeAppCommand = new RelayCommand(execute => window.WindowState = WindowState.Minimized);
            ShowAboutViewCommand = new RelayCommand(execute => MainView = _AboutView);
            ShowSettingsViewCommand = new RelayCommand(execute => MainView = _SettingsView);
            ShowMainViewCommand = new RelayCommand(execute => MainView = _MainView);
            SettingsService.GlobalSettings.PropertyChanged += SettingsChangedEventHandler;
            Refresh();
            MinimizeIcon = "  ⬜  ";

            this.PropertyChanged += PropertyChangedHandler;
            InitializeAsync();
        }

        public static void Refresh()
        {
            SettingsService GlobalSettings = SettingsService.GlobalSettings;
            var type = typeof(SettingsService);
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var property in properties)
            {
                if (property.CanRead && property.CanWrite)
                {
                    var value = property.GetValue(GlobalSettings);
                    property.SetValue(GlobalSettings, value);
                }
            }
        }

        private void PropertyChangedHandler(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(HasInternet))
            {
                if (!HasInternet)
                {
                    GenericReferences.References.MainVM.CurrentView = GenericReferences.References.NoInternetView;
                }
            }
        }

        public async Task InitializeAsync()
        {
           HasInternet = await CheckInternetAsync();
            if (HasInternet)
            {
                try
                {
                    await ConnectToServerAsync();
                }
                catch { GenericReferences.References.MainVM.CurrentView = GenericReferences.References.NoInternetView; Connection = null; ConnectedToServer = false; }
            }
        }

        public async Task ConnectToServerAsync()
        {
            Connection = new HubConnectionBuilder()
            .WithUrl("http://localhost:5020/server")
            .WithAutomaticReconnect()
            .Build();

            Connection.On("FinishGame", async () =>
            {
                if (GenericReferences.References.GameRoundVM.CurrentGameRound != null)
                {
                    GenericReferences.References.GameRoundVM.CurrentGameRound.GameMessages.Add("GOING BACK TO LOBBY IN 5 SECONDS...");
                    GenericReferences.References.GameRoundVM.CurrentGameRound.GameMessages = GenericReferences.References.GameRoundVM.CurrentGameRound.GameMessages;
                    await Task.Delay(5000);
                    GenericReferences.References.GameRoundVM.CurrentGameRound = null;
                    if (GenericReferences.References.PartyLobbyAdminVM.CurrentParty != null)
                        GenericReferences.References.MainVM.CurrentView = GenericReferences.References.PartyLobbyAdminView;
                    else if (GenericReferences.References.PartyLobbyVM.CurrentParty != null) GenericReferences.References.MainVM.CurrentView = GenericReferences.References.PartyLobbyView;
                    else GenericReferences.References.MainVM.CurrentView = GenericReferences.References.PartyBrowserView;
                }
            });

            Connection.On("LeaveGame", () =>
            {
                GenericReferences.References.GameRoundVM.CurrentGameRound = null;
                if (GenericReferences.References.PartyLobbyAdminVM.CurrentParty != null)
                    GenericReferences.References.MainVM.CurrentView = GenericReferences.References.PartyLobbyAdminView;
                else if (GenericReferences.References.PartyLobbyVM.CurrentParty != null) GenericReferences.References.MainVM.CurrentView = GenericReferences.References.PartyLobbyView;
                else GenericReferences.References.MainVM.CurrentView = GenericReferences.References.PartyBrowserView;
            });

            Connection.On<List<string>>("ReceiveGameMessageList", (messages) =>
            {
                if (GenericReferences.References.GameRoundVM.CurrentGameRound != null)
                    GenericReferences.References.GameRoundVM.CurrentGameRound.GameMessages = messages;
            });

            Connection.On<GameRound>("ReceiveGameRound", (Round) =>
            {
                GenericReferences.References.GameRoundVM.CurrentGameRound = Round;
            });

            Connection.On<GameRound>("GameStarting", (Round) =>
            {
                GenericReferences.References.MainVM.CurrentView = GenericReferences.References.GameRoundView;
                GenericReferences.References.GameRoundVM.CurrentGameRound = Round;
            });

            Connection.On<int, int, int, bool>("ReceiveGameProperties", (NoImpChance, OneImpChance, ManyImpChance, EnableHints) => 
            {
                if (GenericReferences.References.PartyLobbyVM.CurrentParty != null)
                {
                    GenericReferences.References.PartyLobbyVM.GameNoImpostersChance = NoImpChance;
                    GenericReferences.References.PartyLobbyVM.GameOneImpostersChance = OneImpChance;
                    GenericReferences.References.PartyLobbyVM.GameManyImpostersChance = ManyImpChance;
                    GenericReferences.References.PartyLobbyVM.GameEnableHints = (EnableHints)? "Hints enabled" : "Hints disabled";
                    GenericReferences.References.PartyLobbyVM.CurrentParty = GenericReferences.References.PartyLobbyVM.CurrentParty;
                }
            });

            Connection.On<List<Player>>("ReceivePartyPlayerList", (PlayerList) =>
            {
                if (GenericReferences.References.PartyLobbyAdminVM.CurrentParty != null)
                {
                    GenericReferences.References.PartyLobbyAdminVM.CurrentParty.Players = PlayerList;
                    GenericReferences.References.PartyLobbyAdminVM.CurrentParty = GenericReferences.References.PartyLobbyAdminVM.CurrentParty;
                }
                if (GenericReferences.References.PartyLobbyVM.CurrentParty != null)
                {
                    GenericReferences.References.PartyLobbyVM.CurrentParty.Players = PlayerList;
                    GenericReferences.References.PartyLobbyVM.CurrentParty = GenericReferences.References.PartyLobbyVM.CurrentParty;
                }
            });

            Connection.On("LeaveParty", () =>
            {
                if (GenericReferences.References.PartyLobbyAdminVM.CurrentParty != null)
                {
                    GenericReferences.References.PartyLobbyAdminVM.CurrentParty = null;
                    GenericReferences.References.MainVM.CurrentView = GenericReferences.References.PartyBrowserView;
                }
                if (GenericReferences.References.PartyLobbyVM.CurrentParty != null)
                {
                    GenericReferences.References.PartyLobbyVM.CurrentParty = null;
                    GenericReferences.References.MainVM.CurrentView = GenericReferences.References.PartyBrowserView;
                }
            });

            Connection.On<List<string>>("ReceivePartyMessageList", (messages) =>
            {
                if (GenericReferences.References.PartyLobbyAdminVM.CurrentParty != null)
                    GenericReferences.References.PartyLobbyAdminVM.CurrentParty.Messages = messages;
                if (GenericReferences.References.PartyLobbyVM.CurrentParty != null)
                    GenericReferences.References.PartyLobbyVM.CurrentParty.Messages = messages;
            });

            Connection.On<string>("ReceiveMessageDebug", (message) =>
            {
                Debug.WriteLine(message);
            });

            Connection.On<InformationPackage>("ReceiveInformationPackage", (InfoPackage) =>
            {
                GenericReferences.References.PartyBrowserVM.Parties = new ObservableCollection<Party>(InfoPackage.PartiesList);
                GenericReferences.References.PartyBrowserVM.TotalPlayerCount = InfoPackage.TotalPlayerCount;
                GenericReferences.References.PartyBrowserVM.WaitingPlayerCount = InfoPackage.WaitingPlayerCount;
                GenericReferences.References.PartyBrowserVM.TotalPartyCount = InfoPackage.TotalPartyCount;
                GenericReferences.References.PartyBrowserVM.WaitingPartyCount = InfoPackage.WaitingPartyCount;
            });

            Connection.Reconnected += async (ConnectionId) =>
            {
                HasInternet = true;
                ConnectedToServer = true;
                Debug.WriteLine($"Reconnected under {ConnectionId}");
            };

            Connection.Reconnecting += async (exception) =>
            {
                HasInternet = await CheckInternetAsync();
                ConnectedToServer = false;
                Debug.WriteLine($"Reconnecting to server, HasInternet: {HasInternet}");
            };

            Connection.Closed += async (exception) =>
            {
                HasInternet = false;
                ConnectedToServer = false;
                Connection = null;
                GenericReferences.References.Username = null;
                Debug.WriteLine($"Reconnection failed");
            };
            
            await Connection.StartAsync();
            ConnectedToServer = true;
            Debug.WriteLine($"Starting connection under {Connection.ConnectionId}");
            GenericReferences.References.MainVM.CurrentView = GenericReferences.References.StartView;
        }

        public async Task WaitForInternetAsync()
        {
            GenericReferences.References.MainVM.CurrentView = GenericReferences.References.NoInternetView;
        }

        private void ToggleFullscreenAppCommandMethod()
        {
            Window window = App.Current.MainWindow;
            window.WindowState = (window.WindowState == WindowState.Maximized) ? WindowState.Normal : WindowState.Maximized;
            MinimizeIcon = (window.WindowState == WindowState.Maximized) ? "  🗗  " : "  ⬜  ";
        }
    }
}