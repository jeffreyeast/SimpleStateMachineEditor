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

#region LayerRecords
    // Layer Records

    internal class AddLayerMemberRecord : TrackableObjectRecord
    {
        protected override string UnitDescription => "Add layer member";
        protected override int UnitType => (int)ActionTypes.AddLayerMember;

        int NewMemberId;
        System.Windows.Point LeftTopPosition;



        internal AddLayerMemberRecord(ViewModel.ViewModelController controller, ViewModel.Layer layer, ObjectModel.LayeredPositionableObject newMember) : base(ActionTypes.AddLayerMember, controller, layer)
        {
            NewMemberId = newMember.Id;
            LeftTopPosition = newMember.LeftTopPosition;
#if DEBUGUNDOREDO
            Debug.WriteLine($@">>> AddLayerMemberRecord.AddLayerMemberRecord: Created {UnitDescription} record, ID: {Id}, NewMemberId: {NewMemberId}");
#endif
        }

        public override void Do(IOleUndoManager pUndoManager)
        {
#if DEBUGUNDOREDO
            Debug.WriteLine(">>> AddLayerMemberRecord.Do");
#endif
            if (Controller.StateMachine.IsChangeAllowed)
            {
                ViewModel.Layer layer = Controller.StateMachine.Find(Id) as ViewModel.Layer;
                ObjectModel.LayeredPositionableObject newMember = Controller.StateMachine.Find(NewMemberId) as ObjectModel.LayeredPositionableObject;
                ObjectModel.LayerPosition layerPosition = ObjectModel.LayerPosition.Create(Controller, layer);
                newMember.LayerPositions.Add(layerPosition);
                layerPosition.LeftTopPosition = LeftTopPosition;
                layer.Members.Add(newMember);
                Controller.StateMachine.EndChange();

                Controller.UndoManager.Add(new DeleteLayerMemberRecord(Controller, layer, newMember));
            }
        }
    }

    internal class AddLayerRecord : NamedObjectRecord
    {
        protected override string UnitDescription => "Add layer";
        protected override int UnitType => (int)ActionTypes.AddLayer;

        public int[] MemberIds;


        internal AddLayerRecord(ViewModel.ViewModelController controller, ViewModel.Layer layer) : base(ActionTypes.AddLayer, controller, layer)
        {
            MemberIds = layer.Members.Select(m => m.Id).ToArray();

#if DEBUGUNDOREDO
            Debug.WriteLine($@">>> AddLayerRecord.AddLayerRecord: Created {UnitDescription} record, ID: {Id}, Name: {Name}");
#endif
        }

        public override void Do(IOleUndoManager pUndoManager)
        {
#if DEBUGUNDOREDO
            Debug.WriteLine(">>> AddLayerRecord.Do");
#endif
            if (Controller.StateMachine.IsChangeAllowed)
            {
                ViewModel.Layer newLayer = new Layer(Controller, this);
                Controller.StateMachine.Layers.Add(newLayer);
                Controller.StateMachine.EndChange();

                Controller.UndoManager.Add(new DeleteLayerRecord(Controller, newLayer));
            }
        }
    }

    internal class DeleteLayerMemberRecord : TrackableObjectRecord
    {
        protected override string UnitDescription => "Remove layer member";
        protected override int UnitType => (int)ActionTypes.RemoveLayerMember;

        int MemberId;



        internal DeleteLayerMemberRecord(ViewModel.ViewModelController controller, ViewModel.Layer layer, ObjectModel.TrackableObject member) : base(ActionTypes.RemoveLayerMember, controller, layer)
        {
            MemberId = member.Id;
#if DEBUGUNDOREDO
            Debug.WriteLine($@">>> DeleteLayerMemberRecord.DeleteLayerMemberRecord: Created {UnitDescription} record, ID: {Id}, NewMemberId: {MemberId}");
#endif
        }

        public override void Do(IOleUndoManager pUndoManager)
        {
#if DEBUGUNDOREDO
            Debug.WriteLine(">>> DeleteLayerMemberRecord.Do");
#endif
            if (Controller.StateMachine.IsChangeAllowed)
            {
                ViewModel.Layer layer = Controller.StateMachine.Find(Id) as ViewModel.Layer;
                ObjectModel.LayeredPositionableObject member = Controller.StateMachine.Find(MemberId) as ObjectModel.LayeredPositionableObject;
                Controller.UndoManager.Add(new AddLayerMemberRecord(Controller, layer, member));
                layer.Members.Remove(member);
                ObjectModel.LayerPosition layerPosition = member.LayerPositions.Where(lp => lp.Layer == layer).Single();
                member.LayerPositions.Remove(layerPosition);
                Controller.StateMachine.EndChange();
            }
        }
    }

    internal class DeleteLayerRecord : NamedObjectRecord
    {
        protected override string UnitDescription => "Delete layer";
        protected override int UnitType => (int)ActionTypes.DeleteLayer;


        internal DeleteLayerRecord(ViewModel.ViewModelController controller, ViewModel.Layer layer) : base(ActionTypes.DeleteLayer, controller, layer)
        {
#if DEBUGUNDOREDO
            Debug.WriteLine($@">>> DeleteLayerRecord.DeleteLayerRecord: Created {UnitDescription} record, ID: {Id}, Name: {Name}");
#endif
        }

        public override void Do(IOleUndoManager pUndoManager)
        {
#if DEBUGUNDOREDO
            Debug.WriteLine(">>> DeleteLayerRecord.Do");
#endif
            if (Controller.StateMachine.IsChangeAllowed)
            {
                ViewModel.Layer targetLayer = Controller.StateMachine.Layers.Where(r => r.Id == Id).First();
                Controller.StateMachine.Layers.Remove(targetLayer);
                Controller.StateMachine.EndChange();

                Controller.UndoManager.Add(new AddLayerRecord(Controller, targetLayer));
            }
        }
    }

    internal class SetLayerActiveRecord : TrackableObjectRecord
    {
        protected override string UnitDescription => "Set layer";
        protected override int UnitType => (int)ActionTypes.SetLayer;

        DesignerControl Designer;
        int OldLayerId;

        internal SetLayerActiveRecord(ViewModel.ViewModelController controller, DesignerControl designer, ViewModel.Layer newLayer, ViewModel.Layer oldLayer) : base(ActionTypes.AddLayer, controller, newLayer)
        {
            Designer = designer;
            OldLayerId = oldLayer.Id;
#if DEBUGUNDOREDO
            Debug.WriteLine($@">>> SetLayerActiveRecord.SetLayerActiveRecord: Created {UnitDescription} record, ID: {Id}, Name: {newLayer.Name}, Old Layer ID: {oldLayer.Id}, Old Layer Name: {oldLayer.Name}");
#endif
        }

        public override void Do(IOleUndoManager pUndoManager)
        {
#if DEBUGUNDOREDO
            Debug.WriteLine(">>> SetLayerActiveRecord.Do");
#endif

            ViewModel.Layer newLayer = Controller.StateMachine.Find(Id) as ViewModel.Layer;
            ViewModel.Layer oldLayer = Controller.StateMachine.Find(OldLayerId) as ViewModel.Layer;

            Designer.CurrentLayer = newLayer;
        }
    }

#endregion
}
