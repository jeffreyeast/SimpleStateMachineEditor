using SimpleStateMachineEditor.ObjectModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace SimpleStateMachineEditor.Icons
{
    /// <summary>
    /// Represents an action in a transition icon's action list
    /// </summary>
    internal class ActionIcon : IconBase
    {
        public override int ContextMenuId => PackageIds.ActionIconContextMenuId;

        internal ListBoxItem ListBoxItem { get; set; }


        public bool ActionIsHighlighted
        {
            get => _actionIsHighlighted;
            set
            {
                if (_actionIsHighlighted != value)
                {
                    _actionIsHighlighted = value;
                    OnPropertyChanged("ActionIsHighlighted");
                }
            }
        }
        bool _actionIsHighlighted;




        internal ActionIcon(DesignerControl designer, ViewModel.Action action) : base(designer, action, null, null)
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

        public override Size Size { get => throw new InvalidOperationException(); set => throw new InvalidOperationException(); }
    }
}
