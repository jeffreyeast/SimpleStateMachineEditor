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
        public ObservableCollection<ITrackableObject> Members { get; private set; }

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
            Members = new ObservableCollection<ITrackableObject>();
            MemberIds = new List<int>();
        }

        //  Constructor for new object creation through commands

        private Layer(ViewModelController controller, string rootName, bool isDefaultLayer) : base(controller, controller.StateMachine.Layers, rootName)
        {
            IsDefaultLayer = isDefaultLayer;
            Members = new ObservableCollection<ITrackableObject>();
            MemberIds = null;
        }

        private Layer(ViewModelController controller) : base(controller)
        {
            IsDefaultLayer = false;
            Members = new ObservableCollection<ITrackableObject>();
            MemberIds = null;
        }

        //  Constructor for use by Redo

        internal Layer(ViewModel.ViewModelController controller, UndoRedo.AddLayerRecord redoRecord) : base(controller, redoRecord)
        {
            using (new UndoRedo.DontLogBlock(controller))
            {
                Members = new ObservableCollection<ITrackableObject>();
                LoadMembersFromIds(controller, controller.StateMachine, redoRecord.MemberIds);
            }
        }

        internal static Layer Create(ViewModelController controller, IconControls.OptionsPropertiesPage optionsPage, bool isDefaultLayer)
        {
            using (new UndoRedo.DontLogBlock(controller))
            {
                return new Layer(controller, optionsPage.LayerRootName, isDefaultLayer);
            }
        }

        internal static Layer Create(ViewModelController controller)
        {
            using (new UndoRedo.DontLogBlock(controller))
            {
                return new Layer(controller);
            }
        }

        internal override void DeserializeCleanup(DeserializeCleanupPhases phase, ViewModelController controller, StateMachine stateMachine)
        {
            base.DeserializeCleanup(phase, controller, stateMachine);
            if (phase == DeserializeCleanupPhases.ObjectResolution)
            {
                LoadMembersFromIds(controller, stateMachine, MemberIds);
            }
        }

        private void LoadMembersFromIds(ViewModelController controller, ViewModel.StateMachine stateMachine, IEnumerable<int> idList)
        {
            using (new UndoRedo.DontLogBlock(controller))
            {

                foreach (int memberId in MemberIds)
                {
                    TrackableObject member = Find(memberId);
                    Members.Add(member);
                }
            }
            MemberIds = null;
        }
    }
}
