using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleStateMachineEditor.ObjectModel
{
    public interface ITransition : ITrackableObject
    {
        event EventHandler EndpointPositionChanged;
        ObjectModel.ITransitionEndpoint SourceState { get; }
        ObjectModel.ITransitionEndpoint DestinationState { get; }
        ViewModel.Transition.TransitionTypes TransitionType { get; }
        ViewModel.EventType TriggerEvent { get; }
    }
}
