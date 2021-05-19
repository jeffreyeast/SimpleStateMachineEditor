using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SimpleStateMachineEditor.ObjectModel
{
    public abstract class TrackableObject : INotifyPropertyChanged
    {
        internal bool IsChangeAllowed => Controller?.CanGuiChangeBegin() ?? true;

        [XmlAttribute]
        [Browsable(false)]
        public int Id
        {
            get => _id;
            set
            {
                if (_id != value)
                {
                    _id = value;
                    OnPropertyChanged("Id");
                }
            }
        }
        int _id;

        [XmlIgnore]
        internal ViewModel.ViewModelController Controller { get; set; }

        internal delegate void RemovingHandler(TrackableObject sender);

        internal event RemovingHandler Removing;
        public event PropertyChangedEventHandler PropertyChanged;


        //  Constructor for use by serializer ONLY

        public TrackableObject()
        {
            Id = -1;
        }

        //  Constructor for use by derived classes

        public TrackableObject(ViewModel.ViewModelController controller)
        {
            Controller = controller;
            Id = Controller.NextId;
        }

        //  For use in Redo recovery

        internal TrackableObject(ViewModel.ViewModelController controller, UndoRedo.TrackableObjectRecord redoRecord)
        {
            Controller = controller;
            Id = redoRecord.Id;
        }

        internal virtual void GetProperty(string propertyName, out string value)
        {
            throw new ArgumentException($@"Property '{propertyName}' not recognized");
        }

        internal virtual void GetProperty(string propertyName, out IEnumerable<string> value)
        {
            throw new ArgumentException($@"Property '{propertyName}' not recognized");
        }

        internal virtual void DeserializeCleanup(ViewModel.ViewModelController controller, ViewModel.StateMachine stateMachine)
        {
            Controller = controller;
            Controller.DeserializeCleanup(this);
        }

        internal void EndChange()
        {
            Controller?.NoteGuiChangeEnd();
        }

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                if (ThreadHelper.CheckAccess())
                {
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
                }
                else
                {
                    ThreadHelper.Generic.BeginInvoke(new Action(() => {
                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
                    }));
                }
            }
        }

        internal virtual void OnRemoving()
        {
            Removing?.Invoke(this);
        }

        internal virtual void ResetSearch() { }

        internal virtual uint Search(string searchString)
        {
            return 0;
        }

        internal virtual void SetProperty(string propertyName, string newValue)
        {
            throw new ArgumentException($@"Property '{propertyName}' not recognized");
        }

        internal virtual void SetProperty(string propertyName, IEnumerable<string> newValue)
        {
            if (newValue.Count() == 1)
            {
                SetProperty(propertyName, newValue.First());
            }
            else
            {
                throw new ArgumentException($@"Property '{propertyName}' not recognized");
            }
        }
    }
}
