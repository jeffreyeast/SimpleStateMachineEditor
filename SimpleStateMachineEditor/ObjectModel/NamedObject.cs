using SimpleStateMachineEditor.ViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SimpleStateMachineEditor.ObjectModel
{
    public class NamedObject : DocumentedObject, INamedObject
    {
        const int LongNameLength = 12;

        [XmlAttribute]
        [Description("Name of the object")]
        public string Name
        {
            get => _name;
            set
            {
                if (_name != value)
                {
                    if (Controller?.UndoManager == null)
                    {
                        _name = value;
                        OnPropertyChanged("Name");
                        WrappedName = WrapName(_name);
                    }
                    else
                    {
                        using (Controller.CreateAtomicGuiChangeBlock("Change Name property"))
                        {
                            Controller.LogUndoAction(new UndoRedo.PropertyChangedRecord(Controller, this, "Name", _name));
                            _name = value;
                            OnPropertyChanged("Name");
                            WrappedName = WrapName(_name);
                        }
                    }
                }
            }
        }
        string _name;

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
        internal NamedObject CoNamedObject 
        {
            get => _coNamedObject;
            set
            {
                if (_coNamedObject != value)
                {
                    NamedObject oldCoNamedObject = _coNamedObject;

                    if (oldCoNamedObject != null)
                    {
                        oldCoNamedObject.PropertyChanged -= CoNamedObjectPropertyChangedHandler;
                        oldCoNamedObject.Removing -= CoNamedObjectIsBeingRemovedHandler;
                    }
                    OnCoNamedObjectChange(oldCoNamedObject, value);
                    Controller?.LogUndoAction(new UndoRedo.PropertyChangedRecord(Controller, this, "CoNamedObject", (oldCoNamedObject?.Id ?? TrackableObject.NullId).ToString()));
                    _coNamedObject = value;
                    if (oldCoNamedObject != null)
                    {
                        oldCoNamedObject.CoNamedObject = null;
                    }
                    if (_coNamedObject == null)
                    {
                        CoNamedObjectId = TrackableObject.NullId;
                    }
                    else
                    {
                        _coNamedObject.PropertyChanged += CoNamedObjectPropertyChangedHandler;
                        _coNamedObject.Removing -= CoNamedObjectIsBeingRemovedHandler;
                        _coNamedObject.CoNamedObject = this;
                    }
                    OnPropertyChanged("CoNamedObject");
                }
            }
        }
        NamedObject _coNamedObject;

        [Browsable(false)]
        [XmlAttribute]
        public int CoNamedObjectId
        {
            get => CoNamedObject?.Id ?? _coNamedObjectId;
            set => _coNamedObjectId = value;
        }
        int _coNamedObjectId = TrackableObject.NullId;




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

        protected NamedObject(ViewModel.ViewModelController controller) : base(controller)
        {
        }

        protected NamedObject(ViewModel.ViewModelController controller, IEnumerable<NamedObject> existingObjectList, string rootName) : base(controller)
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
                CoNamedObject = Find(redoRecord.CoNamedObjectId) as NamedObject;
                if (CoNamedObject != null)
                {
                    CoNamedObject.CoNamedObject = this;
                }
                Name = redoRecord.Name;
            }
        }

        private void CoNamedObjectIsBeingRemovedHandler(IRemovableObject item)
        {
            if (_coNamedObject != null)
            {
                CoNamedObject = null;
            }
        }


        private void CoNamedObjectPropertyChangedHandler(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "Name":
                    Name = CoNamedObject.Name;
                    break;
                default:
                    break;
            }
        }

        internal override void DeserializeCleanup(DeserializeCleanupPhases phase, ViewModelController controller, StateMachine stateMachine)
        {
            base.DeserializeCleanup(phase, controller, stateMachine);
            if (phase == DeserializeCleanupPhases.ObjectResolution)
            {
                CoNamedObject = Find(CoNamedObjectId) as NamedObject;
            }
        }

        internal override void GetProperty(string propertyName, out string value)
        {
            switch (propertyName)
            {
                case "CoNamedObject":
                    value = (CoNamedObject?.Id ?? TrackableObject.NullId).ToString();
                    break;
                case "Name":
                    value = Name;
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

        protected virtual void OnCoNamedObjectChange(NamedObject preValue, NamedObject postValue) { }

        protected override void OnRemoving()
        {
            CoNamedObject = null;
            base.OnRemoving();
        }

        internal override void ResetSearch()
        {
            WasNameFound = false;
            base.ResetSearch();
        }

        internal override uint Search(string searchString)
        {
            uint count = base.Search(searchString);

            WasNameFound = !string.IsNullOrWhiteSpace(Name) && Name.Contains(searchString);

            return count + (uint)(WasNameFound ? 1 : 0) + (uint)(WasDescriptionFound ? 1 : 0);
        }

        internal override void SetProperty(string propertyName, string newValue)
        {
            switch (propertyName)
            {
                case "CoNamedObject":
                    CoNamedObject = Find(int.Parse(newValue)) as NamedObject;
                    break;
                case "Name":
                    Name = newValue;
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
