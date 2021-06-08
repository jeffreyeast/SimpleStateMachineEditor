using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using SimpleStateMachineEditor.ObjectModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace SimpleStateMachineEditor.Icons
{
    internal class TransitionIcon : DraggableIcon
    {
        public override int ContextMenuId => PackageIds.TransitionIconContextMenuId;

        public ObservableCollection<ActionReferenceIcon> ActionIcons { get; private set; }

        public override Point CenterPosition
        {
            get => _centerPosition;
            set
            {
                if (_centerPosition.X != value.X || _centerPosition.Y != value.Y)
                {
                    _centerPosition = value;
                    OnPropertyChanged("CenterPosition");
                }
            }
        }
        Point _centerPosition;

        public bool IsDropCandidate
        {
            get => _isDropCandidate;
            set
            {
                if (_isDropCandidate != value)
                {
                    _isDropCandidate = value;
                    OnPropertyChanged("IsDropCandidate");
                }    
            }
        }
        bool _isDropCandidate;

        public bool IsHighlighted
        {
            get => _isHighlighted;
            set
            {
                if (_isHighlighted != value)
                {
                    _isHighlighted = value;
                    OnPropertyChanged("IsHighlighted");
                }
            }
        }
        bool _isHighlighted;

        internal enum DragTypes
        {
            Nothing,
            Adding,
            ChangingDestination,
            ChangingSource,
        }
        internal DragTypes DragType { get; set; }





        internal TransitionIcon(DesignerControl designer, ObjectModel.ITransition transition, System.Windows.Point? center, System.Windows.Point? leftTop) :
            base(designer, transition, null, null)
        {
            ActionIcons = new ObservableCollection<ActionReferenceIcon>();
            if (transition is ViewModel.Transition t)
            {
                t.ActionReferences.CollectionChanged += TransitionActionsCollectionChangedHandler;
                foreach (ViewModel.ActionReference actionReference in t.ActionReferences)
                {
                    ActionIcons.Add(new ActionReferenceIcon(designer, this, actionReference));
                }
            }
        }

        public override void CancelDrag()
        {
            base.CancelDrag();
            Designer.CancelTransactionDrag(ReferencedObject as ViewModel.Transition);
        }

        protected override FrameworkElement CreateDraggableShape()
        {
            Path newDraggableShape =  new Path()
            {
                DataContext = this,
                Stroke = Brushes.Black,
                StrokeThickness = 1,
            };
            Panel.SetZIndex(newDraggableShape, -1);
            return newDraggableShape;
        }

        protected override Control CreateIcon()
        {
            Control newIconBody = new IconControls.TransitionIconControl(Designer, ReferencedObject as ObjectModel.ITransition)
            {
                DataContext = this,
                Style = Designer.Resources["TransitionIconStyle"] as Style,
            };
            Panel.SetZIndex(newIconBody, -1);
            return newIconBody;
        }

        public override void Drag(Point mousePosition, Point offset)
        {
            Point startPoint;
            Point endPoint;

            switch (DragType)
            {
                case DragTypes.Adding:
                case DragTypes.ChangingDestination:
                    startPoint = Designer.LoadedIcons[(ReferencedObject as ViewModel.Transition).SourceState].CenterPosition;
                    endPoint = new Point(startPoint.X + offset.X, startPoint.Y + offset.Y);
                    break;
                case DragTypes.ChangingSource:
                    endPoint = Designer.LoadedIcons[(ReferencedObject as ViewModel.Transition).DestinationState].CenterPosition;
                    startPoint = new Point(endPoint.X + offset.X, endPoint.Y + offset.Y);
                    break;
                default:
                    throw new NotImplementedException();
            }

            PathGeometry pathGeometry = new PathGeometry();
            (DraggableShape as Path).Data = pathGeometry;
            PathFigure pathFigure = new PathFigure() { StartPoint = startPoint, };
            pathGeometry.Figures.Add(pathFigure);
            pathFigure.Segments.Add(new LineSegment() { Point = endPoint, });
            pathGeometry.Figures.Add(Utility.DrawingAids.DrawArrowHead(startPoint, endPoint));
        }

        public override int Exec(ref Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
        {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();

            if (pguidCmdGroup == PackageGuids.guidSimpleStateMachineEditorPackageCmdSet)
            {
                switch (nCmdID)
                {
                    case PackageIds.SelectNewDestinationCommandId:
                        Designer.ChangeTransitionDestination(ReferencedObject as ViewModel.Transition);
                        return VSConstants.S_OK;
                    case PackageIds.SelectNewSourceCommandId:
                        Designer.ChangeTransitionSource(ReferencedObject as ViewModel.Transition);
                        return VSConstants.S_OK;
                    default:
                        break;
                }
            }
            return base.Exec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);
        }

        protected override void OnCommitDrag(Point dragTerminationPoint)
        {
            if (ReferencedObject is ViewModel.Transition transition)
            {
                Designer.CommitTransactionDrag(transition, dragTerminationPoint);
            }
        }

        /// <summary>
        /// Invoked to handle a "dropped" action string onto an icon
        /// </summary>
        /// <param name="action">The action being dropped</param>
        /// <param name="originState">The source or destination state, whichever is to the left of the other</param>
        /// <param name="clickPosition">The mouse click position, relative to the originState</param>
        /// <returns>The origin-0 relative position of the action within the transtion's action icons</returns>
        internal int ProcessDroppedAction(ViewModel.Action action, ITransitionEndpoint originState, Point clickPosition, bool inhibitDeletion)
        {
            int slot = -1;

            if (ReferencedObject is ViewModel.Transition transition && transition.IsChangeAllowed())
            {
                if (!inhibitDeletion && transition.ActionReferences.Any(ar => ar.Action == action))
                {
                    //  The action is already associated with the transition, so we interpret the request as "remove the action"

                    ViewModel.ActionReference actionReference = transition.ActionReferences.Where(ar => ar.Action == action).Single();
                    transition.ActionReferences.Remove(actionReference);
                }
                else
                {
                    //  This is a new action. We'll try to put it in the list of actions near where they clicked.

                    //  Identify the existing action position closest to the click

                    IconControls.StateIconControl originControl = Designer.LoadedIcons[originState].Body as IconControls.StateIconControl;
                    List<double> segmentMidpointDistancesFromOrigin = new List<double>();

                    foreach (ActionReferenceIcon actionReferenceIcon in ActionIcons)
                    {
                        Point leftTop = Utility.DrawingAids.NormalizePoint(originControl, actionReferenceIcon.ListBoxItem, new Point(0, 0));
                        Point rightBottom = Utility.DrawingAids.NormalizePoint(originControl, actionReferenceIcon.ListBoxItem, new Point(actionReferenceIcon.ListBoxItem.ActualWidth, actionReferenceIcon.ListBoxItem.ActualHeight));
                        double distance = Utility.DrawingAids.Distance((leftTop.X + rightBottom.X) / 2, (leftTop.Y + rightBottom.Y) / 2, 0, 0);
                        segmentMidpointDistancesFromOrigin.Add(distance);
                    }

                    segmentMidpointDistancesFromOrigin.Add(double.MaxValue);


                    //  Now figure the insertion position of the new action into the list of actions for the transition

                    double clickDistance = Utility.DrawingAids.Distance(clickPosition, new Point(0, 0));
                    for (int i = 0; ; i++)
                    {
                        if (clickDistance <= segmentMidpointDistancesFromOrigin[i])
                        {
                            ViewModel.ActionReference newActionReference = new ViewModel.ActionReference(Designer.Model, transition, action);
                            transition.ActionReferences.Insert(i, newActionReference);
                            transition.Controller.LogUndoAction(new UndoRedo.DeleteActionReferenceRecord(transition.Controller, newActionReference));
                            slot = i;
                            break;
                        }
                    }
                }
                transition.EndChange();
                Designer.IconSurface.Focus();
            }

            return slot;
        }

        public override int QueryStatus(ref Guid pguidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText)
        {
            if (pguidCmdGroup == PackageGuids.guidSimpleStateMachineEditorPackageCmdSet)
            {
                for (int i = 0; i < cCmds; i++)
                {
                    switch (prgCmds[i].cmdID)
                    {
                        case PackageIds.DeleteCommandId:
                            prgCmds[i].cmdf = (uint)OLECMDF.OLECMDF_SUPPORTED;
                            if (ReferencedObject is ViewModel.Transition)
                            {
                                prgCmds[i].cmdf |= (uint)(OLECMDF.OLECMDF_ENABLED);
                            }
                            break;

                        case PackageIds.SelectNewDestinationCommandId:
                            prgCmds[i].cmdf = (uint)(OLECMDF.OLECMDF_SUPPORTED);
                            if (ReferencedObject is ViewModel.Transition transition && transition.SourceState != null && transition.DestinationState != null && IsSelectable)
                            {
                                prgCmds[i].cmdf |= (uint)(OLECMDF.OLECMDF_ENABLED);
                            }
                            break;


                        case PackageIds.SelectNewSourceCommandId:
                            prgCmds[i].cmdf = (uint)(OLECMDF.OLECMDF_SUPPORTED);
                            if (ReferencedObject is ViewModel.Transition transition1 && transition1.SourceState != null && transition1.DestinationState != null && IsSelectable)
                            {
                                prgCmds[i].cmdf |= (uint)(OLECMDF.OLECMDF_ENABLED);
                            }
                            break;

                        default:
                            break;
                    }
                }
            }

            return base.QueryStatus(ref pguidCmdGroup, cCmds, prgCmds, pCmdText);
        }

        private void TransitionActionsCollectionChangedHandler(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    for (int i = 0; i < e.NewItems.Count; i++)
                    {
                        ActionIcons.Insert(e.NewStartingIndex + i, new ActionReferenceIcon(Designer, this, e.NewItems[i] as ViewModel.ActionReference));
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (ViewModel.ActionReference actionReference in e.OldItems)
                    {
                        ActionIcons.RemoveAt(e.OldStartingIndex);
                    }
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        public override Size Size { get => throw new InvalidOperationException(); set => throw new InvalidOperationException(); }
    }
}
