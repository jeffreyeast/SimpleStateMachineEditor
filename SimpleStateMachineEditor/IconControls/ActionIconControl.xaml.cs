using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    /// Interaction logic for ActionIconControl.xaml
    /// </summary>
    public partial class ActionIconControl : UserControl, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        bool IsDraggableShape;


        public ActionIconControl()
        {
            InitializeComponent();
            Loaded += ActionIconControl_Loaded;
        }

        public ActionIconControl(bool isDraggableShape)
        {
            IsDraggableShape = isDraggableShape;
            InitializeComponent();
            Loaded += ActionIconControl_Loaded;
        }

        private void ActionIconControl_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= ActionIconControl_Loaded;
            if (DataContext is Icons.ActionIcon icon && !IsDraggableShape)
            {
                icon.Designer = Utility.DrawingAids.FindAncestorOfType<DesignerControl>(this);
                icon.Body = this;
            }
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
