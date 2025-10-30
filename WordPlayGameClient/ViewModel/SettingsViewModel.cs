using System;
using System.Collections.Generic;
using System.ComponentModel;
using WordPlayGameClient.Model;
using WordPlayGameClient.MVVM;

namespace WordPlayGameClient.ViewModel
{
    internal class SettingsViewModel : ViewModelBase
    {
        private bool isDarkModeChecked;
        public bool IsDarkModeChecked
        {
            get {  return isDarkModeChecked; }
            set
            {
                isDarkModeChecked = value;
                SettingsService.GlobalSettings.DarkMode = value;
                OnPropertyChanged();
            }
        }

        public SettingsViewModel()
        {
            SettingsService.GlobalSettings.PropertyChanged += SettingsChangedEventHandler;
            IsDarkModeChecked = SettingsService.GlobalSettings.DarkMode;
        }
    }
}