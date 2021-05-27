using SimpleStateMachineEditor.ObjectModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SimpleStateMachineEditor.Icons
{
    internal abstract class SelectableIcon : IconBase, ISelectableIcon
    {
        public override Control Body
        {
            get => base.Body;
            internal set
            {
                base.Body = value;
                if (Body != null)
                {
                    Body.MouseLeftButtonDown += IconSelectedHandler;
                    Body.MouseRightButtonUp += MouseRightButtonUpHandler;
                }
            }
        }

        public bool IsRegionHighlighted => HighlightedRegion != null;

        public ViewModel.Region HighlightedRegion
        {
            get => _highlightedRegion;
            set
            {
                if (_highlightedRegion != value)
                {
                    _highlightedRegion = value;
                    OnPropertyChanged("HighlightedRegion");
                    OnPropertyChanged("IsRegionHighlighted");
                }
            }
        }
        ViewModel.Region _highlightedRegion;




        protected SelectableIcon(DesignerControl designer, TrackableObject referencedObject, Point? center, Size? size) : base(designer, referencedObject, center, size)
        {
        }

        protected void IconSelectedHandler(object sender, MouseButtonEventArgs e)
        {
            Designer.SelectableIconMouseLeftButtonDownHandler(e.GetPosition(Designer.IconSurface), this);
            e.Handled = true;
        }

        public bool IsSelected => Designer.SelectedIcons.ContainsKey(ReferencedObject);

        public void IsSelectedChanged()
        {
            OnPropertyChanged("IsSelected");
        }

        private void MouseRightButtonUpHandler(object sender, MouseButtonEventArgs e)
        {
            Designer.SelectableIconMouseRightButtonUpHandler(e.GetPosition(Designer.IconSurface), this);
            e.Handled = true;
        }

        protected override void OnRemoving()
        {
            if (Body != null)
            {
                Body.MouseLeftButtonDown -= IconSelectedHandler;
                Body.MouseRightButtonUp -= MouseRightButtonUpHandler;
            }
            base.OnRemoving();
        }

        TrackableObject ISelectableIcon.ReferencedObject => ReferencedObject;
    }
}
