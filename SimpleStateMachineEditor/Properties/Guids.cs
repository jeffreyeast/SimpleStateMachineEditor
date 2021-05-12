// Guids.cs
// MUST match guids.h
using System;

namespace SimpleStateMachineEditor
{
    static class GuidList
    {
        public const string guidDesignerPkgString = "C846A7B8-B60E-488B-9362-E6F81D9FC027";
        public const string guidDesignerCmdSetString = "5F6D7DE8-0E66-4CDC-85DC-D8BF4530AECD";
        public const string guidDesignerEditorFactoryString = "9048D6F4-8DF2-4B36-9B96-5273F2A4FEEA";

        public static readonly Guid guidDesignerCmdSet = new Guid(guidDesignerCmdSetString);
        public static readonly Guid guidDesignerEditorFactory = new Guid(guidDesignerEditorFactoryString);

        public const string guidXmlChooserEditorFactory = @"{A8EEB691-4A11-4A96-A184-0D9413E4A742}";
    };
}