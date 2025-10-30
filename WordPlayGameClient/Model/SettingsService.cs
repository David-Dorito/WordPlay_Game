using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using WordPlayGameClient.MVVM;
using System.IO;

namespace WordPlayGameClient.Model
{
    internal class SettingsService : ViewModelBase
    {
        private bool darkMode;
        public bool DarkMode
        {
            get { return darkMode; }
            set
            {
                darkMode = value;
                OnPropertyChanged();
            }
        }

        public static SettingsService GlobalSettings { get; } = Load();

        public SettingsService() { }

        public static SettingsService Load()
        {
            if (File.Exists("Settings.json"))
                if (File.ReadAllText("Settings.json") != "" && File.ReadAllText("Settings.json") != "null")
                    return JsonSerializer.Deserialize<SettingsService>(File.ReadAllText("Settings.json")) ?? new SettingsService();
            return new SettingsService();
        }

        public static void Save()
        {
            File.WriteAllText("Settings.json", JsonSerializer.Serialize<SettingsService>(GlobalSettings));
        }
    }
}