using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace SimpleStateMachineEditor.MouseStateMachine
{
    /// <summary>
    /// This class implements the mouse selection for the Designer control
    /// </summary>
    internal class DesignerMouseSelectionImplementation : MouseSelection
    {
        DesignerControl Designer;
        Point MousePosition;
        internal Point DragOrigin;
        Icons.IDraggableIcon DraggingIcon;





        internal DesignerMouseSelectionImplementation(DesignerControl designer)
        {
            Designer = designer;
        }


        //  Methods for the designer to pass events to the state machine

        internal void Cancel()
        {
            PostNormalPriorityEvent(EventTypes.Esc);
            Execute();
        }

        internal void MouseLeftButtonDown(Point mousePosition)
        {
            MousePosition = mousePosition;
            PostNormalPriorityEvent(EventTypes.LeftButtonDown);
            Execute();
        }

        internal void MouseLeftButtonDownOnIcon(Point mousePosition, Icons.IDraggableIcon icon)
        {
            MousePosition = mousePosition;
            DraggingIcon = icon;
            PostNormalPriorityEvent(EventTypes.LeftButtonDownOnIcon);
            Execute();
        }

        internal void MouseLeftButtonUp(Point mousePosition)
        {
            MousePosition = mousePosition;
            PostNormalPriorityEvent(EventTypes.LeftButtonUp);
            Execute();
        }

        internal void MouseMove(Point mousePosition)
        {
            MousePosition = mousePosition;
            PostNormalPriorityEvent(EventTypes.MouseMove);
            Execute();
        }

        internal void MouseRightButtonUp(Point mousePosition)
        {
            MousePosition = mousePosition;
            PostNormalPriorityEvent(EventTypes.RightButtonUp);
            Execute();
        }

        internal void MouseRightButtonUpOnIcon(Point mousePosition, Icons.IDraggableIcon icon)
        {
            MousePosition = mousePosition;
            DraggingIcon = icon;
            PostNormalPriorityEvent(EventTypes.RightButtonUpOnIcon);
            Execute();
        }

        internal void StartDraggingTransition(Icons.TransitionIcon icon)
        {
            DraggingIcon = icon;
            DragOrigin = (icon.ReferencedObject as ViewModel.Transition).SourceState == null ?
                Designer.LoadedIcons[(icon.ReferencedObject as ViewModel.Transition).DestinationState].CenterPosition :
                Designer.LoadedIcons[(icon.ReferencedObject as ViewModel.Transition).SourceState].CenterPosition;
            PostNormalPriorityEvent(EventTypes.DraggingTransition);
            Execute();
        }


        //  Implementations of the state machine's action methods


        protected override void CancelDrag()
        {
            Icons.IDraggableIcon[] selectedIcons = Designer.SelectedIcons.Values.ToArray<Icons.IDraggableIcon>();

            foreach (var icon in selectedIcons)
            {
                icon.CancelDrag();
            }
        }

        protected override void ClearSelection()
        {
            Designer.ClearSelectedItems();
        }

        protected override void CommitDrag()
        {
            Point offset = new Point(MousePosition.X - DragOrigin.X, MousePosition.Y - DragOrigin.Y);
            Icons.IDraggableIcon[] selectedIcons = Designer.SelectedIcons.Values.ToArray<Icons.IDraggableIcon>();

            foreach (var icon in selectedIcons)
            {
                icon.CommitDrag(MousePosition, offset);
            }
        }

        protected override void DeselectDraggingIcon()
        {
            Designer.DeselectIcon(DraggingIcon);
        }

        protected override void DragIcon()
        {
            Point offset = new Point(MousePosition.X - DragOrigin.X, MousePosition.Y - DragOrigin.Y);

            foreach (Icons.IDraggableIcon icon in Designer.SelectedIcons.Values)
            {
                icon.Drag(offset);
            }
        }

        protected override void DisplayIconContextMenu()
        {
            System.Guid contextMenuGuid = PackageGuids.guidSimpleStateMachineEditorPackageCmdSet;
            System.Windows.Point screenPoint;
            Microsoft.VisualStudio.Shell.Interop.POINTS point;
            Microsoft.VisualStudio.Shell.Interop.POINTS[] points;

            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();

            Designer.ContextMenuActivationLocation = MousePosition;
            screenPoint = Designer.IconSurface.PointToScreen(Designer.ContextMenuActivationLocation);

            //  If screenPoint is negative, we're on a multiple monitor system and we need to 
            //  adjust for this.
            //
            //  BUG BUG BUG
            // 
            //  It looks like ShowContextMenu has signficant problems working with multiple monitors.
            //  For now, it's error handling behavior is better than nothing.

            point = new Microsoft.VisualStudio.Shell.Interop.POINTS();
            point.x = (short)screenPoint.X;
            point.y = (short)screenPoint.Y;

            points = new[] { point };

            // TODO: error handling
            Designer.UiShell.ShowContextMenu(0, ref contextMenuGuid, DraggingIcon.ContextMenuId, points, DraggingIcon);
        }

        protected override void DisplayStateMachineContextMenu()
        {
            System.Guid contextMenuGuid = PackageGuids.guidSimpleStateMachineEditorPackageCmdSet;
            System.Windows.Point screenPoint;
            Microsoft.VisualStudio.Shell.Interop.POINTS point;
            Microsoft.VisualStudio.Shell.Interop.POINTS[] points;

            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();

            Designer.ContextMenuActivationLocation = MousePosition;
            screenPoint = Designer.IconSurface.PointToScreen(Designer.ContextMenuActivationLocation);

            point = new Microsoft.VisualStudio.Shell.Interop.POINTS();
            point.x = (short)screenPoint.X;
            point.y = (short)screenPoint.Y;

            points = new[] { point };

            // TODO: error handling
            Designer.UiShell.ShowContextMenu(0, ref contextMenuGuid, PackageIds.StateMachineContextMenuId, points, Designer);
        }

        protected override void DragSelectionBox()
        {
            Designer.SelectionBoxIcon.Drag(new Point(MousePosition.X - DragOrigin.X, MousePosition.Y - DragOrigin.Y));
        }

        protected override void EnableSelectionBox()
        {
            Designer.SelectionBoxIcon.StartDrag();
        }

        protected override void RemoveSelectionBox()
        {
            Designer.SelectionBoxIcon.CancelDrag();
        }

        protected override void SaveDragOrigin()
        {
            DragOrigin = MousePosition;
        }

        protected override void SelectDraggingIcon()
        {
            Designer.SelectIcon(DraggingIcon);
        }

        protected override void SelectOccludedIcons()
        {
            Designer.SelectOccludedIcons(DragOrigin, MousePosition);
        }

        protected override void SelectStateMachine()
        {
            Designer.SelectStateMachine();
        }

        protected override void StartDrag()
        {
            foreach (Icons.IDraggableIcon icon in Designer.SelectedIcons.Values)
            {
                icon.StartDrag();
            }
        }

        protected override void StopTrackingMouse()
        {
            Designer.IconSurface.MouseMove -= Designer.MouseMoveHandler;
            Designer.IconSurface.MouseEnter -= Designer.MouseEnterHandler;
            Designer.IconSurface.MouseLeave -= Designer.MouseLeaveHandler;
            Designer.IconSurface.KeyDown -= Designer.KeyDownHandler;
            Mouse.Capture(null);
            Keyboard.ClearFocus();
        }

        protected override void TestIfPositionableIcon()
        {
            PostHighPriorityEvent(DraggingIcon is Icons.PositionableIcon ? EventTypes.Yes : EventTypes.No);
        }

        protected override void TestIsIconSelected()
        {
            PostHighPriorityEvent(Designer.SelectedIcons.ContainsKey(DraggingIcon.ReferencedObject) ? EventTypes.Yes : EventTypes.No);
        }

        protected override void TestShiftKeyState()
        {
            PostHighPriorityEvent((Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift)) ? EventTypes.Yes : EventTypes.No);
        }

        protected override void TrackMouseMovement()
        {
            Mouse.Capture(Designer.IconSurface);
            Keyboard.Focus(Designer.IconSurface);
            Designer.IconSurface.MouseMove += Designer.MouseMoveHandler;
            Designer.IconSurface.MouseEnter += Designer.MouseEnterHandler;
            Designer.IconSurface.MouseLeave += Designer.MouseLeaveHandler;
            Designer.IconSurface.KeyDown += Designer.KeyDownHandler;
        }
    }
}
