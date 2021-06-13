using SimpleStateMachineEditor.ObjectModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SimpleStateMachineEditor.ViewModel
{
    public abstract class TransitionHost : ObjectModel.LayeredPositionableObject, ObjectModel.ITransitionEndpoint
    {

        [XmlIgnore]
        [Browsable(false)]
        public ObservableCollection<ObjectModel.ITransition> TransitionsFrom { get; private set; }
        [XmlIgnore]
        [Browsable(false)]
        public ObservableCollection<ObjectModel.ITransition> TransitionsTo { get; private set; }


        //  Constructor for use by serialization ONLY

        public TransitionHost()
        {
            TransitionsFrom = new ObservableCollection<ObjectModel.ITransition>();
            TransitionsTo = new ObservableCollection<ObjectModel.ITransition>();
        }


        //  Constructor for new object creation through commands

        private TransitionHost(ViewModelController controller, string rootName, ViewModel.Layer currentLayer) : base(controller, controller.StateMachine.States, rootName, currentLayer)
        {
            TransitionsFrom = new ObservableCollection<ObjectModel.ITransition>();
            TransitionsTo = new ObservableCollection<ObjectModel.ITransition>();
        }

        internal TransitionHost(ViewModel.ViewModelController controller, IEnumerable<NamedObject> existingObjectList, string rootName, ViewModel.Layer currentLayer) : base(controller, existingObjectList, rootName, currentLayer)
        {
            TransitionsFrom = new ObservableCollection<ObjectModel.ITransition>();
            TransitionsTo = new ObservableCollection<ObjectModel.ITransition>();
        }

        //  Constructor for use by Redo

        internal TransitionHost(ViewModel.ViewModelController controller, UndoRedo.LayeredPositionableObjectRecord redoRecord) : base(controller, redoRecord)
        {
            using (new UndoRedo.DontLogBlock(Controller))
            {
                TransitionsFrom = new ObservableCollection<ObjectModel.ITransition>();
                TransitionsTo = new ObservableCollection<ObjectModel.ITransition>();
            }
        }

        private void GatherPeers(List<ObjectModel.ITransition> peerTransitions, ObjectModel.ITransition transition, IEnumerable<ObjectModel.ITransition> transitionList)
        {
            foreach (ObjectModel.ITransition t in transitionList)
            {
                if ((t.SourceState == transition.SourceState && t.DestinationState == transition.DestinationState) ||
                    (t.SourceState == transition.DestinationState && t.DestinationState == transition.SourceState))
                {
                    if (!peerTransitions.Contains(t))
                    {
                        peerTransitions.Add(t);
                    }
                }
            }
        }

        //  This method calculates the relative position of a transition among all the transitions that originate or
        //  terminate at this state, which share a common opposite state.

        public int GetRelativePeerPosition(ObjectModel.ITransition transition)
        {
            List<ObjectModel.ITransition> peerTransitions = new List<ObjectModel.ITransition>();
            GatherPeers(peerTransitions, transition, TransitionsFrom);
            GatherPeers(peerTransitions, transition, TransitionsTo);

            int position = 0;
            foreach (ObjectModel.ITransition t in peerTransitions)
            {
                if (t.Id < transition.Id)
                {
                    position++;
                }
            }
            return position;
        }

        public bool HasTransitionMatchingTrigger(EventType triggerEvent)
        {
            if (triggerEvent != null)
            {
                foreach (ObjectModel.ITransition transition in TransitionsFrom)
                {
                    if (transition.TriggerEvent == triggerEvent && CurrentLayer.Members.Contains(transition.SourceState))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        protected override void OnRemoving()
        {
            while (TransitionsFrom.Count > 0)
            {
                TransitionsFrom.First().Remove();
            }
            while (TransitionsTo.Count > 0)
            {
                TransitionsTo.First().Remove();
            }

            base.OnRemoving();
        }
    }
}
