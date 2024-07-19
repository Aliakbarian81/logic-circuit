using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Shapes;

namespace WpfTest
{
    public class PIOAStatus
    {
        public string PIOInput { get; set; }
        public string FilterDataDriver { get; set; }
        public string PinDataStatus { get; set; }
        public string IOPins { get; set; }
        public string InterruptMaskStatus { get; set; }
    }
    /// <summary>
    /// Interaction logic for SimulationWindow.xaml
    /// </summary>
    public partial class SimulationWindow : Window
    {
        public SimulationWindow()
        {
            InitializeComponent();
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            
        }
    }
}
