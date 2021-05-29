using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LexicalAnalyzer
{
    public class ScannerStateMachineImplementation : ScannerStateMachine
    {
        TextReader InputStream;
        Lexeme CurrentLexeme;
        string SavedString;



        public ScannerStateMachineImplementation(TextReader inputStream)
        {
            InputStream = inputStream;
            CurrentLexeme = default(Lexeme);
        }

        public Lexeme Next
        {
            get
            {
                short c = (short)InputStream.Peek();
                if (c == -1)
                {
                    return Execute(EventTypes.EOF);
                }
                else
                {
                    return Execute(CharacterClassifier.Classify((short)c));
                }
            }
        }

        protected override Lexeme Advance()
        {
            InputStream.Read();
            short c = (short)InputStream.Peek();
            if (c == -1)
            {
                PostNormalPriorityEvent(EventTypes.EOF);
            }
            else
            {
                PostNormalPriorityEvent(CharacterClassifier.Classify((short)c));
            }
            return CurrentLexeme;
        }

        protected override Lexeme CheckForDoublePunc()
        {
            switch (SavedString + (char)InputStream.Peek())
            {
                case "==":
                case "!=":
                case ">=":
                case "<=":
                case "&&":
                case "||":
                    PostNormalPriorityEvent(EventTypes.Yes);
                    break;
                default:
                    PostNormalPriorityEvent(EventTypes.No);
                    break;
            }
            return default(Lexeme);
        }

        protected override Lexeme Clear()
        {
            SavedString = "";
            return CurrentLexeme;
        }

        protected override Lexeme EmitChar()
        {
            CurrentLexeme = new Lexeme() { LexemeType = Lexeme.LexemeTypes.Character, Value = SavedString, };
            return CurrentLexeme;
        }

        protected override Lexeme EmitEOF()
        {
            CurrentLexeme = new Lexeme() { LexemeType = Lexeme.LexemeTypes.EOF, Value = null, };
            return CurrentLexeme;
        }

        protected override Lexeme EmitError()
        {
            CurrentLexeme = new Lexeme() { LexemeType = Lexeme.LexemeTypes.Error, Value = null, };
            return CurrentLexeme;
        }

        protected override Lexeme EmitIdentifier()
        {
            CurrentLexeme = new Lexeme() { LexemeType = Lexeme.LexemeTypes.Identifier, Value = SavedString, };
            return CurrentLexeme;
        }

        protected override Lexeme EmitNumber()
        {
            CurrentLexeme = new Lexeme() { LexemeType = Lexeme.LexemeTypes.Number, Value = int.Parse(SavedString), };
            return CurrentLexeme;
        }

        protected override Lexeme EmitPunctuation()
        {
            CurrentLexeme = new Lexeme() { LexemeType = Lexeme.LexemeTypes.Punctuation, Value = SavedString, };
            return CurrentLexeme;
        }

        protected override Lexeme EmitString()
        {
            CurrentLexeme = new Lexeme() { LexemeType = Lexeme.LexemeTypes.String, Value = SavedString, };
            return CurrentLexeme;
        }

        protected override Lexeme IsMinus()
        {
            PostHighPriorityEvent((char)InputStream.Peek() == '-' ? EventTypes.Yes : EventTypes.No);
            return default(Lexeme);
        }

        protected override Lexeme IsPlus()
        {
            PostHighPriorityEvent((char)InputStream.Peek() == '+' ? EventTypes.Yes : EventTypes.No);
            return default(Lexeme);
        }

        protected override Lexeme Save()
        {
            SavedString += (char)InputStream.Peek();
            return default(Lexeme);
        }
    }
}
