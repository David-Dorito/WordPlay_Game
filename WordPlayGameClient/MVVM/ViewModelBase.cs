using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Media;
using WordPlayGameClient.Model;

namespace WordPlayGameClient.MVVM
{
    internal class ViewModelBase : INotifyPropertyChanged
    {
        private Brush backgroundColour;
        public Brush BackgroundColour
        {
            get { return backgroundColour; }
            set
            {
                backgroundColour = value;
                OnPropertyChanged();
            }
        }

        private Brush panelBackgroundColour;
        public Brush PanelBackgroundColour
        {
            get { return panelBackgroundColour; }
            set
            {
                panelBackgroundColour = value;
                OnPropertyChanged();
            }
        }

        private Brush fontColour;
        public Brush FontColour
        {
            get { return fontColour; }
            set
            {
                fontColour = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public static HttpClient httpClient { get; set; } = new HttpClient() { Timeout = TimeSpan.FromSeconds(3.5)};

        protected void OnPropertyChanged([CallerMemberName] string? PropertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(PropertyName));
        }

        protected async Task<bool> CheckInternetAsync()
        {
            try
            {
                var result = await httpClient.GetAsync("https://www.google.com");
                return result.IsSuccessStatusCode;
            }
            catch { return false; }
        }

        protected void SettingsChangedEventHandler(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SettingsService.GlobalSettings.DarkMode))
            {
                BackgroundColour = (SettingsService.GlobalSettings.DarkMode) ? Colours.LightDarkMode : Brushes.White;
                FontColour = (SettingsService.GlobalSettings.DarkMode) ? Brushes.White : Brushes.Black;
                PanelBackgroundColour = (SettingsService.GlobalSettings.DarkMode) ? Colours.DefaultDarkMode : Brushes.LightGray;
            }
        }
    }
}