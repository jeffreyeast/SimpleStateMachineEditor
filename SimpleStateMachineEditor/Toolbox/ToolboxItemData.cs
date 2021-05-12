using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace SimpleStateMachineEditor.Toolbox
{
    /// <summary>
    /// This class implements data to be stored in ToolboxItem.
    /// This class needs to be serializable in order to be passed to the toolbox
    /// and back.
    /// 
    /// Moreover, this assembly path is required to be on VS probing paths to make
    /// deserialization successful. See ToolboxItemData.pkgdef.
    /// </summary>
    [Serializable()]
    public class ToolboxItemData : ISerializable
    {
        #region Fields
        public enum ToolboxItems
        {
            Event,
            State,
            Transition,
        }
        private ToolboxItems ToolboxItem;
        #endregion Fields

        #region Constructors
        /// <summary>
        /// Overloaded constructor.
        /// </summary>
        /// <param name="toolboxItem">Identifies the toolbox item type</param>
        public ToolboxItemData(ToolboxItems toolboxItem)
        {
            ToolboxItem = toolboxItem;
        }
        #endregion Constructors

        #region Properties
        /// <summary>
        /// Gets the ToolboxItemData ToolboxItems type.
        /// </summary>
        public ToolboxItems Item
        {
            get { return ToolboxItem; }
        }
        #endregion Properties

        internal ToolboxItemData(SerializationInfo info, StreamingContext context)
        {
            ToolboxItem = (ToolboxItems)info.GetValue("Item", typeof(ToolboxItems));
        }

        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info != null)
            {
                info.AddValue("Item", ToolboxItem);
            }
        }
    }
}
