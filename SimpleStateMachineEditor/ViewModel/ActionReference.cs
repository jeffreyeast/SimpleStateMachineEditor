using SimpleStateMachineEditor.ObjectModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleStateMachineEditor.ViewModel
{
    /// <summary>
    /// Represents an action within a transition
    ///  Note this class is not XML serializable
    /// </summary>

    public class ActionReference : ObjectModel.TrackableObject
    {
        public Transition Transition { get; private set; }
        public Action Action { get; private set; }



        //  Constructor for general use

        public ActionReference(ViewModelController controller, Transition transition, Action action) : base(controller)
        {
            Transition = transition;
            Action = action;
            Action.Removing += ActionIsBeingRemovedHandler;
        }

        //  Constructor for Redo

        internal ActionReference(ViewModelController controller, UndoRedo.AddActionReferenceRecord redoRecord) : base(controller, redoRecord)
        {
            using (new UndoRedo.DontLogBlock(controller))
            {
                Transition = controller.StateMachine.Find(redoRecord.TransitionId) as Transition;
                Action = controller.StateMachine.Find(redoRecord.ActionId) as Action;
            }
        }

        private void ActionIsBeingRemovedHandler(IRemovableObject item)
        {
            Remove();
        }

        protected override void OnRemoving()
        {
            Action.Removing -= ActionIsBeingRemovedHandler;
            base.OnRemoving();
        }
    }
}
