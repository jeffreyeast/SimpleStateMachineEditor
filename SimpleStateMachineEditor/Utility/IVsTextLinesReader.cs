using Microsoft.VisualStudio;
using Microsoft.VisualStudio.TextManager.Interop;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleStateMachineEditor.Utility
{
    //  Provides a TextStream from an IVsTextLines

    internal class IVsTextLinesReader : TextReader, IDisposable
    {
        IVsTextLines TextLines;
        int LineCount;
        int LineSize;
        int CurrentLineIndex;
        int CurrentOffset;
        bool Errored;

        internal IVsTextLinesReader(IVsTextLines textLines)
        {
            TextLines = textLines;
            Errored = false;
            if (TextLines.GetLineCount(out LineCount) != VSConstants.S_OK)
            {
                Errored = true;
            }
            CurrentLineIndex = -1;
            CurrentOffset = 0;
            LineSize = -1;
        }

        public new void Dispose()
        {
            Dispose(true);
        }

        public new void Dispose(bool disposing)
        {
            if (disposing)
            {
                TextLines = null;
                Errored = true;
            }
        }
        public override int Read()
        {
            if (Errored || CurrentLineIndex >= LineCount)
            {
                return -1;
            }

            if (CurrentOffset >= LineSize)
            {
                CurrentLineIndex++;
                if (CurrentLineIndex >= LineCount)
                {
                    return -1;
                }
                if (TextLines.GetLengthOfLine(CurrentLineIndex, out LineSize) != VSConstants.S_OK)
                {
                    Errored = true;
                    return -1;
                }
            }

            if (TextLines.GetLineText(CurrentLineIndex, CurrentOffset, CurrentLineIndex, CurrentOffset, out string c) == VSConstants.S_OK)
            {
                if (c.Length != 1)
                {
                    Errored = true;
                    return -1;
                }
                CurrentOffset++;
                return (int)c[0];
            }
            else
            {
                Errored = true;
                return -1;
            }
        }

        public override int Read(char[] buffer, int index, int count)
        {
            if (Errored || CurrentLineIndex >= LineCount)
            {
                return 0;
            }

            if (CurrentOffset >= LineSize)
            {
                CurrentLineIndex++;
                CurrentOffset = 0;
                if (CurrentLineIndex >= LineCount)
                {
                    return 0;
                }
                if (TextLines.GetLengthOfLine(CurrentLineIndex, out LineSize) != VSConstants.S_OK)
                {
                    Errored = true;
                    return 0;
                }
            }

            int currentSize = Math.Min(count, LineSize - CurrentOffset);
            if (TextLines.GetLineText(CurrentLineIndex, CurrentOffset, CurrentLineIndex, CurrentOffset + currentSize, out string s) == VSConstants.S_OK)
            {
                if (s.Length < 1)
                {
                    Errored = true;
                    return 0;
                }
                CurrentOffset += s.Length;
                for (int i = 0; i < s.Length; i++)
                {
                    buffer[index + i] = s[i];
                }
                return (int)s.Length;
            }
            else
            {
                Errored = true;
                return 0;
            }
        }

        public override string ReadLine()
        {
            throw new NotImplementedException();
        }

        public override string ReadToEnd()
        {
            throw new NotImplementedException();
        }

    }
}
