using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SimpleStateMachineEditor.ObjectModel
{
    public class DocumentedObject : TrackableObject
    {
        [Description("Comment describing the object")]
        public string Description
        {
            get => _description;
            set
            {
                if (_description != value)
                {
                    using (new ViewModel.ViewModelController.GuiChangeBlock(Controller))
                    {
                        Controller?.LogUndoAction(new UndoRedo.PropertyChangedRecord(Controller, this, "Description", _description));
                        _description = value;
                        OnPropertyChanged("Description");
                    }
                }
            }
        }
        string _description;

        [XmlIgnore]
        [Browsable(false)]
        public bool WasDescriptionFound
        {
            get => _wasDescriptionFound;
            set
            {
                if (_wasDescriptionFound != value)
                {
                    _wasDescriptionFound = value;
                    OnPropertyChanged("WasDescriptionFound");
                }
            }
        }
        bool _wasDescriptionFound;



        //  Constructor for use by serialization ONLY

        public DocumentedObject()
        {
        }

        //  Constructors for use by derived classes

        public DocumentedObject(ViewModel.ViewModelController controller) : base(controller)
        {
        }

        //  Constructor for use by Redo

        internal DocumentedObject(ViewModel.ViewModelController controller, UndoRedo.DocumentedObjectRecord redoRecord) : base(controller, redoRecord)
        {
            using (new UndoRedo.DontLogBlock(controller))
            {
                Description = redoRecord.Description;
            }
        }

        internal override void GetProperty(string propertyName, out string value)
        {
            switch (propertyName)
            {
                case "Description":
                    value = Description;
                    break;
                default:
                    base.GetProperty(propertyName, out value);
                    break;
            }
        }

#if false
        internal override void ResetSearch()
        {
            WasDescriptionFound = false;
            base.ResetSearch();
        }

        internal override uint Search(string searchString)
        {
            uint count = base.Search(searchString);

            WasDescriptionFound = !string.IsNullOrWhiteSpace(Description) && Description.Contains(searchString);

            return count + (uint)(WasDescriptionFound ? 1 : 0);
        }
#endif

        internal override void SetProperty(string propertyName, string newValue)
        {
            switch (propertyName)
            {
                case "Description":
                    Description = newValue;
                    break;
                default:
                    base.SetProperty(propertyName, newValue);
                    break;
            }
        }
    }
}
