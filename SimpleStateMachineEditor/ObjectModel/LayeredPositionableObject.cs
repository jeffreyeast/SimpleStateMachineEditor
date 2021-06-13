using SimpleStateMachineEditor.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Serialization;

namespace SimpleStateMachineEditor.ObjectModel
{
    public abstract class LayeredPositionableObject : ObjectModel.NamedObject, ObjectModel.ILayeredPositionableObject
    {
        [XmlIgnore]
        [Description("Relative position of the icon in the display pane")]
        public System.Windows.Point LeftTopPosition
        {
            get
            {
                if (CurrentLayerPosition == null)
                {
                    throw new InvalidOperationException();
                }
                return CurrentLayerPosition.LeftTopPosition;
            }
            set
            {
                if (CurrentLayerPosition == null)
                {
                    throw new InvalidOperationException();
                }
                CurrentLayerPosition.LeftTopPosition = value;
                OnPropertyChanged("LeftTopPosition");
            }
        }

        [Browsable(false)]
        [XmlElement(elementName:"LeftTopPosition")]
        public System.Windows.Point LegacyLeftTopPosition { get; set; }

        [Browsable(false)]
        public ObservableCollection<ObjectModel.LayerPosition> LayerPositions { get; private set; }

        [Browsable(false)]
        [XmlIgnore]
        public ViewModel.Layer CurrentLayer 
        {
            get => _currentLayer;
            set
            {
                if (_currentLayer != value)
                {
                    _currentLayer = value;
                    _currentLayerPosition = null;
                }
            }
        }
        ViewModel.Layer _currentLayer;

        [Browsable(false)]
        [XmlIgnore]
        public ObjectModel.LayerPosition CurrentLayerPosition
        {
            get
            {
                if (_currentLayerPosition == null)
                {
                    _currentLayerPosition = LayerPositions.Where(lp => lp.Layer == CurrentLayer).FirstOrDefault();
                }
                return _currentLayerPosition;
            }
            set => _currentLayerPosition = value;
        }
        ObjectModel.LayerPosition _currentLayerPosition;


        //  Constructor for use by serialization ONLY

        public LayeredPositionableObject()
        {
            LayerPositions = new ObservableCollection<LayerPosition>();
            LayerPositions.CollectionChanged += ObservableCollectionOfRemovableObjectsChangedHandler;
        }


        //  Internal general-use constructors

        internal LayeredPositionableObject(ViewModel.ViewModelController controller, ViewModel.Layer currentLayer) : base(controller)
        {
            LayerPositions = new ObservableCollection<LayerPosition>();
            LayerPositions.CollectionChanged += ObservableCollectionOfRemovableObjectsChangedHandler;
            CurrentLayer = currentLayer;
        }

        internal LayeredPositionableObject(ViewModel.ViewModelController controller, IEnumerable<NamedObject> existingObjectList, string rootName, ViewModel.Layer currentLayer) : base(controller, existingObjectList, rootName)
        {
            LayerPositions = new ObservableCollection<LayerPosition>();
            LayerPositions.CollectionChanged += ObservableCollectionOfRemovableObjectsChangedHandler;
            CurrentLayer = currentLayer;
        }

        //  Constructor for the use of Redo

        internal LayeredPositionableObject(ViewModel.ViewModelController controller, UndoRedo.LayeredPositionableObjectRecord redoRecord) : base(controller, redoRecord)
        {
            LayerPositions = new ObservableCollection<LayerPosition>();
            LayerPositions.CollectionChanged += ObservableCollectionOfRemovableObjectsChangedHandler;
            using (new UndoRedo.DontLogBlock(controller))
            {
                CurrentLayer = Find(redoRecord.CurrentLayerId) as ViewModel.Layer;
            }
        }

        internal override void DeserializeCleanup(TrackableObject.DeserializeCleanupPhases phase, ViewModelController controller, StateMachine stateMachine)
        {
            base.DeserializeCleanup(phase, controller, stateMachine);
            foreach (LayerPosition layerPosition in LayerPositions)
            {
                layerPosition.DeserializeCleanup(phase, controller, stateMachine);
                layerPosition.PropertyChanged += LayerPositionPropertyChangedHandler;
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

        private void LayerPositionPropertyChangedHandler(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "GroupStatus")
            {
                OnPropertyChanged("GroupStatus");
            }
        }

        protected override void ObservableCollectionOfRemovableObjectsChangedHandler(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (LayerPosition layerPosition in e.NewItems)
                    {
                        layerPosition.PropertyChanged += LayerPositionPropertyChangedHandler;
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (LayerPosition layerPosition in e.OldItems)
                    {
                        layerPosition.PropertyChanged -= LayerPositionPropertyChangedHandler;
                    }
                    break;
                default:
                    throw new NotImplementedException();
            }
            base.ObservableCollectionOfRemovableObjectsChangedHandler(sender, e);
        }

        protected override void OnRemoving()
        {
            foreach (LayerPosition layerPosition in LayerPositions)
            {
                layerPosition.PropertyChanged -= LayerPositionPropertyChangedHandler;
                layerPosition.Remove();
            }
            base.OnRemoving();
        }

        internal override void SetProperty(string propertyName, string newValue)
        {
            switch (propertyName)
            {
                case "CurrentLayer":
                    CurrentLayer = Find(int.Parse(newValue)) as ViewModel.Layer;
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
