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
    /// Represents a group of related icons. Note that icons can be members of more than one region.
    /// </summary>
    internal class RegionIcon : PositionableIcon
    {
        public override int ContextMenuId => PackageIds.RegionIconContextMenuId;
        internal readonly static Size IconSize = new Size(62, 62);


        internal RegionIcon(DesignerControl designer, ViewModel.Region region, System.Windows.Point? center, System.Windows.Point? leftTop) :
            base(designer, region, center ?? new System.Windows.Point(Math.Max(0, leftTop.Value.X + IconSize.Width / 2), Math.Max(0, leftTop.Value.Y + IconSize.Width / 2)), IconSize)
        {
        }

        protected override FrameworkElement CreateDraggableShape()
        {
            return new IconControls.RegionIconControl()
            {
                DataContext = this,
                Opacity = 0.5,
            };
        }

        protected override Control CreateIcon()
        {
            return new IconControls.RegionIconControl()
            {
                DataContext = this,
                Style = Designer.Resources["RegionIconStyle"] as Style,
            };
        }

        public override int Exec(ref Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
        {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();

            if (pguidCmdGroup == PackageGuids.guidSimpleStateMachineEditorPackageCmdSet && ReferencedObject is ViewModel.Region region)
            {
                switch (nCmdID)
                {
                    case PackageIds.HideIconsButCommandId:
                        if (region.IsChangeAllowed)
                        {
                            using (new UndoRedo.AtomicBlock(Designer.Model, "Hide icons"))
                            {
                                foreach (ViewModel.Region targetRegion in Designer.Model.StateMachine.Regions)
                                {
                                    if (targetRegion != region)
                                    {
                                        targetRegion.IsHidden = true;
                                    }
                                }
                                region.IsHidden = false;
                            }
                            Designer.ReComputeHiddenIcons();
                            region.EndChange();
                        }
                        break;
                    case PackageIds.HideIconsCommandId:
                        if (region.IsChangeAllowed)
                        {
                            region.IsHidden = !region.IsHidden;
                            Designer.ReComputeHiddenIcons();
                            region.EndChange();
                        }
                        break;
                    case PackageIds.SetIconDisplayColorsComboId:
                        if (pvaOut != default(IntPtr))
                        {
                            // How do we know how much space is allocated to pvaOut?

                            Marshal.GetNativeVariantForObject(region.DisplayColors.ToString(), pvaOut);

                        }
                        if (pvaIn != default(IntPtr))
                        {
                            int i = (int)Marshal.GetObjectForNativeVariant(pvaIn);
                            if (i >= Utility.BrushesToList.DisplayColors.GetLowerBound(0) && i <= Utility.BrushesToList.DisplayColors.GetUpperBound(0) && region.IsChangeAllowed)
                            {
                                region.DisplayColors = Utility.BrushesToList.DisplayColors[i];
                                region.EndChange();
                            }
                        }
                        return VSConstants.S_OK;
                    case PackageIds.SetIconDisplayColorsComboListId:
                        // How do we know how much space is allocated to pvaOut?

                        Marshal.GetNativeVariantForObject(Utility.BrushesToList.DisplayColors.Select(c => c.ToString()).ToArray(), pvaOut);
                        return VSConstants.S_OK;
                    case PackageIds.ShowAllIconsCommandId:
                        Designer.ShowAllIcons();
                        return VSConstants.S_OK;
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

            foreach (ObjectModel.TrackableObject trackableObject in (ReferencedObject as ViewModel.Region).Members)
            {
                if (Designer.LoadedIcons.ContainsKey(trackableObject))
                {
                    ISelectableIcon selectableIcon = Designer.LoadedIcons[trackableObject];
                    selectableIcon.HighlightedRegion = ReferencedObject as ViewModel.Region;
                }
            }
        }

        protected override void OnHoverEnd()
        {
            base.OnHoverEnd();

            foreach (ObjectModel.TrackableObject trackableObject in (ReferencedObject as ViewModel.Region).Members)
            {
                if (Designer.LoadedIcons.ContainsKey(trackableObject))
                {
                    ISelectableIcon selectableIcon = Designer.LoadedIcons[trackableObject];
                    selectableIcon.HighlightedRegion = null;
                }
            }
        }

        public override int QueryStatus(ref Guid pguidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText)
        {
            if (pguidCmdGroup == PackageGuids.guidSimpleStateMachineEditorPackageCmdSet && ReferencedObject is ViewModel.Region region)
            {
                for (int i = 0; i < cCmds; i++)
                {
                    switch (prgCmds[i].cmdID)
                    {
                        case PackageIds.HideIconsButCommandId:
                            prgCmds[i].cmdf = (uint)OLECMDF.OLECMDF_SUPPORTED | (Designer.Model.StateMachine.Regions.Count <= 1 ? 0 : (uint)OLECMDF.OLECMDF_ENABLED);
                            break;

                        case PackageIds.HideIconsCommandId:
                            prgCmds[i].cmdf = (uint)OLECMDF.OLECMDF_SUPPORTED | (region.Members.Count == 0 ? 0 : (uint)OLECMDF.OLECMDF_ENABLED);
                            if (region.IsHidden)
                            {
                                prgCmds[i].cmdf |= (uint)OLECMDF.OLECMDF_LATCHED;
                            }
                            break;

                        case PackageIds.SetIconDisplayColorsComboId:
                        case PackageIds.SetIconDisplayColorsComboListId:
                        case PackageIds.ShowAllIconsCommandId:
                            prgCmds[i].cmdf = (uint)(OLECMDF.OLECMDF_SUPPORTED | OLECMDF.OLECMDF_ENABLED);
                            break;

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
