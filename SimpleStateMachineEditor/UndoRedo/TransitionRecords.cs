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

#region TransitionRecords

    //  Transition Records

    internal class AddTransitionRecord : DocumentedObjectRecord
    {
        protected override string UnitDescription => "Add transition";
        protected override int UnitType => (int)ActionTypes.AddTransition;

        public ViewModel.Transition.TransitionTypes TransitionType;
        public int DestinationStateId;
        public int SourceStateId;
        public int TriggerEventId;
        public int[] ActionIds;



        internal AddTransitionRecord(ViewModel.ViewModelController controller, ViewModel.Transition transition) : base(ActionTypes.AddTransition, controller, transition) 
        {
            TransitionType = transition.TransitionType;
            DestinationStateId = transition.DestinationStateId;
            SourceStateId = transition.SourceStateId;
            TriggerEventId = transition.TriggerEvent?.Id ?? TrackableObject.NullId;
            ActionIds = transition.ActionReferences.Select(a => a.Action.Id).ToArray();

#if DEBUGUNDOREDO
            Debug.WriteLine($@">>> AddTransitionRecord.AddTransitionRecord: Created {UnitDescription} record, Id: {Id}, Src: {SourceStateId}, Dest: {DestinationStateId}");
#endif
        }

        public override void Do(IOleUndoManager pUndoManager)
        {
#if DEBUGUNDOREDO
            Debug.WriteLine(">>> AddTransitionRecord.Do");
#endif
            using (new ViewModel.ViewModelController.GuiChangeBlock(Controller))
            {
                ViewModel.Transition newTransition = new ViewModel.Transition(Controller, this);
                Controller.StateMachine.Transitions.Add(newTransition);

                Controller.UndoManager.Add(new DeleteTransitionRecord(Controller, newTransition));
            }
        }
    }

    internal class DeleteTransitionRecord : TrackableObjectRecord
    {
        protected override string UnitDescription => $@"Delete transition";
        protected override int UnitType => (int)ActionTypes.DeleteTransition;


        internal DeleteTransitionRecord(ViewModel.ViewModelController controller, ViewModel.Transition transition) : base(ActionTypes.AddEventType, controller, transition) 
        {
#if DEBUGUNDOREDO
            Debug.WriteLine($@">>> DeleteTransitionRecord.DeleteTransitionRecord: Created {UnitDescription} record, Id: {Id}");
#endif
        }

        public override void Do(IOleUndoManager pUndoManager)
        {
#if DEBUGUNDOREDO
            Debug.WriteLine(">>> DeleteTransitionRecord.Do");
#endif
            using (new ViewModel.ViewModelController.GuiChangeBlock(Controller))
            {
                ViewModel.Transition targetTransition = Controller.StateMachine.Transitions.Where(s => s.Id == Id).First();
                AddTransitionRecord addTransitionRecord = new AddTransitionRecord(Controller, targetTransition);
                Controller.StateMachine.Transitions.Remove(targetTransition);

                Controller.UndoManager.Add(addTransitionRecord);
            }
        }
    }

#endregion
}
