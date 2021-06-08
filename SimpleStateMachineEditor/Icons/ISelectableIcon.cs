using Microsoft.VisualStudio.OLE.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace SimpleStateMachineEditor.Icons
{
    internal interface ISelectableIcon : ObjectModel.IRemovableObject, IOleCommandTarget
    {
        Control Body { get; }
        System.Windows.Point CenterPosition { get; set; }
        int ContextMenuId { get; }
        bool IsLayerHighlighted { get; set; }
        bool IsSelectable { get; }
        bool IsSelected { get; }
        void IsSelectedChanged();
        ObjectModel.ITrackableObject ReferencedObject { get; }
    }
}
