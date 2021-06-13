using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using SimpleStateMachineEditor.ObjectModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace SimpleStateMachineEditor.Icons
{
    internal class StateIcon : LayeredPositionableIcon
    {
        public override int ContextMenuId => PackageIds.StateIconContextMenuId;
        internal readonly static Size IconSize = new Size(62, 62);
        public override double Radius => 30;

        GroupIcon DropCandidateIcon
        {
            get => _dropCandidate;
            set
            {
                if (_dropCandidate != value)
                {
                    if (_dropCandidate != null)
                    {
                        _dropCandidate.IsDropCandidate = false;
                    }
                    _dropCandidate = value;
                    if (_dropCandidate != null)
                    {
                        _dropCandidate.IsDropCandidate = true;
                    }
                }
            }
        }
        GroupIcon _dropCandidate;


        internal StateIcon(DesignerControl designer, ViewModel.State state, System.Windows.Point? center, System.Windows.Point? leftTop) :
            base(designer, state, center, leftTop, IconSize)
        {
        }

        public override void CommitDrag(Point dragTerminationPoint, Point offset)
        {
            //  If the mouse is over a group icon, then the user is trying to add the state to the group.

            IEnumerable<GroupIcon> occludedGroupIcons = Utility.DrawingAids.FindOccludedIcons<GroupIcon>(Designer.IconSurface, dragTerminationPoint);
            DropCandidateIcon = occludedGroupIcons.FirstOrDefault();

            if (DropCandidateIcon != null)
            {
                Designer.AddGroupMember(DropCandidateIcon.ReferencedObject as ViewModel.Group, ReferencedObject as ViewModel.State);
                CancelDrag();
            }
            else
            {
                base.CommitDrag(dragTerminationPoint, offset);
            }
        }

        protected override FrameworkElement CreateDraggableShape()
        {
            return new IconControls.StateIconControl()
            {
                DataContext = this,
                Opacity = 0.5,
            };
        }

        protected override Control CreateIcon()
        {
            return new IconControls.StateIconControl()
            {
                DataContext = this,
                Style = Designer.Resources["StateIconStyle"] as Style,
            };
        }

        public override void Drag(Point mousePosition, Point offset)
        {
            base.Drag(mousePosition, offset);

            IEnumerable<GroupIcon> occludedGroupIcons = Utility.DrawingAids.FindOccludedIcons<GroupIcon>(Designer.IconSurface, mousePosition);
            DropCandidateIcon = occludedGroupIcons.FirstOrDefault();
        }

        public override int Exec(ref Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
        {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();

            if (pguidCmdGroup == PackageGuids.guidSimpleStateMachineEditorPackageCmdSet)
            {
                switch (nCmdID)
                {
                    case PackageIds.AddTransitionCommandId:
                        Designer.AddTransition(ReferencedObject as ViewModel.State);
                        return VSConstants.S_OK;
                    case PackageIds.StartStateCommandId:
                        if (Designer.Model.StateMachine.StartState == ReferencedObject)
                        {
                            Designer.SetStartState(null);
                        }
                        else
                        {
                            Designer.SetStartState(ReferencedObject as ViewModel.State);
                        }
                        return VSConstants.S_OK;
                    default:
                        break;
                }
            }
            return base.Exec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);
        }

        protected override void OnEndDrag()
        {
            if (DropCandidateIcon != null)
            {
                DropCandidateIcon = null;
            }
            base.OnEndDrag();
        }

        protected override void OnHover(object sender, EventArgs e)
        {
            base.OnHover(sender, e);

            foreach (ObjectModel.ITransition transition in (ReferencedObject as ViewModel.State).TransitionsFrom)
            {
                if (Designer.LoadedIcons.ContainsKey(transition))
                {
                    (Designer.LoadedIcons[transition] as TransitionIcon).IsHighlighted = true;
                }
            }

            foreach (ViewModel.EventType eventType in Designer.Model.StateMachine.EventTypes)
            {
                (Designer.LoadedIcons[eventType] as EventTypeIcon).TriggerUsageState = ((ReferencedObject as ViewModel.State).HasTransitionMatchingTrigger(eventType) ? EventTypeIcon.DropStates.NotAllowed : EventTypeIcon.DropStates.Allowed);
            }
        }

        protected override void OnHoverEnd()
        {
            base.OnHoverEnd();

            foreach (ObjectModel.ITransition transition in (ReferencedObject as ViewModel.State).TransitionsFrom)
            {
                if (Designer.LoadedIcons.ContainsKey(transition))
                {
                    (Designer.LoadedIcons[transition] as TransitionIcon).IsHighlighted = false;
                }
            }

            foreach (ViewModel.EventType eventType in Designer.Model.StateMachine.EventTypes)
            {
                (Designer.LoadedIcons[eventType] as EventTypeIcon).TriggerUsageState = EventTypeIcon.DropStates.Disabled;
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
                        case PackageIds.AddTransitionCommandId:
                            prgCmds[i].cmdf = (uint)(OLECMDF.OLECMDF_SUPPORTED);
                            if (IsSelectable)
                            {
                                prgCmds[i].cmdf = prgCmds[i].cmdf | (uint)(OLECMDF.OLECMDF_ENABLED);
                            }
                            break;
                        case PackageIds.DeleteCommandId:
                            prgCmds[i].cmdf = (uint)(OLECMDF.OLECMDF_SUPPORTED);
                            if (IsSelectable && (ReferencedObject as ViewModel.State).CurrentLayerPosition.GroupStatus != LayerPosition.GroupStatuses.Implicit)
                            {
                                prgCmds[i].cmdf = prgCmds[i].cmdf | (uint)(OLECMDF.OLECMDF_ENABLED);
                            }
                            break;
                        case PackageIds.StartStateCommandId:
                            prgCmds[i].cmdf = (uint)(OLECMDF.OLECMDF_SUPPORTED);
                            if (IsSelectable)
                            {
                                prgCmds[i].cmdf = prgCmds[i].cmdf | (uint)(OLECMDF.OLECMDF_ENABLED);
                            }
                            if (Designer.Model.StateMachine.StartState == ReferencedObject)
                            {
                                prgCmds[i].cmdf |= (uint)OLECMDF.OLECMDF_LATCHED;
                            }
                            break;
                        default:
                            break;
                    }
                }
            }

            return base.QueryStatus(ref pguidCmdGroup, cCmds, prgCmds, pCmdText);
        }
    }
}
