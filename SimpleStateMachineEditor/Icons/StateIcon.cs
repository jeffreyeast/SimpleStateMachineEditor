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
    internal class StateIcon : PositionableIcon
    {
        public override int ContextMenuId => PackageIds.StateIconContextMenuId;
        internal readonly static Size IconSize = new Size(62, 62);
        internal readonly static double Radius = 30;

        LayerIcon CandidateLayerIcon;


        internal StateIcon(DesignerControl designer, ViewModel.State state, System.Windows.Point? center, System.Windows.Point? leftTop) :
            base(designer, state, center ?? new System.Windows.Point(Math.Max(0, leftTop.Value.X + IconSize.Width / 2), Math.Max(0, leftTop.Value.Y + IconSize.Width / 2)), IconSize)
        {
        }

        public override void CommitDrag(Point dragTerminationPoint, Point offset)
        {
            //  This is really messy. This ought to be generalized somehow.

            //  If the drag ended over a layer icon, then perform a layer membership operation

            IEnumerable<LayerIcon> occludedLayerIcons = Utility.DrawingAids.FindOccludedIcons<LayerIcon>(Designer.LayerIconsListBox,
                                                                                                         Utility.DrawingAids.NormalizePoint(Designer.LayerIconsListBox, Designer.IconSurface, dragTerminationPoint));
            if (occludedLayerIcons.Count() > 0)
            {
                CancelDrag();
                LayerIcon layerIcon = occludedLayerIcons.First();
                ViewModel.Layer layer = layerIcon.ReferencedObject as ViewModel.Layer;

                if (layer.Members.Contains(ReferencedObject))
                {
                    Designer.RemoveLayerMember(layer, ReferencedObject as ViewModel.State);
                }
                else
                {
                    Designer.AddLayerMember(layer, ReferencedObject as ViewModel.State);
                }
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
            if (CandidateLayerIcon != null)
            {
                CandidateLayerIcon.MembershipAction = LayerIcon.MembershipActions.None;
                CandidateLayerIcon = null;
            }

            // If the mouse is over a layer icon, let the user know

            IEnumerable<LayerIcon> occludedLayerIcons = Utility.DrawingAids.FindOccludedIcons<LayerIcon>(Designer.LayerIconsListBox, 
                                                                                                         Utility.DrawingAids.NormalizePoint(Designer.LayerIconsListBox, Designer.IconSurface, mousePosition));

            if (occludedLayerIcons.Count() == 1)
            {
                LayerIcon layerIcon = occludedLayerIcons.First();
                ViewModel.Layer layer = layerIcon.ReferencedObject as ViewModel.Layer;

                if (layer.Members.Contains(ReferencedObject))
                {
                    if (!layer.IsDefaultLayer)
                    {
                        layerIcon.MembershipAction = LayerIcon.MembershipActions.Remove;
                    }
                }
                else
                {
                    layerIcon.MembershipAction = LayerIcon.MembershipActions.Add;
                }
                CandidateLayerIcon = layerIcon;
            }
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
            base.OnEndDrag();
            if (CandidateLayerIcon != null)
            {
                CandidateLayerIcon.MembershipAction = LayerIcon.MembershipActions.None;
                CandidateLayerIcon = null;
            }
        }

        protected override void OnHover(object sender, EventArgs e)
        {
            base.OnHover(sender, e);

            foreach (ViewModel.Transition transition in (ReferencedObject as ViewModel.State).TransitionsFrom)
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

            foreach (ViewModel.Transition transition in (ReferencedObject as ViewModel.State).TransitionsFrom)
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

        protected override void OnStartDrag()
        {
            base.OnStartDrag();
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
