using Microsoft.VisualStudio.OLE.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleStateMachineEditor.UndoRedo
{
    /// <summary>
    /// Encapsulates a set of actions that should all be redone atomically
    /// </summary>
    internal class AtomicBlock : IDisposable
    {
        ViewModel.ViewModelController Controller;
        IOleParentUndoUnit UndoUnit;


        internal AtomicBlock(ViewModel.ViewModelController controller, string description)
        {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
            Controller = controller;
            UndoUnit = new UndoRedo.ParentRecord(controller, description);
            controller.UndoManager.Open(UndoUnit);
        }

        public void Dispose()
        {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
            Controller.UndoManager.Close(UndoUnit, 1);
        }
    }
}
