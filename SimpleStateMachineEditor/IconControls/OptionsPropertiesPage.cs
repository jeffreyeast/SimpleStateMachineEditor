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
        [DisplayName("Default icon group name")]
        [Description("Root name for newly-created icon region groups")]
        public string RegionRootName
        {
            get => _regionRootName;
            set
            {
                if (_regionRootName != value)
                {
                    _regionRootName = value;
                    OnPropertyChanged("RegionRootName");
                }
            }
        }
        string _regionRootName;

        [Category("General")]
        [DisplayName("Default staet name")]
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
            _eventTypeRootName = "E";
            _regionRootName = "Region";
            _stateRootName = "S";
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
