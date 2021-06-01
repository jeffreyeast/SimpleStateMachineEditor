using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SimpleStateMachineEditor.ObjectModel
{
    public abstract class TrackableObject : INotifyPropertyChanged, IRemovableObject
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

        public event PropertyChangedEventHandler PropertyChanged;
        public event ObjectModel.RemovingHandler Removing;

        [XmlIgnore]
        [Browsable(false)]
        public int GID { get; private set; }
        static int _gid = 0;

        //  Constructor for use by serializer ONLY

        public TrackableObject()
        {
            Id = -2;
            GID = System.Threading.Interlocked.Increment(ref _gid);
        }

        //  Constructor for use by derived classes

        public TrackableObject(ViewModel.ViewModelController controller)
        {
            Controller = controller;
            Id = Controller.NextId;
            GID = System.Threading.Interlocked.Increment(ref _gid);
        }

        //  For use in Redo recovery

        internal TrackableObject(ViewModel.ViewModelController controller, UndoRedo.TrackableObjectRecord redoRecord)
        {
            Controller = controller;
            Id = redoRecord.Id;
            GID = System.Threading.Interlocked.Increment(ref _gid);
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

        internal virtual void DeserializeCleanupPhase2(ViewModel.ViewModelController controller, ViewModel.StateMachine stateMachine)
        {
            //  Some objects in legacy .SFSA files may not have derived from TrackableObject, but do now.  Assign such objects unique Ids.

            if (Id == -1)
            {
                Id = controller.NextId;
            }
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

        protected virtual void OnRemoving()
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

        internal virtual void SetProperty(string propertyName, IList<string> newValue)
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

        public void Remove()
        {
            Debug.WriteLine($@">>>TrackableObject.Remove {GetType().ToString()}: {ToString()}");
            OnRemoving();
        }
    }
}
