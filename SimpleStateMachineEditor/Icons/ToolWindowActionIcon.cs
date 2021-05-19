using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace SimpleStateMachineEditor.Icons
{
    public class ToolWindowActionIcon : INotifyPropertyChanged
    {
        DispatcherTimer MouseHoverTimer;
        public ObjectModel.EditableString Name
        {
            get => _name;
            set
            {
                if (_name != value)
                {
                    _name = value;
                    OnPropertyChanged("Name");
                }
            }
        }
        ObjectModel.EditableString _name;

        public ObjectModel.EditableString Description
        {
            get => _description;
            set
            {
                if (_description != value)
                {
                    _description = value;
                    OnPropertyChanged("Description");
                }
            }
        }
        ObjectModel.EditableString _description;

        public bool IsHovering
        {
            get => _isHovering;
            set
            {
                if (_isHovering != value)
                {
                    bool wasHovering = _isHovering;
                    _isHovering = value;
                    OnPropertyChanged("IsHovering");
                    if (wasHovering && !_isHovering)
                    {
                        OnHoverEnd();
                    }
                }
            }
        }
        bool _isHovering;
        public ViewModel.Action Action 
        {
            get => _action;
            private set
            {
                if (_action != value)
                {
                    _action = value;
                    _action.PropertyChanged += ActionPropertyChangedHandler;
                    Name = _action.Name;
                    Description = _action.Description;
                    OnPropertyChanged("Action");
                }
            }
        }
        ViewModel.Action _action;

        public IconControls.ActionsToolWindow ToolWindow;

        public event PropertyChangedEventHandler PropertyChanged;


        //  Constructor used by the DataGrid control to create a new action

        public ToolWindowActionIcon()
        {
            Action = null;
            MouseHoverTimer = new DispatcherTimer();
            MouseHoverTimer.Interval = new TimeSpan(0, 0, 0, 0, Icons.IconBase.HoverDelay);
            MouseHoverTimer.Tick += OnHover;
        }

        //  Constructor used for existing actions

        internal ToolWindowActionIcon(IconControls.ActionsToolWindow toolWindow, ViewModel.Action action)
        {
            ToolWindow = toolWindow;
            Action = action;
            MouseHoverTimer = new DispatcherTimer();
            MouseHoverTimer.Interval = new TimeSpan(0, 0, 0, 0, Icons.IconBase.HoverDelay);
            MouseHoverTimer.Tick += OnHover;
        }

        internal void MouseEnteredHandler(object sender, MouseEventArgs e)
        {
            if (!IsHovering && Action != null)
            {
                MouseHoverTimer.Start();
                e.Handled = true;
            }
        }

        internal void MouseLeaveHandler(object sender, MouseEventArgs e)
        {
            MouseHoverTimer.Stop();
            IsHovering = false;
            e.Handled = true;
        }

        internal void MouseLeftButtonDownHandler(object sender, MouseButtonEventArgs e)
        {
            MouseHoverTimer.Stop();
            IsHovering = false;
            DataObject dataObject = new DataObject(this);
            DragDrop.DoDragDrop(sender as DependencyObject, dataObject, DragDropEffects.Copy);
        }

        private void ActionPropertyChangedHandler(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "Description":
                    Description = Action.Description;
                    break;
                case "Name":
                    Name = Action.Name;
                    break;
                default:
                    break;
            }
        }

        private void OnHover(object sender, EventArgs e)
        {
            MouseHoverTimer.Stop();
            IsHovering = true;

            foreach (ViewModel.Transition transition in Action.Controller.StateMachine.Transitions)
            {
                if (transition.Actions.Contains(Action) && ToolWindow.Designer != null && ToolWindow.Designer.LoadedIcons[transition] is Icons.TransitionIcon transitionIcon)
                {
                    ActionIcon icon = transitionIcon.ActionIcons.Where(actionIcon => actionIcon.ReferencedObject == Action).Single();
                    icon.ActionIsHighlighted = true;
                }
            }
        }

        private void OnHoverEnd()
        {
            foreach (ViewModel.Transition transition in Action.Controller.StateMachine.Transitions)
            {
                if (transition.Actions.Contains(Action) && ToolWindow.Designer != null && ToolWindow.Designer.LoadedIcons[transition] is Icons.TransitionIcon transitionIcon)
                {
                    ActionIcon icon = transitionIcon.ActionIcons.Where(actionIcon => actionIcon.ReferencedObject == Action).Single();
                    icon.ActionIsHighlighted = false;
                }
            }
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
