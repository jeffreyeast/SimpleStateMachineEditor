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
    /// Interaction logic for ActionReferenceIconControl.xaml
    /// </summary>
    public partial class ActionReferenceIconControl : UserControl
    {
        bool IsDraggableShape;


        public ActionReferenceIconControl()
        {
            InitializeComponent();
            Loaded += ActionReferenceIconControl_Loaded;
        }

        public ActionReferenceIconControl(bool isDraggableShape)
        {
            IsDraggableShape = isDraggableShape;
            InitializeComponent();
            Loaded += ActionReferenceIconControl_Loaded;
        }

        private void ActionReferenceIconControl_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= ActionReferenceIconControl_Loaded;
            if (DataContext is Icons.ActionReferenceIcon icon && !IsDraggableShape)
            {
//                icon.Designer = Utility.DrawingAids.FindAncestorOfType<DesignerControl>(this);
                icon.Body = this;
            }
        }
    }
}
