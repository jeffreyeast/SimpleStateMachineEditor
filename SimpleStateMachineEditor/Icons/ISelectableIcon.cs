using Microsoft.VisualStudio.OLE.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace SimpleStateMachineEditor.Icons
{
    internal interface ISelectableIcon : IOleCommandTarget
    {
        Control Body { get; }
        System.Windows.Point CenterPosition { get; set; }
        int ContextMenuId { get; }
        ViewModel.Region HighlightedRegion { get; set; }
        bool IsHidden { get; set; }
        bool IsLayerHighlighted { get; set; }
        bool IsRegionHighlighted { get; }
        bool IsSelected { get; }
        void IsSelectedChanged();
        ObjectModel.TrackableObject ReferencedObject { get; }
    }
}
