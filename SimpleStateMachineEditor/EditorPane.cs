/***************************************************************************

Copyright (c) Microsoft Corporation. All rights reserved.
THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.

***************************************************************************/

using System;
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.IO;
using System.Drawing;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Shell;
using EnvDTE;

using ISysServiceProvider = System.IServiceProvider;
using IOleServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;
using VSStd97CmdID = Microsoft.VisualStudio.VSConstants.VSStd97CmdID;

namespace SimpleStateMachineEditor
{
    /// <summary>
    /// This control hosts the editor and is responsible for
    /// handling the commands targeted to the editor 
    /// </summary>

    [ComVisible(true)]
    public sealed class EditorPane : WindowPane, IOleComponent, IVsDeferredDocView, IVsLinkedUndoClient, IVsToolboxUser, IVsWindowSearch
    {
        #region Fields
        private SimpleStateMachineEditorPackage _thisPackage;
        private string _fileName = string.Empty;
        private DesignerControl _vsDesignerControl;
        private IVsTextLines _textBuffer;
        private uint _componentId;
        private IOleUndoManager _undoManager;
        private ViewModel.ViewModelController _model;
        private SearchTask _searchTask;

        #endregion

        #region "Window.Pane Overrides"
        /// <summary>
        /// Constructor that calls the Microsoft.VisualStudio.Shell.WindowPane constructor then
        /// our initialization functions.
        /// </summary>
        /// <param name="package">Our Package instance.</param>
        public EditorPane(SimpleStateMachineEditorPackage package, string fileName, IVsTextLines textBuffer)
            : base(null)
        {
            _thisPackage = package;
            _fileName = fileName;
            _textBuffer = textBuffer;
        }

        protected override void OnClose()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (_thisPackage.ActiveDesignerControl == Content)
            {
                _thisPackage.ActiveDesignerControl = null;
            }

            ViewModelCoordinator.ReleaseViewModel(FileName);

            // unhook from Undo related services
            if (_undoManager != null)
            {
                IVsLinkCapableUndoManager linkCapableUndoMgr = (IVsLinkCapableUndoManager)_undoManager;
                if (linkCapableUndoMgr != null)
                {
                    linkCapableUndoMgr.UnadviseLinkedUndoClient();
                }

                // Throw away the undo stack etc.
                // It is important to â€œzombifyâ€ the undo manager when the owning object is shutting down.
                // This is done by calling IVsLifetimeControlledObject.SeverReferencesToOwner on the undoManager.
                // This call will clear the undo and redo stacks. This is particularly important to do if
                // your undo units hold references back to your object. It is also important if you use
                // "mdtStrict" linked undo transactions as this sample does (see IVsLinkedUndoTransactionManager). 
                // When one object involved in linked undo transactions clears its undo/redo stacks, then 
                // the stacks of the other documents involved in the linked transaction will also be cleared. 
                IVsLifetimeControlledObject lco = (IVsLifetimeControlledObject)_undoManager;
                lco.SeverReferencesToOwner();
                _undoManager = null;
            }

            IOleComponentManager mgr = GetService(typeof(SOleComponentManager)) as IOleComponentManager;
            mgr.FRevokeComponent(_componentId);

            Dispose(true);

            base.OnClose();
        }
        #endregion

        /// <summary>
        /// Called after the WindowPane has been sited with an IServiceProvider from the environment
        /// 
        protected override void Initialize()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            base.Initialize();

            // Create and initialize the editor
            #region Register with IOleComponentManager
            IOleComponentManager componentManager = (IOleComponentManager)GetService(typeof(SOleComponentManager));
            if (this._componentId == 0 && componentManager != null)
            {
                OLECRINFO[] crinfo = new OLECRINFO[1];
                crinfo[0].cbSize = (uint)Marshal.SizeOf(typeof(OLECRINFO));
                crinfo[0].grfcrf = (uint)_OLECRF.olecrfNeedIdleTime | (uint)_OLECRF.olecrfNeedPeriodicIdleTime;
                crinfo[0].grfcadvf = (uint)_OLECADVF.olecadvfModal | (uint)_OLECADVF.olecadvfRedrawOff | (uint)_OLECADVF.olecadvfWarningsOff;
                crinfo[0].uIdleTimeInterval = 100;
                int hr = componentManager.FRegisterComponent(this, crinfo, out this._componentId);
                ErrorHandler.Succeeded(hr);
            }
            #endregion

            ComponentResourceManager resources = new ComponentResourceManager(typeof(EditorPane));

            #region Hook Undo Manager
            // Attach an IOleUndoManager to our WindowFrame. Merely calling QueryService 
            // for the IOleUndoManager on the site of our IVsWindowPane causes an IOleUndoManager
            // to be created and attached to the IVsWindowFrame. The WindowFrame automaticall 
            // manages to route the undo related commands to the IOleUndoManager object.
            // Thus, our only responsibilty after this point is to add IOleUndoUnits to the 
            // IOleUndoManager (aka undo stack).
            _undoManager = (IOleUndoManager)GetService(typeof(SOleUndoManager));

            // In order to use the IVsLinkedUndoTransactionManager, it is required that you
            // advise for IVsLinkedUndoClient notifications. This gives you a callback at 
            // a point when there are intervening undos that are blocking a linked undo.
            // You are expected to activate your document window that has the intervening undos.
            if (_undoManager != null)
            {
                IVsLinkCapableUndoManager linkCapableUndoMgr = (IVsLinkCapableUndoManager)_undoManager;
                if (linkCapableUndoMgr != null)
                {
                    linkCapableUndoMgr.AdviseLinkedUndoClient(this);
                }
            }
            #endregion

            // hook up our 
            _model = ViewModelCoordinator.GetViewModel(_fileName, _textBuffer);
            _model.UndoManager = _undoManager;

            // Set up the build action for this file
            SetBuildAction(_fileName);

            // This is the user control hosted by the tool window; Note that, even if this class implements IDisposable,
            // we are not calling Dispose on this object. This is because ToolWindowPane calls Dispose on 
            // the object returned by the Content property.
            _vsDesignerControl = new DesignerControl(_thisPackage, _model, GetService(typeof(STrackSelection)) as ITrackSelection);
            Content = _vsDesignerControl;

            RegisterIndependentView(true);

            IMenuCommandService mcs = GetService(typeof(IMenuCommandService)) as IMenuCommandService;
            if (null != mcs)
            {
                // Now create one object derived from MenuCommnad for each command defined in
                // the CTC file and add it to the command service.

                // For each command we have to define its id that is a unique Guid/integer pair, then
                // create the OleMenuCommand object for this command. The EventHandler object is the
                // function that will be called when the user will select the command. Then we add the 
                // OleMenuCommand to the menu service.  The addCommand helper function does all this for us.
                AddCommand(mcs, VSConstants.GUID_VSStandardCommandSet97, (int)VSStd97CmdID.ViewCode,
                                new EventHandler(OnViewCode), new EventHandler(OnQueryViewCode));
            }

            InitializeToolbox();
        }

        private void SetBuildAction(string fileName)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            DTE dte = GetService(typeof(DTE)) as DTE;
            if (dte != null)
            {
                try
                {
                    //  Locate the Document object for this file

                    foreach (Document document in dte.Documents)
                    {
                        if (document.FullName.ToLowerInvariant() == fileName.ToLowerInvariant())
                        {
                            if (Path.GetExtension(fileName).ToLowerInvariant() == EditorFactory.Extension.ToLowerInvariant() && document.ProjectItem != null)
                            {
                                document.ProjectItem.Properties.Item("CustomTool").Value = Generators.SfsaGenerator.Name;
                            }
                            break;
                        }
                    }
                }
                catch
                {

                }
            }
        }

        /// <summary>
        /// returns the name of the file currently loaded
        /// </summary>
        public string FileName
        {
            get { return _fileName; }
        }

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                RegisterIndependentView(false);
            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// Gets an instance of the RunningDocumentTable (RDT) service which manages the set of currently open 
        /// documents in the environment and then notifies the client that an open document has changed
        /// </summary>
        private void NotifyDocChanged()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            // Make sure that we have a file name
            if (_fileName.Length == 0)
                return;

            // Get a reference to the Running Document Table
            IVsRunningDocumentTable runningDocTable = (IVsRunningDocumentTable)GetService(typeof(SVsRunningDocumentTable));

            // Lock the document
            uint docCookie;
            IVsHierarchy hierarchy;
            uint itemID;
            IntPtr docData;
            int hr = runningDocTable.FindAndLockDocument(
                (uint)_VSRDTFLAGS.RDT_ReadLock,
                _fileName,
                out hierarchy,
                out itemID,
                out docData,
                out docCookie
            );
            ErrorHandler.ThrowOnFailure(hr);

            // Send the notification
            hr = runningDocTable.NotifyDocumentChanged(docCookie, (uint)__VSRDTATTRIB.RDTA_DocDataReloaded);

            // Unlock the document.
            // Note that we have to unlock the document even if the previous call failed.
            ErrorHandler.ThrowOnFailure(runningDocTable.UnlockDocument((uint)_VSRDTFLAGS.RDT_ReadLock, docCookie));

            // Check ff the call to NotifyDocChanged failed.
            ErrorHandler.ThrowOnFailure(hr);
        }

        /// <summary>
        /// Helper function used to add commands using IMenuCommandService
        /// </summary>
        /// <param name="mcs"> The IMenuCommandService interface.</param>
        /// <param name="menuGroup"> This guid represents the menu group of the command.</param>
        /// <param name="cmdID"> The command ID of the command.</param>
        /// <param name="commandEvent"> An EventHandler which will be called whenever the command is invoked.</param>
        /// <param name="queryEvent"> An EventHandler which will be called whenever we want to query the status of
        /// the command.  If null is passed in here then no EventHandler will be added.</param>
        private static void AddCommand(IMenuCommandService mcs, Guid menuGroup, int cmdID,
                                       EventHandler commandEvent, EventHandler queryEvent)
        {
            // Create the OleMenuCommand from the menu group, command ID, and command event
            CommandID menuCommandID = new CommandID(menuGroup, cmdID);
            OleMenuCommand command = new OleMenuCommand(commandEvent, menuCommandID);

            // Add an event handler to BeforeQueryStatus if one was passed in
            if (null != queryEvent)
            {
                command.BeforeQueryStatus += queryEvent;
            }

            // Add the command using our IMenuCommandService instance
            mcs.AddCommand(command);
        }

        /// <summary>
        /// Registers an independent view with the IVsTextManager so that it knows
        /// the user is working with a view over the text buffer. This will trigger
        /// the text buffer to prompt the user whether to reload the file if it is
        /// edited outside of the environment.
        /// </summary>
        /// <param name="subscribe">True to subscribe, false to unsubscribe</param>
        void RegisterIndependentView(bool subscribe)
        {
            IVsTextManager textManager = (IVsTextManager)GetService(typeof(SVsTextManager));

            if (textManager != null)
            {
                if (subscribe)
                {
                    textManager.RegisterIndependentView(this, _textBuffer);
                }
                else
                {
                    textManager.UnregisterIndependentView(this, _textBuffer);
                }
            }
        }

        /// <summary>
        /// This method loads a localized string based on the specified resource.
        /// </summary>
        /// <param name="resourceName">Resource to load</param>
        /// <returns>String loaded for the specified resource</returns>
        internal string GetResourceString(string resourceName)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            string resourceValue;
            IVsResourceManager resourceManager = (IVsResourceManager)GetService(typeof(SVsResourceManager));
            if (resourceManager == null)
            {
                throw new InvalidOperationException("Could not get SVsResourceManager service. Make sure the package is Sited before calling this method");
            }
            Guid packageGuid = _thisPackage.GetType().GUID;
            int hr = resourceManager.LoadResourceString(ref packageGuid, -1, resourceName, out resourceValue);
            ErrorHandler.ThrowOnFailure(hr);
            return resourceValue;
        }

#region Commands

        private void OnQueryViewCode(object sender, EventArgs e)
        {
            OleMenuCommand command = (OleMenuCommand)sender;
            command.Enabled = true;
        }

        private void OnQuerySetIconDisplayColorsCombo(object sender, EventArgs e)
        {
            OleMenuCommand command = (OleMenuCommand)sender;
            command.Enabled = true;
        }

        private void OnQuerySetIconDisplayColorsComboList(object sender, EventArgs e)
        {
            OleMenuCommand command = (OleMenuCommand)sender;
            command.Enabled = true;
        }

        private void OnViewCode(object sender, EventArgs e)
        {
            ViewCode();
        }

        private void ViewCode()
        {
            Guid guid_source_code_text_editor = new Guid("{8B382828-6202-11d1-8870-0000F87579D2}");
            Guid guid_source_code_text_editor_with_encoding = new Guid("{C7747503-0E24-4FBE-BE4B-94180C3947D7}");
            
            // Open the referenced document using our editor.
            IVsWindowFrame frame;
            IVsUIHierarchy hierarchy;
            uint itemid;
            VsShellUtilities.OpenDocumentWithSpecificEditor(this, FileName,
                guid_source_code_text_editor, VSConstants.LOGVIEWID_Primary, out hierarchy, out itemid, out frame);
            ErrorHandler.ThrowOnFailure(frame.Show());
        }


#endregion

#region IVsLinkedUndoClient

        public int OnInterveningUnitBlockingLinkedUndo()
        {
            return VSConstants.E_FAIL;
        }

#endregion

#region IVsDeferredDocView

        /// <summary>
        /// Assigns out parameter with the Guid of the EditorFactory.
        /// </summary>
        /// <param name="pGuidCmdId">The output parameter that receives a value of the Guid of the EditorFactory.</param>
        /// <returns>S_OK if Marshal operations completed successfully.</returns>
        int IVsDeferredDocView.get_CmdUIGuid(out Guid pGuidCmdId)
        {
            pGuidCmdId = GuidList.guidDesignerEditorFactory;
            return VSConstants.S_OK;
        }

        /// <summary>
        /// Assigns out parameter with the document view being implemented.
        /// </summary>
        /// <param name="ppUnkDocView">The parameter that receives a reference to current view.</param>
        /// <returns>S_OK if Marshal operations completed successfully.</returns>
        [EnvironmentPermission(SecurityAction.Demand)]
        int IVsDeferredDocView.get_DocView(out IntPtr ppUnkDocView)
        {
            ppUnkDocView = Marshal.GetIUnknownForObject(this);
            return VSConstants.S_OK;
        }

#endregion

#region IOleComponent

        int IOleComponent.FContinueMessageLoop(uint uReason, IntPtr pvLoopData, MSG[] pMsgPeeked)
        {
            return VSConstants.S_OK;
        }

        int IOleComponent.FDoIdle(uint grfidlef)
        {
            if (_vsDesignerControl != null)
            {
                if (_vsDesignerControl.DoIdle())
                {
                    IVsUIShell uiShell = ((System.IServiceProvider)_thisPackage).GetService(typeof(SVsUIShell)) as IVsUIShell;
                    uiShell.RefreshPropertyBrowser(Utility.OleAutomation.DISPID_UNKNOWN);

                }
            }
            return VSConstants.S_OK;
        }

        int IOleComponent.FPreTranslateMessage(MSG[] pMsg)
        {
            return VSConstants.S_OK;
        }

        int IOleComponent.FQueryTerminate(int fPromptUser)
        {
            return 1; //true
        }

        int IOleComponent.FReserved1(uint dwReserved, uint message, IntPtr wParam, IntPtr lParam)
        {
            return VSConstants.S_OK;
        }

        IntPtr IOleComponent.HwndGetWindow(uint dwWhich, uint dwReserved)
        {
            return IntPtr.Zero;
        }

        void IOleComponent.OnActivationChange(IOleComponent pic, int fSameComponent, OLECRINFO[] pcrinfo, int fHostIsActivating, OLECHOSTINFO[] pchostinfo, uint dwReserved) { }
        void IOleComponent.OnAppActivate(int fActive, uint dwOtherThreadID) { }
        void IOleComponent.OnEnterState(uint uStateID, int fEnter) { }
        void IOleComponent.OnLoseActivation() { }
        void IOleComponent.Terminate() { }

#endregion
#region IVsToolboxUser

        void InitializeToolbox()
        {
#if false
            ThreadHelper.ThrowIfNotOnUIThread();

            // If toolboxData have initialized, skip creating a new one.
            if (toolboxData == null)
            {
                // Get the toolbox service
                IVsToolbox toolbox = (IVsToolbox)GetService(typeof(SVsToolbox));

                toolboxData = new OleDataObject[3];

                // Create the data objects that will store the data for the toolbox items.
                toolboxData[(int)Toolbox.ToolboxItemData.ToolboxItems.Event] = new OleDataObject();
                toolboxData[(int)Toolbox.ToolboxItemData.ToolboxItems.Event].SetData(typeof(Toolbox.ToolboxItemData), new Toolbox.ToolboxItemData(Toolbox.ToolboxItemData.ToolboxItems.Event));
                toolboxData[(int)Toolbox.ToolboxItemData.ToolboxItems.State] = new OleDataObject();
                toolboxData[(int)Toolbox.ToolboxItemData.ToolboxItems.State].SetData(typeof(Toolbox.ToolboxItemData), new Toolbox.ToolboxItemData(Toolbox.ToolboxItemData.ToolboxItems.State));
                toolboxData[(int)Toolbox.ToolboxItemData.ToolboxItems.Transition] = new OleDataObject();
                toolboxData[(int)Toolbox.ToolboxItemData.ToolboxItems.Transition].SetData(typeof(Toolbox.ToolboxItemData), new Toolbox.ToolboxItemData(Toolbox.ToolboxItemData.ToolboxItems.Transition));

                TBXITEMINFO[] itemInfo = new TBXITEMINFO[1];

                for (Toolbox.ToolboxItemData.ToolboxItems item = Toolbox.ToolboxItemData.ToolboxItems.Event; item <= Toolbox.ToolboxItemData.ToolboxItems.Transition; item++)
                {
                    itemInfo[0].bstrText = item.ToString();
                    itemInfo[0].hBmp = IntPtr.Zero;
                    itemInfo[0].dwFlags = (uint)__TBXITEMINFOFLAGS.TBXIF_DONTPERSIST;
                    ErrorHandler.ThrowOnFailure(toolbox.AddItem(toolboxData[(int)item], itemInfo, "State Machine"));
                }
            }
#endif
        }

        public int IsSupported(IDataObject pDO)
        {
            return VSConstants.S_FALSE;
        }

        public int ItemPicked(IDataObject pDO)
        {
            return VSConstants.S_FALSE;
        }
        #endregion

        #region IVsWindowSearch

        public IVsSearchTask CreateSearch(uint dwCookie, IVsSearchQuery pSearchQuery, IVsSearchCallback pSearchCallback)
        {
            _searchTask = null;
            if (pSearchQuery == null || string.IsNullOrWhiteSpace(pSearchQuery.SearchString) || pSearchCallback == null)
            {
                return null;
            }
            _searchTask = new SearchTask(dwCookie, pSearchQuery, pSearchCallback, _vsDesignerControl);
            return _searchTask;
        }

        public void ClearSearch()
        {
            _searchTask?.Reset();
        }

        public void ProvideSearchSettings(IVsUIDataSource pSearchSettings)
        {
        }

        public bool OnNavigationKeyDown(uint dwNavigationKey, uint dwModifiers)
        {
            return false;
        }

        public bool SearchEnabled => true;

        public Guid Category => _thisPackage.GetType().GUID;

        public IVsEnumWindowSearchFilters SearchFiltersEnum => null;

        public IVsEnumWindowSearchOptions SearchOptionsEnum => null;

#endregion
    }
}
