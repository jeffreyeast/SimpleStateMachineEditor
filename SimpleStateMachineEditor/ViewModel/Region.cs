﻿using SimpleStateMachineEditor.ObjectModel;
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
    //      The Region class represents a state.
    //--
    public class Region : ObjectModel.PositionableObject
    {
        [DisplayName("Display colors")]
        [Description("Foreground and background colors")]
        public Utility.DisplayColors DisplayColors
        {
            get => _displayColors;
            set
            {
                if ((_displayColors == null ^ value == null) ||  !_displayColors.Equals(value))
                {
                    Controller?.LogUndoAction(new UndoRedo.PropertyChangedRecord(Controller, this, "DisplayColors", _displayColors?.ToString()));
                    _displayColors = value;
                    OnPropertyChanged("DisplayColors");
                }
            }
        }
        Utility.DisplayColors _displayColors;

        [Description("Icons in this region are not shown on the display")]
        public bool IsHidden
        {
            get => _isHidden;
            set
            {
                if (_isHidden != value)
                {
                    _isHidden = value;
                    OnPropertyChanged("IsHidden");
                }
            }
        }
        bool _isHidden;

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

        public Region()
        {
            DisplayColors = Utility.BrushesToList.DisplayColors.Where(dc => dc.Foreground.ColorName == "White" && dc.Background.ColorName == "OrangeRed").Single();
            Members = new ObservableCollection<TrackableObject>();
            Members.CollectionChanged += Members_CollectionChanged;
            MemberIds = new List<int>();
        }

        //  Constructor for new object creation through commands

        private Region(ViewModelController controller, string rootName) : base(controller, controller.StateMachine.Regions, rootName)
        {
            DisplayColors = Utility.BrushesToList.DisplayColors.Where(dc => dc.Foreground.ColorName == "White" && dc.Background.ColorName == "OrangeRed").Single();
            Members = new ObservableCollection<TrackableObject>();
            Members.CollectionChanged += Members_CollectionChanged;
            MemberIds = null;
        }

        //  Constructor for use by Redo

        internal Region(ViewModel.ViewModelController controller, UndoRedo.AddRegionRecord redoRecord) : base(controller, redoRecord)
        {
            using (new UndoRedo.DontLogBlock(controller))
            {
                DisplayColors = redoRecord.DisplayColors;
                IsHidden = redoRecord.IsHidden;
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

        internal static Region Create(ViewModelController controller, IconControls.OptionsPropertiesPage optionsPage)
        {
            using (new UndoRedo.DontLogBlock(controller))
            {
                return new Region(controller, optionsPage.RegionRootName);
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

        internal override void GetProperty(string propertyName, out string value)
        {
            switch (propertyName)
            {
                case "DisplayColors":
                    value = DisplayColors.ToString();
                    break;

                case "IsHidden":
                    value = IsHidden.ToString();
                    break;

                default:
                    base.GetProperty(propertyName, out value);
                    break;
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

        internal override void SetProperty(string propertyName, string newValue)
        {
            switch (propertyName)
            {
                case "DisplayColors":
                    DisplayColors = Utility.DisplayColors.Parse(newValue);
                    break;
                case "IsHidden":
                    IsHidden = bool.Parse(newValue);
                    break;
                default:
                    base.SetProperty(propertyName, newValue);
                    break;
            }
        }

        /// <summary>
        /// Invoked when an icon is "dropped" onto a region icon.
        /// </summary>
        /// <param name="referencedObject"></param>
        internal void ToggleMember(TrackableObject member)
        {
            if (IsChangeAllowed)
            {
                if (Members.Contains(member))
                {
                    Members.Remove(member);
                }
                else
                {
                    Members.Add(member);
                }
                EndChange();
            }
        }
    }
}
