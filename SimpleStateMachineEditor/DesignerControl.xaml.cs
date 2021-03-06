using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;
using SimpleStateMachineEditor.ObjectModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SimpleStateMachineEditor
{
    /// <summary>
    /// Interaction logic for DesignerControl.xaml
    /// </summary>
    public partial class DesignerControl : UserControl, IOleCommandTarget
    {
        const string AddEventTypeDescription = "Add event type";
        const string AddGroupDescription = "Add group";
        const string AddGroupMemberDescription = "Add group member";
        const string AddLayerDescription = "Add layer";
        const string AddLayerMemberDescription = "Add layer member";
        const string AddStateDescription = "Add state";
        const string AddTransitionDescription = "Add transition";
        const string ChangeTransitionDestinationDescription = "Change transition end state";
        const string ChangeTransitionSourceDescription = "Change transition start state";

        public ViewModel.ViewModelController Model
        {
            get => _model;
            set
            {
                if (_model != value)
                {
                    if (_model != null)
                    {
                        _model.PropertyChanged -= ModelPropertyChangedHandler;
                    }
                    _model = value;
                    if (_model != null)
                    {
                        _model.PropertyChanged += ModelPropertyChangedHandler;
                    }
                }
            }
        }
        ViewModel.ViewModelController _model;
        ViewModel.StateMachine StateMachine;
        internal IVsUIShell UiShell;
        ITrackSelection SelectionTracker;
        internal List<ObjectModel.ITrackableObject> SelectedObjects;
        internal Dictionary<ObjectModel.ITrackableObject, Icons.ISelectableIcon> SelectedIcons;
        internal Dictionary<ObjectModel.ITrackableObject, Icons.ISelectableIcon> LoadedIcons;
        Microsoft.VisualStudio.Shell.SelectionContainer SelectionContainer;
        public SimpleStateMachineEditorPackage Package;
        internal System.Windows.Point ContextMenuActivationLocation;
        internal Icons.SelectionBoxIcon SelectionBoxIcon;
        internal MouseStateMachine.DesignerMouseSelectionImplementation MouseStateMachine;
        IInputElement OldKeyboardFocus;
        public IconControls.OptionsPropertiesPage OptionsPage { get; private set; }
        IOleParentUndoUnit CurrentParentUndoUnit;
        internal ViewModel.Layer DefaultLayer;
        internal ViewModel.Layer CurrentLayer
        {
            get => _currentLayer;
            set
            {
                if (_currentLayer != value)
                {
                    if (_currentLayer != null)
                    {
                        _currentLayer.Members.CollectionChanged -= CurrentLayerMembersCollectionChangedHandler;
                        _currentLayer.IsCurrentLayer = false;
                        Model.LogUndoAction(new UndoRedo.SetLayerActiveRecord(Model, this, _currentLayer, value));
                    }
                    _currentLayer = value;
                    if (_currentLayer != null)
                    {
                        _currentLayer.Members.CollectionChanged += CurrentLayerMembersCollectionChangedHandler;
                        _currentLayer.IsCurrentLayer = true;
                        SetCurrentLayer();
                    }
                    ClearSelectedItems();
                    LoadViewModelIcons();
                }
            }
        }
        ViewModel.Layer _currentLayer;




        internal DesignerControl(SimpleStateMachineEditorPackage package, ViewModel.ViewModelController model, ITrackSelection selectionTracker)
        {
            Package = package;
            UiShell = ((System.IServiceProvider)package).GetService(typeof(SVsUIShell)) as IVsUIShell;
            OptionsPage = Package.OptionsPropertiesPage;
            Model = model;
            SelectedObjects = new List<ObjectModel.ITrackableObject>();
            LoadedIcons = new Dictionary<ObjectModel.ITrackableObject, Icons.ISelectableIcon>();
            SelectedIcons = new Dictionary<ObjectModel.ITrackableObject, Icons.ISelectableIcon>();
            SelectionContainer = new Microsoft.VisualStudio.Shell.SelectionContainer(true, false);
            SelectionContainer.SelectableObjects = SelectedObjects;
            SelectionContainer.SelectedObjects = SelectedObjects;
            SelectionBoxIcon = new Icons.SelectionBoxIcon(this, null, null);

            InitializeComponent();

            DataContext = Model.StateMachine;
            SelectionTracker = selectionTracker;

            Loaded += DesignerControl_Loaded;
        }

        internal void AddEventType(Point? center = null)
        {
            using (Model.CreateAtomicGuiChangeBlock(AddEventTypeDescription))
            {
                ViewModel.EventType newEventType = ViewModel.EventType.Create(Model, OptionsPage);
                Model.LogUndoAction(new UndoRedo.DeleteEventTypeRecord(Model, newEventType));
                if (!center.HasValue)
                {
                    center = FindEmptySpace(Icons.StateIcon.IconSize);
                }
                newEventType.LeftTopPosition = new Point(center.Value.X - Icons.EventTypeIcon.IconSize.Width / 2, center.Value.Y - Icons.EventTypeIcon.IconSize.Height / 2);
                Model.StateMachine.EventTypes.Add(newEventType);
            }
        }

        internal void AddGroup(Point? center = null)
        {
            using (Model.CreateAtomicGuiChangeBlock(AddGroupDescription))
            {
                ViewModel.Group newGroup = ViewModel.Group.Create(Model, OptionsPage, CurrentLayer);
                Model.LogUndoAction(new UndoRedo.DeleteGroupRecord(Model, newGroup));
                ViewModel.Layer newLayer = ViewModel.Layer.Create(Model, LayerPosition.GroupStatuses.Explicit);
                Model.LogUndoAction(new UndoRedo.DeleteLayerRecord(Model, newLayer));
                newLayer.Name = newGroup.Name;
                newGroup.CoNamedObject = newLayer;
                if (!center.HasValue)
                {
                    center = FindEmptySpace(Icons.GroupIcon.IconSize);
                }
                Point position = new Point(center.Value.X - Icons.GroupIcon.IconSize.Width / 2, center.Value.Y - Icons.GroupIcon.IconSize.Height / 2);
                Model.StateMachine.Layers.Add(newLayer);
                AddLayerMember(DefaultLayer, newGroup, LayerPosition.GroupStatuses.NotGrouped, position);
                if (CurrentLayer != DefaultLayer)
                {
                    AddLayerMember(CurrentLayer, newGroup, LayerPosition.GroupStatuses.NotGrouped, position);
                }

                Model.StateMachine.Groups.Add(newGroup);
            }
        }

        internal void AddGroupMember(ViewModel.Group group, ViewModel.State state)
        {
            if (!state.LayerPositions.Any(lp => lp.GroupStatus == LayerPosition.GroupStatuses.Explicit))
            {
                using (Model.CreateAtomicGuiChangeBlock(AddGroupMemberDescription))
                {
                    AddLayerMemberInternal(group.Layer, state, LayerPosition.GroupStatuses.Explicit, state.LeftTopPosition);

                    foreach (ObjectModel.ITransition transition in state.TransitionsFrom)
                    {
                        if (!(transition.DestinationState is ViewModel.Group) && !group.Members.Contains(transition.DestinationState))
                        {
                            AddLayerMemberInternal(group.Layer, transition.DestinationState, LayerPosition.GroupStatuses.Implicit, transition.DestinationState.LeftTopPosition);
                        }
                    }
                    foreach (ObjectModel.ITransition transition in state.TransitionsTo)
                    {
                        if (!(transition.SourceState is ViewModel.Group) && !group.Members.Contains(transition.SourceState))
                        {
                            AddLayerMemberInternal(group.Layer, transition.SourceState, LayerPosition.GroupStatuses.Implicit, transition.SourceState.LeftTopPosition);
                        }
                    }
                }
            }
        }

        private void AddLayer(object sender, RoutedEventArgs e)
        {
            using (Model.CreateAtomicGuiChangeBlock(AddLayerDescription))
            {
                ViewModel.Layer newLayer = ViewModel.Layer.Create(Model, OptionsPage, false, LayerPosition.GroupStatuses.NotGrouped);
                Model.LogUndoAction(new UndoRedo.DeleteLayerRecord(Model, newLayer));
                Model.StateMachine.Layers.Add(newLayer);
            }
        }

        /// <summary>
        /// Method for use by *commands* to add an object to a layer
        /// </summary>
        internal void AddLayerMember(ViewModel.Layer layer, ObjectModel.ITransitionEndpoint newMember, LayerPosition.GroupStatuses groupStatus, Point position)
        {
            if ((newMember is ViewModel.Group && layer.CoNamedObject == null) || 
                (newMember is ViewModel.State state && 
                 ((layer.CoNamedObject == null && !newMember.LayerPositions.Any(lp => lp.GroupStatus == LayerPosition.GroupStatuses.Explicit)) || 
                  (layer.CoNamedObject != null && groupStatus != LayerPosition.GroupStatuses.NotGrouped))))
            {
                AddLayerMemberInternal(layer, newMember, groupStatus, position);
            }
        }

        private void AddLayerMemberInternal(ViewModel.Layer layer, ObjectModel.ITransitionEndpoint newMember, LayerPosition.GroupStatuses groupStatus, Point position)
        {
            using (Model.CreateAtomicGuiChangeBlock(AddLayerMemberDescription))
            {
                ObjectModel.LayerPosition layerPosition = newMember.LayerPositions.Where(lp => lp.Layer == layer).FirstOrDefault();
                if (layerPosition == null)
                {
                    layerPosition = ObjectModel.LayerPosition.Create(Model, layer, groupStatus);
                    Model.LogUndoAction(new UndoRedo.DeleteLayerMemberRecord(Model, layerPosition, newMember));
                    layerPosition.LeftTopPosition = position;
                    newMember.LayerPositions.Add(layerPosition);
                    layer.Members.Add(newMember);
                }
                else
                {
                    layerPosition.GroupStatus = (LayerPosition.GroupStatuses)Math.Max((int)layerPosition.GroupStatus, (int)groupStatus);
                    layerPosition.LeftTopPosition = position;
                }
            }
        }

        internal void AddState(Point? center = null)
        {
            using (Model.CreateAtomicGuiChangeBlock(AddStateDescription))
            {
                ViewModel.State newState = ViewModel.State.Create(Model, OptionsPage, CurrentLayer);
                Model.LogUndoAction(new UndoRedo.DeleteStateRecord(Model, newState));
                if (!center.HasValue)
                {
                    center = FindEmptySpace(Icons.StateIcon.IconSize);
                }
                Point position = new Point(center.Value.X - Icons.StateIcon.IconSize.Width / 2, center.Value.Y - Icons.StateIcon.IconSize.Height / 2);
                AddLayerMember(DefaultLayer, newState, LayerPosition.GroupStatuses.NotGrouped, position);
                if (CurrentLayer != DefaultLayer)
                {
                    AddLayerMember(CurrentLayer, newState, CurrentLayer.DefaultGroupStatus, position);
                }

                Model.StateMachine.States.Add(newState);
            }
        }

        internal void AddTransition(ViewModel.State sourceState = null)
        {
            if (sourceState == null)
            {
                //  See if we have a source state selected, if not, we can't execute the command

                if (SelectedObjects.Count == 1 && SelectedObjects[0] is ViewModel.State state)
                {
                    sourceState = state;
                }
            }

            if (sourceState != null)
            {
                //  The user has executed an Add Transition command on the source state. We create the transition
                //  in a dragging state

                CurrentParentUndoUnit = new UndoRedo.ParentRecord(Model, AddTransitionDescription);
                Model.UndoManager.Open(CurrentParentUndoUnit);

                ViewModel.Transition newTransition = new ViewModel.Transition(Model, sourceState);

                // We're dragging the new transition icon, so the user can bind it to the destination state

                Icons.TransitionIcon transitionIcon = new Icons.TransitionIcon(this, newTransition, null, null);
                transitionIcon.DragType = Icons.TransitionIcon.DragTypes.Adding;
                MouseStateMachine.StartDraggingTransition(transitionIcon);
            }
        }

        internal void AlignmentCommand(uint commandId)
        {
            if (SelectedIcons.Count > 1 && SelectedIcons.Values.First() is Icons.PositionableIcon positionableIcon)
            {
                positionableIcon.Exec(ref PackageGuids.guidSimpleStateMachineEditorPackageCmdSet, commandId, 0, default(IntPtr), default(IntPtr));
            }
        }

        private void BuildDefaultLayer()
        {
            using (new UndoRedo.DontLogBlock(Model))
            {
                using (new ViewModel.ViewModelController.GuiChangeBlock(Model))
                {
                    DefaultLayer = ViewModel.Layer.Create(Model, OptionsPage, true, LayerPosition.GroupStatuses.NotGrouped);
                    Model.StateMachine.Layers.Add(DefaultLayer);

                    foreach (ViewModel.State state in Model.StateMachine.States)
                    {
                        DefaultLayer.Members.Add(state);
                        state.CurrentLayer = DefaultLayer;
                        ObjectModel.LayerPosition layerPosition = ObjectModel.LayerPosition.Create(Model, DefaultLayer, LayerPosition.GroupStatuses.NotGrouped);
                        layerPosition.LeftTopPosition = state.LegacyLeftTopPosition;
                        state.LayerPositions.Add(layerPosition);
                    }
                }
            }
        }

        internal void CancelTransactionDrag(ViewModel.Transition transition)
        {
            if (LoadedIcons.ContainsKey(transition))
            {
                (LoadedIcons[transition] as Icons.TransitionIcon).DragType = Icons.TransitionIcon.DragTypes.Nothing;
                IconSurface.Children.Add(LoadedIcons[transition].Body);
            }
            else
            {
                transition.Remove();
            }

            Model.UndoManager.Close(CurrentParentUndoUnit, 0);
        }

        internal void ChangeTransitionDestination(ViewModel.Transition transition)
        {
            //  The user has executed a Select new transition destinatin command. We switch the transition to a dragging state

            if (transition.SourceState != null && transition.DestinationState != null)
            {
                CurrentParentUndoUnit = new UndoRedo.ParentRecord(Model, ChangeTransitionDestinationDescription);
                Model.UndoManager.Open(CurrentParentUndoUnit);

                Icons.TransitionIcon transitionIcon = LoadedIcons[transition] as Icons.TransitionIcon;
                IconSurface.Children.Remove(transitionIcon.Body);

                // We're dragging the transition icon, so the user can bind it to the destination state

                transitionIcon.DragType = Icons.TransitionIcon.DragTypes.ChangingDestination;
                MouseStateMachine.StartDraggingTransition(transitionIcon);
            }
        }

        internal void ChangeTransitionSource(ViewModel.Transition transition)
        {
            //  The user has executed a Select new transition source command. We switch the transition to a dragging state

            if (transition.SourceState != null && transition.DestinationState != null)
            {
                CurrentParentUndoUnit = new UndoRedo.ParentRecord(Model, ChangeTransitionSourceDescription);
                Model.UndoManager.Open(CurrentParentUndoUnit);

                Icons.TransitionIcon transitionIcon = LoadedIcons[transition] as Icons.TransitionIcon;
                IconSurface.Children.Remove(transitionIcon.Body);

                // We're dragging the transition icon, so the user can bind it to the source state

                transitionIcon.DragType = Icons.TransitionIcon.DragTypes.ChangingSource;
                MouseStateMachine.StartDraggingTransition(transitionIcon);
            }
        }

        internal void ClearSelectedItems()
        {
            Icons.ISelectableIcon[] previouslySelectedIcons = SelectedIcons.Values.ToArray();
            SelectedObjects.Clear();
            SelectedIcons.Clear();
            foreach (Icons.ISelectableIcon icon in previouslySelectedIcons)
            {
                icon.IsSelectedChanged();
            }
        }

        internal void CommitTransactionDrag(ViewModel.Transition transition, Point dragTerminationPoint)
        {
            //  The user has finished dragging the transition around. So find the state icon nearest the
            //  drop point, and bind the transition to this as its destination

            double nearestDistance = double.MaxValue;
            ViewModel.State nearestState = null;

            foreach (var state in Model.StateMachine.States)
            {
                if (LoadedIcons.ContainsKey(state))
                {
                    Icons.StateIcon stateIcon = LoadedIcons[state] as Icons.StateIcon;
                    double distance = Utility.DrawingAids.Distance(dragTerminationPoint, stateIcon.CenterPosition);
                    if (distance < nearestDistance)
                    {
                        nearestDistance = distance;
                        nearestState = state;
                    }
                }
            }

            if (nearestState == null)
            {
                throw new InvalidProgramException();
            }

            using (new ViewModel.ViewModelController.GuiChangeBlock(Model))
            {
                CurrentParentUndoUnit.GetDescription(out string operationDescription);

                switch (operationDescription)
                {
                    case AddTransitionDescription:
                        using (new UndoRedo.DontLogBlock(Model))
                        {
                            transition.DestinationState = nearestState;
                        }
                        Model.LogUndoAction(new UndoRedo.DeleteTransitionRecord(Model, transition));
                        Model.StateMachine.Transitions.Add(transition);
                        break;

                    case ChangeTransitionDestinationDescription:
                        transition.DestinationState = nearestState;
                        IconSurface.Children.Add(LoadedIcons[transition].Body);
                        break;

                    case ChangeTransitionSourceDescription:
                        transition.SourceState = nearestState;
                        IconSurface.Children.Add(LoadedIcons[transition].Body);

                        //  If this would result in a duplicate trigger, remove the trigger from the transition. An alternative
                        //  UI model would be to refuse the bind.

                        if (transition.TriggerEvent != null && transition.SourceState.TransitionsFrom.Where(t => t.TriggerEvent == transition.TriggerEvent).Count() > 1)
                        {
                            transition.TriggerEvent = null;
                        }
                        break;

                    default:
                        throw new NotImplementedException();
                }

                (LoadedIcons[transition] as Icons.TransitionIcon).DragType = Icons.TransitionIcon.DragTypes.Nothing;

                Model.UndoManager.Close(CurrentParentUndoUnit, 1);
                SelectSingle(LoadedIcons[transition]);
            }
        }

        private void CurrentLayerMembersCollectionChangedHandler(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (ObjectModel.TrackableObject newMember in e.NewItems)
                    {
                        LoadViewModelIcon(newMember);

                        if (newMember is ViewModel.State state)
                        {
                            foreach (ViewModel.Transition transition in state.TransitionsFrom)
                            {
                                LoadViewModelIcon(transition);
                            }
                            foreach (ViewModel.Transition transition in state.TransitionsTo)
                            {
                                LoadViewModelIcon(transition);
                            }
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (ObjectModel.TrackableObject member in e.OldItems)
                    {
                        UnloadViewModelIcon(member);

                        if (member is ViewModel.State state)
                        {
                            foreach (ViewModel.Transition transition in state.TransitionsFrom)
                            {
                                UnloadViewModelIcon(transition);
                            }
                            foreach (ViewModel.Transition transition in state.TransitionsTo)
                            {
                                UnloadViewModelIcon(transition);
                            }
                        }
                    }
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        private void DeleteActionReference(ViewModel.Transition transition, ViewModel.ActionReference actionReference, int slot)
        {
            using (Model.CreateAtomicGuiChangeBlock("Remove action"))
            {
                transition.ActionReferences.RemoveAt(slot);
                Model.LogUndoAction(new UndoRedo.AddActionReferenceRecord(Model, actionReference, slot));
            }
        }

        private void  DeleteEventType(ViewModel.EventType eventType)
        {
            using (Model.CreateAtomicGuiChangeBlock("Remove event type"))
            {
                foreach (ViewModel.Transition t in Model.StateMachine.Transitions)
                {
                    if (t.TriggerEvent == eventType)
                    {
                        t.TriggerEvent = null;
                    }
                }
                Model.StateMachine.EventTypes.Remove(eventType);
                Model.LogUndoAction(new UndoRedo.AddEventTypeRecord(Model, eventType));
            }
        }

        private void DeleteGroup(ViewModel.Group group)
        {
            using (Model.CreateAtomicGuiChangeBlock("Remove group"))
            {
                ViewModel.Layer layer = group.Layer;
                ObjectModel.ITransitionEndpoint[] explicitMembers = group.Members.Where(m => m.LayerPositions.Any(lp => lp.Layer == group.Layer && lp.GroupStatus == LayerPosition.GroupStatuses.Explicit)).ToArray();
                foreach (ObjectModel.ITransitionEndpoint member in explicitMembers)
                { 
                    RemoveLayerMember(layer, member);
                }

                group.Remove();
                Model.StateMachine.Groups.Remove(group);
                DeleteLayer(layer);

                while (group.LayerPositions.Count > 0)
                {
                    RemoveLayerMember(group.LayerPositions.First().Layer, group);
                }
                Model.LogUndoAction(new UndoRedo.AddGroupRecord(Model, group));
            }
        }

        internal void DeleteIcon(Icons.ISelectableIcon icon)
        {
            if (icon is Icons.ActionReferenceIcon actionReferenceIcon && icon.ReferencedObject is ViewModel.ActionReference actionReference)
            {
                DeleteActionReference(actionReferenceIcon.Transition, actionReference, actionReference.Transition.ActionReferences.IndexOf(actionReference));
            }
            else if (icon.ReferencedObject is ViewModel.State state)
            {
                DeleteState(state);
            }
            else if (icon.ReferencedObject is ViewModel.EventType eventType)
            {
                DeleteEventType(eventType);
            }
            else if (icon.ReferencedObject is ViewModel.Group group)
            {
                DeleteGroup(group);
            }
            else if (icon.ReferencedObject is ViewModel.Layer layer)
            {
                if (!layer.IsDefaultLayer)
                {
                    DeleteLayer(layer);
                }
            }
            else if (icon.ReferencedObject is ViewModel.Transition transition)
            {
                DeleteTransition(transition);
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        private void DeleteLayer(ViewModel.Layer layer)
        {
            if (CurrentLayer == layer)
            {
                CurrentLayer = DefaultLayer;
            }

            using (Model.CreateAtomicGuiChangeBlock("Remove layer"))
            {
                ObjectModel.ITransitionEndpoint[] members = layer.Members.ToArray();
                foreach (ObjectModel.ITransitionEndpoint o in members)
                {
                    RemoveLayerPosition(o, o.LayerPositions.Where(lp => lp.Layer == layer).Single());
                }
                Model.StateMachine.Layers.Remove(layer);
                Model.LogUndoAction(new UndoRedo.AddLayerRecord(Model, layer));
            }
        }

        private void DeleteState(ViewModel.State state)
        {
            using (Model.CreateAtomicGuiChangeBlock("Remove state"))
            {
                while (state.TransitionsFrom.Count > 0)
                {
                    DeleteTransition(state.TransitionsFrom.First() as ViewModel.Transition);
                }
                while (state.TransitionsTo.Count > 0)
                {
                    DeleteTransition(state.TransitionsTo.First() as ViewModel.Transition);
                }
                while (state.LayerPositions.Count > 0)
                {
                    RemoveLayerMember(state.LayerPositions.First().Layer, state);
                }
                Model.StateMachine.States.Remove(state);
                Model.LogUndoAction(new UndoRedo.AddStateRecord(Model, state));
            }
        }

        private void DeleteTransition(ViewModel.Transition transition)
        {
            using (Model.CreateAtomicGuiChangeBlock("Remove transition"))
            {
                Model.StateMachine.Transitions.Remove(transition);
                Model.LogUndoAction(new UndoRedo.AddTransitionRecord(Model, transition));
            }
        }

        internal void DeleteSelectedIcons()
        {
            Icons.ISelectableIcon[] pendingDeletes = SelectedIcons.Values.ToArray();

            using (Model.CreateAtomicGuiChangeBlock("Delete"))
            {
                foreach (var icon in pendingDeletes)
                {
                    DeleteIcon(icon);
                }
            }
        }

        internal void DeselectIcon(Icons.ISelectableIcon draggingIcon)
        {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();

            SelectedObjects.Remove(draggingIcon.ReferencedObject);
            SelectedIcons.Remove(draggingIcon.ReferencedObject);
            draggingIcon.IsSelectedChanged();
            SelectionTracker.OnSelectChange(SelectionContainer);
        }

        private void DesignerControl_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= DesignerControl_Loaded;
            Unloaded += DesignerControl_Unloaded;

            Package.ErrorList.Clear();

            if (Model.StateMachine == null)
            {
                Package.ErrorList.WriteVisualStudioErrorList(Utility.ErrorList.MessageCategory.Severe, Model.ErrorMessage, Model.FileName, 1, 1);
            }
            else if (Model.StateMachine.Layers.Count == 0)
            {
                BuildDefaultLayer();
            }

            MouseStateMachine = new MouseStateMachine.DesignerMouseSelectionImplementation(this);
            MonitorStateMachineForChanges(false);
            SelectStateMachine();
            LoadViewModelIcons();

            //  On startup, the DTE window gets focus long before this control (or EditorPane) is created

            if (IsFocused && Package.ActiveDesignerControl == null)
            {
                Package.ActiveDesignerControl = this;
            }
            else
            {
                GotFocus += DesignerControl_GotFocus;
            }
        }

        private void DesignerControl_GotFocus(object sender, RoutedEventArgs e)
        {
            //  There seems to be a bug in the DTE ActiveWindow event delivery, it occurs the first time a window
            //  gets focus.  So we'll try to help them out here.

            GotFocus -= DesignerControl_GotFocus;
            Package.ActiveDesignerControl = this;
        }

        private void DesignerControl_Unloaded(object sender, RoutedEventArgs e)
        {
            Loaded += DesignerControl_Loaded;
            Unloaded -= DesignerControl_Unloaded;

            using (new UndoRedo.DontLogBlock(Model))
            {
                MonitorStateMachineForChanges(true);
                ClearSelectedItems();
                CurrentLayer = DefaultLayer = null;
            }
        }

        internal bool DoIdle()
        {
            return Model.DoIdle();
        }

        public int Exec(ref Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
        {
            if (pguidCmdGroup == PackageGuids.guidSimpleStateMachineEditorPackageCmdSet)
            {
                switch (nCmdID)
                {
                    case PackageIds.AddEventTypeCommandId:
                        AddEventType(ContextMenuActivationLocation);
                        return VSConstants.S_OK;
                    case PackageIds.AddGroupCommandId:
                        AddGroup(ContextMenuActivationLocation);
                        return VSConstants.S_OK;
                    case PackageIds.AddStateCommandId:
                        AddState(ContextMenuActivationLocation);
                        return VSConstants.S_OK;
                    default:
                        return (int)Microsoft.VisualStudio.OLE.Interop.Constants.OLECMDERR_E_NOTSUPPORTED;
                }
            }
            return (int)Microsoft.VisualStudio.OLE.Interop.Constants.OLECMDERR_E_UNKNOWNGROUP;
        }

        /// <summary>
        /// Look for space on the screen that doesn't have anything on it.
        /// </summary>
        /// <param name="iconSize"></param>
        /// <returns></returns>
        private Point FindEmptySpace(Size iconSize)
        {
            for (int pass = 1; ; pass++)
            {
                double startX;
                double centerY;

                //  Set the default search starting position

                switch (pass)
                {
                    case 1:
                        //  Start roughly in the middle of the screen. Try to ensure that if new icons wrap, we'll put them in the same
                        //  "row" and "column" as we start

                        startX = Math.Floor(ActualWidth / 2 / (iconSize.Width * 1.5)) * iconSize.Width * 1.5;
                        centerY = Math.Floor(ActualHeight / 2 / (iconSize.Height * 1.5)) * iconSize.Height * 1.5;
                        break;
                    case 2:
                        //  The lower-right quadrant is full, so use the whole screen. Again, pretend the screen is a virtual checkerboard,
                        //  to keep things neat

                        startX = iconSize.Width * 1.5;
                        centerY = iconSize.Height * 1.5;
                        break;
                    default:
                        //  The entire screen is full. 

                        return new Point(Math.Floor(ActualWidth / 2 / (iconSize.Width * 1.5)) * iconSize.Width * 1.5, Math.Floor(ActualHeight / 2 / (iconSize.Height * 1.5)) * iconSize.Height * 1.5);
                }

                //  And search for empty space

                double centerX = startX;

                while (centerX - iconSize.Width / 2 >= 0 && centerX + iconSize.Width / 2 <= ActualWidth &&
                        centerY - iconSize.Height / 2 >= 0 && centerY + iconSize.Height / 2 <= ActualHeight)
                {
                    // Check if this spot is available.

                    RectangleGeometry rectangle = new RectangleGeometry(new Rect(new Point(centerX - iconSize.Width / 2, centerY - iconSize.Height / 2), iconSize));
                    IEnumerable<Icons.ISelectableIcon> occludedIcons = FindOccludedIcons(rectangle);

                    if (occludedIcons.Count() == 0)
                    {
                        return new Point(centerX, centerY);
                    }

                    // Not available, check the next spot

                    centerX += iconSize.Width * 1.5;
                    if (centerX + iconSize.Width > ActualWidth)
                    {
                        centerX = startX;
                        centerY += iconSize.Height * 1.5;
                    }
                }
            }
        }

        internal Icons.ISelectableIcon FindNearestSelectedIcon(Point referencePoint)
        {
            double nearestDistance = double.MaxValue;
            Icons.ISelectableIcon nearestIcon = null;

            foreach (Icons.ISelectableIcon icon in SelectedIcons.Values)
            {
                double distance = Utility.DrawingAids.Distance(referencePoint, icon.CenterPosition);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestIcon = icon;
                }
            }

            return nearestIcon;
        }

        internal IEnumerable<T> FindOccludedIcons<T>(Point targetPoint)
        {
            //  We're going to use the point at the center of the subject icon for the test. Only icons of the designated type are returned.

            return Utility.DrawingAids.FindOccludedIcons<T>(IconSurface, targetPoint);
        }

        internal IEnumerable<Icons.ISelectableIcon> FindOccludedIcons(Geometry geometry)
        {
            List<Icons.ISelectableIcon> occludedIcons = new List<Icons.ISelectableIcon>();

            VisualTreeHelper.HitTest(IconSurface, null, new HitTestResultCallback((HitTestResult result) =>
            {
                if (result.VisualHit is FrameworkElement fe && fe.DataContext is Icons.ISelectableIcon icon && !occludedIcons.Contains(icon))
                {
                    occludedIcons.Add(icon);
                }
                return HitTestResultBehavior.Continue;
            }),
                new GeometryHitTestParameters(geometry));

            return occludedIcons;
        }

        internal Icons.TransitionIcon FindOccludedTransitionIcon(Icons.EventTypeIcon eventTypeIcon, Point referencePoint)
        {
            //  First, collect the set of transition icons which intersect with the event type icon. We'll use the entire path of
            //  the event Icon, rather than just the center point.

            Path eventPath = (eventTypeIcon.DraggableShape as IconControls.EventTypeIconControl).FindName("Path") as Path;
            Panel eventIconPanel = (eventTypeIcon.DraggableShape as IconControls.EventTypeIconControl).FindName("IconPanel") as Panel;
            Geometry normalizedEventPath;

            try
            {
                normalizedEventPath = Utility.DrawingAids.NormalizeGeometry(IconSurface, eventIconPanel, eventPath.Data);
            }
            catch
            {
                //  Early in the life of the draggable shape, it may not be connected to a visual

                return null;
            }

            List<Icons.TransitionIcon> occludedIcons = new List<Icons.TransitionIcon>();

            VisualTreeHelper.HitTest(IconSurface, null, new HitTestResultCallback((HitTestResult result) =>
                {
                    if (result.VisualHit is FrameworkElement fe && fe.DataContext is Icons.TransitionIcon transitionIcon && !occludedIcons.Contains(transitionIcon))
                    {
                        occludedIcons.Add(transitionIcon);
                    }
                    return HitTestResultBehavior.Continue;
                }), 
                new GeometryHitTestParameters(normalizedEventPath));

            //  If we didn't find more than one, we're done

            switch (occludedIcons.Count)
            {
                case 0:
                    return null;

                case 1:
                    return occludedIcons[0];

                default:
                    break;
            }

            //  Alas, there's more than one. Return the one closest to the mouse cursor

            double nearestDistance = double.MaxValue;
            Icons.TransitionIcon nearestIcon = null;

            foreach (var icon in occludedIcons)
            {
                double distance;

                if (icon.Body.IsVisible)
                {
                    Path connectorPath = icon.Body.FindName("ConnectorPath") as Path;
                    distance = Utility.DrawingAids.TangentalDistance(connectorPath.Data, referencePoint);
                }
                else if (icon.DraggableShape.IsVisible)
                {
                    distance = Utility.DrawingAids.TangentalDistance((icon.DraggableShape as Path).Data, referencePoint);
                }
                else
                {
                    throw new InvalidOperationException();
                }

                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestIcon = icon;
                }
            }

            return nearestIcon;
        }

        private void GroupMembershipChangedHandler(ViewModel.Group sender, ViewModel.Group.MembershipChangeArgument e)
        {
            switch (e.Action)
            {
                case ViewModel.Group.MembershipChangeAction.Add:
                case ViewModel.Group.MembershipChangeAction.Change:
                case ViewModel.Group.MembershipChangeAction.Remove:
                    ResolveGroupMemberVisibility(e.Endpoint);
                    break;
                case ViewModel.Group.MembershipChangeAction.AddTransition:
                    LoadViewModelIcon(e.Transition);
                    break;
                case ViewModel.Group.MembershipChangeAction.RemoveTransition:
                    UnloadViewModelIcon(e.Transition);
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        internal bool IsVisibleInLayer(ObjectModel.ILayeredPositionableObject o)
        {
            bool layerFound = false;
            bool explicitNonCurrentLayerFound = false;
            bool currentLayerIsNotGrouped = false;

            foreach (ObjectModel.LayerPosition lp in o.LayerPositions)
            {
                if (lp.Layer == CurrentLayer)
                {
                    layerFound = true;
                    if (lp.GroupStatus == LayerPosition.GroupStatuses.NotGrouped)
                    {
                        currentLayerIsNotGrouped = true;
                    }
                }
                else if (lp.GroupStatus == LayerPosition.GroupStatuses.Explicit)
                {
                    explicitNonCurrentLayerFound = true;
                }
            }

            return layerFound && (!currentLayerIsNotGrouped || !explicitNonCurrentLayerFound);
        }

        private void LoadViewModelIcon(ObjectModel.ITrackableObject trackableObject)
        {
            if (trackableObject is ViewModel.EventType eventType)
            {
                if (!LoadedIcons.ContainsKey(eventType))
                {
                    Icons.EventTypeIcon eventTypeIcon = new Icons.EventTypeIcon(this, eventType, null, eventType.LeftTopPosition);
                    IconSurface.Children.Add(eventTypeIcon.Body);
                    LoadedIcons.Add(eventType, eventTypeIcon);
                }
            }
            else if (trackableObject is ViewModel.Group group)
            {
                if (IsVisibleInLayer(group))
                {
                    if (!LoadedIcons.ContainsKey(group))
                    {
                        Icons.GroupIcon groupIcon = new Icons.GroupIcon(this, group, null, group.LeftTopPosition);
                        IconSurface.Children.Add(groupIcon.Body);
                        LoadedIcons.Add(group, groupIcon);
                    }

                    foreach (ViewModel.Transition transition in group.TransitionsFrom)
                    {
                        LoadViewModelIcon(transition);
                    }
                    foreach (ViewModel.Transition transition in group.TransitionsTo)
                    {
                        LoadViewModelIcon(transition);
                    }
                }
            }
            else if (trackableObject is ViewModel.State state)
            {
                if (IsVisibleInLayer(state))
                {
                    if (!LoadedIcons.ContainsKey(state))
                    {
                        Icons.StateIcon stateIcon = new Icons.StateIcon(this, state, null, state.LeftTopPosition);
                        IconSurface.Children.Add(stateIcon.Body);
                        LoadedIcons.Add(state, stateIcon);
                    }

                    foreach (ViewModel.Transition transition in state.TransitionsFrom)
                    {
                        LoadViewModelIcon(transition);
                    }
                    foreach (ViewModel.Transition transition in state.TransitionsTo)
                    {
                        LoadViewModelIcon(transition);
                    }
                }
            }
            else if (trackableObject is ViewModel.Transition transition)
            {
                if (IsVisibleInLayer(transition.SourceState) && IsVisibleInLayer(transition.DestinationState) &&
                    !LoadedIcons.ContainsKey(transition))
                {
                    Icons.TransitionIcon transitionIcon = new Icons.TransitionIcon(this, transition, null, null);
                    IconSurface.Children.Add(transitionIcon.Body);
                    LoadedIcons.Add(transition, transitionIcon);
                }
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        internal void LoadViewModelIcons()
        {
            Mouse.OverrideCursor = Cursors.Wait;

            IconSurface.Children.Clear();
            while (LoadedIcons.Values.Count > 0)
            {
                Icons.IconBase icon = LoadedIcons.Values.First() as Icons.IconBase;
                LoadedIcons.Remove(icon.ReferencedObject);
                icon.Remove();
            }

            if (Model.StateMachine != null)
            {
                DefaultLayer = Model.StateMachine.Layers.Where(l => l.IsDefaultLayer).Single();
                if (_currentLayer == null)
                {
                    _currentLayer = DefaultLayer;
                    _currentLayer.Members.CollectionChanged += CurrentLayerMembersCollectionChangedHandler;
                    _currentLayer.IsCurrentLayer = true;
                }

                SetCurrentLayer();

                foreach (var eventType in Model.StateMachine.EventTypes)
                {
                    LoadViewModelIcon(eventType);
                }

                foreach (var state in Model.StateMachine.States)
                {
                    LoadViewModelIcon(state);
                }

                foreach (var group in Model.StateMachine.Groups)
                {
                    LoadViewModelIcon(group);
                }

                foreach (var transition in Model.StateMachine.Transitions)
                {
                    LoadViewModelIcon(transition);
                }
            }

            Mouse.OverrideCursor = null;
        }

        internal void KeyDownHandler(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                MouseStateMachine.Cancel();
                e.Handled = true;
            }
        }

        private void LayerListBoxItemLoadedHandler(object sender, RoutedEventArgs e)
        {
            if (sender is ListBoxItem listBoxItem && listBoxItem.Content is ViewModel.Layer layer)
            {
                if (listBoxItem.IsArrangeValid)
                {
                    LayerListBoxItem_Save(listBoxItem, layer);
                }
                else
                {
                    listBoxItem.LayoutUpdated += LayerListBoxItem_LayoutUpdated;
                }
            }
        }

        private void LayerListBoxItem_LayoutUpdated(object sender, EventArgs e)
        {
            if (sender is ListBoxItem listBoxItem && listBoxItem.Content is ViewModel.Layer layer)
            {
                listBoxItem.LayoutUpdated -= LayerListBoxItem_LayoutUpdated;
                LayerListBoxItem_Save(listBoxItem, layer);
            }
        }

        private void LayerListBoxItem_Save(ListBoxItem listBoxItem, ViewModel.Layer layer)
        {
            Icons.LayerIcon layerIcon = new Icons.LayerIcon(this, layer, null, null);
            layerIcon.Body = Utility.DrawingAids.FindChildOfSpecificType<IconControls.LayerIconControl>(listBoxItem);
            if (layerIcon.Body == null)
            {
                throw new InvalidOperationException();
            }
            layerIcon.Body.DataContext = layerIcon;
        }

        private void ModelPropertyChangedHandler(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "StateMachine")
            {
                ReloadModel();
            }
        }

        private void MonitorStateMachineForChanges(bool isUnloading)
        {
            if (StateMachine != null)
            {
                StateMachine.EventTypes.CollectionChanged -= StateMachineEventTypesCollectionChangedHandler;
                StateMachine.Groups.CollectionChanged -= StateMachineGroupsCollectionChangedHandler;
                foreach (ViewModel.Group group in StateMachine.Groups)
                {
                    group.MembershipChanged -= GroupMembershipChangedHandler;
                }
                StateMachine.States.CollectionChanged -= StateMachineStatesCollectionChangedHandler;
                StateMachine.Transitions.CollectionChanged -= StateMachineTransitionsCollectionChangedHandler;
            }
            StateMachine = Model.StateMachine;
            if (StateMachine != null && !isUnloading)
            {
                StateMachine.EventTypes.CollectionChanged += StateMachineEventTypesCollectionChangedHandler;
                StateMachine.Groups.CollectionChanged += StateMachineGroupsCollectionChangedHandler;
                foreach (ViewModel.Group group in StateMachine.Groups)
                {
                    group.MembershipChanged += GroupMembershipChangedHandler;
                }
                StateMachine.States.CollectionChanged += StateMachineStatesCollectionChangedHandler;
                StateMachine.Transitions.CollectionChanged += StateMachineTransitionsCollectionChangedHandler;
            }
        }

        internal void MouseDoubleClickHandler(Icons.ISelectableIcon selectedIcon)
        {
            if (selectedIcon is Icons.GroupIcon groupIcon)
            {
                SelectLayer((groupIcon.ReferencedObject as ViewModel.Group).Layer);
            }
        }

        internal void MouseEnterHandler(object sender, MouseEventArgs e)
        {
            if (OldKeyboardFocus == IconSurface)
            {
                Keyboard.Focus(IconSurface);
            }
        }

        internal void MouseLeaveHandler(object sender, MouseEventArgs e)
        {
            OldKeyboardFocus = Keyboard.FocusedElement;
            if (OldKeyboardFocus == IconSurface)
            {
                Keyboard.ClearFocus();
            }
        }

        private void MouseLeftButtonDownHandler(object sender, MouseButtonEventArgs e)
        {
            MouseStateMachine.MouseLeftButtonDown(e.GetPosition(IconSurface));
            e.Handled = true;
        }

        private void MouseLeftButtonUpHandler(object sender, MouseButtonEventArgs e)
        {
            MouseStateMachine.MouseLeftButtonUp(e.GetPosition(IconSurface));
            e.Handled = true;
        }

        internal void MouseMoveHandler(object sender, MouseEventArgs e)
        {
            MouseStateMachine.MouseMove(e.GetPosition(IconSurface));
            e.Handled = true;
        }

        internal void MouseRightButtonDownHandler(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            MouseStateMachine.MouseRightButtonDown(e.GetPosition(IconSurface));
            e.Handled = true;
        }

        internal void MouseRightButtonUpHandler(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            MouseStateMachine.MouseRightButtonUp(e.GetPosition(IconSurface));
            e.Handled = true;
        }

        public int QueryStatus(ref Guid pguidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText)
        {
            if (pguidCmdGroup == PackageGuids.guidSimpleStateMachineEditorPackageCmdSet)
            {
                for (int i = 0; i < cCmds; i++)
                {
                    switch (prgCmds[i].cmdID)
                    {
                        case PackageIds.AddEventTypeCommandId:
                        case PackageIds.AddGroupCommandId:
                        case PackageIds.AddStateCommandId:
                            prgCmds[i].cmdf = (uint)(OLECMDF.OLECMDF_SUPPORTED);
                            if (Model.StateMachine != null)
                            {
                                prgCmds[i].cmdf = prgCmds[i].cmdf | (uint)(OLECMDF.OLECMDF_ENABLED);
                            }
                            break;
                        default:
                            prgCmds[i].cmdf = 0;
                            break;
                    }
                }
                return VSConstants.S_OK;
            }
            else
            {
                return (int)Microsoft.VisualStudio.OLE.Interop.Constants.OLECMDERR_E_UNKNOWNGROUP;
            }
        }

        private void ReloadModel()
        {
            ClearSelectedItems();
            IconSurface.Children.Clear();
            MonitorStateMachineForChanges(false);

            Package.ErrorList.Clear();

            if (Model.StateMachine == null)
            {
                Package.ErrorList.WriteVisualStudioErrorList(Utility.ErrorList.MessageCategory.Severe, Model.ErrorMessage, Model.FileName, 1, 1);
            }
            else
            {
                if (CurrentLayer != null)
                {
                    CurrentLayer = Model.StateMachine.Find(CurrentLayer.Id) as ViewModel.Layer;
                }

                Icons.ISelectableIcon[] previouslySelectedIcons = SelectedIcons.Values.ToArray();

                LoadViewModelIcons();

                if (previouslySelectedIcons.Length == 0)
                {
                    SelectStateMachine();
                }
                else
                {
                    foreach (var oldIcon in previouslySelectedIcons)
                    {
                        ObjectModel.TrackableObject trackableObject = Model.StateMachine.States.Where(s => s.Id == oldIcon.ReferencedObject.Id).FirstOrDefault();
                        if (trackableObject == null)
                        {
                            trackableObject = Model.StateMachine.EventTypes.Where(e => e.Id == oldIcon.ReferencedObject.Id).FirstOrDefault();
                        }

                        if (trackableObject != null && LoadedIcons.ContainsKey(trackableObject))
                        {
                            SelectedObjects.Add(trackableObject);
                            SelectedIcons.Add(trackableObject, LoadedIcons[trackableObject]);
                            LoadedIcons[trackableObject].IsSelectedChanged();
                        }
                    }
                }
            }
        }

        internal void RemoveLayerMember(ViewModel.Layer layer, ObjectModel.ITransitionEndpoint member)
        {
            using (Model.CreateAtomicGuiChangeBlock("Remove layer member"))
            {
                ObjectModel.LayerPosition layerPosition = member.LayerPositions.Where(lp => lp.Layer == layer).Single();
                switch (layerPosition.GroupStatus)
                {
                    case LayerPosition.GroupStatuses.Explicit:
                        layerPosition.GroupStatus = LayerPosition.GroupStatuses.Implicit;
                        ObjectModel.ITransitionEndpoint[] implicitEndpoints = layer.Members.Where(m => m.LayerPositions.Any(lp => lp.Layer == layer && lp.GroupStatus == LayerPosition.GroupStatuses.Implicit)).ToArray();
                        foreach (ObjectModel.ITransitionEndpoint endpoint in implicitEndpoints)
                        {
                            endpoint.IsValid = false;
                        }
                        foreach (ObjectModel.ITransitionEndpoint endpoint in layer.Members.Where(m => m.LayerPositions.Any(lp => lp.Layer == layer && lp.GroupStatus == LayerPosition.GroupStatuses.Explicit)))
                        {
                            foreach (ObjectModel.ITransition transition in endpoint.TransitionsFrom)
                            {
                                transition.DestinationState.IsValid = true;
                            }
                            foreach (ObjectModel.ITransition transition in endpoint.TransitionsTo)
                            {
                                transition.SourceState.IsValid = true;
                            }
                        }
                        foreach (ObjectModel.ITransitionEndpoint endpoint in implicitEndpoints.Where(m => !m.IsValid))
                        {
                            RemoveLayerPosition(endpoint, endpoint.LayerPositions.Where(lp => lp.Layer == layer).Single());
                        }
                        break;
                    case LayerPosition.GroupStatuses.Implicit:
                        break;
                    case LayerPosition.GroupStatuses.NotGrouped:
                        RemoveLayerPosition(member, layerPosition);
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
        }

        private void RemoveLayerPosition(ObjectModel.ITransitionEndpoint endpoint, ObjectModel.LayerPosition layerPosition)
        {
            Model.LogUndoAction(new UndoRedo.AddLayerMemberRecord(Model, layerPosition, endpoint));
            endpoint.LayerPositions.Remove(layerPosition);
            layerPosition.Layer.Members.Remove(endpoint);
            layerPosition.Remove();
        }

        private void ResolveGroupMemberVisibility(ObjectModel.ITransitionEndpoint endpoint)
        {
            if (LoadedIcons.ContainsKey(endpoint))
            {
                if (!IsVisibleInLayer(endpoint))
                {
                    DeselectIcon(LoadedIcons[endpoint]);
                    UnloadGroupedViewModelIcons(endpoint);
                }
            }
            else if (IsVisibleInLayer(endpoint))
            {
                LoadViewModelIcon(endpoint);
            }
        }

        internal void SelectableIconMouseLeftButtonDownHandler(System.Windows.Point dragOrigin, Icons.ISelectableIcon icon)
        {
            MouseStateMachine.MouseLeftButtonDownOnIcon(dragOrigin, icon);
        }

        internal void SelectableIconMouseRightButtonDownHandler(System.Windows.Point dragOrigin, Icons.ISelectableIcon icon)
        {
            MouseStateMachine.MouseRightButtonDownOnIcon(dragOrigin, icon);
        }

        internal void SelectableIconMouseRightButtonUpHandler(System.Windows.Point dragOrigin, Icons.ISelectableIcon icon)
        {
            MouseStateMachine.MouseRightButtonUpOnIcon(dragOrigin, icon);
        }

        internal void SelectIcon(Icons.ISelectableIcon icon)
        {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();

            if (SelectedObjects.Count == 1 && (SelectedObjects[0] == Model.StateMachine || !(icon is Icons.PositionableIcon) || !(SelectedIcons.Values.First() is Icons.PositionableIcon)))
            {
                ClearSelectedItems();
            }

            if (!SelectedObjects.Contains(icon.ReferencedObject))
            {
                SelectedObjects.Add(icon.ReferencedObject);
                SelectedIcons.Add(icon.ReferencedObject, icon);
                icon.IsSelectedChanged();
                SelectionTracker.OnSelectChange(SelectionContainer);
            }
        }

        internal void SelectLayer(ViewModel.Layer layer)
        {
            using (new UndoRedo.AtomicBlock(Model, "Set layer"))
            {
                CurrentLayer = layer;
            }
        }

        internal void SelectOccludedIcons(Point dragOrigin, Point mousePosition)
        {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();

            //  Collect the set of icons in the selection box

            List<Icons.ISelectableIcon> selectedIcons = new List<Icons.ISelectableIcon>();

            foreach (Icons.ISelectableIcon draggableIcon in LoadedIcons.Values)
            {
                if (draggableIcon.ReferencedObject is ObjectModel.IPositionableObject positionableObject &&
                    Utility.DrawingAids.Occludes(dragOrigin, mousePosition, positionableObject.LeftTopPosition, new Point((draggableIcon as Icons.IconBase).Right, (draggableIcon as Icons.IconBase).Bottom)))
                {
                    selectedIcons.Add(draggableIcon);
                }
            }

            //  If the shift key is pressed, we're adding these to the selection. Otherwise, we're replacing the selection

            if (!Keyboard.IsKeyDown(Key.LeftShift) && !Keyboard.IsKeyDown(Key.RightShift))
            {
                ClearSelectedItems();
                if (selectedIcons.Count == 0)
                {
                    SelectStateMachine();
                }
            }

            if (selectedIcons.Count > 0)
            {
                foreach (Icons.ISelectableIcon icon in selectedIcons)
                {
                    if (!SelectedIcons.ContainsKey(icon.ReferencedObject))
                    {
                        (SelectionContainer.SelectableObjects as List<ObjectModel.ITrackableObject>).Add(icon.ReferencedObject);
                        SelectedIcons.Add(icon.ReferencedObject, icon);
                        icon.IsSelectedChanged();
                    }
                }
            }

            SelectionTracker.OnSelectChange(SelectionContainer);
        }

        internal void SelectSingle(Icons.ISelectableIcon icon)
        {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();

            if (SelectedObjects.Count != 1 || SelectedObjects[0] != icon.ReferencedObject)
            {
                ClearSelectedItems();
                (SelectionContainer.SelectableObjects as List<ObjectModel.ITrackableObject>).Add(icon.ReferencedObject);
                SelectedIcons.Add(icon.ReferencedObject, icon);
                icon.IsSelectedChanged();
                SelectionTracker.OnSelectChange(SelectionContainer);
            }
        }

        internal void SelectStateMachine()
        {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();

            if (SelectedObjects.Count != 1 || SelectedObjects[0] != Model.StateMachine)
            {
                ClearSelectedItems();
                (SelectionContainer.SelectableObjects as List<ObjectModel.ITrackableObject>).Add(Model.StateMachine);
                SelectionTracker.OnSelectChange(SelectionContainer);
            }
        }

        internal void SelectStateMachineHandler(object sender, MouseButtonEventArgs e)
        {
            SelectStateMachine();
        }

        internal void SetStartState(ViewModel.State state)
        {
            using (new ViewModel.ViewModelController.GuiChangeBlock(Model))
            {
                Model.StateMachine.StartState = state;
            }
        }

        internal void SortSelectedIcons(Icons.ISelectableIcon draggableIcon)
        {
            Icons.ISelectableIcon[] selectedIcons = SelectedIcons.Values.Where(i => i.ReferencedObject is ObjectModel.NamedObject).OrderBy(i => (i.ReferencedObject as ObjectModel.NamedObject).Name).ToArray();
            Point[] iconPositions = selectedIcons.Select(i => i.CenterPosition).OrderBy(p => p.Y).ThenBy(p => p.X).ToArray();

            using (Model.CreateAtomicGuiChangeBlock("Sort icons"))
            {
                for (int i = selectedIcons.GetLowerBound(0); i <= selectedIcons.GetUpperBound(0); i++)
                {
                    selectedIcons[i].CenterPosition = iconPositions[i];
                }
            }
        }

        private void StateMachineEventTypesCollectionChangedHandler(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (ViewModel.EventType newEventType in e.NewItems)
                    {
                        LoadViewModelIcon(newEventType);
                        ClearSelectedItems();
                        SelectIcon(LoadedIcons[newEventType]);
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (ViewModel.EventType eventType in e.OldItems)
                    {
                        UnloadViewModelIcon(eventType);
                    }
                    break;
                default:
                    break;
            }
        }

        private void StateMachineGroupsCollectionChangedHandler(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (ViewModel.Group newGroup in e.NewItems)
                    {
                        newGroup.MembershipChanged += GroupMembershipChangedHandler;
                        if (CurrentLayer.Members.Contains(newGroup))
                        {
                            LoadViewModelIcon(newGroup);
                            ClearSelectedItems();
                            SelectIcon(LoadedIcons[newGroup]);
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (ViewModel.Group group in e.OldItems)
                    {
                        group.MembershipChanged -= GroupMembershipChangedHandler;
                        UnloadViewModelIcon(group);
                    }
                    break;
                default:
                    break;
            }
        }

        private void StateMachineLayersCollectionChangedHandler(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
#if false
                    foreach (ViewModel.Layer newLayer in e.NewItems)
                    {
                        Icons.LayerIcon layerIcon = new Icons.LayerIcon(this, newLayer, null, null);
                        ClearSelectedItems();
                        SelectIcon(layerIcon);
                    }
#endif
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (ViewModel.Layer layer in e.OldItems)
                    {
                        if (layer == CurrentLayer)
                        {
                            CurrentLayer = DefaultLayer;
                        }
                        DeselectIcon(LoadedIcons[layer]);
                    }
                    break;
                default:
                    break;
            }
        }

        private void StateMachineStatesCollectionChangedHandler(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (ViewModel.State newState in e.NewItems)
                    {
                        if (CurrentLayer.Members.Contains(newState))
                        {
                            LoadViewModelIcon(newState);
                            ClearSelectedItems();
                            SelectIcon(LoadedIcons[newState]);
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (ViewModel.State state in e.OldItems)
                    {
                        UnloadViewModelIcon(state);
                    }
                    break;
                default:
                    break;
            }
        }

        private void StateMachineTransitionsCollectionChangedHandler(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (ViewModel.Transition newTransition in e.NewItems)
                    {
                        LoadViewModelIcon(newTransition);
                        if (LoadedIcons.ContainsKey(newTransition))
                        {
                            ClearSelectedItems();
                            SelectIcon(LoadedIcons[newTransition]);
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (ViewModel.Transition transition in e.OldItems)
                    {
                        UnloadViewModelIcon(transition);
                    }
                    break;
                default:
                    break;
            }
        }

        private void UnloadGroupedViewModelIcons(ObjectModel.ITransitionEndpoint transitionHost)
        {
            UnloadViewModelIcon(transitionHost);
            foreach (ObjectModel.ITransition transition in transitionHost.TransitionsFrom)
            {
                UnloadViewModelIcon(transition);
            }
            foreach (ObjectModel.ITransition transition in transitionHost.TransitionsTo)
            {
                UnloadViewModelIcon(transition);
            }
        }

        private void UnloadViewModelIcon(ObjectModel.ITrackableObject trackableObject)
        {
            if (LoadedIcons.ContainsKey(trackableObject))
            {
                Icons.ISelectableIcon icon = LoadedIcons[trackableObject];
                DeselectIcon(icon);
                IconSurface.Children.Remove(icon.Body);
                icon.Remove();
                LoadedIcons.Remove(trackableObject);
            }
        }

        protected void ZoomSliderDoubleClickHandler(object sender, MouseButtonEventArgs e)
        {
            ZoomSlider.Value = 1;
        }

        protected void ZoomSliderMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (ZoomSlider != null)
            {
                //  We desire the zoom factor to change about 10% for each click of the mousewheel. Microsoft has
                //  decided that their mouse delta will be 120 for each click -- so normalize the clicks by this quantity 
                //  to get the desired behavior. Why they don't have a constant for this value is beyond me.

                double cannonicalDelta = 120;
                double percentChange = (e.Delta * 10) / cannonicalDelta;
                double currentZoomFactor = ZoomSlider.Value;
                double newZoomFactor = Math.Max(ZoomSlider.Minimum, Math.Min(ZoomSlider.Maximum, currentZoomFactor + percentChange / 100));
                ZoomSlider.Value = newZoomFactor;
            }
        }

        private void SetCurrentLayer()
        {
            foreach (ViewModel.Group group in Model.StateMachine.Groups)
            {
                group.CurrentLayer = CurrentLayer;
            }
            foreach (ViewModel.State state in Model.StateMachine.States)
            {
                state.CurrentLayer = CurrentLayer;
            }
        }
    }
}
