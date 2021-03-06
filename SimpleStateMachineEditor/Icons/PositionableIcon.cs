using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using SimpleStateMachineEditor.ObjectModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SimpleStateMachineEditor.Icons
{
    /// <summary>
    /// Icons that implement the alignment commands
    /// </summary>
    internal abstract class PositionableIcon : DraggableIcon
    {
        internal PositionableIcon(DesignerControl designer, TrackableObject o, Point? center, Size? size) : base(designer, o, center, size)
        {
        }


        private void AlignBottom()
        {
            using (new ViewModel.ViewModelController.GuiChangeBlock(Designer.Model))
            {
                //  Find the lowest bottom

                double lowestBottom = 0;

                foreach (IconBase icon in Designer.SelectedIcons.Values)
                {
                    if (icon is IDraggableIcon draggableIcon)
                    {
                        lowestBottom = Math.Max(lowestBottom, icon.Bottom);
                    }
                }

                //  Alighment everyone to the common bottom

                foreach (IconBase icon in Designer.SelectedIcons.Values)
                {
                    if (icon.ReferencedObject is ObjectModel.IPositionableObject positionableObject)
                    {
                        positionableObject.LeftTopPosition = new Point(positionableObject.LeftTopPosition.X, lowestBottom - icon.Size.Height);
                    }
                }
            }
        }

        private void AlignHorizontalCenter()
        {
            using (new ViewModel.ViewModelController.GuiChangeBlock(Designer.Model))
            {
                //  Use the icon whose context menu fired the command as the reference point

                IconBase referenceIcon = Designer.FindNearestSelectedIcon(ContextMenuActivationLocation) as IconBase;

                if (referenceIcon != null && Designer.SelectedIcons.Values.Count > 1)
                {
                    //  Alighment everyone to the reference value

                    foreach (IconBase icon in Designer.SelectedIcons.Values)
                    {
                        if (icon is IDraggableIcon)
                        {
                            icon.CenterPosition = new Point(referenceIcon.CenterPosition.X, icon.CenterPosition.Y);
                        }
                    }
                }
            }
        }

        private void AlignLeft()
        {
            using (new ViewModel.ViewModelController.GuiChangeBlock(Designer.Model))
            {
                //  Find the furthest left

                double furthestLeft = double.MaxValue;

                foreach (IconBase icon in Designer.SelectedIcons.Values)
                {
                    if (icon.ReferencedObject is ObjectModel.IPositionableObject positionableObject)
                    {
                        furthestLeft = Math.Min(furthestLeft, positionableObject.LeftTopPosition.X);
                    }
                }

                //  Alighment everyone to the common value

                foreach (IconBase icon in Designer.SelectedIcons.Values)
                {
                    if (icon.ReferencedObject is ObjectModel.IPositionableObject positionableObject)
                    {
                        positionableObject.LeftTopPosition = new Point(furthestLeft, positionableObject.LeftTopPosition.Y);
                    }
                }
            }
        }

        private void AlignRight()
        {
            using (new ViewModel.ViewModelController.GuiChangeBlock(Designer.Model))
            {
                //  Find the furthest right

                double furthestRight = 0;

                foreach (IconBase icon in Designer.SelectedIcons.Values)
                {
                    if (icon is IDraggableIcon draggableIcon)
                    {
                        furthestRight = Math.Max(furthestRight, icon.Right);
                    }
                }

                //  Alighment everyone to the common value

                foreach (IconBase icon in Designer.SelectedIcons.Values)
                {
                    if (icon.ReferencedObject is ObjectModel.IPositionableObject positionableObject)
                    {
                        positionableObject.LeftTopPosition = new Point(furthestRight - icon.Size.Width, positionableObject.LeftTopPosition.Y);
                    }
                }
            }
        }

        private void AlignTop()
        {
            using (new ViewModel.ViewModelController.GuiChangeBlock(Designer.Model))
            {
                //  Find the highest top

                double highestTop = double.MaxValue;

                foreach (IconBase icon in Designer.SelectedIcons.Values)
                {
                    if (icon.ReferencedObject is ObjectModel.IPositionableObject positionableObject)
                    {
                        highestTop = Math.Min(highestTop, positionableObject.LeftTopPosition.Y);
                    }
                }

                //  Alighment everyone to the common value

                foreach (IconBase icon in Designer.SelectedIcons.Values)
                {
                    if (icon.ReferencedObject is ObjectModel.IPositionableObject positionableObject)
                    {
                        positionableObject.LeftTopPosition = new Point(positionableObject.LeftTopPosition.X, highestTop);
                    }
                }
            }
        }

        private void AlignVerticalCenter()
        {
            using (new ViewModel.ViewModelController.GuiChangeBlock(Designer.Model))
            {
                //  Use the icon whose context menu fired the command as the reference point

                IconBase referenceIcon = Designer.FindNearestSelectedIcon(ContextMenuActivationLocation) as IconBase;

                if (referenceIcon != null && Designer.SelectedIcons.Values.Count > 1)
                {
                    //  Alighment everyone to the reference value

                    foreach (IconBase icon in Designer.SelectedIcons.Values)
                    {
                        if (icon is IDraggableIcon)
                        {
                            icon.CenterPosition = new Point(icon.CenterPosition.X, referenceIcon.CenterPosition.Y);
                        }
                    }
                }
            }
        }

        private void DistributeHorizontally()
        {
            using (new ViewModel.ViewModelController.GuiChangeBlock(Designer.Model))
            {
                //  Find the bounds

                double minX = double.MaxValue;
                double maxX = 0;
                int count = 0;

                foreach (IconBase icon in Designer.SelectedIcons.Values)
                {
                    if (icon is IDraggableIcon draggableIcon)
                    {
                        minX = Math.Min(minX, icon.CenterPosition.X);
                        maxX = Math.Max(maxX, icon.CenterPosition.X);
                        count++;
                    }
                }

                if (count > 1 && maxX > minX)
                {
                    double deltaX = (maxX - minX) / (count - 1);
                    double nextX = minX;

                    //  Distribute the icons

                    foreach (IconBase icon in Designer.SelectedIcons.Values)
                    {
                        if (icon is IDraggableIcon)
                        {
                            icon.CenterPosition = new Point(nextX, icon.CenterPosition.Y);
                            nextX += deltaX;
                        }
                    }
                }
            }
        }

        private void DistributeVertically()
        {
            using (new ViewModel.ViewModelController.GuiChangeBlock(Designer.Model))
            {
                //  Find the bounds

                double minY = double.MaxValue;
                double maxY = 0;
                int count = 0;

                foreach (IconBase icon in Designer.SelectedIcons.Values)
                {
                    if (icon is IDraggableIcon draggableIcon)
                    {
                        minY = Math.Min(minY, icon.CenterPosition.Y);
                        maxY = Math.Max(maxY, icon.CenterPosition.Y);
                        count++;
                    }
                }

                if (count > 1 && maxY > minY)
                {
                    double deltaY = (maxY - minY) / (count - 1);
                    double nextY = minY;

                    //  Distribute the icons

                    foreach (IconBase icon in Designer.SelectedIcons.Values)
                    {
                        if (icon is IDraggableIcon)
                        {
                            icon.CenterPosition = new Point(icon.CenterPosition.X, nextY);
                            nextY += deltaY;
                        }
                    }
                }
            }
        }

        public override int Exec(ref Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
        {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();

            if (pguidCmdGroup == PackageGuids.guidSimpleStateMachineEditorPackageCmdSet)
            {
                switch (nCmdID)
                {
                    case PackageIds.AlignBottomCommandId:
                        using (Designer.Model.CreateAtomicGuiChangeBlock("Align bottom"))
                        {
                            AlignBottom();
                        }
                        return VSConstants.S_OK;
                    case PackageIds.AlignHorizontalCenterCommandId:
                        using (Designer.Model.CreateAtomicGuiChangeBlock("Align center"))
                        {
                            AlignHorizontalCenter();
                        }
                        return VSConstants.S_OK;
                    case PackageIds.AlignLeftCommandId:
                        using (Designer.Model.CreateAtomicGuiChangeBlock("Align left"))
                        {
                            AlignLeft();
                        }
                        return VSConstants.S_OK;
                    case PackageIds.AlignRightCommandId:
                        using (Designer.Model.CreateAtomicGuiChangeBlock("Align right"))
                        {
                            AlignRight();
                        }
                        return VSConstants.S_OK;
                    case PackageIds.AlignTopCommandId:
                        using (Designer.Model.CreateAtomicGuiChangeBlock("Align top"))
                        {
                            AlignTop();
                        }
                        return VSConstants.S_OK;
                    case PackageIds.AlignVerticalCenterCommandId:
                        using (Designer.Model.CreateAtomicGuiChangeBlock("Align center"))
                        {
                            AlignVerticalCenter();
                        }
                        return VSConstants.S_OK;
                    case PackageIds.DistributeHorizontallyCommandId:
                        using (Designer.Model.CreateAtomicGuiChangeBlock("Distribute horizontally"))
                        {
                            DistributeHorizontally();
                        }
                        return VSConstants.S_OK;
                    case PackageIds.DistributeVerticallyCommandId:
                        using (Designer.Model.CreateAtomicGuiChangeBlock("Distribute vertically"))
                        {
                            DistributeVertically();
                        }
                        return VSConstants.S_OK;
                    default:
                        break;
                }
            }
            return base.Exec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);
        }

        public override int QueryStatus(ref Guid pguidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText)
        {
            if (pguidCmdGroup == PackageGuids.guidSimpleStateMachineEditorPackageCmdSet)
            {
                for (int i = 0; i < cCmds; i++)
                {
                    switch (prgCmds[i].cmdID)
                    {
                        case PackageIds.AlignBottomCommandId:
                        case PackageIds.AlignHorizontalCenterCommandId:
                        case PackageIds.AlignLeftCommandId:
                        case PackageIds.AlignRightCommandId:
                        case PackageIds.AlignTopCommandId:
                        case PackageIds.AlignVerticalCenterCommandId:
                        case PackageIds.DistributeHorizontallyCommandId:
                        case PackageIds.DistributeVerticallyCommandId:
                            prgCmds[i].cmdf = (uint)(OLECMDF.OLECMDF_SUPPORTED | OLECMDF.OLECMDF_ENABLED);
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
