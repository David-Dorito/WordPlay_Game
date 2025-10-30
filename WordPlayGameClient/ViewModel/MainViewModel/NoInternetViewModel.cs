using System;
using System.Collections.Generic;
using System.Diagnostics;
using WordPlayGameClient.Model;
using WordPlayGameClient.MVVM;

namespace WordPlayGameClient.ViewModel.MainViewModel
{
    internal class NoInternetViewModel : ViewModelBase
    {
        public RelayCommand RetryConnection { get; }

        public bool AppClosing { get; set; } = false;

        private bool retryButtonCooldown;
        public bool RetryButtonCooldown
        {
            get { return retryButtonCooldown; }
            set
            {
                retryButtonCooldown = value;
                OnPropertyChanged();
                RetryConnection.RaiseCanExecuteChanged();
            }
        }

        public NoInternetViewModel()
        {
            SettingsService.GlobalSettings.PropertyChanged += SettingsChangedEventHandler;
            RetryConnection = new RelayCommand(execute => RetryConnectionAsync(), canExecute => !RetryButtonCooldown);
        }

        public async Task RetryConnectionAsync()
        {
            RetryButtonCooldown = true;
            GenericReferences.References.MainWindowVM.HasInternet = await CheckInternetAsync();
            if (GenericReferences.References.MainWindowVM.HasInternet)
            {
                try
                {
                    await GenericReferences.References.MainWindowVM.ConnectToServerAsync();
                    RetryButtonCooldown = false;
                }
                catch { Debug.WriteLine("Retry button failed"); RetryButtonCooldown = false; }
            }
        }
    }
}