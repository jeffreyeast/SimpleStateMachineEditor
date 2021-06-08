using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Serialization;

namespace SimpleStateMachineEditor.ObjectModel
{
    /// <summary>
    /// This is the poor-man's dictionary for layer positions. We can't use a Dictionary because XML can't serialize it
    /// </summary>
    public class LayerPosition : TrackableObject
    {
        [XmlIgnore]
        public ViewModel.Layer Layer { get; set; }

        [XmlAttribute]
        public int LayerId
        {
            get => Layer?.Id ?? _layerId;
            set => _layerId = value;
        }
        int _layerId = TrackableObject.NullId;

        public Point LeftTopPosition 
        {
            get => _leftTopPosition;
            set
            {
                if ((_leftTopPosition.X != value.X || _leftTopPosition.Y != value.Y) && IsChangeAllowed())
                {
                    Controller?.LogUndoAction(new UndoRedo.PropertyChangedRecord(Controller, this, "LeftTopPosition", LeftTopPosition.ToString()));
                    _leftTopPosition = value;
                    OnPropertyChanged("LeftTopPosition");
                    EndChange();
                }
            }
        }
        Point _leftTopPosition;

        //  Legacy save files used Position, not LeftTopPosition

        [XmlElement(elementName:"Position")]
        public Point LegacyPosition 
        {
            get => _leftTopPosition;
            set => _leftTopPosition = value; 
        }


        //  Constructor for use by serialization ONLY

        public LayerPosition()
        {
        }

        //  Constructor for new object creation through commands

        private LayerPosition(ViewModel.ViewModelController controller, ViewModel.Layer layer) : base(controller)
        {
            Layer = layer;
        }

        //  Constructor for use by Redo

        internal LayerPosition(ViewModel.ViewModelController controller, UndoRedo.AddLayerMemberRecord redoRecord) : base(controller, redoRecord)
        {
            using (new UndoRedo.DontLogBlock(controller))
            {
                Layer = Find(redoRecord.LayerId) as ViewModel.Layer;
                LeftTopPosition = redoRecord.LeftTopPosition;
            }
        }

        internal static LayerPosition Create(ViewModel.ViewModelController controller, ViewModel.Layer layer)
        {
            return new LayerPosition(controller, layer);
        }

        internal override void DeserializeCleanup(TrackableObject.DeserializeCleanupPhases phase, ViewModel.ViewModelController controller, ViewModel.StateMachine stateMachine)
        {
            base.DeserializeCleanup(phase, controller, stateMachine);

            if (phase == DeserializeCleanupPhases.ObjectResolution)
            {
                Layer = Find(_layerId) as ViewModel.Layer;
                if (Layer == null)
                {
                    throw new ArgumentOutOfRangeException("ObjectModel.LayeredPositionalObject.LayerId", _layerId, "Unable to locate layer");
                }
            }
        }

        internal override void GetProperty(string propertyName, out string value)
        {
            switch (propertyName)
            {
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
