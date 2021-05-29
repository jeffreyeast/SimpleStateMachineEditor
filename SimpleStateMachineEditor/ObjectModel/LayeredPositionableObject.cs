using SimpleStateMachineEditor.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Serialization;

namespace SimpleStateMachineEditor.ObjectModel
{
    public abstract class LayeredPositionableObject : ObjectModel.NamedObject, ObjectModel.IPositionableObject
    {
        [XmlIgnore]
        [Description("Relative position of the icon in the display pane")]
        public System.Windows.Point LeftTopPosition
        {
            get
            {
                if (CurrentLayer == null)
                {
                    throw new InvalidOperationException();
                }
                return LayerPositions.Where(p => p.Layer == CurrentLayer).Single().LeftTopPosition;
            }
            set
            {
                if (CurrentLayer == null)
                {
                    throw new InvalidOperationException();
                }
                ObjectModel.LayerPosition layerPosition = LayerPositions.Where(p => p.Layer == CurrentLayer).Single();
                layerPosition.LeftTopPosition = value;
                OnPropertyChanged("LeftTopPosition");
            }
        }

        [Browsable(false)]
        [XmlElement(elementName:"LeftTopPosition")]
        public System.Windows.Point LegacyLeftTopPosition { get; set; }

        [Browsable(false)]
        public ObservableCollection<ObjectModel.LayerPosition> LayerPositions { get; private set; }

        [XmlIgnore]
        internal ViewModel.Layer CurrentLayer { get; set; }

        


        //  Constructor for use by serialization ONLY

        public LayeredPositionableObject()
        {
            LayerPositions = new ObservableCollection<LayerPosition>();
        }


        //  Internal general-use constructors

        internal LayeredPositionableObject(ViewModel.ViewModelController controller, ViewModel.Layer currentLayer) : base(controller)
        {
            LayerPositions = new ObservableCollection<LayerPosition>();
            CurrentLayer = currentLayer;
            LeftTopPosition = new Point(0, 0);
        }

        internal LayeredPositionableObject(ViewModel.ViewModelController controller, IEnumerable<NamedObject> existingObjectList, string rootName, ViewModel.Layer currentLayer) : base(controller, existingObjectList, rootName)
        {
            LayerPositions = new ObservableCollection<LayerPosition>();
            CurrentLayer = currentLayer;
            LeftTopPosition = new Point(0, 0);
        }

        //  Constructor for the use of Redo

        internal LayeredPositionableObject(ViewModel.ViewModelController controller, UndoRedo.LayeredPositionableObjectRecord redoRecord) : base(controller, redoRecord)
        {
            LayerPositions = new ObservableCollection<LayerPosition>();
            using (new UndoRedo.DontLogBlock(controller))
            {
                CurrentLayer = controller.StateMachine.Find(redoRecord.CurrentLayerId) as ViewModel.Layer;
            }
        }

        internal override void DeserializeCleanup(ViewModelController controller, StateMachine stateMachine)
        {
            base.DeserializeCleanup(controller, stateMachine);
            foreach (LayerPosition layerPosition in LayerPositions)
            {
                layerPosition.DeserializeCleanup(controller, stateMachine);
            }
        }

        internal override void GetProperty(string propertyName, out string value)
        {
            switch (propertyName)
            {
                case "CurrentLayer":
                    value = CurrentLayer.Id.ToString();
                    break;
                case "LeftTopPosition":
                    value = LeftTopPosition.ToString();
                    break;
                default:
                    base.GetProperty(propertyName, out value);
                    break;
            }
        }

        internal override void SetProperty(string propertyName, string newValue)
        {
            switch (propertyName)
            {
                case "CurrentLayer":
                    CurrentLayer = Controller.StateMachine.Find(int.Parse(newValue)) as ViewModel.Layer;
                    break;
                case "LeftTopPosition":
                    LeftTopPosition = System.Windows.Point.Parse(newValue);
                    break;
                default:
                    base.SetProperty(propertyName, newValue);
                    break;
            }
        }
    }
}
