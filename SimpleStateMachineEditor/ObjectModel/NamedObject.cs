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
                    if (Controller?.LoggingIsEnabled ?? false)
                    {
                        Controller?.UndoManager.Add(new UndoRedo.PropertyChangedRecord(Controller, this, "Name", _name));
                    }
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
                    if (Controller?.LoggingIsEnabled ?? false)
                    {
                        Controller?.UndoManager.Add(new UndoRedo.PropertyChangedRecord(Controller, this, "Description", _description));
                    }
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




        //  Constructor for use by serialization ONLY

        public NamedObject()
        {
        }

        //  Constructors for use by derived classes

        public NamedObject(ViewModel.ViewModelController controller) : base(controller)
        {
        }

        public NamedObject(ViewModel.ViewModelController controller, IEnumerable<NamedObject> existingObjectList, string rootName) : base(controller)
        {
            int uniquifier = 1;

            do
            {
                Name = $@"{rootName}{uniquifier++}";
            } while (existingObjectList.Where(o => o.Name == Name).Count() > 0);
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
