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
    /// <summary>
    /// Represents an action in a transition icon's action list
    /// </summary>
    internal class ActionReferenceIcon : DraggableIcon
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

        TransitionIcon HighlightedDropTarget
        {
            get => _highlightedDropTarget;
            set
            {
                if (_highlightedDropTarget != value)
                {
                    if (_highlightedDropTarget != null)
                    {
                        _highlightedDropTarget.IsDropCandidate = false;
                    }
                    _highlightedDropTarget = value;
                    if (_highlightedDropTarget != null)
                    {
                        _highlightedDropTarget.IsDropCandidate = true;
                    }
                }
            }
        }
        TransitionIcon _highlightedDropTarget;



        internal ActionReferenceIcon(DesignerControl designer, TransitionIcon transitionIcon, ViewModel.ActionReference actionReference) : base(designer, actionReference, null, null)
        {
            TransitionIcon = transitionIcon;
            Transition = transitionIcon.ReferencedObject as ViewModel.Transition;
        }

        public override void CancelDrag()
        {
            base.CancelDrag();
            HighlightedDropTarget = null;
        }

        protected override FrameworkElement CreateDraggableShape()
        {
            return new IconControls.ActionReferenceIconControl(true)
            {
                DataContext = this,
                Opacity = 0.5,
            };
        }

        public override void CommitDrag(Point dragTerminationPoint, Point offset)
        {
            Designer.IconSurface.Children.Remove(DraggableShape);

            HighlightedDropTarget = null;

            //  Identify the transition closest to the drop point

            TransitionIcon closestTransitionIcon = FindClosestTransitionIcon(dragTerminationPoint);

            if (closestTransitionIcon != null)
            {
                int oldSlot = TransitionIcon.ActionIcons.IndexOf(this);
                ViewModel.Action action = (ReferencedObject as ViewModel.ActionReference).Action;

                if (closestTransitionIcon == TransitionIcon)
                {
                    //  The user is attempting to re-order the actions for the transition

                    ObjectModel.ITransitionEndpoint originState = Designer.LoadedIcons[Transition.SourceState].CenterPosition.X < Designer.LoadedIcons[Transition.DestinationState].CenterPosition.X ? Transition.SourceState : Transition.DestinationState;

                    using (Transition.Controller.CreateAtomicGuiChangeBlock("Move action"))
                    {
                        int newSlot = TransitionIcon.ProcessDroppedAction(action, originState, Utility.DrawingAids.NormalizePoint(Designer.LoadedIcons[originState].Body, Designer.IconSurface, dragTerminationPoint), true);
                        if (newSlot <= oldSlot)
                        {
                            oldSlot++;
                        }
                        Designer.Model.LogUndoAction(new UndoRedo.AddActionReferenceRecord(Designer.Model, Transition.ActionReferences[oldSlot], oldSlot));
                        Transition.ActionReferences.RemoveAt(oldSlot);
                    }
                }
                else
                {
                    ViewModel.Transition closestTransition = closestTransitionIcon.ReferencedObject as ViewModel.Transition;

                    if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                    {
                        //  The user is copying this action to another transition

                        ObjectModel.ITransitionEndpoint originState = Designer.LoadedIcons[closestTransition.SourceState].CenterPosition.X < Designer.LoadedIcons[closestTransition.DestinationState].CenterPosition.X ? closestTransition.SourceState : closestTransition.DestinationState;

                        using (closestTransition.Controller.CreateAtomicGuiChangeBlock("Copy action"))
                        {
                            closestTransitionIcon.ProcessDroppedAction(action, originState, Utility.DrawingAids.NormalizePoint(Designer.LoadedIcons[originState].Body, Designer.IconSurface, dragTerminationPoint), true);
                        }
                    }
                    else
                    {
                        // The user is moving this action from this transition to another

                        ObjectModel.ITransitionEndpoint originState = Designer.LoadedIcons[closestTransition.SourceState].CenterPosition.X < Designer.LoadedIcons[closestTransition.DestinationState].CenterPosition.X ? closestTransition.SourceState : closestTransition.DestinationState;

                        using (closestTransition.Controller.CreateAtomicGuiChangeBlock("Move action"))
                        {
                            closestTransitionIcon.ProcessDroppedAction(action, originState, Utility.DrawingAids.NormalizePoint(Designer.LoadedIcons[originState].Body, Designer.IconSurface, dragTerminationPoint), true);
                            Designer.Model.LogUndoAction(new UndoRedo.AddActionReferenceRecord(Designer.Model, Transition.ActionReferences[oldSlot], oldSlot));
                            Transition.ActionReferences.RemoveAt(oldSlot);
                        }
                    }
                }
                Designer.ClearSelectedItems();
            }
        }

        protected override Control CreateIcon()
        {
            return null;
        }

        public override void Drag(Point mousePosition, Point offset)
        {
            Point center = CenterPosition;
            Size iconSize = Size;
            DraggableShape.Margin = new Thickness(Math.Max(0, center.X - iconSize.Width / 2 + offset.X), Math.Max(0, center.Y - iconSize.Height / 2 + offset.Y), 0, 0);

            //  Identify the transition closest to the drop point and highlight it

            HighlightedDropTarget = FindClosestTransitionIcon(mousePosition);
        }

        private TransitionIcon FindClosestTransitionIcon(Point mousePosition)
        {
            double minDistance = double.MaxValue;
            TransitionIcon closestTransitionIcon = null;

            foreach (TransitionIcon transitionIcon in Designer.LoadedIcons.Values.OfType<TransitionIcon>())
            {
                if ((transitionIcon.ReferencedObject as ViewModel.Transition).TransitionType == ViewModel.Transition.TransitionTypes.Normal)
                {
                    double distance = Utility.DrawingAids.TangentalDistance((transitionIcon.Body as IconControls.TransitionIconControl).ConnectorPath.Data, mousePosition);
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        closestTransitionIcon = transitionIcon;
                    }
                }
            }
            return closestTransitionIcon;
        }

        public override Size Size => Body.RenderSize;

        public override void StartDrag()
        {
            Designer.IconSurface.Children.Add(DraggableShape);
            Drag(new Point(0, 0), new Point(0, 0));
        }

    }
}
