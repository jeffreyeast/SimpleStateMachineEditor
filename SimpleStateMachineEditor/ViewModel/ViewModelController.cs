using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SimpleStateMachineEditor.ViewModel
{
    // The ViewModelController is responsible for coordinating the views of the underlying model.
    public class ViewModelController : IDisposable, INotifyPropertyChanged
    {
        /// <summary>
        /// Encapsulates a set of actions that will change the view model. Intended for use with the using statement.
        /// </summary>
        internal class GuiChangeBlock : IDisposable
        {
            ViewModel.ViewModelController Controller;


            internal GuiChangeBlock(ViewModel.ViewModelController controller)
            {
                Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
                Controller = controller;
                controller?.BeginGuiChange();
            }

            public void Dispose()
            {
                Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
                Controller?.EndGuiChange();
            }
        }





        internal string FileName { get; set; }
        IVsTextLines TextBuffer { get; set; }
        internal int ReferenceCount { get; set; }
        public StateMachine StateMachine 
        {
            get => _stateMachine;
            set
            {
                if (_stateMachine != value)
                {
                    _stateMachine = value;
                    OnPropertyChanged("StateMachine");
                }
            }
        }
        StateMachine _stateMachine;
        int GuiChangeCount = 0;

        internal int NextId => Interlocked.Increment (ref _nextId);
        int _nextId = 0;
        internal Dictionary<int, ObjectModel.TrackableObject> AllFindableObjects;

        enum States
        {
            NotParsable,
            ParsedAndNotModified,
            ModifyiedByTextEditor,
            ModifyingByGuiEditor,
            ModifiedByGuiEditor,
            Reconciling,
        }


        States State;
        int IgnoreTextBufferChanges = 0;

        public event PropertyChangedEventHandler PropertyChanged;

        internal IOleUndoManager UndoManager { get; set; }
        internal volatile int InhibitUndoRedoCount;     //  Logging is disabled if count > 0

        bool LoggingIsEnabled => InhibitUndoRedoCount == 0;




        public ViewModelController(string stateMachineDescription)
        {
            using (StringReader reader = new StringReader(stateMachineDescription))
            {
                _nextId = 0;
                StateMachine.Deserialize(this, reader);
            }
        }

        public ViewModelController(IVsTextLines textBuffer, string fileName)
        {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();

            FileName = fileName;
            TextBuffer = textBuffer;
            ReconcileModelFromText();
        }

        internal void BeginGuiChange()
        {
            lock (this)
            {
                while (true)
                {
                    switch (State)
                    {
                        //  The Reconciling case is a little bit subtle -- because of the lock(this) statement above,
                        //  the only way to get here is through recursion on this thread, which means we're doing something
                        //  related to preparing the buffer for usage.

                        case States.Reconciling:
                            GuiChangeCount++;
                            return;

                        //  The remaining cases are straight-forward

                        case States.ModifiedByGuiEditor:
                        case States.ParsedAndNotModified:
                        case States.ModifyingByGuiEditor:
                            State = States.ModifyingByGuiEditor;
                            GuiChangeCount++;
                            return;

                        //  Try to incorporate the text editor changes 

                        case States.ModifyiedByTextEditor:
                            ReconcileModelFromText();
                            break;

                        //  Whatever is in the text buffer isn't currently usable

                        case States.NotParsable:
                            return;

                        default:
                            throw new NotImplementedException();
                    }
                }
            }
        }

        internal void DeserializeCleanup(ObjectModel.TrackableObject o)
        {
            _nextId = Math.Max(_nextId, o.Id);
        }

        public void Dispose()
        {
            StateMachine = null;
            AllFindableObjects = null;
        }

        /// <summary>
        /// Runs when there's nothing else happening
        /// </summary>
        /// <param name="textBuffer"></param>
        /// <returns>True if the buffer has been reconciled, False if the buffer has not been reconciled</returns>
        internal bool DoIdle()
        {
            lock(this)
            {
                switch (State)
                {
                    case States.ModifiedByGuiEditor:
                        ReconcileModelFromGui();
                        return State == States.ParsedAndNotModified;
                    case States.ModifyingByGuiEditor:
                        return false;
                    case States.ModifyiedByTextEditor:
                        ReconcileModelFromText();
                        return State == States.ParsedAndNotModified;
                    case States.ParsedAndNotModified:
                    case States.NotParsable:
                    case States.Reconciling:
                        return false;
                    default:
                        throw new NotImplementedException();
                }
            }
        }

        void EndGuiChange()
        {
            lock (this)
            {
                switch (State)
                {
                    case States.ModifyingByGuiEditor:
                        if (--GuiChangeCount == 0)
                        {
                            State = States.ModifiedByGuiEditor;
                        }
                        return;
                    case States.ModifyiedByTextEditor:
                    case States.ModifiedByGuiEditor:
                    case States.NotParsable:
                    case States.ParsedAndNotModified:
                        throw new InvalidOperationException("SimpleStateMachineEditor.ViewModelController: Invalid buffer change");
                    case States.Reconciling:
                        --GuiChangeCount;
                        return;
                    default:
                        throw new NotImplementedException();
                }
            }
        }

        internal void LogUndoAction(UndoRedo.UndoRedoRecord undoRedoRecord)
        {
            if (LoggingIsEnabled)
            {
                UndoManager.Add(undoRedoRecord);
            }
        }

        internal void NoteTextViewChange()
        {
            if (Interlocked.CompareExchange(ref IgnoreTextBufferChanges, 0, 0) == 0)
            {
                lock (this)
                {
                    switch (State)
                    {
                        case States.ModifiedByGuiEditor:
                        case States.ModifyingByGuiEditor:
                        case States.ModifyiedByTextEditor:
                            break;
                        case States.NotParsable:
                        case States.ParsedAndNotModified:
                            State = States.ModifyiedByTextEditor;
                            break;
                        case States.Reconciling:
                            break;
                        default:
                            throw new NotImplementedException();
                    }
                }
            }
        }

        void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        void ReconcileModelFromGui()
        {
            try
            {
                State = States.Reconciling;

                using (StringWriter writer = new StringWriter())
                {
                    int hr;

                    StateMachine.Serialize(writer);
                    IntPtr pNewText = Marshal.StringToCoTaskMemUni(writer.ToString());

                    try
                    {
                        if (TextBuffer.GetLineCount(out int lineCount) != VSConstants.S_OK)
                        {
                            throw new InvalidOperationException("SimpleStateMachneEditor.ViewModelController: Unable to load serialized state machine into buffer (1)");
                        }
                        if (TextBuffer.GetLengthOfLine(lineCount - 1, out int lastLineLength) != VSConstants.S_OK)
                        {
                            throw new InvalidOperationException("SimpleStateMachneEditor.ViewModelController: Unable to load serialized state machine into buffer (2)");
                        }

                        Interlocked.Exchange(ref IgnoreTextBufferChanges, 1);
                        try
                        {
                            if ((hr = TextBuffer.ReplaceLines(0, 0, lineCount - 1, lastLineLength, pNewText, writer.ToString().Length, new TextSpan[1])) != VSConstants.S_OK)
                            {
                                throw new InvalidOperationException("SimpleStateMachneEditor.ViewModelController: Unable to load serialized state machine into buffer, hr = " + hr.ToString());
                            }
                        }
                        finally
                        {
                            Interlocked.Exchange(ref IgnoreTextBufferChanges, 0);
                        }

                        State = States.ParsedAndNotModified;
                    }
                    finally
                    {
                        Marshal.FreeCoTaskMem(pNewText);
                    }
                }
            }
            catch
            {
                State = States.NotParsable;
            }
        }

        void ReconcileModelFromText()
        {
            try
            {
                State = States.Reconciling;

                using (Utility.IVsTextLinesReader reader = new Utility.IVsTextLinesReader(TextBuffer))
                {
                    _nextId = 0;
                    StateMachine.Deserialize(this, reader);
                }
                StateMachine?.ApplyDefaults(FileName);
                State = States.ParsedAndNotModified;
            }
            catch
            {
                State = States.NotParsable;
            }
        }
    }
}
