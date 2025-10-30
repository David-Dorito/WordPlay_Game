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
    /// Interaction logic for PartyLobbyAdminView.xaml
    /// </summary>
    public partial class PartyLobbyAdminView : UserControl
    {
        internal PartyLobbyAdminViewModel ViewModel { get; }
        public PartyLobbyAdminView()
        {
            InitializeComponent();
            ViewModel = new PartyLobbyAdminViewModel();
            GenericReferences.References.PartyLobbyAdminVM = ViewModel;
            this.DataContext = ViewModel;
        }
    }
}
