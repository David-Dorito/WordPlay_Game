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
using WordPlayGameClient.ViewModel.MainViewModel;
using WordPlayGameClient.Model;

namespace WordPlayGameClient.View.MainView
{
    /// <summary>
    /// Interaction logic for PartyBrowserView.xaml
    /// </summary>
    public partial class PartyBrowserView : UserControl
    {
        internal PartyBrowserViewModel ViewModel { get; }
        public PartyBrowserView()
        {
            InitializeComponent();
            ViewModel = new PartyBrowserViewModel();
            GenericReferences.References.PartyBrowserVM = ViewModel;
            this.DataContext = ViewModel;
        }
    }
}
