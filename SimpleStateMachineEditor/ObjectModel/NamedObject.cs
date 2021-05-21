using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SimpleStateMachineEditor.ObjectModel
{
    public class NamedObject : TrackableObject
    {
        const int LongNameLength = 12;

        [XmlAttribute]
        [Description("Name of the object")]
        public string Name
        {
            get => _name;
            set
            {
                if (_name != value && IsChangeAllowed)
                {
                    Controller?.LogUndoAction(new UndoRedo.PropertyChangedRecord(Controller, this, "Name", _name));
                    _name = value;
                    OnPropertyChanged("Name");
                    EndChange();
                    WrappedName = WrapName(_name);
                }
            }
        }
        string _name;

        [Description("Comment describing the object")]
        public string Description
        {
            get => _description;
            set
            {
                if (_description != value && IsChangeAllowed)
                {
                    Controller?.LogUndoAction(new UndoRedo.PropertyChangedRecord(Controller, this, "Description", _description));
                    _description = value;
                    OnPropertyChanged("Description");
                    EndChange();
                }
            }
        }
        string _description;

        [XmlIgnore]
        [Browsable(false)]
        public string WrappedName
        {
            get => _wrappedName;
            set
            {
                if (_wrappedName != value)
                {
                    _wrappedName = value;
                    OnPropertyChanged("WrappedName");
                }
            }
        }
        string _wrappedName;

        [XmlIgnore]
        [Browsable(false)]
        public bool WasNameFound
        {
            get => _wasNameFound;
            set
            {
                if (_wasNameFound != value)
                {
                    _wasNameFound = value;
                    OnPropertyChanged("WasNameFound");
                }
            }
        }
        bool _wasNameFound;

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

        public NamedObject()
        {
        }

        //  Constructor for use during DeserializationCleanup

        protected NamedObject(ViewModel.ViewModelController controller, string name) : base(controller)
        {
            _name = name;
        }

        //  Constructors for use by derived classes

        public NamedObject(ViewModel.ViewModelController controller) : base(controller)
        {
        }

        public NamedObject(ViewModel.ViewModelController controller, IEnumerable<NamedObject> existingObjectList, string rootName) : base(controller)
        {
            int uniquifier = 1;
            string candidateName = "";

            do
            {
                candidateName = $@"{rootName}{uniquifier++}";
            } while (existingObjectList.Where(o => o.Name == candidateName).Count() > 0);

            Name = candidateName;
        }

        //  Constructor for use by Redo

        internal NamedObject(ViewModel.ViewModelController controller, UndoRedo.NamedObjectRecord redoRecord) : base (controller, redoRecord)
        {
            using (new UndoRedo.DontLogBlock(controller))
            {
                Name = redoRecord.Name;
                Description = redoRecord.Description;
            }
        }

        internal override void GetProperty(string propertyName, out string value)
        {
            switch (propertyName)
            {
                case "Name":
                    value = Name;
                    break;
                case "Description":
                    value = Description;
                    break;
                default:
                    base.GetProperty(propertyName, out value);
                    break;
            }
        }

        private bool IsBreakableCharacter(char v)
        {
            return v == '_' || v == '.' || Char.IsUpper(v);
        }

        internal override void ResetSearch()
        {
            WasNameFound = false;
            WasDescriptionFound = false;
            base.ResetSearch();
        }

        internal override uint Search(string searchString)
        {
            uint count = base.Search(searchString);

            WasNameFound = !string.IsNullOrWhiteSpace(Name) && Name.Contains(searchString);
//            WasDescriptionFound = !string.IsNullOrWhiteSpace(Description) && Description.Contains(searchString);

            return count + (uint)(WasNameFound ? 1 : 0) + (uint)(WasDescriptionFound ? 1 : 0);
        }

        internal override void SetProperty(string propertyName, string newValue)
        {
            switch (propertyName)
            {
                case "Name":
                    Name = newValue;
                    break;
                case "Description":
                    Description = newValue;
                    break;
                default:
                    base.SetProperty(propertyName, newValue);
                    break;
            }
        }

        public override string ToString()
        {
            return (Name ?? "<unnamed>");
        }

        private string WrapName(string name)
        {
            
            string result = "";

            if (!string.IsNullOrWhiteSpace(name))
            {
                List<string> segments = new List<string>();
                int lastBreakableCharacterIndex = 0;
                int i;

                for (i = 1; i < name.Length; i++)
                {
                    if (IsBreakableCharacter(name[i]))
                    {
                        segments.Add(name.Substring(lastBreakableCharacterIndex, i - lastBreakableCharacterIndex));
                        lastBreakableCharacterIndex = i;
                    }
                }
                segments.Add(name.Substring(lastBreakableCharacterIndex, i - lastBreakableCharacterIndex));

                int lineLength = 0;
                foreach (string segment in segments)
                {
                    if (lineLength + segment.Length >= LongNameLength)
                    {
                        result += "\r\n";
                        lineLength = 0;
                    }
                    result += segment;
                    lineLength += segment.Length;
                }
            }

            return result;
        }
    }
}
