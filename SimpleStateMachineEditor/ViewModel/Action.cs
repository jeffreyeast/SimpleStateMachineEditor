using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleStateMachineEditor.ViewModel
{
    /// <summary>
    /// Represents a method to be invoked when a transition is taken
    /// </summary>
    public class Action : ObjectModel.NamedObject, IComparable<Action>
    {

        //  Constructor for use by serialization ONLY

        public Action()
        {

        }
        //  Constructor for new object creation during DeserializationCleanup

        internal Action(ViewModelController controller, string name) : base(controller, name)
        {

        }


        //  Constructor for new object creation through commands

        private Action(ViewModelController controller, IEnumerable<ObjectModel.NamedObject> existingObjectList, string rootName) : base(controller, existingObjectList, rootName)
        {

        }

        //  Constructor for use by Redo

        internal Action(ViewModel.ViewModelController controller, UndoRedo.AddActionRecord redoRecord) : base(controller, redoRecord)
        {

        }

        public int CompareTo(Action other)
        {
            return Name.CompareTo(other.Name);
        }

        internal static Action Create(ViewModelController controller, string name)
        {
            using (new UndoRedo.DontLogBlock(controller))
            {
                return new Action(controller, controller.StateMachine.Actions, name);
            }
        }
    }
}
