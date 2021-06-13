using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleStateMachineEditor.ObjectModel
{
    public interface ITrackableObject : IRemovableObject
    {
        ViewModel.ViewModelController Controller { get; }
        int Id { get; }
        bool IsValid { get; set; }
    }
}
