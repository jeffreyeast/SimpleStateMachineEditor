using SimpleStateMachineEditor.ObjectModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace SimpleStateMachineEditor.Icons
{
    /// <summary>
    /// Represents an action in a transition icon's action list
    /// </summary>
    internal class ActionIcon : DraggableIcon
    {
        public override int ContextMenuId => PackageIds.ActionIconContextMenuId;

        internal ListBoxItem ListBoxItem 
        {
            get => _listBoxItem;
            set
            {
                if (_listBoxItem != value)
                {
                    _listBoxItem = value;
                    OnPropertyChanged("ListBoxItem");
                }
            }
        }
        ListBoxItem _listBoxItem;

        public TransitionIcon TransitionIcon
        {
            get => _transitionIcon;
            private set
            {
                if (_transitionIcon != value)
                {
                    _transitionIcon = value;
                    OnPropertyChanged("TransitionIcon");
                }
            }
        }
        TransitionIcon _transitionIcon;

        public ViewModel.Transition Transition
        {
            get => _transition;
            private set
            {
                if (_transition != value)
                {
                    _transition = value;
                    OnPropertyChanged("Transition");
                }
            }
        }
        ViewModel.Transition _transition;

        public bool ActionIsHighlighted
        {
            get => _actionIsHighlighted;
            set
            {
                if (_actionIsHighlighted != value)
                {
                    _actionIsHighlighted = value;
                    OnPropertyChanged("ActionIsHighlighted");
                }
            }
        }
        bool _actionIsHighlighted;

        public override Point CenterPosition
        {
            get => Utility.DrawingAids.NormalizePoint(Designer.IconSurface, Body, new Point(Body.ActualWidth / 2, Body.ActualHeight / 2));
            set => throw new InvalidOperationException();
        }




        internal ActionIcon(DesignerControl designer, TransitionIcon transitionIcon, ViewModel.Action action) : base(designer, action, null, null)
        {
            TransitionIcon = transitionIcon;
            Transition = transitionIcon.ReferencedObject as ViewModel.Transition;
        }

        protected override FrameworkElement CreateDraggableShape()
        {
            return new IconControls.ActionIconControl(true)
            {
                DataContext = this,
                Opacity = 0.5,
            };
        }

        public override void CommitDrag(Point dragTerminationPoint, Point offset)
        {
            Designer.IconSurface.Children.Remove(DraggableShape);

            //  The user is attempting to re-order the actions for the transition

            int oldSlot = TransitionIcon.ActionIcons.IndexOf(this);
            ViewModel.State originState = Designer.LoadedIcons[Transition.SourceState].CenterPosition.X < Designer.LoadedIcons[Transition.DestinationState].CenterPosition.X ? Transition.SourceState : Transition.DestinationState;

            if (Transition.IsChangeAllowed)
            {
                using (new UndoRedo.AtomicBlock(Transition.Controller, "Move action"))
                {
                    int newSlot = TransitionIcon.ProcessDroppedAction(ReferencedObject as ViewModel.Action, originState, Utility.DrawingAids.NormalizePoint(Designer.LoadedIcons[originState].Body, Designer.IconSurface, dragTerminationPoint), true);
                    Transition.Actions.RemoveAt(oldSlot + (newSlot > oldSlot ? 0 : 1));
                }
                Transition.EndChange();
                Designer.ClearSelectedItems();
            }
        }

        protected override Control CreateIcon()
        {
            return null;
        }

        public override void Drag(Point offset)
        {
            Point center = CenterPosition;
            Size iconSize = Size;
            DraggableShape.Margin = new Thickness(Math.Max(0, center.X - iconSize.Width / 2 + offset.X), Math.Max(0, center.Y - iconSize.Height / 2 + offset.Y), 0, 0);
        }

        public override Size Size => Body.RenderSize;

        public override void StartDrag()
        {
            Designer.IconSurface.Children.Add(DraggableShape);
            Drag(new Point(0, 0));
        }

    }
}
