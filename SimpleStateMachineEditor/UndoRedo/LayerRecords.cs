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

        internal int LayerId;
        int NewMemberId;
        internal System.Windows.Point LeftTopPosition;
        internal LayerPosition.GroupStatuses GroupStatus;



        internal AddLayerMemberRecord(ViewModel.ViewModelController controller, ObjectModel.LayerPosition layerPosition, ObjectModel.ILayeredPositionableObject newMember) : base(ActionTypes.AddLayerMember, controller, layerPosition)
        {
            LayerId = layerPosition.LayerId;
            NewMemberId = newMember.Id;
            LeftTopPosition = layerPosition.LeftTopPosition;
            GroupStatus = layerPosition.GroupStatus;
#if DEBUGUNDOREDO
            Debug.WriteLine($@">>> AddLayerMemberRecord.AddLayerMemberRecord: Created {UnitDescription} record, ID: {Id}, LayerID: {LayerId}, NewMemberId: {NewMemberId}");
#endif
        }

        public override void Do(IOleUndoManager pUndoManager)
        {
#if DEBUGUNDOREDO
            Debug.WriteLine(">>> AddLayerMemberRecord.Do");
#endif
            using (new ViewModel.ViewModelController.GuiChangeBlock(Controller))
            {
                ViewModel.Layer layer = Controller.StateMachine.Find(LayerId) as ViewModel.Layer;
                ObjectModel.ITransitionEndpoint newMember = Controller.StateMachine.Find(NewMemberId) as ObjectModel.ITransitionEndpoint;
                ObjectModel.LayerPosition layerPosition = new ObjectModel.LayerPosition(Controller, this);
                newMember.LayerPositions.Add(layerPosition);
                layer.Members.Add(newMember);

                Controller.UndoManager.Add(new DeleteLayerMemberRecord(Controller, layerPosition, newMember));
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
            using (new ViewModel.ViewModelController.GuiChangeBlock(Controller))
            {
                ViewModel.Layer newLayer = new Layer(Controller, this);
                Controller.StateMachine.Layers.Add(newLayer);

                Controller.UndoManager.Add(new DeleteLayerRecord(Controller, newLayer));
            }
        }
    }

    internal class DeleteLayerMemberRecord : TrackableObjectRecord
    {
        protected override string UnitDescription => "Remove layer member";
        protected override int UnitType => (int)ActionTypes.RemoveLayerMember;

        int LayerId;
        int MemberId;



        internal DeleteLayerMemberRecord(ViewModel.ViewModelController controller, ObjectModel.LayerPosition layerPosition, ObjectModel.ITransitionEndpoint member) : base(ActionTypes.RemoveLayerMember, controller, layerPosition)
        {
            LayerId = layerPosition.LayerId;
            MemberId = member.Id;
#if DEBUGUNDOREDO
            Debug.WriteLine($@">>> DeleteLayerMemberRecord.DeleteLayerMemberRecord: Created {UnitDescription} record, ID: {Id}, LayerId:{LayerId}, NewMemberId: {MemberId}");
#endif
        }

        public override void Do(IOleUndoManager pUndoManager)
        {
#if DEBUGUNDOREDO
            Debug.WriteLine(">>> DeleteLayerMemberRecord.Do");
#endif
            using (new ViewModel.ViewModelController.GuiChangeBlock(Controller))
            {
                ViewModel.Layer layer = Controller.StateMachine.Find(LayerId) as ViewModel.Layer;
                ObjectModel.ITransitionEndpoint member = Controller.StateMachine.Find(MemberId) as ObjectModel.ITransitionEndpoint;
                ObjectModel.LayerPosition layerPosition = Controller.StateMachine.Find(Id) as ObjectModel.LayerPosition;
                Controller.UndoManager.Add(new AddLayerMemberRecord(Controller, layerPosition, member));
                member.LayerPositions.Remove(layerPosition);
                layer.Members.Remove(member);
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
            using (new ViewModel.ViewModelController.GuiChangeBlock(Controller))
            {
                ViewModel.Layer targetLayer = Controller.StateMachine.Layers.Where(r => r.Id == Id).First();
                AddLayerRecord addLayerRecord = new AddLayerRecord(Controller, targetLayer);
                Controller.StateMachine.Layers.Remove(targetLayer);

                Controller.UndoManager.Add(addLayerRecord);
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
