
namespace SimpleStateMachineEditor.Utility
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Text.RegularExpressions;
    using EnvDTE;

    using Microsoft.VisualStudio;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.Shell.Interop;
    using Microsoft.VisualStudio.TextManager.Interop;

    internal class ErrorList
    {
        internal enum MessageCategory
        {
            Warning,
            Error,
            Severe,
        }

        ErrorListProvider _errorListProvider;




        internal ErrorList(IServiceProvider serviceProvider)
        {
            _errorListProvider = new ErrorListProvider(serviceProvider);
            _errorListProvider.ProviderName = "Simple State Machine Error List Provider";
            _errorListProvider.ProviderGuid = new Guid("DE387235-1718-4A64-A448-24E3E118150D");
        }

        internal void Clear()
        {
            _errorListProvider.Tasks.Clear();
        }

        internal void WriteVisualStudioErrorList(MessageCategory category, string text, string path, int line, int column)
        {
            TaskPriority priority = category == MessageCategory.Error ? TaskPriority.High : TaskPriority.Normal;
            TaskErrorCategory errorCategory = category >= MessageCategory.Error ? TaskErrorCategory.Error : TaskErrorCategory.Warning;

            ErrorTask task = new ErrorTask();
            task.Document = path;
            task.Line = line - 1; // The task list does +1 before showing this number.  
            task.Column = column - 1; // The task list does +1 before showing this number.  
            task.Text = text;
            task.Priority = priority; // High or Normal  
            task.ErrorCategory = errorCategory; // Error or Warning, no support for Message yet  
            task.Category = TaskCategory.BuildCompile;
            _errorListProvider.Tasks.Add(task);

            if (category == MessageCategory.Severe)
            {
                _errorListProvider.BringToFront();
            }
        }
    }
}
