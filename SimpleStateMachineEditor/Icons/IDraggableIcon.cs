using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace SimpleStateMachineEditor.Icons
{
    interface IDraggableIcon
    {
        Control Body { get; }
        void CancelDrag();
        System.Windows.Point CenterPosition { get; set; }
        int ContextMenuId { get; }
        void CommitDrag(System.Windows.Point dragTerminationPoint, System.Windows.Point offset);
        FrameworkElement DraggableShape { get; set; }
        void Drag(System.Windows.Point offset);
        ViewModel.Region HighlightedRegion { get; set; }
        bool IsHidden { get; set; }
        bool IsRegionHighlighted { get;  }
        bool IsSelected { get; }
        void IsSelectedChanged();
        ObjectModel.TrackableObject ReferencedObject { get; }
        void StartDrag();
    }
}
