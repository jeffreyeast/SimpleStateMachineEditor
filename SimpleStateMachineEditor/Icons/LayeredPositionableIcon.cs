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
    internal abstract class LayeredPositionableIcon : PositionableIcon
    {
        LayerIcon CandidateLayerIcon;


        internal LayeredPositionableIcon(DesignerControl designer, ObjectModel.LayeredPositionableObject layeredPositionalObject, System.Windows.Point? center, System.Windows.Point? leftTop, Size iconSize) :
            base(designer, layeredPositionalObject, center ?? new System.Windows.Point(Math.Max(0, leftTop.Value.X + iconSize.Width / 2), Math.Max(0, leftTop.Value.Y + iconSize.Width / 2)), iconSize)
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
                    Designer.RemoveLayerMember(layer, ReferencedObject as ObjectModel.LayeredPositionableObject);
                }
                else
                {
                    Designer.AddLayerMember(layer, ReferencedObject as ObjectModel.LayeredPositionableObject);
                }
            }
            else
            {
                base.CommitDrag(dragTerminationPoint, offset);
            }
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

            foreach (ObjectModel.ITransition transition in (ReferencedObject as ObjectModel.ITransitionEndpoint).TransitionsFrom)
            {
                if (Designer.LoadedIcons.ContainsKey(transition))
                {
                    (Designer.LoadedIcons[transition] as TransitionIcon).IsHighlighted = true;
                }
            }

            foreach (ViewModel.EventType eventType in Designer.Model.StateMachine.EventTypes)
            {
                (Designer.LoadedIcons[eventType] as EventTypeIcon).TriggerUsageState = ((ReferencedObject as ObjectModel.ITransitionEndpoint).HasTransitionMatchingTrigger(eventType) ? EventTypeIcon.DropStates.NotAllowed : EventTypeIcon.DropStates.Allowed);
            }
        }

        protected override void OnHoverEnd()
        {
            base.OnHoverEnd();

            foreach (ObjectModel.ITransition transition in (ReferencedObject as ObjectModel.ITransitionEndpoint).TransitionsFrom)
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
    }
}
