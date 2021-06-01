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

#region ParentRecord
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

    #endregion
}
