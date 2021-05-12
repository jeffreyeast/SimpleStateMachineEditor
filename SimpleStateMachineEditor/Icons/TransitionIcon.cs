using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using SimpleStateMachineEditor.ObjectModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace SimpleStateMachineEditor.Icons
{
    internal class TransitionIcon : DraggableIcon
    {
        public override int ContextMenuId => PackageIds.TransitionIconContextMenuId;


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


        internal TransitionIcon(DesignerControl designer, ViewModel.Transition transition, System.Windows.Point? center, System.Windows.Point? leftTop) :
            base(designer, transition, null, null)
        {
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
            Control newIconBody = new IconControls.TransitionIconControl(Designer, ReferencedObject as ViewModel.Transition)
            {
                DataContext = this,
                Style = Designer.Resources["TransitionIconStyle"] as Style,
            };
            Panel.SetZIndex(newIconBody, -1);
            return newIconBody;
        }

        public override void Drag(Point offset)
        {
            Point startPoint;
            Point endPoint;

            if ((ReferencedObject as ViewModel.Transition).SourceState == null)
            {
                endPoint = Designer.LoadedIcons[(ReferencedObject as ViewModel.Transition).DestinationState].CenterPosition;
                startPoint = new Point(endPoint.X + offset.X, endPoint.Y + offset.Y);
            }
            else
            {
                startPoint = Designer.LoadedIcons[(ReferencedObject as ViewModel.Transition).SourceState].CenterPosition;
                endPoint = new Point(startPoint.X + offset.X, startPoint.Y + offset.Y);
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

        public override int QueryStatus(ref Guid pguidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText)
        {
            if (pguidCmdGroup == PackageGuids.guidSimpleStateMachineEditorPackageCmdSet)
            {
                for (int i = 0; i < cCmds; i++)
                {
                    switch (prgCmds[i].cmdID)
                    {
                        case PackageIds.DeleteCommandId:
                            prgCmds[i].cmdf = (uint)(OLECMDF.OLECMDF_SUPPORTED | OLECMDF.OLECMDF_ENABLED);
                            break;

                        case PackageIds.SelectNewDestinationCommandId:
                            prgCmds[i].cmdf = (uint)(OLECMDF.OLECMDF_SUPPORTED);
                            if (ReferencedObject is ViewModel.Transition transition && transition.SourceState != null && transition.DestinationState != null)
                            {
                                prgCmds[i].cmdf |= (uint)(OLECMDF.OLECMDF_ENABLED);
                            }
                            break;


                        case PackageIds.SelectNewSourceCommandId:
                            prgCmds[i].cmdf = (uint)(OLECMDF.OLECMDF_SUPPORTED);
                            if (ReferencedObject is ViewModel.Transition transition1 && transition1.SourceState != null && transition1.DestinationState != null)
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

        public override Size Size { get => throw new InvalidOperationException(); set => throw new InvalidOperationException(); }
    }
}
