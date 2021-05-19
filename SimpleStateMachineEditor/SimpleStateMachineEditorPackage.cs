using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextTemplating.VSHost;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;

namespace SimpleStateMachineEditor
{
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    ///
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the 
    /// IVsPackage interface and uses the registration attributes defined in the framework to 
    /// register itself and its components with the shell.
    /// </summary>
    // This attribute tells the PkgDef creation utility (CreatePkgDef.exe) that this class is
    // a package.
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]

    // Register the class as a Designer View 
    [ProvideXmlEditorChooserDesignerView("SimpleStateMachineEditor", "sfsa", LogicalViewID.Designer, 0x61,
        DesignerLogicalViewEditor = typeof(EditorFactory),
        Namespace = "http://schemas.microsoft.com/developer/SimpleStateMachineEditor/2005",
        MatchExtensionAndNamespace = false)]
    // And which type of files we want to handle
    [ProvideEditorExtension(typeof(EditorFactory), EditorFactory.Extension, 0x41, NameResourceID = 102)]
    // We register that our editor supports LOGVIEWID_Designer logical view
    [ProvideEditorLogicalView(typeof(EditorFactory), LogicalViewID.Designer)]

    [EditorFactoryNotifyForProject("{E5E767DF-5B5E-4087-A426-836E19569C79}", EditorFactory.Extension, GuidList.guidXmlChooserEditorFactory)]
    // Microsoft Visual C# Project
    [EditorFactoryNotifyForProject("{F7DACEED-AD3B-4100-8F35-782BD05518D1}", EditorFactory.Extension, GuidList.guidXmlChooserEditorFactory)]

    // We build a code-behind file, which contains the C# code to define the state transitions
    [ProvideCodeGenerator(typeof(Generators.SfsaGenerator), Generators.SfsaGenerator.Name, Generators.SfsaGenerator.Description, true, ProjectSystem = ProvideCodeGeneratorAttribute.CSharpProjectGuid, RegisterCodeBase = true)]
    [ProvideCodeGeneratorExtension(Generators.SfsaGenerator.Name, EditorFactory.Extension)]

    // We have an options page in the tools/options menu, whereby configuration values can be viewed and changed
    [ProvideOptionPage(typeof(IconControls.OptionsPropertiesPage), "Simple State Machine Editor", "General", 0, 0, true)]

    // The Actions window is only visible when a pane containing a Simple State Machine is active. It's default location is
    // as a tab of the Solutions group. However, once it's there, it stays, and may load very eary in the IDE's life. 
    [ProvideToolWindow(typeof(IconControls.ActionsToolWindow), Style = VsDockStyle.Tabbed, Window = "3AE79031-E1BC-11D0-8F78-00A0C9110057")]    
    [ProvideToolWindowVisibility(typeof(IconControls.ActionsToolWindow), PackageGuids.guidDesignControlCmdUIContextString)]

    //  Define a command context that will be active when a pane containing a Simple State Machine is active. We use
    //  this in the .VSCT file to control menu and toolbar visibility
    [ProvideUIContextRule(PackageGuids.guidDesignControlCmdUIContextString,
        name: "Simple State Machine UI context",
        expression: "(SingleProject | MultipleProjects) & DotSFSA",
        termNames: new[] { "SingleProject", "MultipleProjects", "DotSFSA"},
        termValues: new[] { VSConstants.UICONTEXT.SolutionHasSingleProject_string, VSConstants.UICONTEXT.SolutionHasMultipleProjects_string, "HierSingleSelectionName:.sfsa$"})]

    // This attribute is used to register the informations needed to show the this package
    // in the Help/About dialog of Visual Studio.
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    // This attribute is needed to let the shell know that this package exposes some menus.
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(GuidList.guidDesignerPkgString)]
    [ProvideToolWindow(typeof(SimpleStateMachineEditor.IconControls.ActionsToolWindow))]
    public sealed class SimpleStateMachineEditorPackage : AsyncPackage, INotifyPropertyChanged
    {
        IVsMonitorSelection SelectionMonitorSvc;
        internal IconControls.OptionsPropertiesPage OptionsPropertiesPage;
        DTE DTE;

        internal DesignerControl ActiveDesignerControl
        {
            get => _activeDesignerControl;
            set
            {
                if (_activeDesignerControl != value)
                {
                    _activeDesignerControl = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ActiveDesignerControl"));
                }
            }
        }
        DesignerControl _activeDesignerControl;

        public event PropertyChangedEventHandler PropertyChanged;



        /// <summary>
        /// Default constructor of the package.
        /// Inside this method you can place any initialization code that does not require 
        /// any Visual Studio service because at this point the package object is created but 
        /// not sited yet inside Visual Studio environment. The place to do all the other 
        /// initialization is the Initialize method.
        /// </summary>
        public SimpleStateMachineEditorPackage()
        {
        }



        #region Package Members


        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token to monitor for initialization cancellation, which can occur when VS is shutting down.</param>
        /// <param name="progress">A provider for progress updates.</param>
        /// <returns>A task representing the async work of package initialization, or an already completed task if there is none. Do not return null from this method.</returns>
        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            await base.InitializeAsync(cancellationToken, progress);


            // When initialized asynchronously, the current thread may be a background thread at this point.
            // Do any initialization that requires the UI thread after switching to the UI thread.
            await this.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);


            // Register for service activity events
            DTE = await GetServiceAsync(typeof(DTE)) as DTE;
            if (DTE == null)
            {
                throw new InvalidOperationException();
            }
            DTE.Events.WindowEvents.WindowActivated += WindowActivatedHandler;

            SelectionMonitorSvc = await GetServiceAsync(typeof(SVsShellMonitorSelection)) as IVsMonitorSelection;
            if (SelectionMonitorSvc == null)
            {
                throw new InvalidOperationException("Unable to access SVsShellMonitorSelection service");
            }

            //Create Editor Factory. Note that the base Package class will call Dispose on it.
            base.RegisterEditorFactory(new EditorFactory(this));

            //  Register the commands on the toolbar

            await Commands.AddEventTypeCommand.InitializeAsync(this);
            await Commands.AddRegionCommand.InitializeAsync(this);
            await Commands.AddStateCommand.InitializeAsync(this);
            await Commands.AddTransitionCommand.InitializeAsync(this);
            await Commands.DeleteCommand.InitializeAsync(this);
            await Commands.AlignmentCommand.InitializeAsync(this, PackageIds.AlignLeftCommandId);
            await Commands.AlignmentCommand.InitializeAsync(this, PackageIds.AlignHorizontalCenterCommandId);
            await Commands.AlignmentCommand.InitializeAsync(this, PackageIds.AlignRightCommandId);
            await Commands.AlignmentCommand.InitializeAsync(this, PackageIds.AlignTopCommandId);
            await Commands.AlignmentCommand.InitializeAsync(this, PackageIds.AlignVerticalCenterCommandId);
            await Commands.AlignmentCommand.InitializeAsync(this, PackageIds.AlignBottomCommandId);
            await Commands.AlignmentCommand.InitializeAsync(this, PackageIds.DistributeHorizontallyCommandId);
            await Commands.AlignmentCommand.InitializeAsync(this, PackageIds.DistributeVerticallyCommandId);

            OptionsPropertiesPage = (IconControls.OptionsPropertiesPage)GetDialogPage(typeof(IconControls.OptionsPropertiesPage));
            await SimpleStateMachineEditor.IconControls.ActionsToolWindowCommand.InitializeAsync(this);
        }

        private void WindowActivatedHandler(Window GotFocus, Window LostFocus)
        {
            if (GotFocus.Object is EditorPane editorPane)
            {
                ActiveDesignerControl = editorPane.Content as DesignerControl;
                IconControls.ActionsToolWindow toolWindow = FindToolWindow(typeof(IconControls.ActionsToolWindow), 0, false) as IconControls.ActionsToolWindow;
                if (toolWindow != null)
                {
                    toolWindow.Designer = ActiveDesignerControl;
                }
                else
                {
                    string designerName = "<null>";
                    if (ActiveDesignerControl.Model.StateMachine != null)
                    {
                        designerName = ActiveDesignerControl.Model.StateMachine.Name ?? "<unnamed>";
                    }
                    Debug.WriteLine($@">>>SimpleStateMachineEditorPackage.WindowActivatedHandler, ActiveDesignerControl = {designerName}, ToolWindow is null");
                }
            }
        }
        #endregion

    }
}
