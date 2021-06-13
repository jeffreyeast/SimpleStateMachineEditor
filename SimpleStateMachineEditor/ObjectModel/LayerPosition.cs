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
                if ((_leftTopPosition.X != value.X || _leftTopPosition.Y != value.Y))
                {
                    using (new ViewModel.ViewModelController.GuiChangeBlock(Controller))
                    {
                        Controller?.LogUndoAction(new UndoRedo.PropertyChangedRecord(Controller, this, "LeftTopPosition", LeftTopPosition.ToString()));
                        _leftTopPosition = value;
                        OnPropertyChanged("LeftTopPosition");
                    }
                }
            }
        }
        Point _leftTopPosition;

        //  The relative ordinal position of the members of GroupStatuses must be maintained

        public enum GroupStatuses
        {
            NotGrouped,
            Implicit,
            Explicit,
        }

        [XmlAttribute]
        [Browsable(false)]
        public GroupStatuses GroupStatus
        {
            get => _groupStatus;
            set
            {
                if (_groupStatus != value)
                {
                    Controller?.LogUndoAction(new UndoRedo.PropertyChangedRecord(Controller, this, "GroupStatus", GroupStatus.ToString()));
                    _groupStatus = value;
                    OnPropertyChanged("GroupStatus");
                }
            }
        }
        GroupStatuses _groupStatus;

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
            GroupStatus = GroupStatuses.NotGrouped;
        }

        //  Constructor for new object creation through commands

        private LayerPosition(ViewModel.ViewModelController controller, ViewModel.Layer layer, GroupStatuses groupStatus) : base(controller)
        {
            using (new UndoRedo.DontLogBlock(controller))
            {
                Layer = layer;
                GroupStatus = groupStatus;
            }
        }

        //  Constructor for use by Redo

        internal LayerPosition(ViewModel.ViewModelController controller, UndoRedo.AddLayerMemberRecord redoRecord) : base(controller, redoRecord)
        {
            using (new UndoRedo.DontLogBlock(controller))
            {
                Layer = Find(redoRecord.LayerId) as ViewModel.Layer;
                LeftTopPosition = redoRecord.LeftTopPosition;
                GroupStatus = redoRecord.GroupStatus;
            }
        }

        internal static LayerPosition Create(ViewModel.ViewModelController controller, ViewModel.Layer layer, GroupStatuses groupStatus)
        {
            return new LayerPosition(controller, layer, groupStatus);
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
                case "GroupStatus":
                    value = GroupStatus.ToString();
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
                case "GroupStatus":
                    GroupStatus = (GroupStatuses)Enum.Parse(typeof(GroupStatuses), newValue);
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
