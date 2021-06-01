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
#region UndoRedoRecord

    internal abstract class UndoRedoRecord : IOleUndoUnit
    {
        public static readonly Guid UndoRedoClassID = new Guid("F8507D32-10D6-4EE0-B861-6895066A3304");

        public enum ActionTypes
        {
            AddAction,
            AddActionReference,
            AddEventType,
            AddLayer,
            AddLayerMember,
            AddState,
            AddTransition,
            ChangeProperty,
            ChangePropertyList,
            DeleteAction,
            DeleteActionReference,
            DeleteEventType,
            DeleteLayer,
            DeleteState,
            DeleteTransition,
            MoveTransition,
            Parent,
            RemoveLayerMember,
            SetLayer,
        }

        internal ActionTypes ActionType;

        protected ViewModel.ViewModelController Controller;
        protected abstract string UnitDescription { get; }
        protected abstract int UnitType { get; }


        protected UndoRedoRecord(ActionTypes actionType, ViewModel.ViewModelController controller)
        {
            ActionType = actionType;
            Controller = controller;
        }

        public abstract void Do(IOleUndoManager pUndoManager);

        public void GetDescription(out string pBstr)
        {
            pBstr = UnitDescription;
        }

        public void GetUnitType(out Guid pClsid, out int plID)
        {
            pClsid = UndoRedoClassID;
            plID = UnitType;
        }

        public virtual void OnNextAdd() { }

        public override string ToString()
        {
            return UnitDescription;
        }
    }

    #endregion
    
}
