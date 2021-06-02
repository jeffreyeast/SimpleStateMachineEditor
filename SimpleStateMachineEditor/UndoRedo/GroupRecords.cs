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

#region GroupRecords

    // Group Records

    internal class AddGroupRecord : LayeredPositionableObjectRecord
    {
        protected override string UnitDescription => "Add group";
        protected override int UnitType => (int)ActionTypes.AddGroup;



        internal AddGroupRecord(ViewModel.ViewModelController controller, ViewModel.Group group) : base(ActionTypes.AddGroup, controller, group) 
        {
#if DEBUGUNDOREDO
            Debug.WriteLine($@">>> AddGroupRecord.AddGroupRecord: Created {UnitDescription} record, ID: {Id}, Name: {Name}");
#endif
        }

        public override void Do(IOleUndoManager pUndoManager)
        {
#if DEBUGUNDOREDO
            Debug.WriteLine(">>> AddGroupRecord.Do");
#endif
            if (Controller.StateMachine.IsChangeAllowed)
            {
                ViewModel.Group newGroup = new ViewModel.Group(Controller, this);
                Controller.StateMachine.Groups.Add(newGroup);
                Controller.StateMachine.EndChange();

                Controller.UndoManager.Add(new DeleteGroupRecord (Controller, newGroup));
            }
        }
    }

    internal class DeleteGroupRecord : LayeredPositionableObjectRecord
    {
        protected override string UnitDescription => "Delete group";
        protected override int UnitType => (int)ActionTypes.DeleteGroup;


        internal DeleteGroupRecord(ViewModel.ViewModelController controller, ViewModel.Group group) : base(ActionTypes.AddEventType, controller, group)
        {
#if DEBUGUNDOREDO
            Debug.WriteLine($@">>> DeleteGroupRecord.DeleteGroupRecord: Created {UnitDescription} record, ID: {Id}, Name: {Name}");
#endif
        }

        public override void Do(IOleUndoManager pUndoManager)
        {
#if DEBUGUNDOREDO
            Debug.WriteLine(">>> DeleteGroupRecord.Do");
#endif
            if (Controller.StateMachine.IsChangeAllowed)
            {
                ViewModel.Group targetGroup = Controller.StateMachine.Groups.Where(s => s.Id == Id).First();
                Controller.StateMachine.Groups.Remove(targetGroup);
                Controller.StateMachine.EndChange();

                Controller.UndoManager.Add(new AddGroupRecord (Controller, targetGroup));
            }
        }
    }

#endregion
}
