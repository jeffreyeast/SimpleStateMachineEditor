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

#region StateRecords

    // State Records

    internal class AddStateRecord : LayeredPositionableObjectRecord
    {
        protected override string UnitDescription => "Add state";
        protected override int UnitType => (int)ActionTypes.AddState;

        public ViewModel.State.StateTypes StateType;



        internal AddStateRecord(ViewModel.ViewModelController controller, ViewModel.State state) : base(ActionTypes.AddState, controller, state) 
        {
            StateType = state.StateType;
#if DEBUGUNDOREDO
            Debug.WriteLine($@">>> AddStateRecord.AddStateRecord: Created {UnitDescription} record, ID: {Id}, Name: {Name}");
#endif
        }

        public override void Do(IOleUndoManager pUndoManager)
        {
#if DEBUGUNDOREDO
            Debug.WriteLine(">>> AddStateRecord.Do");
#endif
            using (new ViewModel.ViewModelController.GuiChangeBlock(Controller))
            {
                ViewModel.State newState = new ViewModel.State(Controller, this);
                Controller.StateMachine.States.Add(newState);

                Controller.UndoManager.Add(new DeleteStateRecord (Controller, newState));
            }
        }
    }

    internal class DeleteStateRecord : LayeredPositionableObjectRecord
    {
        protected override string UnitDescription => "Delete state";
        protected override int UnitType => (int)ActionTypes.DeleteState;


        internal DeleteStateRecord(ViewModel.ViewModelController controller, ViewModel.State state) : base(ActionTypes.AddEventType, controller, state)
        {
#if DEBUGUNDOREDO
            Debug.WriteLine($@">>> DeleteStateRecord.DeleteStateRecord: Created {UnitDescription} record, ID: {Id}, Name: {Name}");
#endif
        }

        public override void Do(IOleUndoManager pUndoManager)
        {
#if DEBUGUNDOREDO
            Debug.WriteLine(">>> DeleteStateRecord.Do");
#endif
            using (new ViewModel.ViewModelController.GuiChangeBlock(Controller))
            {
                ViewModel.State targetState = Controller.StateMachine.States.Where(s => s.Id == Id).First();
                AddStateRecord addStateRecord = new AddStateRecord(Controller, targetState);
                Controller.StateMachine.States.Remove(targetState);

                Controller.UndoManager.Add(addStateRecord);
            }
        }
    }

#endregion
}
