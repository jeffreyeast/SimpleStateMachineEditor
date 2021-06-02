using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleStateMachineEditor.IconControls
{
    public class OptionsPropertiesPage : DialogPage, INotifyPropertyChanged
    {
        [Category("General")]
        [DisplayName("Action font size")]
        [Description("Font size for action method names")]
        public double ActionFontSize
        {
            get => _actionFontSize;
            set
            {
                if (_actionFontSize != value && value >= 4)
                {
                    _actionFontSize = value;
                    OnPropertyChanged("ActionFontSize");
                }
            }
        }
        double _actionFontSize;

        [Category("General")]
        [DisplayName("Event name font size")]
        [Description("Font size for event names on the display")]
        public double EventNameFontSize
        {
            get => _eventNameFontSize;
            set
            {
                if (_eventNameFontSize != value && value >= 4)
                {
                    _eventNameFontSize = value;
                    OnPropertyChanged("EventNameFontSize");
                }
            }
        }
        double _eventNameFontSize;

        [Category("General")]
        [DisplayName("Hide action names")]
        [Description("Editor will not show action names for transitions if set")]
        public bool HideActionNames 
        {
            get => _hideActionNames;
            set
            {
                if (_hideActionNames != value)
                {
                    _hideActionNames = value;
                    OnPropertyChanged("HideActionNames");
                }
            }
        }
        bool _hideActionNames;

        [Category("General")]
        [DisplayName("Default action name")]
        [Description("Root name for newly-created action methods")]
        public string ActionRootName
        {
            get => _actionRootName;
            set
            {
                if (_actionRootName != value)
                {
                    _actionRootName = value;
                    OnPropertyChanged("ActionRootName");
                }
            }
        }
        string _actionRootName;

        [Category("General")]
        [DisplayName("Default event type name")]
        [Description("Root name for newly-created event types")]
        public string EventTypeRootName
        {
            get => _eventTypeRootName;
            set
            {
                if (_eventTypeRootName != value)
                {
                    _eventTypeRootName = value;
                    OnPropertyChanged("EventTypeRootName");
                }
            }
        }
        string _eventTypeRootName;

        [Category("General")]
        [DisplayName("Default group name")]
        [Description("Root name for newly-created groups of states")]
        public string GroupRootName
        {
            get => _groupRootName;
            set
            {
                if (_groupRootName != value)
                {
                    _groupRootName = value;
                    OnPropertyChanged("GroupRootName");
                }
            }
        }
        string _groupRootName;

        [Category("General")]
        [DisplayName("Default icon layer name")]
        [Description("Root name for newly-created icon layers")]
        public string LayerRootName
        {
            get => _layerRootName;
            set
            {
                if (_layerRootName != value)
                {
                    _layerRootName = value;
                    OnPropertyChanged("LayerRootName");
                }
            }
        }
        string _layerRootName;

        [Category("General")]
        [DisplayName("Default state name")]
        [Description("Root name for newly-created states")]
        public string StateRootName
        {
            get => _stateRootName;
            set
            {
                if (_stateRootName != value)
                {
                    _stateRootName = value;
                    OnPropertyChanged("StateRootName");
                }
            }
        }
        string _stateRootName;



        public event PropertyChangedEventHandler PropertyChanged;




        public OptionsPropertiesPage()
        {
            _actionFontSize = 10;
            _eventNameFontSize = 14;
            _hideActionNames = false;
            _actionRootName = "DoSomething";
            _eventTypeRootName = "E";
            _groupRootName = "Group";
            _layerRootName = "Layer";
            _stateRootName = "S";
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
