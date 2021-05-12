using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleStateMachineEditor
{
    //  This class monitors any text views to see if they change. If so, it notifies the view model (if there is one)

    [Export(typeof(IVsTextViewCreationListener))]
    [TextViewRole(PredefinedTextViewRoles.Editable)]
    [ContentType("text")]
    internal sealed class VsTextViewListener : IVsTextViewCreationListener
    {
        [Import]
        internal IVsEditorAdaptersFactoryService AdapterService = null;
        ITextView TextView;
        ITextBuffer2 TextBuffer = null;
        string FileName;
        bool IsRegisteredForChangeTracking = false;
        ViewModel.ViewModelController Model;

        public void VsTextViewCreated(IVsTextView textViewAdapter)
        {
            TextView = AdapterService.GetWpfTextView(textViewAdapter);
            if (TextView == null)
            {
                return;
            }

            TextBuffer = TextView.TextBuffer as ITextBuffer2;
            if (TextBuffer == null)
            {
                return;
            }

            if (TextView.TextBuffer.Properties.ContainsProperty(typeof(ITextDocument)))
            {
                FileName = (TextView.TextBuffer.Properties[typeof(ITextDocument)] as ITextDocument).FilePath;
                if (System.IO.Path.GetExtension(FileName).ToLower() == EditorFactory.Extension.ToLower() &&
                    textViewAdapter.GetBuffer(out IVsTextLines textLines) == VSConstants.S_OK)
                {
                    Model = ViewModelCoordinator.GetViewModel(FileName, textLines);
                    TextView.Closed += TextViewClosedHandler;
                    TextBuffer.ChangedOnBackground += TextBufferChangedHandler;
                    IsRegisteredForChangeTracking = true;
                }
            }
        }

        private void TextBufferChangedHandler(object sender, TextContentChangedEventArgs e)
        {
            Model.NoteTextViewChange();
        }

        private void TextViewClosedHandler(object sender, EventArgs e)
        {
            if (IsRegisteredForChangeTracking)
            {
                ViewModelCoordinator.ReleaseViewModel(FileName);
                TextView.Closed -= TextViewClosedHandler;
                TextBuffer.ChangedOnBackground -= TextBufferChangedHandler;
                IsRegisteredForChangeTracking = false;
            }
        }
    }
}
