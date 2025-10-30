using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WordPlayGameClient.Model;
using WordPlayGameClient.ViewModel.MainViewModel;

namespace WordPlayGameClient.View.MainView
{
    /// <summary>
    /// Interaction logic for GameRoundView.xaml
    /// </summary>
    public partial class GameRoundView : UserControl
    {
        internal GameRoundViewModel ViewModel { get; }
        public GameRoundView()
        {
            InitializeComponent();
            ViewModel = new GameRoundViewModel();
            GenericReferences.References.GameRoundVM = ViewModel;
            this.DataContext = ViewModel;
        }
    }
}
