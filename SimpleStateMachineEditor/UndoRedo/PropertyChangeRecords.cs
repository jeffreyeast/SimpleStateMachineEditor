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
#region PropertyChangeRecords

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
        protected override int UnitType => (int)ActionTypes.ChangePropertyList;

        public string ObjectType;
        public string PropertyName;
        public string[] Value;


        internal ListValuedPropertyChangedRecord(ViewModel.ViewModelController controller, TrackableObject trackableObject, string propertyName, IEnumerable<string> targetList) : base(ActionTypes.ChangePropertyList, controller, trackableObject)
        {
            ObjectType = trackableObject.GetType().ToString();
            PropertyName = propertyName;
            Value = targetList.ToArray<string>();
#if DEBUGUNDOREDO
            Debug.WriteLine($@">>> ListValuedPropertyChangedRecord.ListValuedPropertyChangedRecord: Created {UnitDescription} record, Id: {Id}, PropertyName: {PropertyName}, Value: {ArrayToString(Value)}");
#endif
        }

        public override void Do(IOleUndoManager pUndoManager)
        {
#if DEBUGUNDOREDO
            Debug.WriteLine($@">>> ListValuedPropertyChangedRecord.Do (Property {PropertyName}, Value: {ArrayToString(Value)})");
#endif
            if (Controller.StateMachine.IsChangeAllowed)
            {
                ObjectModel.TrackableObject trackableObject = Controller.StateMachine.Find(Id);
                trackableObject?.SetProperty(PropertyName, Value);

                Controller.StateMachine.EndChange();
            }
        }

#if DEBUGUNDOREDO
        private string ArrayToString ( IEnumerable<string> a)
        {
            string result = "{";
            string separator = "";
            foreach (var s in a)
            {
                result += separator + s;
                separator = ",";
            }
            return result + "}";
        }
#endif
    }
#endregion
}
