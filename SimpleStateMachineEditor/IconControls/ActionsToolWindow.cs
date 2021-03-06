using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Threading;

namespace SimpleStateMachineEditor.IconControls
{
    /// <summary>
    /// This class implements the tool window exposed by this package and hosts a user control.
    /// </summary>
    /// <remarks>
    /// In Visual Studio tool windows are composed of a frame (implemented by the shell) and a pane,
    /// usually implemented by the package implementer.
    /// <para>
    /// This class derives from the ToolWindowPane class provided from the MPF in order to use its
    /// implementation of the IVsUIElementPane interface.
    /// </para>
    /// </remarks>
    [Guid("fb4dba76-8316-407c-ac05-514d0a959bd0")]
    public class ActionsToolWindow : ToolWindowPane, INotifyPropertyChanged
    {
        //  This is the designer whose actions are currently reflected in the tool window

        public DesignerControl Designer
        {
            get => _designer;
            set
            {
                ThreadHelper.ThrowIfNotOnUIThread();
                if (_designer != value)
                {
                    string oldDesignerName = "<null>";
                    if (_designer != null)
                    {
                        oldDesignerName = _designer.Model.StateMachine?.Name ?? "<unnamed>";
                        _designer.Model.PropertyChanged -= DesignerModelPropertyChangedHandler;
                        _designer.Unloaded -= DesignerUnloadedHandler;
                    }
                    _designer = value;
                    string newDesignerName = "<null>";
                    if (_designer != null)
                    {
                        newDesignerName = _designer.Model.StateMachine?.Name ?? "<unnamed>";
                        if (_designer.Model.StateMachine != null)
                        {
                            ReloadToolWindowActionIcons();
                        }
                        _designer.Model.PropertyChanged += DesignerModelPropertyChangedHandler;
                        _designer.Unloaded += DesignerUnloadedHandler;
                    }
                    OnPropertyChanged("Designer");
                }
            }
        }
        DesignerControl _designer;

        //  This is the set of icons displayed in the tools window's data grid

        public ObservableCollection<Icons.ToolWindowActionIcon> ToolWindowActionIcons { get; private set; }


        public event PropertyChangedEventHandler PropertyChanged;




        /// <summary>
        /// Initializes a new instance of the <see cref="ActionsToolWindow"/> class.
        /// </summary>
        public ActionsToolWindow() : base(null)
        {
            this.Caption = "Actions";
            ToolWindowActionIcons = new ObservableCollection<Icons.ToolWindowActionIcon>();
            this.Content = new ActionsToolWindowControl(this);
        }

        private void DesignerModelPropertyChangedHandler(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "StateMachine")
            {
                ReloadToolWindowActionIcons();
            }
        }
        private void DesignerUnloadedHandler(object sender, RoutedEventArgs e)
        {
            Designer = (Package as SimpleStateMachineEditorPackage).ActiveDesignerControl;
        }

        private void LoadToolWindowActionIconsInternal()
        {
            HashSet<ViewModel.Action> actions = new HashSet<ViewModel.Action>(Designer.Model.StateMachine.Actions);
            List<Icons.ToolWindowActionIcon> iconsPendingDeletion = new List<Icons.ToolWindowActionIcon>();
            HashSet<ViewModel.Action> actionsWithIcons = new HashSet<ViewModel.Action>();

            foreach (Icons.ToolWindowActionIcon icon in ToolWindowActionIcons)
            {
                if (actions.Contains(icon.Action))
                {
                    actionsWithIcons.Add(icon.Action);
                }
                else
                {
                    iconsPendingDeletion.Add(icon);
                }
            }

            foreach (Icons.ToolWindowActionIcon icon in iconsPendingDeletion)
            {
                ToolWindowActionIcons.Remove(icon);
            }

            actions.ExceptWith(actionsWithIcons);
            List<ViewModel.Action> actionsNeedingIcons = new List<ViewModel.Action>(actions);
            actionsNeedingIcons.Sort();

            int needyIndex = 0;
            int existingIndex = 0;

            while (needyIndex < actionsNeedingIcons.Count)
            {
                if (existingIndex >= ToolWindowActionIcons.Count || actionsNeedingIcons[needyIndex].Name.CompareTo(ToolWindowActionIcons[existingIndex].Action.Name) < 0)
                {
                    ToolWindowActionIcons.Insert(existingIndex++, new Icons.ToolWindowActionIcon(this, actionsNeedingIcons[needyIndex++]));
                }
                else
                {
                    existingIndex++;
                }
            }
        }

        protected override void OnCreate()
        {
            base.OnCreate();
            Designer = (Package as SimpleStateMachineEditorPackage).ActiveDesignerControl;
        }

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void ReloadToolWindowActionIcons(bool clearIcons = false)
        {
            if (Designer.Model.StateMachine != null)
            {
                Designer.Model.StateMachine.Actions.CollectionChanged -= ViewModelActionsCollectionChangedHandler;
                ToolWindowActionIcons.CollectionChanged -= ToolWindowActionIconsChangedHandler;
                foreach (Icons.ToolWindowActionIcon icon in ToolWindowActionIcons)
                {
                    icon.PropertyChanged -= ToolWindowActionIconPropertyChangedHandler;
                }

                if (clearIcons)
                {
                    ToolWindowActionIcons.Clear();
                }
                LoadToolWindowActionIconsInternal();

                Designer.Model.StateMachine.Actions.CollectionChanged += ViewModelActionsCollectionChangedHandler;
                ToolWindowActionIcons.CollectionChanged += ToolWindowActionIconsChangedHandler;
                foreach (Icons.ToolWindowActionIcon icon in ToolWindowActionIcons)
                {
                    icon.PropertyChanged += ToolWindowActionIconPropertyChangedHandler;
                }
            }
        }

        private void ToolWindowActionIconPropertyChangedHandler(object sender, PropertyChangedEventArgs e)
        {
            if (sender is Icons.ToolWindowActionIcon icon)
            {
                switch (e.PropertyName)
                {
                    case "Description":
                        using (new ViewModel.ViewModelController.GuiChangeBlock(Designer.Model))
                        {
                            icon.Action.Description = icon.Description;
                        }
                        break;
                    case "Name":
                        using (new ViewModel.ViewModelController.GuiChangeBlock(Designer.Model))
                        {
                            icon.Action.Name = icon.Name;
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        private void ToolWindowActionIconsChangedHandler(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    using (Designer.Model.CreateAtomicGuiChangeBlock("Add action"))
                    {
                        foreach (Icons.ToolWindowActionIcon icon in e.NewItems)
                        {
                            Designer.Model.LogUndoAction(new UndoRedo.DeleteActionRecord(Designer.Model, icon.Action));
                            icon.PropertyChanged += ToolWindowActionIconPropertyChangedHandler;
                            Designer.Model.StateMachine.Actions.Add(icon.Action);
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    using (Designer.Model.CreateAtomicGuiChangeBlock("Delete action"))
                    {
                        foreach (Icons.ToolWindowActionIcon icon in e.OldItems)
                        {
                            icon.PropertyChanged -= ToolWindowActionIconPropertyChangedHandler;
                            Designer.Model.StateMachine.Actions.Remove(icon.Action);
                            Designer.Model.LogUndoAction(new UndoRedo.AddActionRecord(Designer.Model, icon.Action));
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Replace:
                case NotifyCollectionChangedAction.Move:
                case NotifyCollectionChangedAction.Reset:
                default:
                    throw new NotImplementedException();
            }
        }

        private void ViewModelActionsCollectionChangedHandler(object sender, NotifyCollectionChangedEventArgs e)
        {
            ReloadToolWindowActionIcons(false);
        }
    }
}
