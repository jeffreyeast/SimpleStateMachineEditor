using Microsoft.VisualStudio.TextManager.Interop;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleStateMachineEditor
{
    //  The ViewModelCoordinator is responsible for managing the junction between:
    //      - Editor panes. These are created on project load, when creating a new state machine, or by "New Window"
    //      - Text views of the file. These can be created on project load or by "View Code".
    //
    //  In all such cases, there is only one ViewModel instance for the document.

    internal static class ViewModelCoordinator
    {
        static Dictionary<string, ViewModel.ViewModelController> ViewModels = new Dictionary<string, ViewModel.ViewModelController>();

        internal static ViewModel.ViewModelController GetViewModel(string fileName, IVsTextLines textLines)
        {
            lock (ViewModels)
            {
                if (!ViewModels.ContainsKey(fileName))
                {
                    ViewModels.Add(fileName, new ViewModel.ViewModelController(textLines, fileName));
                }

                ViewModel.ViewModelController viewModelController = ViewModels[fileName];
                viewModelController.ReferenceCount++;
                return viewModelController;
            }
        }

        internal static void ReleaseViewModel(string fileName)
        {
            lock (ViewModels)
            {
                if (ViewModels.ContainsKey(fileName))
                {
                    ViewModel.ViewModelController viewModelController = ViewModels[fileName];
                    if (--viewModelController.ReferenceCount == 0)
                    {
                        ViewModels.Remove(fileName);
                        viewModelController.Dispose();
                    }
                }
            }
        }
    }
}
