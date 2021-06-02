using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleStateMachineEditor.ObjectModel
{
    internal interface ITransitionEndpoint
    {
        ObservableCollection<ViewModel.Transition> TransitionsFrom { get; }
        ObservableCollection<ViewModel.Transition> TransitionsTo { get; }
        int GetRelativePeerPosition(ViewModel.Transition transition);
        bool HasTransitionMatchingTrigger(ViewModel.EventType triggerEvent);
    }
}
