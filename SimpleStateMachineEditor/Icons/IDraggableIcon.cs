using Microsoft.VisualStudio.OLE.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace SimpleStateMachineEditor.Icons
{
    interface IDraggableIcon : ISelectableIcon
    {
        void CancelDrag();
        void CommitDrag(System.Windows.Point dragTerminationPoint, System.Windows.Point offset);
        FrameworkElement DraggableShape { get; set; }
        void Drag(System.Windows.Point mousePosition, System.Windows.Point offset);
        void StartDrag();
    }
}
