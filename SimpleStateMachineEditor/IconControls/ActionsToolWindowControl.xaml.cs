using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace SimpleStateMachineEditor.IconControls
{
    /// <summary>
    /// Interaction logic for ActionsToolWindowControl.
    /// </summary>
    public partial class ActionsToolWindowControl : UserControl, INotifyPropertyChanged
    {
        public ActionsToolWindow ToolWindow { get; private set; }

#if false
#if DEBUG
        DispatcherTimer InitializationTimer;
#endif
#endif

        public event PropertyChangedEventHandler PropertyChanged;





        /// <summary>
        /// Initializes a new instance of the <see cref="ActionsToolWindowControl"/> class.
        /// </summary>
        public ActionsToolWindowControl(ActionsToolWindow toolWindow)
        {
            ToolWindow = toolWindow;
            this.InitializeComponent();
        }


        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void ActionMouseLeftButtonDownHandler(object sender, MouseButtonEventArgs e)
        {
            if (sender is DataGridRow row && row.Item is Icons.ToolWindowActionIcon icon)
            {
                icon.MouseLeftButtonDownHandler(sender, e);
            }
        }

        private void ActionMouseEnteredHandler(object sender, MouseEventArgs e)
        {
            if (sender is DataGridRow row && row.Item is Icons.ToolWindowActionIcon icon)
            {
                icon.MouseEnteredHandler(sender, e);
            }
        }

        private void ActionMouseLeaveHandler(object sender, MouseEventArgs e)
        {
            if (sender is DataGridRow row && row.Item is Icons.ToolWindowActionIcon icon)
            {
                icon.MouseLeaveHandler(sender, e);
            }
        }

        private void NewToolWindowActionHandler(object sender, AddingNewItemEventArgs e)
        {
            ViewModel.Action action = ViewModel.Action.Create(ToolWindow.Designer.Model, ToolWindow.Designer.OptionsPage.ActionRootName);
            e.NewItem = new Icons.ToolWindowActionIcon(ToolWindow, action);
        }
    }
}