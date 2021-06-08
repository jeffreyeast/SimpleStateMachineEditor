using Microsoft.VisualStudio;
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
    internal abstract class DraggableIcon : SelectableIcon, IDraggableIcon
    {

        internal DraggableIcon(DesignerControl designer, ITrackableObject o, System.Windows.Point? center, Size? size) :
            base(designer, o, center, size)
        {
        }

        public virtual void CancelDrag()
        {
            Designer.IconSurface.Children.Remove(DraggableShape);
            OnEndDrag();
        }

        public virtual void CommitDrag(Point dragTerminationPoint, Point offset)
        {
            Designer.IconSurface.Children.Remove(DraggableShape);
            OnEndDrag();

            //  This is simply dropping the icon onto the surface

            CenterPosition = new System.Windows.Point(Math.Max(0, CenterPosition.X + offset.X), Math.Max(0, CenterPosition.Y + offset.Y));
            OnCommitDrag(dragTerminationPoint);
        }

        public virtual void Drag(Point mousePosition, Point offset)
        {
            DraggableShape.Margin = new Thickness(Math.Max(0, Body.Margin.Left + offset.X), Math.Max(0, Body.Margin.Top + offset.Y), 0, 0);
        }

        protected virtual void OnCommitDrag(Point dragTerminationPoint) { }
        protected virtual void OnEndDrag() { }
        protected virtual void OnStartDrag() { }

        public virtual void StartDrag()
        {
            Designer.IconSurface.Children.Add(DraggableShape);
            DraggableShape.Margin = Body.Margin;
            OnStartDrag();
        }
    }
}
