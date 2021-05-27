using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace SimpleStateMachineEditor.Icons
{
    /// <summary>
    /// Represents a group of related icons that are displayed and editted together. Note that icons can be members of more than one layer.
    /// </summary>
    internal class LayerIcon : SelectableIcon
    {
        public override int ContextMenuId => PackageIds.LayerIconContextMenuId;
        internal readonly static Size IconSize = new Size(32, 32);

        public enum MembershipActions
        {
            None,
            Add,
            Remove,
        }
        public MembershipActions MembershipAction
        {
            get => _membershipAction;
            set
            {
                if (_membershipAction != value)
                {
                    _membershipAction = value;
                    OnPropertyChanged("MembershipAction");
                }
            }
        }
        MembershipActions _membershipAction;



        internal LayerIcon(DesignerControl designer, ViewModel.Layer layer, System.Windows.Point? center, System.Windows.Point? leftTop) :
            base(designer, layer, null, null)
        {
        }

        protected override FrameworkElement CreateDraggableShape()
        {
            return null;
        }

        protected override Control CreateIcon()
        {
            return null;
        }

        protected override void OnHover(object sender, EventArgs e)
        {
            base.OnHover(sender, e);

            IsLayerHighlighted = true;
            foreach (ObjectModel.TrackableObject trackableObject in (ReferencedObject as ViewModel.Layer).Members)
            {
                if (Designer.LoadedIcons.ContainsKey(trackableObject))
                {
                    ISelectableIcon selectableIcon = Designer.LoadedIcons[trackableObject];
                    selectableIcon.IsLayerHighlighted = true;
                }
            }
        }

        protected override void OnHoverEnd()
        {
            base.OnHoverEnd();

            IsLayerHighlighted = false;
            foreach (ObjectModel.TrackableObject trackableObject in (ReferencedObject as ViewModel.Layer).Members)
            {
                if (Designer.LoadedIcons.ContainsKey(trackableObject))
                {
                    ISelectableIcon selectableIcon = Designer.LoadedIcons[trackableObject];
                    selectableIcon.IsLayerHighlighted = false;
                }
            }
        }

        public override int QueryStatus(ref Guid pguidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText)
        {
            if (pguidCmdGroup == PackageGuids.guidSimpleStateMachineEditorPackageCmdSet && ReferencedObject is ViewModel.Layer layer)
            {
                for (int i = 0; i < cCmds; i++)
                {
                    switch (prgCmds[i].cmdID)
                    {
                        case PackageIds.DeleteCommandId:
                            prgCmds[i].cmdf = (uint)OLECMDF.OLECMDF_SUPPORTED | (layer.IsDefaultLayer ? 0 : (uint)OLECMDF.OLECMDF_ENABLED);
                            break;

                        default:
                            break;
                    }
                }
            }

            return base.QueryStatus(ref pguidCmdGroup, cCmds, prgCmds, pCmdText);
        }

        internal void Select()
        {
            Designer.SelectLayer(ReferencedObject as ViewModel.Layer);
            Designer.SelectSingle(this);
        }

        public override Size Size { get => IconSize; set => throw new InvalidOperationException(); }
    }
}
