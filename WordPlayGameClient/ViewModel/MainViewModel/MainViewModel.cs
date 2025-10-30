using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Controls;
using WordPlayGameClient.Model;
using WordPlayGameClient.MVVM;
using WordPlayGameClient.View.MainView;

namespace WordPlayGameClient.ViewModel.MainViewModel
{
    internal class MainViewModel : ViewModelBase
    {
        private UserControl currentView;
        public UserControl CurrentView
        {
            get { return currentView; }
            set
            {
                currentView = value;
                OnPropertyChanged();
            }
        }

        public MainViewModel()
        {
            StartView _StartView = new StartView();
            PartyBrowserView _PartyBrowserView = new PartyBrowserView();
            PartyLobbyView _PartyLobbyView = new PartyLobbyView();
            PartyLobbyAdminView _PartyLobbyAdminView = new PartyLobbyAdminView();
            GameRoundView _GameRoundView = new GameRoundView();
            SettingsService.GlobalSettings.PropertyChanged += SettingsChangedEventHandler;
            GenericReferences.References.StartView = _StartView;
            GenericReferences.References.PartyBrowserView = _PartyBrowserView;
            GenericReferences.References.PartyLobbyView = _PartyLobbyView;
            GenericReferences.References.PartyLobbyAdminView = _PartyLobbyAdminView;
            GenericReferences.References.GameRoundView = _GameRoundView;
            CurrentView = _StartView;
        }
    }
}