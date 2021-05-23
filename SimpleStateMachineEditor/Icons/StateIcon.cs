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

namespace SimpleStateMachineEditor.Icons
{
    internal class StateIcon : PositionableIcon
    {
        public override int ContextMenuId => PackageIds.StateIconContextMenuId;
        internal readonly static Size IconSize = new Size(61, 61);
        internal readonly static double Radius = 30;




        internal StateIcon(DesignerControl designer, ViewModel.State state, System.Windows.Point? center, System.Windows.Point? leftTop) :
            base(designer, state, center ?? new System.Windows.Point(Math.Max(0, leftTop.Value.X + IconSize.Width / 2), Math.Max(0, leftTop.Value.Y + IconSize.Width / 2)), IconSize)
        {
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

        protected override void OnHover(object sender, EventArgs e)
        {
            base.OnHover(sender, e);

            foreach (ViewModel.Transition transition in (ReferencedObject as ViewModel.State).TransitionsFrom)
            {
                (Designer.LoadedIcons[transition] as TransitionIcon).IsHighlighted = true;
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
                (Designer.LoadedIcons[transition] as TransitionIcon).IsHighlighted = false;
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
                            if (Designer.SelectedIcons.Count == 1)
                            {
                                prgCmds[i].cmdf = prgCmds[i].cmdf | (uint)(OLECMDF.OLECMDF_ENABLED);
                            }
                            break;
                        case PackageIds.StartStateCommandId:
                            prgCmds[i].cmdf = (uint)(OLECMDF.OLECMDF_SUPPORTED);
                            if (Designer.SelectedIcons.Count == 1)
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
