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

#region ActionRecords
    // Action Records

    internal class AddActionRecord : NamedObjectRecord
    {
        protected override string UnitDescription => "Add action";
        protected override int UnitType => (int)ActionTypes.AddAction;



        internal AddActionRecord(ViewModel.ViewModelController controller, ViewModel.Action actionType) : base(ActionTypes.AddAction, controller, actionType)
        {
#if DEBUGUNDOREDO
            Debug.WriteLine($@">>> AddActionRecord.AddActionRecord: Created {UnitDescription} record, ID: {Id}, Name: {Name}");
#endif
        }

        public override void Do(IOleUndoManager pUndoManager)
        {
#if DEBUGUNDOREDO
            Debug.WriteLine(">>> AddActionRecord.Do");
#endif
            using (new ViewModel.ViewModelController.GuiChangeBlock(Controller))
            {
                ViewModel.Action newAction = new ViewModel.Action(Controller, this);
                Controller.StateMachine.Actions.Add(newAction);

                Controller.UndoManager.Add(new DeleteActionRecord(Controller, newAction));
            }
        }
    }

    internal class AddActionReferenceRecord : TrackableObjectRecord
    {
        protected override string UnitDescription => "Add action reference";
        protected override int UnitType => (int)ActionTypes.AddActionReference;

        internal int TransitionId;
        internal int ActionId;
        internal int Slot;


        internal AddActionReferenceRecord(ViewModel.ViewModelController controller, ViewModel.ActionReference actionReference, int slot) : base(ActionTypes.AddActionReference, controller, actionReference)
        {
            TransitionId = actionReference.Transition.Id;
            ActionId = actionReference.Action.Id;
            Slot = slot;
#if DEBUGUNDOREDO
            Debug.WriteLine($@">>> AddActionReferenceRecord.AddActionReferenceRecord: Created {UnitDescription} record, ID: {Id}, TransitionId: {TransitionId}, ActionId: {ActionId}, Slot: {Slot}");
#endif
        }

        public override void Do(IOleUndoManager pUndoManager)
        {
#if DEBUGUNDOREDO
            Debug.WriteLine(">>> AddActionReferenceRecord.Do");
#endif
            using (new ViewModel.ViewModelController.GuiChangeBlock(Controller))
            {
                ViewModel.ActionReference newActionReference = new ViewModel.ActionReference(Controller, this);
                newActionReference.Transition.ActionReferences.Insert(Slot, newActionReference);

                Controller.UndoManager.Add(new DeleteActionReferenceRecord(Controller, newActionReference));
            }
        }
    }

    internal class DeleteActionRecord : NamedObjectRecord
    {
        protected override string UnitDescription => "Delete action";
        protected override int UnitType => (int)ActionTypes.DeleteAction;


        internal DeleteActionRecord(ViewModel.ViewModelController controller, ViewModel.Action actionType) : base(ActionTypes.DeleteAction, controller, actionType)
        {
#if DEBUGUNDOREDO
            Debug.WriteLine($@">>> DeleteActionRecord.DeleteActionRecord: Created {UnitDescription} record, ID: {Id}, Name: {Name}");
#endif
        }

        public override void Do(IOleUndoManager pUndoManager)
        {
#if DEBUGUNDOREDO
            Debug.WriteLine(">>> DeleteActionRecord.Do");
#endif
            using (new ViewModel.ViewModelController.GuiChangeBlock(Controller))
            {
                ViewModel.Action targetAction = Controller.StateMachine.Actions.Where(e => e.Id == Id).First();
                AddActionRecord addActionRecord = new AddActionRecord(Controller, targetAction);
                Controller.StateMachine.Actions.Remove(targetAction);

                Controller.UndoManager.Add(addActionRecord);
            }
        }
    }

    internal class DeleteActionReferenceRecord : TrackableObjectRecord
    {
        protected override string UnitDescription => "Delete action reference";
        protected override int UnitType => (int)ActionTypes.DeleteActionReference;

        internal int TransitionId;
        internal int Slot;


        internal DeleteActionReferenceRecord(ViewModel.ViewModelController controller, ViewModel.ActionReference actionReference) : base(ActionTypes.DeleteActionReference, controller, actionReference)
        {
            TransitionId = actionReference.Transition.Id;
            Slot = actionReference.Transition.ActionReferences.IndexOf(actionReference);
#if DEBUGUNDOREDO
            Debug.WriteLine($@">>> DeleteActionReferenceRecord.DeleteActionReferenceRecord: Created {UnitDescription} record, ID: {Id}, TransitionId: {TransitionId}, Slot: {Slot}");
#endif
        }

        public override void Do(IOleUndoManager pUndoManager)
        {
#if DEBUGUNDOREDO
            Debug.WriteLine(">>> DeleteActionReferenceRecord.Do");
#endif
            using (new ViewModel.ViewModelController.GuiChangeBlock(Controller))
            {
                ViewModel.Transition targetTransition = Controller.StateMachine.Find(TransitionId) as ViewModel.Transition;
                ViewModel.ActionReference targetActionReference = targetTransition.ActionReferences[Slot];
                targetTransition.ActionReferences.RemoveAt(Slot);

                Controller.UndoManager.Add(new AddActionReferenceRecord(Controller, targetActionReference, Slot));
            }
        }
    }

    #endregion
}
