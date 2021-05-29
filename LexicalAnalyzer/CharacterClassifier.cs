using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LexicalAnalyzer
{
    public static class CharacterClassifier
    {
        public static ScannerStateMachine.EventTypes Classify(short c)
        {
            if (c == -1)
            {
                return ScannerStateMachine.EventTypes.EOF;
            }
            else if (Char.IsDigit((char)c))
            {
                return ScannerStateMachine.EventTypes.IsNumber;
            }
            else if (char.IsLetter((char)c))
            {
                return ScannerStateMachine.EventTypes.IsAlpha;
            }
            else
            {
                switch ((char)c)
                {
                    case '\'':
                        return ScannerStateMachine.EventTypes.IsApost;
                    case '"':
                        return ScannerStateMachine.EventTypes.IsDblQuote;
                    case '-':
                    case '+':
                        return ScannerStateMachine.EventTypes.IsSign;
                    case '_':
                        return ScannerStateMachine.EventTypes.IsUnderscore;
                    case '/':
                        return ScannerStateMachine.EventTypes.IsSlash;
                    case '\n':
                    case '\r':
                        return ScannerStateMachine.EventTypes.EOL;
                    default:
                        if (char.IsWhiteSpace((char)c))
                        {
                            return ScannerStateMachine.EventTypes.IsWhiteSpc;
                        }
                        else
                        {
                            return ScannerStateMachine.EventTypes.IsPunc;
                        }
                }
            }
        }
    }
}
