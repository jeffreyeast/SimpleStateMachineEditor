using SimpleStateMachineEditor.ObjectModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SimpleStateMachineEditor.Icons
{
    internal class SelectionBoxIcon : DraggableIcon
    {
        public override int ContextMenuId => throw new NotImplementedException();


        public PathGeometry Path { get; private set; }




        internal SelectionBoxIcon(DesignerControl designer, Point? center, Size? size) : base(designer, null, center, size)
        {
            Path = new PathGeometry();
        }


        protected override FrameworkElement CreateDraggableShape()
        {
            return new IconControls.SelectionBox()
            {
                DataContext = this,
            };
        }

        protected override Control CreateIcon()
        {
            return null;
        }

        public override void CommitDrag(Point dragTerminationPoint, Point offset)
        {
            Designer.IconSurface.Children.Remove(DraggableShape);
        }

        public override void Drag(Point mousePosition, Point offset)
        {
            PathFigure pathFigure = new PathFigure() { StartPoint = Designer.MouseStateMachine.DragOrigin, };
            pathFigure.Segments.Add(new LineSegment() { Point = new Point(Designer.MouseStateMachine.DragOrigin.X, Designer.MouseStateMachine.DragOrigin.Y + offset.Y ), });
            pathFigure.Segments.Add(new LineSegment() { Point = new Point(Designer.MouseStateMachine.DragOrigin.X + offset.X, Designer.MouseStateMachine.DragOrigin.Y + offset.Y), });
            pathFigure.Segments.Add(new LineSegment() { Point = new Point(Designer.MouseStateMachine.DragOrigin.X + offset.X, Designer.MouseStateMachine.DragOrigin.Y), });
            pathFigure.Segments.Add(new LineSegment() { Point = new Point(Designer.MouseStateMachine.DragOrigin.X, Designer.MouseStateMachine.DragOrigin.Y), });
            Path.Figures.Clear();
            Path.Figures.Add(pathFigure);
        }

        public override void StartDrag()
        {
            Designer.IconSurface.Children.Add(DraggableShape);
            Path.Figures.Clear();
        }
    }
}
