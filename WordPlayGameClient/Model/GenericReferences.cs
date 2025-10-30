using System;
using System.Collections.Generic;
using WordPlayGameClient.View;
using WordPlayGameClient.View.MainView;
using WordPlayGameClient.ViewModel;
using WordPlayGameClient.ViewModel.MainViewModel;

namespace WordPlayGameClient.Model
{
    internal class GenericReferences
    {
        public MainWindowViewModel MainWindowVM { get; set; }
        public NoInternetViewModel NoInternetVM { get; set; }
        public SettingsViewModel SettingsVM { get; set; }
        public StartViewModel StartVM { get; set; }
        public PartyBrowserViewModel PartyBrowserVM { get; set; }
        public MainViewModel MainVM { get; set; }
        public PartyLobbyViewModel PartyLobbyVM { get; set; }
        public PartyLobbyAdminViewModel PartyLobbyAdminVM { get; set; }
        public GameRoundViewModel GameRoundVM { get; set; }

        public MainWindow MainWindow { get; set; }
        public NoInternetView NoInternetView { get; set; }
        public SettingsView SettingsView { get; set; }
        public StartView StartView { get; set; }
        public PartyBrowserView PartyBrowserView { get; set; }
        public MainView MainView { get; set; }
        public AboutView AboutView { get; set; }
        public PartyLobbyView PartyLobbyView { get; set; }
        public PartyLobbyAdminView PartyLobbyAdminView { get; set; }
        public GameRoundView GameRoundView { get; set; }

        public string Username { get; set; } = "";

        private GenericReferences() { }

        public static GenericReferences References { get; set; } = new GenericReferences();
    }
}