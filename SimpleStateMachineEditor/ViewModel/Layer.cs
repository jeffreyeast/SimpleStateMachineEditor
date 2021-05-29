using SimpleStateMachineEditor.ObjectModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Xml.Serialization;

namespace SimpleStateMachineEditor.ViewModel
{
    //++
    //      The Layer class represents a set of objects that are viewed and editted together
    //--
    public class Layer : ObjectModel.NamedObject
    {
        [ReadOnly(true)]
        public bool IsDefaultLayer { get; set; }

        [Browsable(false)]
        [XmlIgnore]
        public bool IsCurrentLayer
        {
            get => _isCurrentLayer;
            set
            {
                if (_isCurrentLayer != value)
                {
                    _isCurrentLayer = value;
                    OnPropertyChanged("IsCurrentLayer");
                }
            }
        }
        bool _isCurrentLayer;

        [Browsable(false)]
        [XmlIgnore]
        public ObservableCollection<TrackableObject> Members { get; private set; }

        [Browsable(false)]
        public List<int> MemberIds 
        {
            get
            {
                if (_memberIds == null)
                {
                    _memberIds = Members.Select(m => m.Id).ToList();
                }
                return _memberIds;
            }
            private set
            {
                _memberIds = value;
            }
        }
        List<int> _memberIds;




        //  Constructor for use by serialization ONLY

        public Layer()
        {
            Members = new ObservableCollection<TrackableObject>();
            Members.CollectionChanged += Members_CollectionChanged;
            MemberIds = new List<int>();
        }

        //  Constructor for new object creation through commands

        private Layer(ViewModelController controller, string rootName, bool isDefaultLayer) : base(controller, controller.StateMachine.Layers, rootName)
        {
            IsDefaultLayer = isDefaultLayer;
            Members = new ObservableCollection<TrackableObject>();
            Members.CollectionChanged += Members_CollectionChanged;
            MemberIds = null;
        }

        //  Constructor for use by Redo

        internal Layer(ViewModel.ViewModelController controller, UndoRedo.AddLayerRecord redoRecord) : base(controller, redoRecord)
        {
            using (new UndoRedo.DontLogBlock(controller))
            {
                Members = new ObservableCollection<TrackableObject>();
                Members.CollectionChanged += Members_CollectionChanged;
                foreach (int memberId in redoRecord.MemberIds)
                {
                    TrackableObject member = Controller.StateMachine.Find(memberId);
                    Members.Add(member);
                }
                MemberIds = null;
            }
        }

        internal static Layer Create(ViewModelController controller, IconControls.OptionsPropertiesPage optionsPage, bool isDefaultLayer)
        {
            using (new UndoRedo.DontLogBlock(controller))
            {
                return new Layer(controller, optionsPage.LayerRootName, isDefaultLayer);
            }
        }

        internal override void DeserializeCleanup(ViewModelController controller, ViewModel.StateMachine stateMachine)
        {
            base.DeserializeCleanup(controller, stateMachine);

            using (new UndoRedo.DontLogBlock(controller))
            {

                foreach (int memberId in MemberIds)
                {
                    TrackableObject member = stateMachine.Find(memberId);
                    Members.Add(member);
                }
            }
        }

        private void Members_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            MemberIds = null;

            switch (e.Action)
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    foreach (TrackableObject member in e.NewItems)
                    {
                        member.Removing += MemberIsBeingRemovedHandler;
                    }
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    foreach (TrackableObject member in e.OldItems)
                    {
                        member.Removing -= MemberIsBeingRemovedHandler;
                    }
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        private void MemberIsBeingRemovedHandler(IRemovableObject member)
        {
            member.Removing -= MemberIsBeingRemovedHandler;
            Members.Remove(member as ObjectModel.TrackableObject);
        }

        protected override void OnRemoving()
        {
            foreach (TrackableObject member in Members)
            {
                member.Removing -= MemberIsBeingRemovedHandler;
            }
            base.OnRemoving();
        }
    }
}
