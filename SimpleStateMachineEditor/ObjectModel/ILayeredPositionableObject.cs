using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleStateMachineEditor.ObjectModel
{
    public interface ILayeredPositionableObject : INamedObject, IPositionableObject
    {
        ObservableCollection<ObjectModel.LayerPosition> LayerPositions { get; }
        ViewModel.Layer CurrentLayer { get; set; }
        ObjectModel.LayerPosition CurrentLayerPosition { get; }
    }
}
