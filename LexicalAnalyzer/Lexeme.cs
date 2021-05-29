using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LexicalAnalyzer
{
    public class Lexeme
    {
        public enum LexemeTypes
        {
            EOF,
            Identifier,
            Number,
            String,
            Character,
            Punctuation,
            Error,
        }

        public LexemeTypes LexemeType { get; set; }
        public object Value { get; set; }

        public override string ToString()
        {
            switch (LexemeType)
            {
                case LexemeTypes.EOF:
                    return "<EOF>";
                case LexemeTypes.Error:
                    return "<Error>";
                case LexemeTypes.Identifier:
                case LexemeTypes.Number:
                case LexemeTypes.Punctuation:
                case LexemeTypes.Character:
                case LexemeTypes.String:
                    return Value.ToString();
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
