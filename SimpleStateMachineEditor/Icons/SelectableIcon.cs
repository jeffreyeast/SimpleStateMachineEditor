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
                    Body.MouseLeftButtonDown += MouseLeftButtonDownHandler;
                    Body.MouseRightButtonDown += MouseRightButtonDownHandler;
                    Body.MouseRightButtonUp += MouseRightButtonUpHandler;
                }
            }
        }




        protected SelectableIcon(DesignerControl designer, TrackableObject referencedObject, Point? center, Size? size) : base(designer, referencedObject, center, size)
        {
        }

        public bool IsSelectable => (Designer.SelectedObjects.Count == 1 && (IsSelected || Designer.SelectedObjects[0] is ViewModel.StateMachine)) || Designer.SelectedIcons.Count == 0;

        public bool IsSelected => Designer.SelectedObjects.Contains(ReferencedObject);

        public void IsSelectedChanged()
        {
            OnPropertyChanged("IsSelected");
        }

        protected void MouseLeftButtonDownHandler(object sender, MouseButtonEventArgs e)
        {
            Designer.SelectableIconMouseLeftButtonDownHandler(e.GetPosition(Designer.IconSurface), this);
            e.Handled = true;
        }

        private void MouseRightButtonDownHandler(object sender, MouseButtonEventArgs e)
        {
            Designer.SelectableIconMouseRightButtonDownHandler(e.GetPosition(Designer.IconSurface), this);
            e.Handled = true;
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
                Body.MouseLeftButtonDown -= MouseLeftButtonDownHandler;
                Body.MouseRightButtonDown -= MouseRightButtonDownHandler;
                Body.MouseRightButtonUp -= MouseRightButtonUpHandler;
            }
            base.OnRemoving();
        }

        TrackableObject ISelectableIcon.ReferencedObject => ReferencedObject;
    }
}
