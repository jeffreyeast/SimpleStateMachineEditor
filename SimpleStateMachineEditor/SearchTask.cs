using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleStateMachineEditor
{
    /// <summary>
    /// Used to search the DesignerControl's icons for a string
    /// </summary>
    internal class SearchTask : IVsSearchTask
    {
        DesignerControl Designer;
        public uint Id { get; private set; }
        public IVsSearchQuery SearchQuery { get; private set; }
        public uint Status { get; private set; }
        public int ErrorCode { get; private set; }
        IVsSearchCallback SearchCallback;
        IEnumerable<ObjectModel.TrackableObject> SearchableObjects;
        uint StringCount;
        volatile bool StopRequested;



        internal SearchTask(uint dwCookie, IVsSearchQuery pSearchQuery, IVsSearchCallback pSearchCallback, DesignerControl designer)
        {
            Designer = designer;
            Id = dwCookie;
            SearchQuery = pSearchQuery;
            SearchCallback = pSearchCallback;
            Status = (uint)__VSSEARCHTASKSTATUS.STS_CREATED;
        }

        private IEnumerable<ObjectModel.TrackableObject> GatherObjects()
        {
            List<ObjectModel.TrackableObject> objects = new List<ObjectModel.TrackableObject>(Designer.Model.StateMachine.EventTypes.Count +
                                                                                              Designer.Model.StateMachine.Regions.Count +
                                                                                              Designer.Model.StateMachine.States.Count +
                                                                                              Designer.Model.StateMachine.Transitions.Count);

            objects.AddRange(Designer.Model.StateMachine.Actions);
            objects.AddRange(Designer.Model.StateMachine.EventTypes);
            objects.AddRange(Designer.Model.StateMachine.Regions);
            objects.AddRange(Designer.Model.StateMachine.States);
            objects.AddRange(Designer.Model.StateMachine.Transitions);

            return objects;
        }

        internal void Reset()
        {
            if (SearchableObjects != null)
            {
                foreach (ObjectModel.TrackableObject o in SearchableObjects)
                {
                    o.ResetSearch();
                }
            }
        }

        public void Start()
        {
            Status = (uint)__VSSEARCHTASKSTATUS.STS_STARTED;
            StringCount = 0;
            StopRequested = false;
            SearchableObjects = GatherObjects();

            foreach (ObjectModel.TrackableObject o in SearchableObjects)
            {
                StringCount += o.Search(SearchQuery.SearchString);
                if (StopRequested)
                {
                    break;
                }
            }

            SearchCallback.ReportComplete(this, StringCount);
        }

        public void Stop()
        {
            StopRequested = true;
            Status = (uint)__VSSEARCHTASKSTATUS.STS_STOPPED;
        }
    }
}
