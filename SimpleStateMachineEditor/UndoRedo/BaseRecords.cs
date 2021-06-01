using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using SimpleStateMachineEditor.ObjectModel;
using SimpleStateMachineEditor.ViewModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell.Design;
using System.ComponentModel.Design;

namespace SimpleStateMachineEditor.UndoRedo
{
#region BaseClasses
    internal abstract class DocumentedObjectRecord : TrackableObjectRecord
    {
        internal string Description;

        protected DocumentedObjectRecord(ActionTypes actionType, ViewModel.ViewModelController controller, ObjectModel.DocumentedObject documentedObject) : base(actionType, controller, documentedObject)
        {
            Description = (documentedObject.Description == null ? null : string.Copy(documentedObject.Description));
        }
    }

    internal abstract class LayeredPositionableObjectRecord : NamedObjectRecord
    {
        internal int CurrentLayerId;

        protected LayeredPositionableObjectRecord(ActionTypes actionType, ViewModel.ViewModelController controller, ObjectModel.LayeredPositionableObject positionableObject) : base(actionType, controller, positionableObject)
        {
            CurrentLayerId = positionableObject.CurrentLayer.Id;
        }
    }

    internal abstract class PositionableObjectRecord : NamedObjectRecord
    {
        internal System.Windows.Point LeftTopPosition;

        protected PositionableObjectRecord(ActionTypes actionType, ViewModel.ViewModelController controller, ObjectModel.PositionableObject positionableObject) : base(actionType, controller, positionableObject)
        {
            LeftTopPosition = positionableObject.LeftTopPosition;
        }
    }

    internal abstract class NamedObjectRecord : DocumentedObjectRecord
    {
        internal string Name;

        protected NamedObjectRecord(ActionTypes actionType, ViewModel.ViewModelController controller, ObjectModel.NamedObject namedObject) : base(actionType, controller, namedObject)
        {
            Name = (namedObject.Name == null ? null : string.Copy(namedObject.Name));
        }
    }

    internal abstract class TrackableObjectRecord : UndoRedoRecord
    {
        internal int Id;

        protected TrackableObjectRecord(ActionTypes actionType, ViewModel.ViewModelController controller, ObjectModel.TrackableObject trackableObject) : base(actionType, controller)
        {
            Id = trackableObject.Id;
        }
    }
#endregion
}
