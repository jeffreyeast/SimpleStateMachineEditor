using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Serialization;

namespace SimpleStateMachineEditor.ObjectModel
{
    /// <summary>
    /// This is the poor-man's dictionary for layer positions. We can't use a Dictionary because XML can't serialize it
    /// </summary>
    public class LayerPosition : INotifyPropertyChanged
    {
        [XmlIgnore]
        public ViewModel.Layer Layer { get; set; }

        [XmlAttribute]
        public int LayerId
        {
            get => Layer?.Id ?? _layerId;
            set => _layerId = value;
        }
        int _layerId = -1;

        public Point Position 
        {
            get => _position;
            set
            {
                if (_position.X != value.X || _position.Y != value.Y)
                {
                    _position = value;
                    OnPropertyChanged("Position");
                }
            }
        }
        Point _position;

        public event PropertyChangedEventHandler PropertyChanged;




        internal void DeserializeCleanup(ViewModel.ViewModelController controller, ViewModel.StateMachine stateMachine)
        {
            Layer = stateMachine.Find(_layerId) as ViewModel.Layer;
            if (Layer == null)
            {
                throw new ArgumentOutOfRangeException("ObjectModel.LayeredPositionalObject.LayerId", _layerId, "Unable to locate layer");
            }
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
