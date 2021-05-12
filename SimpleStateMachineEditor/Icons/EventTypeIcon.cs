﻿using Microsoft.VisualStudio;
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
    internal class EventTypeIcon : PositionableIcon
    {
        public override int ContextMenuId => PackageIds.EventTypeIconContextMenuId;
        internal readonly static Size IconSize = new Size(61, 61);


        public enum DropStates
        {
            Disabled,
            Allowed,
            NotAllowed,
        }

        public DropStates DropState
        {
            get => _dropState;
            set
            {
                if (_dropState != value)
                {
                    _dropState = value;
                    OnPropertyChanged("DropState");
                }
            }
        }
        DropStates _dropState;

        TransitionIcon DropCandididate
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
        TransitionIcon _dropCandidate;

        public DropStates TriggerUsageState
        {
            get => _triggerUsageStates;
            set
            {
                if (_triggerUsageStates != value)
                {
                    _triggerUsageStates = value;
                    OnPropertyChanged("TriggerUsageState");
                }
            }
        }
        DropStates _triggerUsageStates;




        internal EventTypeIcon(DesignerControl designer, ViewModel.EventType eventType, System.Windows.Point? center, System.Windows.Point? leftTop) :
            base(designer, eventType, center ?? new System.Windows.Point(Math.Max(0, leftTop.Value.X + IconSize.Width / 2), Math.Max(0, leftTop.Value.Y + IconSize.Width / 2)), IconSize)
        {
            DropState = DropStates.Disabled;
        }

        protected override FrameworkElement CreateDraggableShape()
        {
            return new IconControls.EventTypeIconControl(true)
            {
                DataContext = this,
                Opacity = 0.5,
            };
        }

        public override void CancelDrag()
        {
            DropCandididate = null;
            base.CancelDrag();
        }

        protected override Control CreateIcon()
        {
            return new IconControls.EventTypeIconControl(false)
            {
                DataContext = this,
                Style = Designer.Resources["EventTypeIconStyle"] as Style,
            };
        }

        public override void CommitDrag(Point dragTerminationPoint, Point offset)
        {
            DropCandididate = null;

            //  If the event type icon is on top of a transition icon, then the user is asking us to associate
            //  the event as the trigger for the transition. Otherwise, they're just moving the event type icon.

            TransitionIcon transitionIcon = Designer.FindOccludedTransitionIcon(this, dragTerminationPoint);
            if (transitionIcon == null || !transitionIcon.ReferencedObject.IsChangeAllowed)
            {
                base.CommitDrag(dragTerminationPoint, offset);
            }
            else
            {
                ViewModel.Transition transition = transitionIcon.ReferencedObject as ViewModel.Transition;

                // Check if there's another transition for this event type. Refuse the drop if so.

                if (!transition.SourceState.HasTransitionMatchingTrigger(ReferencedObject as ViewModel.EventType))
                {
                    transition.TriggerEvent = ReferencedObject as ViewModel.EventType;
                }

                transition.EndChange();
                CancelDrag();
            }
        }

        public override void Drag(Point offset)
        {
            base.Drag(offset);

            TransitionIcon transitionIcon = Designer.FindOccludedTransitionIcon(this, new Point((ReferencedObject as ViewModel.EventType).LeftTopPosition.X + offset.X, (ReferencedObject as ViewModel.EventType).LeftTopPosition.Y + offset.Y));

            if (transitionIcon == null)
            {
                DropState = DropStates.Disabled;
            }
            else
            {
                ViewModel.Transition transition = transitionIcon.ReferencedObject as ViewModel.Transition;
                DropState = transition.SourceState.HasTransitionMatchingTrigger(ReferencedObject as ViewModel.EventType) ? DropStates.NotAllowed : DropStates.Allowed;
            }
            DropCandididate = DropState == DropStates.Allowed ? transitionIcon : null;
        }

        public override int Exec(ref Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
        {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();

            if (pguidCmdGroup == PackageGuids.guidSimpleStateMachineEditorPackageCmdSet)
            {
                switch (nCmdID)
                {
                    case PackageIds.SortCommandId:
                        Designer.SortSelectedIcons(this);
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

            foreach (ViewModel.Transition transition in Designer.Model.StateMachine.Transitions)
            {
                if (transition.TriggerEvent == ReferencedObject)
                {
                    (Designer.LoadedIcons[transition] as TransitionIcon).IsHighlighted = true;
                }
            }
        }

        protected override void OnHoverEnd()
        {
            base.OnHoverEnd();

            foreach (ViewModel.Transition transition in Designer.Model.StateMachine.Transitions)
            {
                if (transition.TriggerEvent == ReferencedObject)
                {
                    (Designer.LoadedIcons[transition] as TransitionIcon).IsHighlighted = false;
                }
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
                        case PackageIds.SortCommandId:
                            prgCmds[i].cmdf = (uint)OLECMDF.OLECMDF_SUPPORTED;
                            if (Designer.SelectedIcons.Count > 1 || (Designer.SelectedIcons.Count == 1 && !Designer.SelectedIcons.ContainsKey(ReferencedObject)))
                            {
                                prgCmds[i].cmdf = prgCmds[i].cmdf | (uint)OLECMDF.OLECMDF_ENABLED;
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
