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
#region EventTypeRecords

    // Event Type Records

    internal class AddEventTypeRecord : PositionableObjectRecord
    {
        protected override string UnitDescription => "Add event type";
        protected override int UnitType => (int)ActionTypes.AddEventType;



        internal AddEventTypeRecord(ViewModel.ViewModelController controller, ViewModel.EventType eventType) : base(ActionTypes.AddEventType, controller, eventType) 
        {
#if DEBUGUNDOREDO
            Debug.WriteLine($@">>> AddEventTypeRecord.AddEventTypeRecord: Created {UnitDescription} record, ID: {Id}, Name: {Name}");
#endif
        }

        public override void Do(IOleUndoManager pUndoManager)
        {
#if DEBUGUNDOREDO
            Debug.WriteLine(">>> AddEventTypeRecord.Do");
#endif
            if (Controller.StateMachine.IsChangeAllowed)
            {
                ViewModel.EventType newEventType = new EventType(Controller, this);
                Controller.StateMachine.EventTypes.Add(newEventType);
                Controller.StateMachine.EndChange();

                Controller.UndoManager.Add(new DeleteEventTypeRecord(Controller, newEventType));
            }
        }
    }

    internal class DeleteEventTypeRecord : PositionableObjectRecord
    {
        protected override string UnitDescription => "Delete event type";
        protected override int UnitType => (int)ActionTypes.DeleteEventType;


        internal DeleteEventTypeRecord(ViewModel.ViewModelController controller, ViewModel.EventType eventType) : base(ActionTypes.DeleteEventType, controller, eventType) 
        {
#if DEBUGUNDOREDO
            Debug.WriteLine($@">>> DeleteEventTypeRecord.DeleteEventTypeRecord: Created {UnitDescription} record, ID: {Id}, Name: {Name}");
#endif
        }

        public override void Do(IOleUndoManager pUndoManager)
        {
#if DEBUGUNDOREDO
            Debug.WriteLine(">>> DeleteEventTypeRecord.Do");
#endif
            if (Controller.StateMachine.IsChangeAllowed)
            {
                ViewModel.EventType targetEventType = Controller.StateMachine.EventTypes.Where(e => e.Id == Id).First();
                Controller.StateMachine.EventTypes.Remove(targetEventType);
                Controller.StateMachine.EndChange();

                Controller.UndoManager.Add(new AddEventTypeRecord(Controller, targetEventType));
            }
        }
    }

#endregion
}
