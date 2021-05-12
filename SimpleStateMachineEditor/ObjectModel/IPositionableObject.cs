using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleStateMachineEditor.ObjectModel
{
    interface IPositionableObject
    {
        System.Windows.Point LeftTopPosition { get; set; }
    }
}
