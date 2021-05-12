using System.Globalization;
using System.Windows;
using System.Windows.Controls;

namespace SimpleStateMachineEditor.Toolbox
{
    /// <summary>
    /// Interaction logic for EventToolboxControl.xaml.
    /// </summary>
    [ProvideToolboxControl("SimpleStateMachineEditor", false)]
    public partial class EventToolboxControl : UserControl
    {
        public EventToolboxControl()
        {
            InitializeComponent();
        }
    }
}
