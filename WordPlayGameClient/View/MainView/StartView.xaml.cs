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
    /// Interaction logic for StartView.xaml
    /// </summary>
    public partial class StartView : UserControl
    {
        internal StartViewModel ViewModel { get; }
        public StartView()
        {
            InitializeComponent();
            ViewModel = new StartViewModel();
            GenericReferences.References.StartVM = ViewModel;
            this.DataContext = ViewModel;
        }
    }
}
