using System.Globalization;
using System.Windows;
using System.Windows.Controls;

namespace SimpleStateMachineEditor.Toolbox
{
    /// <summary>
    /// Interaction logic for StateToolboxControl.xaml.
    /// </summary>
    [ProvideToolboxControl("SimpleStateMachineEditor", false)]
    public partial class StateToolboxControl : UserControl
    {
        public StateToolboxControl()
        {
            InitializeComponent();
        }
    }
}
