using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleStateMachineEditor.ObjectModel
{
    public interface ITransitionEndpoint : INamedObject
    {
        ViewModel.Group AssociatedGroup { get; }
        int GetRelativePeerPosition(ObjectModel.ITransition transition);
        bool HasTransitionMatchingTrigger(ViewModel.EventType triggerEvent);
        bool IsGrouped { get; }
        ObservableCollection<ObjectModel.ITransition> TransitionsFrom { get; }
        ObservableCollection<ObjectModel.ITransition> TransitionsTo { get; }
    }
}
