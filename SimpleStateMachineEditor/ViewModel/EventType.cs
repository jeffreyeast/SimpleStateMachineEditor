using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleStateMachineEditor.ViewModel
{
    //++
    //  The Event class represents an event type.
    //--
    public class EventType : ObjectModel.PositionableObject
    {
        public const string WildcardEventTypeName = "*";



        //  Constructor for use by serialization ONLY

        public EventType ()
        {

        }

        //  Constructor for new object creation through commands

        private EventType (ViewModelController controller, string rootName) : base(controller, controller.StateMachine.EventTypes, rootName)
        {

        }

        //  Constructor for use by Redo

        internal EventType(ViewModel.ViewModelController controller, UndoRedo.AddEventTypeRecord redoRecord) : base(controller, redoRecord)
        {

        }

        internal static EventType Create(ViewModelController controller, IconControls.OptionsPropertiesPage optionsPage)
        {
            using (new UndoRedo.DontLogBlock(controller))
            {
                return new EventType(controller, optionsPage.EventTypeRootName);
            }
        }
    }
}
