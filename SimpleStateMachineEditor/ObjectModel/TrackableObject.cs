using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace SimpleStateMachineEditor.ObjectModel
{
    public abstract class TrackableObject : INotifyPropertyChanged, IRemovableObject, ITrackableObject
    {
        public bool IsChangeAllowed(){ return Controller?.CanGuiChangeBegin() ?? true; }

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

        internal const int NullId = -2;

        [XmlIgnore]
        public ViewModel.ViewModelController Controller { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        public event ObjectModel.RemovingHandler Removing;

        [XmlIgnore]
        [Browsable(false)]
        public int GID { get; private set; }
        static int _gid = 0;

        //  Constructor for use by serializer ONLY

        public TrackableObject()
        {
            Id = NullId;
            GID = System.Threading.Interlocked.Increment(ref _gid);
        }

        //  Constructor for use by derived classes

        public TrackableObject(ViewModel.ViewModelController controller)
        {
            Controller = controller;
            Id = Controller.NextId;
            controller.AllFindableObjects.Add(Id, this);
            GID = System.Threading.Interlocked.Increment(ref _gid);
        }

        //  For use in Redo recovery

        internal TrackableObject(ViewModel.ViewModelController controller, UndoRedo.TrackableObjectRecord redoRecord)
        {
            Controller = controller;
            Id = redoRecord.Id;
            controller.AllFindableObjects.Add(Id, this);
            GID = System.Threading.Interlocked.Increment(ref _gid);
        }

        protected void ObservableCollectionOfRemovableObjectsChangedHandler(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (ObjectModel.TrackableObject o in e.OldItems)
                    {
                        o.Remove();
                    }
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        internal enum DeserializeCleanupPhases
        {
            First,

            MaxIdDetermination = First,
            MissingIdAssignment,
            IdRegistration,
            ObjectResolution,

            Last = ObjectResolution,
        }

        internal virtual void DeserializeCleanup(DeserializeCleanupPhases phase, ViewModel.ViewModelController controller, ViewModel.StateMachine stateMachine)
        {
            switch (phase)
            {
                case DeserializeCleanupPhases.MaxIdDetermination:
                    Controller = controller;
                    Controller.DeserializeCleanup(this);
                    break;
                case DeserializeCleanupPhases.MissingIdAssignment:
                    //  Some objects in legacy .SFSA files may not have derived from TrackableObject, but do now.  Assign such objects unique Ids.

                    if (Id == NullId)
                    {
                        Id = controller.NextId;
                    }
                    break;
                case DeserializeCleanupPhases.IdRegistration:
                    Controller.AllFindableObjects.Add(Id, this);
                    break;
                case DeserializeCleanupPhases.ObjectResolution:
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        public void EndChange()
        {
            Controller?.NoteGuiChangeEnd();
        }

        internal ObjectModel.TrackableObject Find(int id)
        {
            return Controller.AllFindableObjects[id];
        }

        internal virtual void GetProperty(string propertyName, out string value)
        {
            throw new ArgumentException($@"Property '{propertyName}' not recognized");
        }

        internal virtual void GetProperty(string propertyName, out IEnumerable<string> value)
        {
            throw new ArgumentException($@"Property '{propertyName}' not recognized");
        }

        internal void OnPropertyChanged(object sender, string propertyName)
        {
            if (PropertyChanged != null)
            {
                if (ThreadHelper.CheckAccess())
                {
                    PropertyChanged?.Invoke(sender, new PropertyChangedEventArgs(propertyName));
                }
                else
                {
                    ThreadHelper.Generic.BeginInvoke(new Action(() => {
                        PropertyChanged?.Invoke(sender, new PropertyChangedEventArgs(propertyName));
                    }));
                }
            }
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
            Controller.AllFindableObjects.Remove(Id);
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
