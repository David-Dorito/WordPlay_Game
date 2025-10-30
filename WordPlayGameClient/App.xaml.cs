using System.Configuration;
using System.Data;
using System.Windows;
using CommunityToolkit.Mvvm.Messaging;
using WordPlayGameClient.Model;
using WordPlayGameClient.MVVM;
using WordPlayGameClient.ViewModel;

namespace WordPlayGameClient
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            Application.Current.Exit += OnExit;
        }

        private void OnExit(object sender, ExitEventArgs e)
        {
            SettingsService.Save();
            GenericReferences.References.NoInternetVM.AppClosing = true;
        }
    }
}
