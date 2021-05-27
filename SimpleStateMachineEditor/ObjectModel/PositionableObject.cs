using SimpleStateMachineEditor.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleStateMachineEditor.ObjectModel
{
    public abstract class PositionableObject : ObjectModel.NamedObject, ObjectModel.IPositionableObject
    {
        [Description("Relative position of the icon in the display pane")]
        public System.Windows.Point LeftTopPosition
        {
            get => _leftTopPosition;
            set
            {
                if ((_leftTopPosition.X != value.X || _leftTopPosition.Y != value.Y) && IsChangeAllowed)
                {
                    Controller?.LogUndoAction(new UndoRedo.PropertyChangedRecord(Controller, this, "LeftTopPosition", _leftTopPosition.ToString()));
                    _leftTopPosition = value;
                    OnPropertyChanged("LeftTopPosition");
                    EndChange();
                }
            }
        }
        System.Windows.Point _leftTopPosition;




        //  Constructor for use by serialization ONLY

        public PositionableObject()
        {
        }


        //  Internal general-use constructors

        internal PositionableObject(ViewModel.ViewModelController controller) : base(controller)
        {
        }

        internal PositionableObject(ViewModel.ViewModelController controller, IEnumerable<NamedObject> existingObjectList, string rootName) : base(controller, existingObjectList, rootName)
        {
        }

        //  Constructor for the use of Redo

        internal PositionableObject(ViewModel.ViewModelController controller, UndoRedo.PositionableObjectRecord redoRecord) : base(controller, redoRecord)
        {
            using (new UndoRedo.DontLogBlock(controller))
            {
                LeftTopPosition = redoRecord.LeftTopPosition;
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
