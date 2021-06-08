using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleStateMachineEditor.ViewModel
{
    //++
    //  The GroupTransition class represents a transition between a state or group and another state or group.
    //
    //  This is a ephemeral object, never serialized.
    //--
    public class GroupTransition : ObjectModel.DocumentedObject, ObjectModel.ITransition
    {
        public ObjectModel.ITransitionEndpoint SourceState { get; private set; }

        [Description("The state which will be transitioned to")]
        public ObjectModel.ITransitionEndpoint DestinationState { get; private set; }

        [Description("The event which will trigger the transition")]
        public EventType TriggerEvent { get; private set; }

        [Browsable(false)]
        public bool WasTriggerFound
        {
            get => _wasTriggerFound;
            set
            {
                if (_wasTriggerFound != value)
                {
                    _wasTriggerFound = value;
                    OnPropertyChanged("WasTriggerFound");
                }
            }
        }
        bool _wasTriggerFound;

        public event EventHandler EndpointPositionChanged;


        //  Internal constructor for general use

        internal GroupTransition(ViewModelController controller, ObjectModel.ITransitionEndpoint source, ObjectModel.ITransitionEndpoint destination, ViewModel.EventType trigger) : base (controller)
        {
            SourceState = source;
            SourceState.PropertyChanged += EndpointPropertyChangedHandler;
            DestinationState = destination;
            DestinationState.PropertyChanged += EndpointPropertyChangedHandler;
            TriggerEvent = trigger;
        }

        private void EndpointPropertyChangedHandler(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "LeftTopPosition":
                case "Transitions":
                    EndpointPositionChanged?.Invoke(this, new EventArgs());
                    break;

                default:
                    break;
            }
        }

        internal override void ResetSearch()
        {
            WasTriggerFound = false;
            base.ResetSearch();
        }

        internal override uint Search(string searchString)
        {
            uint count =  base.Search(searchString);

            WasTriggerFound = TriggerEvent != null && !string.IsNullOrWhiteSpace(TriggerEvent.Name) && TriggerEvent.Name.Contains(searchString);

            return count + (uint)(WasTriggerFound ? 1 : 0);
        }

        public override string ToString()
        {
            return $@"{(SourceState?.Name ?? "<null>")}<({(TriggerEvent?.Name ?? "<null>")})>{(DestinationState?.Name ?? "<null>")}";
        }
    }
}
