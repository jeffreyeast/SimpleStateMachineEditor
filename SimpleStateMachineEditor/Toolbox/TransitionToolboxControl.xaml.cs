using System.Globalization;
using System.Windows;
using System.Windows.Controls;

namespace SimpleStateMachineEditor.Toolbox
{
    /// <summary>
    /// Interaction logic for TransitionToolboxControl.xaml.
    /// </summary>
    [ProvideToolboxControl("SimpleStateMachineEditor", false)]
    public partial class TransitionToolboxControl : UserControl
    {
        public TransitionToolboxControl()
        {
            InitializeComponent();
        }
    }
}
