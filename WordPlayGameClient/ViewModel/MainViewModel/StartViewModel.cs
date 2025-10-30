using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Microsoft.AspNetCore.SignalR.Client;
using WordPlayGameClient.Model;
using WordPlayGameClient.MVVM;

namespace WordPlayGameClient.ViewModel.MainViewModel
{

    internal class StartViewModel : ViewModelBase
    {
        public RelayCommand UsernameConfirmed { get; }

        private string displayText;
        public string DisplayText
        {
            get { return displayText; }
            set
            {
                displayText = value;
                OnPropertyChanged();
            }
        }

        private string inputText = "";
        public string InputText
        {
            get { return inputText; }
            set
            {
                inputText = value;
                DisplayText = string.IsNullOrEmpty(value) ? " Enter Username" : "";
                OnPropertyChanged();
            }
        }

        private bool registerPlayerCooldown;
        private bool RegisterPlayerCooldown
        {
            get { return registerPlayerCooldown; }
            set
            {
                registerPlayerCooldown = value;
                this.UsernameConfirmed.RaiseCanExecuteChanged();
            }
        }

        public StartViewModel()
        {
            UsernameConfirmed = new RelayCommand(execute => RegisterPlayerAsync(InputText), canExecute => GenericReferences.References.MainWindowVM.ConnectedToServer && !RegisterPlayerCooldown);
            SettingsService.GlobalSettings.PropertyChanged += SettingsChangedEventHandler;
            InputText = InputText;
        }

        private async Task RegisterPlayerAsync(string Username)
        {
            try
            {
                RegisterPlayerCooldown = true;
                bool UsernameTaken = await GenericReferences.References.MainWindowVM.Connection.InvokeAsync<bool>("RegisterPlayerAsync", Username);

                if (!UsernameTaken)
                {
                    GenericReferences.References.Username = Username;
                    GenericReferences.References.MainVM.CurrentView = GenericReferences.References.PartyBrowserView;
                    InformationPackage InfoPackage = await GenericReferences.References.MainWindowVM.Connection.InvokeAsync<InformationPackage>("GetPartiesAsync"); ;
                    GenericReferences.References.PartyBrowserVM.Parties = new ObservableCollection<Party>(InfoPackage.PartiesList);
                }
                else { InputText = ""; DisplayText = " Username taken"; }
                RegisterPlayerCooldown = false;
            }
            catch { RegisterPlayerCooldown = false; }
        }
    }
}