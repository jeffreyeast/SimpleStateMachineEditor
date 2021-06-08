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
    internal class GroupIcon : LayeredPositionableIcon
    {
        public override int ContextMenuId => PackageIds.GroupIconContextMenuId;
        internal readonly static Size IconSize = new Size(122, 122);
        internal readonly static double Radius = 60;



        internal GroupIcon(DesignerControl designer, ViewModel.Group group, System.Windows.Point? center, System.Windows.Point? leftTop) :
            base(designer, group, center, leftTop, IconSize)
        {
        }

        protected override FrameworkElement CreateDraggableShape()
        {
            return new IconControls.GroupIconControl()
            {
                DataContext = this,
                Opacity = 0.5,
            };
        }

        protected override Control CreateIcon()
        {
            return new IconControls.GroupIconControl()
            {
                DataContext = this,
                Style = Designer.Resources["GroupIconStyle"] as Style,
            };
        }

        public override int Exec(ref Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
        {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();

            if (pguidCmdGroup == PackageGuids.guidSimpleStateMachineEditorPackageCmdSet)
            {
                switch (nCmdID)
                {
                    default:
                        break;
                }
            }
            return base.Exec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);
        }

        protected override void OnHover(object sender, EventArgs e)
        {
            base.OnHover(sender, e);

            foreach (ObjectModel.ITransition transition in (ReferencedObject as ViewModel.Group).TransitionsFrom)
            {
                if (Designer.LoadedIcons.ContainsKey(transition))
                {
                    (Designer.LoadedIcons[transition] as TransitionIcon).IsHighlighted = true;
                }
            }

            foreach (ViewModel.EventType eventType in Designer.Model.StateMachine.EventTypes)
            {
                (Designer.LoadedIcons[eventType] as EventTypeIcon).TriggerUsageState = ((ReferencedObject as ViewModel.Group).HasTransitionMatchingTrigger(eventType) ? EventTypeIcon.DropStates.NotAllowed : EventTypeIcon.DropStates.Allowed);
            }
        }

        protected override void OnHoverEnd()
        {
            base.OnHoverEnd();

            foreach (ObjectModel.ITransition transition in (ReferencedObject as ViewModel.Group).TransitionsFrom)
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
                        default:
                            break;
                    }
                }
            }

            return base.QueryStatus(ref pguidCmdGroup, cCmds, prgCmds, pCmdText);
        }
    }
}
