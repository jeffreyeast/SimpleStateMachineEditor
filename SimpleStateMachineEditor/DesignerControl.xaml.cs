using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;
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
        const string AddLayerDescription = "Add layer";
        const string AddStateDescription = "Add state";
        const string AddTransitionDescription = "Add transition";
        const string ChangeTransitionDestinationDescription = "Change transition end state";
        const string ChangeTransitionSourceDescription = "Change transition start state";

        public ViewModel.ViewModelController Model { get; set; }
        ViewModel.StateMachine StateMachine;
        internal IVsUIShell UiShell;
        ITrackSelection SelectionTracker;
        internal List<ObjectModel.TrackableObject> SelectedObjects;
        internal Dictionary<ObjectModel.TrackableObject, Icons.ISelectableIcon> SelectedIcons;
        internal Dictionary<ObjectModel.TrackableObject, Icons.ISelectableIcon> LoadedIcons;
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
                    }
                    ClearSelectedItems();
                    SetCurrentLayer();
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
            SelectedObjects = new List<ObjectModel.TrackableObject>();
            LoadedIcons = new Dictionary<ObjectModel.TrackableObject, Icons.ISelectableIcon>();
            SelectedIcons = new Dictionary<ObjectModel.TrackableObject, Icons.ISelectableIcon>();
            SelectionContainer = new Microsoft.VisualStudio.Shell.SelectionContainer(true, false);
            SelectionContainer.SelectableObjects = SelectedObjects;
            SelectionContainer.SelectedObjects = SelectedObjects;
            SelectionBoxIcon = new Icons.SelectionBoxIcon(this, null, null);

            InitializeComponent();

            DataContext = Model.StateMachine;
            SelectionTracker = selectionTracker;
            Model.PropertyChanged += ModelPropertyChangedHandler;

            Loaded += DesignerControl_Loaded;
            Unloaded += DesignerControl_Unloaded;
        }

        internal void AddEventType(Point? center = null)
        {
            if (Model.StateMachine.IsChangeAllowed)
            {
                using (new UndoRedo.AtomicBlock(Model, AddEventTypeDescription))
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

                Model.StateMachine.EndChange();
            }
        }

        private void AddLayer(object sender, RoutedEventArgs e)
        {
            if (Model.StateMachine.IsChangeAllowed)
            {
                using (new UndoRedo.AtomicBlock(Model, AddLayerDescription))
                {
                    ViewModel.Layer newLayer = ViewModel.Layer.Create(Model, OptionsPage, false);
                    Model.LogUndoAction(new UndoRedo.DeleteLayerRecord(Model, newLayer));
                    Model.StateMachine.Layers.Add(newLayer);
                }

                Model.StateMachine.EndChange();
            }
        }

        internal void AddLayerMember(ViewModel.Layer layer, ObjectModel.LayeredPositionableObject newMember, Point? position = null)
        {
            if (Model.StateMachine.IsChangeAllowed)
            {
                using (new UndoRedo.AtomicBlock(Model, "Add layer member"))
                {
                    Model.LogUndoAction(new UndoRedo.DeleteLayerMemberRecord(Model, layer, newMember));
                    ObjectModel.LayerPosition layerPosition = ObjectModel.LayerPosition.Create(Model, layer);
                    if (position.HasValue)
                    {
                        layerPosition.LeftTopPosition = position.Value;
                    }
                    else
                    {
                        layerPosition.LeftTopPosition = newMember.LeftTopPosition;
                    }
                    newMember.LayerPositions.Add(layerPosition);
                    layer.Members.Add(newMember);
                }

                Model.StateMachine.EndChange();
            }
        }

        internal void AddState(Point? center = null)
        {
            if (Model.StateMachine.IsChangeAllowed)
            {
                using (new UndoRedo.AtomicBlock(Model, AddStateDescription))
                {
                    ViewModel.State newState = ViewModel.State.Create(Model, OptionsPage, CurrentLayer);
                    Model.LogUndoAction(new UndoRedo.DeleteStateRecord(Model, newState));
                    if (!center.HasValue)
                    {
                        center = FindEmptySpace(Icons.StateIcon.IconSize);
                    }
                    Point position = new Point(center.Value.X - Icons.StateIcon.IconSize.Width / 2, center.Value.Y - Icons.StateIcon.IconSize.Height / 2);
                    AddLayerMember(DefaultLayer, newState, position);
                    if (CurrentLayer != DefaultLayer)
                    {
                        AddLayerMember(CurrentLayer, newState, position);
                    }

                    Model.StateMachine.States.Add(newState);
                }

                Model.StateMachine.EndChange();
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
                if (Model.StateMachine.IsChangeAllowed)
                {
                    DefaultLayer = ViewModel.Layer.Create(Model, OptionsPage, true);
                    Model.StateMachine.Layers.Add(DefaultLayer);

                    foreach (ViewModel.State state in Model.StateMachine.States)
                    {
                        DefaultLayer.Members.Add(state);
                        state.CurrentLayer = DefaultLayer;
                        ObjectModel.LayerPosition layerPosition = ObjectModel.LayerPosition.Create(Model, DefaultLayer);
                        layerPosition.LeftTopPosition = state.LegacyLeftTopPosition;
                        state.LayerPositions.Add(layerPosition);
                    }

                    Model.StateMachine.EndChange();
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

            if (transition.IsChangeAllowed)
            {
                CurrentParentUndoUnit.GetDescription(out string operationDescription);

                switch (operationDescription)
                {
                    case AddTransitionDescription:
                        transition.DestinationState = nearestState;
                        Model.StateMachine.Transitions.Add(transition);
                        Model.LogUndoAction(new UndoRedo.DeleteTransitionRecord(Model, transition));
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

                transition.EndChange();
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
            if (Model.StateMachine.IsChangeAllowed)
            {
                using (new UndoRedo.AtomicBlock(Model, "Remove action"))
                {
                    transition.ActionReferences.RemoveAt(slot);
                    Model.LogUndoAction(new UndoRedo.AddActionReferenceRecord(Model, actionReference, slot));
                }
                Model.StateMachine.EndChange();
            }
        }

        private void  DeleteEventType(ViewModel.EventType eventType)
        {
            if (Model.StateMachine.IsChangeAllowed)
            {
                using (new UndoRedo.AtomicBlock(Model, "Remove event type"))
                {
                    foreach (ViewModel.Transition t in Model.StateMachine.Transitions)
                    {
                        if (t.TriggerEvent == eventType)
                        {
                            t.TriggerEvent = null;
                        }
                    }
                    eventType.Remove();
                    Model.StateMachine.EventTypes.Remove(eventType);
                    Model.LogUndoAction(new UndoRedo.AddEventTypeRecord(Model, eventType));
                }
                Model.StateMachine.EndChange();
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
            if (Model.StateMachine.IsChangeAllowed)
            {
                using (new UndoRedo.AtomicBlock(Model, "Remove layer"))
                {
                    foreach (ViewModel.State state in Model.StateMachine.States)
                    {
                        ObjectModel.LayerPosition layerPosition;

                        while ((layerPosition = state.LayerPositions.Where(lp => lp.Layer == layer).FirstOrDefault()) != null)
                        {
                            layerPosition.Remove();
                            state.LayerPositions.Remove(layerPosition);
                            Model.LogUndoAction(new UndoRedo.AddLayerMemberRecord(Model, layerPosition.Layer, state));
                        }
                    }
                    layer.Remove();
                    Model.StateMachine.Layers.Remove(layer);
                    Model.LogUndoAction(new UndoRedo.AddLayerRecord(Model, layer));
                }
                Model.StateMachine.EndChange();
            }
        }

        private void DeleteState(ViewModel.State state)
        {
            if (Model.StateMachine.IsChangeAllowed)
            {
                using (new UndoRedo.AtomicBlock(Model, "Remove state"))
                {
                    while (state.TransitionsFrom.Count > 0)
                    {
                        DeleteTransition(state.TransitionsFrom.First());
                    }
                    while (state.TransitionsTo.Count > 0)
                    {
                        DeleteTransition(state.TransitionsTo.First());
                    }
                    foreach (ObjectModel.LayerPosition layerPosition in state.LayerPositions)
                    {
                        Model.LogUndoAction(new UndoRedo.AddLayerMemberRecord(Model, layerPosition.Layer, state));
                    }
                    state.Remove();
                    Model.StateMachine.States.Remove(state);
                    Model.LogUndoAction(new UndoRedo.AddStateRecord(Model, state));
                }
                Model.StateMachine.EndChange();
            }
        }

        private void DeleteTransition(ViewModel.Transition transition)
        {
            if (Model.StateMachine.IsChangeAllowed)
            {
                using (new UndoRedo.AtomicBlock(Model, "Remove transition"))
                {
                    transition.Remove();
                    Model.StateMachine.Transitions.Remove(transition);
                    Model.LogUndoAction(new UndoRedo.AddTransitionRecord(Model, transition));
                }
                Model.StateMachine.EndChange();
            }
        }

        internal void DeleteSelectedIcons()
        {
            Icons.ISelectableIcon[] pendingDeletes = SelectedIcons.Values.ToArray();

            using (new UndoRedo.AtomicBlock(Model, "Delete"))
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
            MonitorStateMachineForChanges(true);
        }

        internal bool DoIdle(IVsTextLines textBuffer)
        {
            return Model.DoIdle(textBuffer);
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

        private bool IsSelectionNull => SelectedIcons.Count == 0;

        private void LoadViewModelIcon(ObjectModel.TrackableObject trackableObject)
        {
            if (!LoadedIcons.ContainsKey(trackableObject))
            {
                if (trackableObject is ViewModel.EventType eventType)
                {
                    Icons.EventTypeIcon eventTypeIcon = new Icons.EventTypeIcon(this, eventType, null, eventType.LeftTopPosition);
                    IconSurface.Children.Add(eventTypeIcon.Body);
                    LoadedIcons.Add(eventType, eventTypeIcon);
                }
                else if (trackableObject is ViewModel.State state)
                {
                    if (CurrentLayer.Members.Contains(state))
                    {
                        Icons.StateIcon stateIcon = new Icons.StateIcon(this, state, null, state.LeftTopPosition);
                        IconSurface.Children.Add(stateIcon.Body);
                        LoadedIcons.Add(state, stateIcon);

                        foreach (ViewModel.Transition transition in state.TransitionsFrom)
                        {
                            LoadViewModelIcon(trackableObject);
                        }
                        foreach (ViewModel.Transition transition in state.TransitionsTo)
                        {
                            LoadViewModelIcon(trackableObject);
                        }
                    }
                }
                else if (trackableObject is ViewModel.Transition transition)
                {
                    if (CurrentLayer.Members.Contains(transition.SourceState) && CurrentLayer.Members.Contains(transition.DestinationState))
                    {
                        Icons.TransitionIcon transitionIcon = new Icons.TransitionIcon(this, transition, null, null);
                        IconSurface.Children.Add(transitionIcon.Body);
                        LoadedIcons.Add(transition, transitionIcon);
                    }
                }
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
                if (Model.StateMachine.Layers.Count == 0)
                {
                    BuildDefaultLayer();
                }

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
                Icons.LayerIcon layerIcon = new Icons.LayerIcon(this, layer, null, null);
                layerIcon.Body = Utility.DrawingAids.FindChildOfSpecificType<IconControls.LayerIconControl>(listBoxItem);
                layerIcon.Body.DataContext = layerIcon;
            }
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
                StateMachine.States.CollectionChanged -= StateMachineStatesCollectionChangedHandler;
                StateMachine.Transitions.CollectionChanged -= StateMachineTransitionsCollectionChangedHandler;
            }
            StateMachine = Model.StateMachine;
            if (StateMachine != null && !isUnloading)
            {
                StateMachine.EventTypes.CollectionChanged += StateMachineEventTypesCollectionChangedHandler;
                StateMachine.States.CollectionChanged += StateMachineStatesCollectionChangedHandler;
                StateMachine.Transitions.CollectionChanged += StateMachineTransitionsCollectionChangedHandler;
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
            return VSConstants.S_OK;
        }

        private void ReloadModel()
        {
            ClearSelectedItems();
            IconSurface.Children.Clear();
            MonitorStateMachineForChanges(false);

            if (Model.StateMachine != null)
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

        internal void RemoveLayerMember(ViewModel.Layer layer, ObjectModel.LayeredPositionableObject member)
        {
            if (layer != DefaultLayer)
            {
                if (Model.StateMachine.IsChangeAllowed)
                {
                    using (new UndoRedo.AtomicBlock(Model, "Remove layer member"))
                    {
                        Model.LogUndoAction(new UndoRedo.AddLayerMemberRecord(Model, layer, member));
                        layer.Members.Remove(member);
                        ObjectModel.LayerPosition layerPosition = member.LayerPositions.Where(lp => lp.Layer == layer).Single();
                        member.LayerPositions.Remove(layerPosition);
                    }
                    Model.StateMachine.EndChange();
                }
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
                        (SelectionContainer.SelectableObjects as List<ObjectModel.TrackableObject>).Add(icon.ReferencedObject);
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
                (SelectionContainer.SelectableObjects as List<ObjectModel.TrackableObject>).Add(icon.ReferencedObject);
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
                (SelectionContainer.SelectableObjects as List<ObjectModel.TrackableObject>).Add(Model.StateMachine);
                SelectionTracker.OnSelectChange(SelectionContainer);
            }
        }

        internal void SelectStateMachineHandler(object sender, MouseButtonEventArgs e)
        {
            SelectStateMachine();
        }

        internal void SetStartState(ViewModel.State state)
        {
            if (Model.StateMachine.IsChangeAllowed)
            {
                Model.StateMachine.StartState = state;
                Model.StateMachine.EndChange();
            }
        }

        internal void SortSelectedIcons(Icons.ISelectableIcon draggableIcon)
        {
            Icons.ISelectableIcon[] selectedIcons = SelectedIcons.Values.Where(i => i.ReferencedObject is ObjectModel.NamedObject).OrderBy(i => (i.ReferencedObject as ObjectModel.NamedObject).Name).ToArray();
            Point[] iconPositions = selectedIcons.Select(i => i.CenterPosition).OrderBy(p => p.Y).ThenBy(p => p.X).ToArray();

            if (Model.StateMachine.IsChangeAllowed)
            {
                using (new UndoRedo.AtomicBlock(Model, "Sort icons"))
                {
                    for (int i = selectedIcons.GetLowerBound(0); i <= selectedIcons.GetUpperBound(0); i++)
                    {
                        selectedIcons[i].CenterPosition = iconPositions[i];
                    }
                }
                Model.StateMachine.EndChange();
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

        private void StateMachineLayersCollectionChangedHandler(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (ViewModel.Layer newLayer in e.NewItems)
                    {
                        Icons.LayerIcon layerIcon = new Icons.LayerIcon(this, newLayer, null, null);
                        ClearSelectedItems();
                        SelectIcon(layerIcon);
                    }
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

        private void UnloadViewModelIcon(ObjectModel.TrackableObject trackableObject)
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
            foreach (ViewModel.State state in Model.StateMachine.States)
            {
                state.CurrentLayer = CurrentLayer;
            }
        }
    }
}
