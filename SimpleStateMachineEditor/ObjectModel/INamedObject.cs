using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleStateMachineEditor.ObjectModel
{
    public interface INamedObject : ITrackableObject
    {
        string Description { get; }
        string Name { get; }
    }
}
