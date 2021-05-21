using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleStateMachineEditor.ObjectModel
{
    public delegate void RemovingHandler(IRemovableObject item);

    /// <summary>
    /// An object which can be removed and whose removal can be detected, in addition to which, notifies
    /// registered listeners of changes to its properties.
    /// </summary>
    public interface IRemovableObject : INotifyPropertyChanged
    {
        void Remove();
        event RemovingHandler Removing;
    }
}
