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
        const string AddRegionDescription = "Add region type";
        const string AddStateDescription = "Add state";
        const string AddTransitionDescription = "Add transition";
        const string ChangeTransitionDestinationDescription = "Change transition end state";
        const string ChangeTransitionSourceDescription = "Change transition start state";

        internal ViewModel.ViewModelController Model { get; set; }
        ViewModel.StateMachine StateMachine;
        internal IVsUIShell UiShell;
        ITrackSelection SelectionTracker;
        internal List<ObjectModel.TrackableObject> SelectedObjects;
        internal Dictionary<ObjectModel.TrackableObject, Icons.IDraggableIcon> SelectedIcons;
        internal Dictionary<ObjectModel.TrackableObject, Icons.IDraggableIcon> LoadedIcons;
        Microsoft.VisualStudio.Shell.SelectionContainer SelectionContainer;
        public SimpleStateMachineEditorPackage Package;
        internal System.Windows.Point ContextMenuActivationLocation;
        internal Icons.SelectionBoxIcon SelectionBoxIcon;
        internal MouseStateMachine.DesignerMouseSelectionImplementation MouseStateMachine;
        IInputElement OldKeyboardFocus;
        public IconControls.OptionsPropertiesPage OptionsPage { get; private set; }
        IOleParentUndoUnit CurrentParentUndoUnit;





        internal DesignerControl(SimpleStateMachineEditorPackage package, ViewModel.ViewModelController model, ITrackSelection selectionTracker)
        {
            Package = package;
            UiShell = ((System.IServiceProvider)package).GetService(typeof(SVsUIShell)) as IVsUIShell;
            OptionsPage = Package.OptionsPropertiesPage;
            Model = model;
            SelectedObjects = new List<ObjectModel.TrackableObject>();
            LoadedIcons = new Dictionary<ObjectModel.TrackableObject, Icons.IDraggableIcon>();
            SelectedIcons = new Dictionary<ObjectModel.TrackableObject, Icons.IDraggableIcon>();
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
                    Model.UndoManager.Add(new UndoRedo.DeleteEventTypeRecord(Model, newEventType));
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

        internal void AddRegion(Point? center = null)
        {
            if (Model.StateMachine.IsChangeAllowed)
            {
                using (new UndoRedo.AtomicBlock(Model, AddRegionDescription))
                {
                    ViewModel.Region newRegion = ViewModel.Region.Create(Model, OptionsPage);
                    Model.UndoManager.Add(new UndoRedo.DeleteRegionRecord(Model, newRegion));
                    if (!center.HasValue)
                    {
                        center = FindEmptySpace(Icons.StateIcon.IconSize);
                    }
                    newRegion.LeftTopPosition = new Point(center.Value.X - Icons.RegionIcon.IconSize.Width / 2, center.Value.Y - Icons.RegionIcon.IconSize.Height / 2);
                    Model.StateMachine.Regions.Add(newRegion);
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
                    ViewModel.State newState = ViewModel.State.Create(Model, OptionsPage);
                    Model.UndoManager.Add(new UndoRedo.DeleteStateRecord(Model, newState));
                    if (!center.HasValue)
                    {
                        center = FindEmptySpace(Icons.StateIcon.IconSize);
                    }
                    newState.LeftTopPosition = new Point(center.Value.X - Icons.StateIcon.IconSize.Width / 2, center.Value.Y - Icons.StateIcon.IconSize.Height / 2);

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

        internal void CancelTransactionDrag(ViewModel.Transition transition)
        {
            if (LoadedIcons.ContainsKey(transition))
            {
                IconSurface.Children.Add(LoadedIcons[transition].Body);
            }
            else
            {
                transition.OnRemoving();
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

                // We're dragging the transition icon, so the user can bind it to the destination state

                MouseStateMachine.StartDraggingTransition(transitionIcon);
            }
        }

        internal void ClearSelectedItems()
        {
            Icons.IDraggableIcon[] previouslySelectedIcons = SelectedIcons.Values.ToArray();
            SelectedObjects.Clear();
            SelectedIcons.Clear();
            foreach (Icons.IDraggableIcon icon in previouslySelectedIcons)
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
                Icons.StateIcon stateIcon = LoadedIcons[state] as Icons.StateIcon;
                double distance = Utility.DrawingAids.Distance(dragTerminationPoint, stateIcon.CenterPosition);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestState = state;
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
                        Model.UndoManager.Add(new UndoRedo.DeleteTransitionRecord(Model, transition));
                        transition.DestinationState = nearestState;
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
                        //  UI model would be to refuse the bind, but we've already terminated the drag operation, so that's not
                        //  feasible.

                        if (transition.SourceState.HasTransitionMatchingTrigger(transition.TriggerEvent))
                        {
                            transition.TriggerEvent = null;
                        }
                        break;

                    default:
                        throw new NotImplementedException();
                }

                Model.UndoManager.Close(CurrentParentUndoUnit, 1);
                SelectSingle(LoadedIcons[transition]);

                transition.EndChange();
            }
        }

        internal void DeleteIcon(Icons.IDraggableIcon icon)
        {
            using (new UndoRedo.AtomicBlock(Model, "Delete"))
            {
                if (icon.ReferencedObject is ViewModel.State state && Model.StateMachine.States.Contains(state) && Model.StateMachine.IsChangeAllowed)
                {
                    ViewModel.Transition[] transitions = state.TransitionsFrom.ToArray();
                    foreach (ViewModel.Transition t in transitions)
                    {
                        DeleteIcon(LoadedIcons[t]);
                    }
                    transitions = state.TransitionsTo.ToArray();
                    foreach (ViewModel.Transition t in transitions)
                    {
                        DeleteIcon(LoadedIcons[t]);
                    }
                    Model.StateMachine.States.Remove(state);
                    Model.UndoManager.Add(new UndoRedo.AddStateRecord (Model, state));
                    Model.StateMachine.EndChange();
                }
                else if (icon.ReferencedObject is ViewModel.EventType eventType && Model.StateMachine.EventTypes.Contains(eventType) && Model.StateMachine.IsChangeAllowed)
                {
                    foreach (ViewModel.Transition t in Model.StateMachine.Transitions)
                    {
                        if (t.TriggerEvent == eventType)
                        {
                            t.TriggerEvent = null;
                        }
                    }
                    Model.StateMachine.EventTypes.Remove(eventType);
                    Model.UndoManager.Add(new UndoRedo.AddEventTypeRecord(Model, eventType));
                    Model.StateMachine.EndChange();
                }
                else if (icon.ReferencedObject is ViewModel.Region region && Model.StateMachine.Regions.Contains(region) && Model.StateMachine.IsChangeAllowed)
                {
                    Model.StateMachine.Regions.Remove(region);
                    Model.UndoManager.Add(new UndoRedo.AddRegionRecord(Model, region));
                    Model.StateMachine.EndChange();
                }
                else if (icon.ReferencedObject is ViewModel.Transition transition && Model.StateMachine.Transitions.Contains(transition) && Model.StateMachine.IsChangeAllowed)
                {
                    Model.StateMachine.Transitions.Remove(transition);
                    Model.UndoManager.Add(new UndoRedo.AddTransitionRecord(Model, transition));
                    Model.StateMachine.EndChange();
                }
            }
        }

        internal void DeleteSelectedIcons()
        {
            Icons.IDraggableIcon[] pendingDeletes = SelectedIcons.Values.ToArray();

            using (new UndoRedo.AtomicBlock(Model, "Delete"))
            {
                foreach (var icon in pendingDeletes)
                {
                    DeleteIcon(icon);
                }
            }
        }

        internal void DeselectIcon(Icons.IDraggableIcon draggingIcon)
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
        }

        private void DesignerControl_Unloaded(object sender, RoutedEventArgs e)
        {
            MonitorStateMachineForChanges(true);
        }

        internal bool DoIdle(IVsTextLines textBuffer)
        {
            return Model.DoIdle(textBuffer);
        }

        internal void DraggableIconMouseLeftButtonDownHandler(System.Windows.Point dragOrigin, Icons.IDraggableIcon icon)
        {
            MouseStateMachine.MouseLeftButtonDownOnIcon(dragOrigin, icon);
        }

        internal void DraggableIconMouseRightButtonUpHandler(System.Windows.Point dragOrigin, Icons.IDraggableIcon icon)
        {
            MouseStateMachine.MouseRightButtonUpOnIcon(dragOrigin, icon);
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
                    case PackageIds.AddRegionCommandId:
                        AddRegion(ContextMenuActivationLocation);
                        return VSConstants.S_OK;
                    case PackageIds.AddStateCommandId:
                        AddState(ContextMenuActivationLocation);
                        return VSConstants.S_OK;
                    case PackageIds.ShowAllIconsCommandId:
                        ShowAllIcons();
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
                    IEnumerable<Icons.IDraggableIcon> occludedIcons = FindOccludedIcons(rectangle);

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

        internal Icons.IDraggableIcon FindNearestSelectedIcon(Point referencePoint)
        {
            double nearestDistance = double.MaxValue;
            Icons.IDraggableIcon nearestIcon = null;

            foreach (Icons.IDraggableIcon icon in SelectedIcons.Values)
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

            List<T> occludedIcons = new List<T>();

            VisualTreeHelper.HitTest(IconSurface, null, new HitTestResultCallback((HitTestResult result) =>
            {
            if (result.VisualHit is FrameworkElement fe && fe.DataContext is T icon && icon.GetType() == typeof(T) && !occludedIcons.Contains(icon))
                {
                    occludedIcons.Add(icon);
                }
                return HitTestResultBehavior.Continue;
            }),
                new PointHitTestParameters(targetPoint));

            return occludedIcons;
        }

        internal IEnumerable<Icons.IDraggableIcon> FindOccludedIcons(Geometry geometry)
        {
            List<Icons.IDraggableIcon> occludedIcons = new List<Icons.IDraggableIcon>();

            VisualTreeHelper.HitTest(IconSurface, null, new HitTestResultCallback((HitTestResult result) =>
            {
                if (result.VisualHit is FrameworkElement fe && fe.DataContext is Icons.IDraggableIcon icon && !occludedIcons.Contains(icon))
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

        private void LoadViewModelIcons()
        {
            IconSurface.Children.Clear();
            LoadedIcons.Clear();

            if (Model.StateMachine != null)
            {
                foreach (var state in Model.StateMachine.States)
                {
                    Icons.StateIcon stateIcon = new Icons.StateIcon(this, state, null, state.LeftTopPosition);
                    IconSurface.Children.Add(stateIcon.Body);
                    LoadedIcons.Add(state, stateIcon);
                }

                foreach (var eventType in Model.StateMachine.EventTypes)
                {
                    Icons.EventTypeIcon eventTypeIcon = new Icons.EventTypeIcon(this, eventType, null, eventType.LeftTopPosition);
                    IconSurface.Children.Add(eventTypeIcon.Body);
                    LoadedIcons.Add(eventType, eventTypeIcon);
                }

                foreach (var region in Model.StateMachine.Regions)
                {
                    Icons.RegionIcon regionIcon = new Icons.RegionIcon(this, region, null, region.LeftTopPosition);
                    IconSurface.Children.Add(regionIcon.Body);
                    LoadedIcons.Add(region, regionIcon);
                }

                foreach (var transition in Model.StateMachine.Transitions)
                {
                    Icons.TransitionIcon transitionIcon = new Icons.TransitionIcon(this, transition, null, null);
                    IconSurface.Children.Add(transitionIcon.Body);
                    LoadedIcons.Add(transition, transitionIcon);
                }

                ReComputeHiddenIcons();
            }
        }

        internal void KeyDownHandler(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                MouseStateMachine.Cancel();
                e.Handled = true;
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
                StateMachine.Regions.CollectionChanged -= StateMachineRegionsCollectionChangedHandler;
                StateMachine.States.CollectionChanged -= StateMachineStatesCollectionChangedHandler;
                StateMachine.Transitions.CollectionChanged -= StateMachineTransitionsCollectionChangedHandler;
            }
            StateMachine = Model.StateMachine;
            if (StateMachine != null && !isUnloading)
            {
                StateMachine.EventTypes.CollectionChanged += StateMachineEventTypesCollectionChangedHandler;
                StateMachine.Regions.CollectionChanged += StateMachineRegionsCollectionChangedHandler;
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

        internal void MouseRightButtonUpHandler(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            MouseStateMachine.MouseRightButtonUp(e.GetPosition(IconSurface));
            e.Handled = true;
        }

        public int QueryStatus(ref Guid pguidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText)
        {
            return VSConstants.S_OK;
        }

        internal void ReComputeHiddenIcons()
        {
            //  First, collect the set of potentially hidden icons

            HashSet<Icons.IDraggableIcon> hiddenIcons = new HashSet<Icons.IDraggableIcon>();

            foreach (ViewModel.Region region in Model.StateMachine.Regions)
            {
                if (region.IsHidden)
                {
                    foreach (ObjectModel.TrackableObject trackableObject in region.Members)
                    {
                        hiddenIcons.Add(LoadedIcons[trackableObject]);
                    }
                }
            }

            //  Now make certain that icons for regions that are *not* hidden are not hidden

            foreach (ViewModel.Region region in Model.StateMachine.Regions)
            {
                if (!region.IsHidden)
                {
                    foreach (ObjectModel.TrackableObject trackableObject in region.Members)
                    {
                        hiddenIcons.Remove(LoadedIcons[trackableObject]);
                    }
                }
            }

            //  And finalize the icons

            foreach (Icons.IDraggableIcon icon in LoadedIcons.Values)
            {
                if (icon is Icons.TransitionIcon transitionIcon && transitionIcon.ReferencedObject is ViewModel.Transition transition)
                {
                    icon.IsHidden = hiddenIcons.Contains(LoadedIcons[transition.SourceState]) || hiddenIcons.Contains(LoadedIcons[transition.DestinationState]);
                }
                else
                {
                    icon.IsHidden = hiddenIcons.Contains(icon);
                }
            }
        }

        private void ReloadModel()
        {
            ClearSelectedItems();
            IconSurface.Children.Clear();
            MonitorStateMachineForChanges(false);

            if (Model.StateMachine != null)
            {
                Icons.IDraggableIcon[] previouslySelectedIcons = SelectedIcons.Values.ToArray();

                LoadViewModelIcons();
                ReComputeHiddenIcons();

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
                            if (trackableObject == null)
                            {
                                trackableObject = Model.StateMachine.Regions.Where(r => r.Id == oldIcon.ReferencedObject.Id).FirstOrDefault();
                            }
                        }

                        if (trackableObject != null)
                        {
                            SelectedObjects.Add(trackableObject);
                            SelectedIcons.Add(trackableObject, LoadedIcons[trackableObject]);
                            LoadedIcons[trackableObject].IsSelectedChanged();
                        }
                    }
                }
            }
        }

        private void RemoveIconSelection(Icons.IDraggableIcon icon)
        {
            if (SelectedIcons.ContainsKey(icon.ReferencedObject) && SelectedObjects.Count > 0)
            {
                SelectedObjects.Remove(icon.ReferencedObject);
                SelectedIcons.Remove(icon.ReferencedObject);
                icon.IsSelectedChanged();
            }
        }

        internal void SelectIcon(Icons.IDraggableIcon icon)
        {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();

            if (SelectedObjects.Count == 1 && SelectedObjects[0] == Model.StateMachine)
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

        internal void SelectOccludedIcons(Point dragOrigin, Point mousePosition)
        {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();

            //  Collect the set of icons in the selection box

            List<Icons.IDraggableIcon> selectedIcons = new List<Icons.IDraggableIcon>();

            foreach (Icons.IDraggableIcon draggableIcon in LoadedIcons.Values)
            {
                if (draggableIcon.ReferencedObject is ObjectModel.PositionableObject positionableObject &&
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
                foreach (Icons.IDraggableIcon icon in selectedIcons)
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

        internal void SelectSingle(Icons.IDraggableIcon icon)
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

        internal void SetStartState(ViewModel.State state)
        {
            if (Model.StateMachine.IsChangeAllowed)
            {
                Model.StateMachine.StartState = state;
                Model.StateMachine.EndChange();
            }
        }

        internal void SelectStateMachineHandler(object sender, MouseButtonEventArgs e)
        {
            SelectStateMachine();
        }

        internal void ShowAllIcons()
        {
            if (Model.StateMachine.IsChangeAllowed)
            {
                using (new UndoRedo.AtomicBlock(Model, "Show all icons"))
                {
                    foreach (ViewModel.Region region in Model.StateMachine.Regions)
                    {
                        region.IsHidden = false;
                    }
                }

                ReComputeHiddenIcons();

                Model.StateMachine.EndChange();
            }
        }

        internal void SortSelectedIcons(Icons.IDraggableIcon draggableIcon)
        {
            Icons.IDraggableIcon[] selectedIcons = SelectedIcons.Values.Where(i => i.ReferencedObject is ObjectModel.NamedObject).OrderBy(i => (i.ReferencedObject as ObjectModel.NamedObject).Name).ToArray();
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
                        Icons.EventTypeIcon eventTypeIcon = new Icons.EventTypeIcon(this, newEventType, null, newEventType.LeftTopPosition);
                        IconSurface.Children.Add(eventTypeIcon.Body);
                        LoadedIcons.Add(newEventType, eventTypeIcon);

                        ClearSelectedItems();
                        SelectIcon(eventTypeIcon);
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (ViewModel.EventType eventType in e.OldItems)
                    {
                        RemoveIconSelection(LoadedIcons[eventType]);
                        IconSurface.Children.Remove(LoadedIcons[eventType].Body);
                        LoadedIcons.Remove(eventType);
                    }
                    break;
                default:
                    break;
            }
        }

        private void StateMachineRegionsCollectionChangedHandler(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (ViewModel.Region newRegion in e.NewItems)
                    {
                        Icons.RegionIcon regionIcon = new Icons.RegionIcon(this, newRegion, null, newRegion.LeftTopPosition);
                        IconSurface.Children.Add(regionIcon.Body);
                        LoadedIcons.Add(newRegion, regionIcon);

                        ClearSelectedItems();
                        SelectIcon(regionIcon);
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (ViewModel.Region region in e.OldItems)
                    {
                        RemoveIconSelection(LoadedIcons[region]);
                        IconSurface.Children.Remove(LoadedIcons[region].Body);
                        LoadedIcons.Remove(region);
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
                        Icons.StateIcon stateIcon = new Icons.StateIcon(this, newState, null, newState.LeftTopPosition);
                        IconSurface.Children.Add(stateIcon.Body);
                        LoadedIcons.Add(newState, stateIcon);

                        ClearSelectedItems();
                        SelectIcon(stateIcon);
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (ViewModel.State state in e.OldItems)
                    {
                        RemoveIconSelection(LoadedIcons[state]);
                        IconSurface.Children.Remove(LoadedIcons[state].Body);
                        LoadedIcons.Remove(state);
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
                        Icons.TransitionIcon transitionIcon = new Icons.TransitionIcon(this, newTransition, null, null);
                        IconSurface.Children.Add(transitionIcon.Body);
                        LoadedIcons.Add(newTransition, transitionIcon);

                        ClearSelectedItems();
                        SelectIcon(transitionIcon);
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (ViewModel.Transition transition in e.OldItems)
                    {
                        RemoveIconSelection(LoadedIcons[transition]);
                        IconSurface.Children.Remove(LoadedIcons[transition].Body);
                        LoadedIcons.Remove(transition);
                    }
                    break;
                default:
                    break;
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
    }
}
