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

namespace SimpleStateMachineEditor.IconControls
{
    /// <summary>
    /// Interaction logic for LayerIconControl.xaml
    /// </summary>
    public partial class LayerIconControl : UserControl
    {
        public LayerIconControl()
        {
            InitializeComponent();
        }

        private void SelectLayer(object sender, RoutedEventArgs e)
        {
            (DataContext as Icons.LayerIcon).Select();
        }
    }
}
