using Microsoft.VisualStudio.OLE.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleStateMachineEditor.UndoRedo
{
    /// <summary>
    /// Encapsulates a set of actions that should not be logged for undo/redo
    /// </summary>
    internal class DontLogBlock : IDisposable
    {
        ViewModel.ViewModelController Controller;


        internal DontLogBlock(ViewModel.ViewModelController controller)
        {
            Controller = controller;
            System.Threading.Interlocked.Increment(ref Controller.InhibitUndoRedoCount);
        }

        public void Dispose()
        {
            System.Threading.Interlocked.Decrement(ref Controller.InhibitUndoRedoCount);
            Controller = null;
        }
    }
}
