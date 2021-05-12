using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using SimpleStateMachineEditor.ObjectModel;
using SimpleStateMachineEditor.ViewModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell.Design;
using System.ComponentModel.Design;

namespace SimpleStateMachineEditor.UndoRedo
{
    internal abstract class UndoRedoRecord : IOleUndoUnit
    {
        public static readonly Guid UndoRedoClassID = new Guid("F8507D32-10D6-4EE0-B861-6895066A3304");

        public enum ActionTypes
        {
            AddEventType,
            AddRegion,
            AddState,
            AddTransition,
            ChangeProperty,
            DeleteEventType,
            DeleteRegion,
            DeleteState,
            DeleteTransition,
            MoveTransition,
            Parent,
        }

        internal ActionTypes ActionType;

        protected ViewModel.ViewModelController Controller;
        protected abstract string UnitDescription { get; }
        protected abstract int UnitType { get; }


        protected UndoRedoRecord(ActionTypes actionType, ViewModel.ViewModelController controller)
        {
            ActionType = actionType;
            Controller = controller;
        }

        public abstract void Do(IOleUndoManager pUndoManager);

        public void GetDescription(out string pBstr)
        {
            pBstr = UnitDescription;
        }

        public void GetUnitType(out Guid pClsid, out int plID)
        {
            pClsid = UndoRedoClassID;
            plID = UnitType;
        }

        public virtual void OnNextAdd() { }

        public override string ToString()
        {
            return UnitDescription;
        }
    }


    internal abstract class PositionableObjectRecord : NamedObjectRecord
    {
        internal System.Windows.Point LeftTopPosition;

        protected PositionableObjectRecord(ActionTypes actionType, ViewModel.ViewModelController controller, ObjectModel.PositionableObject positionableObject) : base(actionType, controller, positionableObject)
        {
            LeftTopPosition = positionableObject.LeftTopPosition;
        }
    }

    internal abstract class NamedObjectRecord : TrackableObjectRecord
    {
        internal string Name;
        internal string Description;

        protected NamedObjectRecord(ActionTypes actionType, ViewModel.ViewModelController controller, ObjectModel.NamedObject namedObject) : base(actionType, controller, namedObject)
        {
            Name = (namedObject.Name == null ? null : string.Copy(namedObject.Name));
            Description = (namedObject.Description == null ? null : string.Copy(namedObject.Description));
        }
    }

    internal abstract class TrackableObjectRecord : UndoRedoRecord
    {
        internal int Id;

        protected TrackableObjectRecord(ActionTypes actionType, ViewModel.ViewModelController controller, ObjectModel.TrackableObject trackableObject) : base(actionType, controller)
        {
            Id = trackableObject.Id;
        }
    }

    // Grouping construct for multiple-undo records

    internal class ParentRecord : UndoRedoRecord, IOleParentUndoUnit
    {
        protected override string UnitDescription => _unitDescription;
        string _unitDescription = "ParentRecord";
        protected override int UnitType => (int)ActionTypes.Parent;

        Stack<IOleUndoUnit> Children;
        List<IOleParentUndoUnit> OpenChildren;
        int Sequence;

        static int NextSequence = 0;



        internal ParentRecord(ViewModel.ViewModelController controller, string description) : base(ActionTypes.Parent, controller) 
        {
            Children = new Stack<IOleUndoUnit>();
            OpenChildren = new List<IOleParentUndoUnit>();
            _unitDescription = $@"{description}";
            Sequence = System.Threading.Interlocked.Increment(ref NextSequence);
#if DEBUGUNDOREDO
            Debug.WriteLine($@">>> ParentRecord.ParentRecord({Sequence}/{UnitDescription}): Created  parent record");
#endif
        }

        public override void Do(IOleUndoManager pUndoManager)
        {
#if DEBUGUNDOREDO
            Debug.WriteLine($@">>> ParentRecord.Do({Sequence}/{_unitDescription}): begin");
#endif

            using (new UndoRedo.AtomicBlock(Controller, _unitDescription))
            {
                while (Children.Count > 0)
                {
                    IOleUndoUnit child = Children.Pop();
                    child.Do(pUndoManager);
                }
            }

#if DEBUGUNDOREDO
            Debug.WriteLine($@">>> ParentRecord.Do({Sequence}/{_unitDescription}): end");
#endif
        }

        public void Open(IOleParentUndoUnit pPUU)
        {
#if DEBUGUNDOREDO
            Debug.WriteLine($@">>> ParentRecord.Open({Sequence}{_unitDescription}): Open pPUU({(pPUU as ParentRecord).Sequence}/{(pPUU as ParentRecord)._unitDescription})");
#endif
            OpenChildren.Add(pPUU);
        }

        public int Close(IOleParentUndoUnit pPUU, int fCommit)
        {
            if (OpenChildren.Count == 0)
            {
#if DEBUGUNDOREDO
                Debug.WriteLine($@">>> ParentRecord.Close({Sequence}/{_unitDescription}):) Reporting Closed");
#endif
                return VSConstants.S_FALSE;
            }

#if DEBUGUNDOREDO
            Debug.WriteLine($@">>> ParentRecord.Close({Sequence}/{_unitDescription}): CLosing pPUU({(pPUU as ParentRecord).Sequence}/{(pPUU as ParentRecord)._unitDescription})");
#endif

            int hr = OpenChildren.Last().Close(pPUU, fCommit);
            switch (hr)
            {
                case VSConstants.S_OK:
#if DEBUGUNDOREDO
                    Debug.WriteLine($@">>> ParentRecord.Close({Sequence}/{_unitDescription}):  pPUU({(pPUU as ParentRecord).Sequence}/{(pPUU as ParentRecord)._unitDescription}) returned S_OK");
#endif
                    return VSConstants.S_OK;
                case VSConstants.S_FALSE:
                    if (OpenChildren.Contains(pPUU))
                    {
                        OpenChildren.Remove(pPUU);
#if DEBUGUNDOREDO
                        Debug.WriteLine($@">>> ParentRecord.Close({Sequence}/{_unitDescription}):  pPUU({(pPUU as ParentRecord).Sequence}/{(pPUU as ParentRecord)._unitDescription}) returned S_FALSE, we're returning S_OK");
#endif
                        return VSConstants.S_OK;
                    }
                    else
                    {
#if DEBUGUNDOREDO
                        Debug.WriteLine($@">>> ParentRecord.Close({Sequence}/{_unitDescription}):  pPUU({(pPUU as ParentRecord).Sequence}/{(pPUU as ParentRecord)._unitDescription}) returned S_FALSE, we're returning E_INVALIDARG");
#endif
                        return VSConstants.E_INVALIDARG;
                    }
                default:
                    throw new NotImplementedException();
            }
        }

        public void Add(IOleUndoUnit pUU)
        {
            pUU.GetDescription(out string description);
#if DEBUGUNDOREDO
            Debug.WriteLine($@">>> ParentRecord.Add({Sequence}/{_unitDescription}): {description}");
#endif

            Children.Push(pUU);
        }

        public int FindUnit(IOleUndoUnit pUU)
        {
            throw new NotImplementedException();
        }

        public void GetParentState(out uint pdwState)
        {
            throw new NotImplementedException();
        }
    }

    // Event Type Records

    internal class AddEventTypeRecord : PositionableObjectRecord
    {
        protected override string UnitDescription => "Add event type";
        protected override int UnitType => (int)ActionTypes.AddEventType;



        internal AddEventTypeRecord(ViewModel.ViewModelController controller, ViewModel.EventType eventType) : base(ActionTypes.AddEventType, controller, eventType) 
        {
#if DEBUGUNDOREDO
            Debug.WriteLine($@">>> AddEventTypeRecord.AddEventTypeRecord: Created {UnitDescription} record, ID: {Id}, Name: {Name}");
#endif
        }

        public override void Do(IOleUndoManager pUndoManager)
        {
#if DEBUGUNDOREDO
            Debug.WriteLine(">>> AddEventTypeRecord.Do");
#endif
            if (Controller.StateMachine.IsChangeAllowed)
            {
                ViewModel.EventType newEventType = new EventType(Controller, this);
                Controller.StateMachine.EventTypes.Add(newEventType);
                Controller.StateMachine.EndChange();

                Controller.UndoManager.Add(new DeleteEventTypeRecord(Controller, newEventType));
            }
        }
    }

    internal class DeleteEventTypeRecord : PositionableObjectRecord
    {
        protected override string UnitDescription => "Delete event type";
        protected override int UnitType => (int)ActionTypes.DeleteEventType;


        internal DeleteEventTypeRecord(ViewModel.ViewModelController controller, ViewModel.EventType eventType) : base(ActionTypes.DeleteEventType, controller, eventType) 
        {
#if DEBUGUNDOREDO
            Debug.WriteLine($@">>> DeleteEventTypeRecord.DeleteEventTypeRecord: Created {UnitDescription} record, ID: {Id}, Name: {Name}");
#endif
        }

        public override void Do(IOleUndoManager pUndoManager)
        {
#if DEBUGUNDOREDO
            Debug.WriteLine(">>> DeleteEventTypeRecord.Do");
#endif
            if (Controller.StateMachine.IsChangeAllowed)
            {
                ViewModel.EventType targetEventType = Controller.StateMachine.EventTypes.Where(e => e.Id == Id).First();
                Controller.StateMachine.EventTypes.Remove(targetEventType);
                Controller.StateMachine.EndChange();

                Controller.UndoManager.Add(new AddEventTypeRecord(Controller, targetEventType));
            }
        }
    }

    // Region Records

    internal class AddRegionRecord : PositionableObjectRecord
    {
        protected override string UnitDescription => "Add region";
        protected override int UnitType => (int)ActionTypes.AddRegion;

        public Utility.DisplayColors DisplayColors;
        public bool IsHidden;
        public int[] MemberIds;


        internal AddRegionRecord(ViewModel.ViewModelController controller, ViewModel.Region region) : base(ActionTypes.AddRegion, controller, region)
        {
            DisplayColors = region.DisplayColors;
            IsHidden = region.IsHidden;
            MemberIds = region.Members.Select(m => m.Id).ToArray();

#if DEBUGUNDOREDO
            Debug.WriteLine($@">>> AddRegionRecord.AddRegionRecord: Created {UnitDescription} record, ID: {Id}, Name: {Name}");
#endif
        }

        public override void Do(IOleUndoManager pUndoManager)
        {
#if DEBUGUNDOREDO
            Debug.WriteLine(">>> AddRegionRecord.Do");
#endif
            if (Controller.StateMachine.IsChangeAllowed)
            {
                ViewModel.Region newRegion = new Region(Controller, this);
                Controller.StateMachine.Regions.Add(newRegion);
                Controller.StateMachine.EndChange();

                Controller.UndoManager.Add(new DeleteRegionRecord(Controller, newRegion));
            }
        }
    }

    internal class DeleteRegionRecord : PositionableObjectRecord
    {
        protected override string UnitDescription => "Delete region";
        protected override int UnitType => (int)ActionTypes.DeleteRegion;


        internal DeleteRegionRecord(ViewModel.ViewModelController controller, ViewModel.Region region) : base(ActionTypes.DeleteRegion, controller, region)
        {
#if DEBUGUNDOREDO
            Debug.WriteLine($@">>> DeleteRegionRecord.DeleteRegionRecord: Created {UnitDescription} record, ID: {Id}, Name: {Name}");
#endif
        }

        public override void Do(IOleUndoManager pUndoManager)
        {
#if DEBUGUNDOREDO
            Debug.WriteLine(">>> DeleteRegionRecord.Do");
#endif
            if (Controller.StateMachine.IsChangeAllowed)
            {
                ViewModel.Region targetRegion = Controller.StateMachine.Regions.Where(r => r.Id == Id).First();
                Controller.StateMachine.Regions.Remove(targetRegion);
                Controller.StateMachine.EndChange();

                Controller.UndoManager.Add(new AddRegionRecord(Controller, targetRegion));
            }
        }
    }


    // State Records

    internal class AddStateRecord : PositionableObjectRecord
    {
        protected override string UnitDescription => "Add state";
        protected override int UnitType => (int)ActionTypes.AddState;

        public ViewModel.State.StateTypes StateType;



        internal AddStateRecord(ViewModel.ViewModelController controller, ViewModel.State state) : base(ActionTypes.AddState, controller, state) 
        {
            StateType = state.StateType;
#if DEBUGUNDOREDO
            Debug.WriteLine($@">>> AddStateRecord.AddStateRecord: Created {UnitDescription} record, ID: {Id}, Name: {Name}");
#endif
        }

        public override void Do(IOleUndoManager pUndoManager)
        {
#if DEBUGUNDOREDO
            Debug.WriteLine(">>> AddStateRecord.Do");
#endif
            if (Controller.StateMachine.IsChangeAllowed)
            {
                ViewModel.State newState = new ViewModel.State(Controller, this);
                Controller.StateMachine.States.Add(newState);
                Controller.StateMachine.EndChange();

                Controller.UndoManager.Add(new DeleteStateRecord (Controller, newState));
            }
        }
    }

    internal class DeleteStateRecord : PositionableObjectRecord
    {
        protected override string UnitDescription => "Delete state";
        protected override int UnitType => (int)ActionTypes.DeleteState;


        internal DeleteStateRecord(ViewModel.ViewModelController controller, ViewModel.State state) : base(ActionTypes.AddEventType, controller, state)
        {
#if DEBUGUNDOREDO
            Debug.WriteLine($@">>> DeleteStateRecord.DeleteStateRecord: Created {UnitDescription} record, ID: {Id}, Name: {Name}");
#endif
        }

        public override void Do(IOleUndoManager pUndoManager)
        {
#if DEBUGUNDOREDO
            Debug.WriteLine(">>> DeleteStateRecord.Do");
#endif
            if (Controller.StateMachine.IsChangeAllowed)
            {
                ViewModel.State targetState = Controller.StateMachine.States.Where(s => s.Id == Id).First();
                Controller.StateMachine.States.Remove(targetState);
                Controller.StateMachine.EndChange();

                Controller.UndoManager.Add(new AddStateRecord (Controller, targetState));
            }
        }
    }

    //  Transition Records

    internal class AddTransitionRecord : TrackableObjectRecord
    {
        protected override string UnitDescription => "Add transition";
        protected override int UnitType => (int)ActionTypes.AddTransition;

        public IEnumerable<string> Actions;
        public int DestinationStateId;
        public int SourceStateId;
        public int TriggerEventId;



        internal AddTransitionRecord(ViewModel.ViewModelController controller, ViewModel.Transition transition) : base(ActionTypes.AddTransition, controller, transition) 
        {
            transition.GetProperty("Actions", out Actions);
            DestinationStateId = transition.DestinationStateId;
            SourceStateId = transition.SourceStateId;
            TriggerEventId = transition.TriggerEvent?.Id ?? -1;
#if DEBUGUNDOREDO
            Debug.WriteLine($@">>> AddTransitionRecord.AddTransitionRecord: Created {UnitDescription} record, Id: {Id}, Src: {SourceStateId}, Dest: {DestinationStateId}");
#endif
        }

        public override void Do(IOleUndoManager pUndoManager)
        {
#if DEBUGUNDOREDO
            Debug.WriteLine(">>> AddTransitionRecord.Do");
#endif
            if (Controller.StateMachine.IsChangeAllowed)
            {
                ViewModel.Transition newTransition = new ViewModel.Transition(Controller, this);
                Controller.StateMachine.Transitions.Add(newTransition);
                Controller.StateMachine.EndChange();

                Controller.UndoManager.Add(new DeleteTransitionRecord(Controller, newTransition));
            }
        }
    }

    internal class DeleteTransitionRecord : TrackableObjectRecord
    {
        protected override string UnitDescription => $@"Delete transition";
        protected override int UnitType => (int)ActionTypes.DeleteTransition;


        internal DeleteTransitionRecord(ViewModel.ViewModelController controller, ViewModel.Transition transition) : base(ActionTypes.AddEventType, controller, transition) 
        {
#if DEBUGUNDOREDO
            Debug.WriteLine($@">>> DeleteTransitionRecord.DeleteTransitionRecord: Created {UnitDescription} record, Id: {Id}");
#endif
        }

        public override void Do(IOleUndoManager pUndoManager)
        {
#if DEBUGUNDOREDO
            Debug.WriteLine(">>> DeleteTransitionRecord.Do");
#endif
            if (Controller.StateMachine.IsChangeAllowed)
            {
                ViewModel.Transition targetTransition = Controller.StateMachine.Transitions.Where(s => s.Id == Id).First();
                Controller.StateMachine.Transitions.Remove(targetTransition);
                Controller.StateMachine.EndChange();

                Controller.UndoManager.Add(new AddTransitionRecord(Controller, targetTransition));
            }
        }
    }

    //  General property-changed record


    internal class PropertyChangedRecord : TrackableObjectRecord
    {
        protected override string UnitDescription => $@"Property {PropertyName} changed";
        protected override int UnitType => (int)ActionTypes.ChangeProperty;

        public string ObjectType;
        public string PropertyName;
        public string Value;


        internal PropertyChangedRecord(ViewModel.ViewModelController controller, TrackableObject trackableObject, string propertyName, string v) : base(ActionTypes.ChangeProperty, controller, trackableObject)
        {
            ObjectType = trackableObject.GetType().ToString();
            PropertyName = propertyName;
            Value = v;
#if DEBUGUNDOREDO
            Debug.WriteLine($@">>> PropertyChangedRecord.PropertyChangedRecord: Created {UnitDescription} record, Id: {Id}, PropertyName: {PropertyName}, Value: {v}");
#endif
        }

        public override void Do(IOleUndoManager pUndoManager)
        {
#if DEBUGUNDOREDO
            Debug.WriteLine($@">>> PropertyChangedRecord.Do (Property {PropertyName}, Value: {Value})");
#endif
            if (Controller.StateMachine.IsChangeAllowed)
            {
                ObjectModel.TrackableObject trackableObject = Controller.StateMachine.Find(Id);
                trackableObject?.SetProperty(PropertyName, Value);

                Controller.StateMachine.EndChange();
            }
        }
    }

    //  General property-changed record for list-valued properties


    internal class ListValuedPropertyChangedRecord : TrackableObjectRecord
    {
        protected override string UnitDescription => $@"Property {PropertyName} changed";
        protected override int UnitType => (int)ActionTypes.ChangeProperty;

        public string ObjectType;
        public string PropertyName;
        public string[] Value;


        internal ListValuedPropertyChangedRecord(ViewModel.ViewModelController controller, TrackableObject trackableObject, string propertyName, IEnumerable<string> targetList) : base(ActionTypes.ChangeProperty, controller, trackableObject)
        {
            ObjectType = trackableObject.GetType().ToString();
            PropertyName = propertyName;
            Value = targetList.ToArray<string>();
#if DEBUGUNDOREDO
            Debug.WriteLine($@">>> PropertyChangedRecord.PropertyChangedRecord: Created {UnitDescription} record, Id: {Id}, PropertyName: {PropertyName}");
#endif
        }

        public override void Do(IOleUndoManager pUndoManager)
        {
#if DEBUGUNDOREDO
            Debug.WriteLine($@">>> PropertyChangedRecord.Do (Property {PropertyName}, Value: {Value})");
#endif
            if (Controller.StateMachine.IsChangeAllowed)
            {
                ObjectModel.TrackableObject trackableObject = Controller.StateMachine.Find(Id);
                trackableObject?.SetProperty(PropertyName, Value);

                Controller.StateMachine.EndChange();
            }
        }
    }
}
