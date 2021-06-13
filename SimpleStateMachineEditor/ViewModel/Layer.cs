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
        [XmlAttribute]
        [ReadOnly(true)]
        public bool IsDefaultLayer { get; set; }

        [Browsable(false)]
        [XmlElement(elementName:"IsDefaultLayer")]
        public bool? DeprecatedIsDefaultLayer { get; set; }

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
        public ObservableCollection<ITransitionEndpoint> Members { get; private set; }

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

        [XmlAttribute]
        [Browsable(false)]
        public LayerPosition.GroupStatuses DefaultGroupStatus { get; set; }



        //  Constructor for use by serialization ONLY

        public Layer()
        {
            Members = new ObservableCollection<ITransitionEndpoint>();
            Members.CollectionChanged += Members_CollectionChanged;
            MemberIds = new List<int>();
            DefaultGroupStatus = LayerPosition.GroupStatuses.NotGrouped;
        }

        //  Constructor for new object creation through commands

        private Layer(ViewModelController controller, string rootName, bool isDefaultLayer, LayerPosition.GroupStatuses defaultGroupStatus) : base(controller, controller.StateMachine.Layers, rootName)
        {
            IsDefaultLayer = isDefaultLayer;
            Members = new ObservableCollection<ITransitionEndpoint>();
            Members.CollectionChanged += Members_CollectionChanged;
            MemberIds = null;
            DefaultGroupStatus = defaultGroupStatus;
        }

        private Layer(ViewModelController controller, LayerPosition.GroupStatuses defaultGroupStatus) : base(controller)
        {
            IsDefaultLayer = false;
            Members = new ObservableCollection<ITransitionEndpoint>();
            Members.CollectionChanged += Members_CollectionChanged;
            MemberIds = null;
            DefaultGroupStatus = defaultGroupStatus;
        }

        //  Constructor for use by Redo

        internal Layer(ViewModel.ViewModelController controller, UndoRedo.AddLayerRecord redoRecord) : base(controller, redoRecord)
        {
            using (new UndoRedo.DontLogBlock(controller))
            {
                DefaultGroupStatus = redoRecord.DefaultGroupStatus;
                Members = new ObservableCollection<ITransitionEndpoint>();
                Members.CollectionChanged += Members_CollectionChanged;
                LoadMembersFromIds(controller, controller.StateMachine, redoRecord.MemberIds);
            }
        }

        internal static Layer Create(ViewModelController controller, IconControls.OptionsPropertiesPage optionsPage, bool isDefaultLayer, LayerPosition.GroupStatuses defaultGroupStatus)
        {
            using (new UndoRedo.DontLogBlock(controller))
            {
                return new Layer(controller, optionsPage.LayerRootName, isDefaultLayer, defaultGroupStatus);
            }
        }

        internal static Layer Create(ViewModelController controller, LayerPosition.GroupStatuses defaultGroupStatus)
        {
            using (new UndoRedo.DontLogBlock(controller))
            {
                return new Layer(controller, defaultGroupStatus);
            }
        }

        internal override void DeserializeCleanup(DeserializeCleanupPhases phase, ViewModelController controller, StateMachine stateMachine)
        {
            base.DeserializeCleanup(phase, controller, stateMachine);
            if (phase == DeserializeCleanupPhases.ObjectResolution)
            {
                if (DeprecatedIsDefaultLayer.HasValue)
                {
                    IsDefaultLayer = DeprecatedIsDefaultLayer.Value;
                }
                LoadMembersFromIds(controller, stateMachine, MemberIds);
            }
        }

        private void LoadMembersFromIds(ViewModelController controller, ViewModel.StateMachine stateMachine, IEnumerable<int> idList)
        {
            using (new UndoRedo.DontLogBlock(controller))
            {

                foreach (int memberId in MemberIds)
                {
                    ObjectModel.ITransitionEndpoint member = Find(memberId) as ObjectModel.ITransitionEndpoint;
                    Members.Add(member);
                }
            }
            MemberIds = null;
        }

        private void Members_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            MemberIds = null;
        }

        protected override void OnRemoving()
        {
            Members.CollectionChanged -= Members_CollectionChanged;
            base.OnRemoving();
        }
    }
}
