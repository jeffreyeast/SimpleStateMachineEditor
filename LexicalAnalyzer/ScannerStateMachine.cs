// ------------------------------------------------------------------------------
// <auto-generated>
//     This file was generated by SfsaGenerator
// </auto-generated>
// ------------------------------------------------------------------------------

namespace LexicalAnalyzer
{
    using System;
    using System.Collections.Generic;
    using SimpleStateMachine;

    
    public abstract class ScannerStateMachine : StateMachineWithReturnValueBase<Lexeme>
    {
        public enum EventTypes
        {
            ///<summary>Wildcard</summary>
            Wildcard,
            EOF,
            EOL,
            IsAlpha,
            IsApost,
            IsDblQuote,
            IsNumber,
            IsPunc,
            IsSign,
            IsSlash,
            IsUnderscore,
            IsWhiteSpc,
            No,
            Yes,
        };

        static readonly string[] EventTypeNames = new string[]
        {
            "*",
            "EOF",
            "EOL",
            "IsAlpha",
            "IsApost",
            "IsDblQuote",
            "IsNumber",
            "IsPunc",
            "IsSign",
            "IsSlash",
            "IsUnderscore",
            "IsWhiteSpc",
            "No",
            "Yes",
        };

        static readonly string[] StateNames = new string[]
        {
            "BadString",
            "CheckingDblPunc",
            "CheckingForDblApost",
            "DblQuoteHit",
            "EndQuotedChar",
            "IdStart",
            "InBlockComment",
            "InComment",
            "IsDblPunc",
            "Number",
            "PossibleBlkComment",
            "PossibleComment",
            "PossibleEndOfBlock1",
            "PossibleEndOfBlock2",
            "PossibleNumber",
            "PosssibleCompoundOp",
            "QuotedChar",
            "SeekingEOL",
            "Start",
            "StartChar",
            "StartString",
            "UnexpectedEOF",
        };

        protected override int StartState => Array.IndexOf(StateNames, "Start");

        static readonly StateTypes[] StateClassifications = new StateTypes[]
        {
            StateTypes.Error,
            StateTypes.Normal,
            StateTypes.Normal,
            StateTypes.Normal,
            StateTypes.Normal,
            StateTypes.Normal,
            StateTypes.Normal,
            StateTypes.Normal,
            StateTypes.Normal,
            StateTypes.Normal,
            StateTypes.Normal,
            StateTypes.Normal,
            StateTypes.Normal,
            StateTypes.Normal,
            StateTypes.Normal,
            StateTypes.Normal,
            StateTypes.Normal,
            StateTypes.Normal,
            StateTypes.Normal,
            StateTypes.Normal,
            StateTypes.Normal,
            StateTypes.Error,
        };

        /// <summary>
        /// Action Routines
        /// 
        /// You must override each of these action routines in your implementation.
        /// </summary>

        protected abstract Lexeme Advance();
        protected abstract Lexeme CheckForDoublePunc();
        protected abstract Lexeme Clear();
        protected abstract Lexeme EmitChar();
        protected abstract Lexeme EmitEOF();
        protected abstract Lexeme EmitError();
        protected abstract Lexeme EmitIdentifier();
        protected abstract Lexeme EmitNumber();
        protected abstract Lexeme EmitPunctuation();
        protected abstract Lexeme EmitString();
        protected abstract Lexeme IsMinus();
        protected abstract Lexeme IsPlus();
        protected abstract Lexeme Save();


        protected override Transition<Action>[,] Transitions => _transitions;
        Transition<Action>[,] _transitions;

        public ScannerStateMachine() : base (StateClassifications, EventTypeNames, StateNames)
        {
            _transitions = new Transition<Action>[,]
            {
                { // BadString(0)
                    new Transition<Action>(0, new Action[] { base.InvalidTransition, }),  // *(0)
                    new Transition<Action>(0, new Action[] { base.InvalidTransition, }),  // EOF(1)
                    new Transition<Action>(0, new Action[] { base.InvalidTransition, }),  // EOL(2)
                    new Transition<Action>(0, new Action[] { base.InvalidTransition, }),  // IsAlpha(3)
                    new Transition<Action>(0, new Action[] { base.InvalidTransition, }),  // IsApost(4)
                    new Transition<Action>(0, new Action[] { base.InvalidTransition, }),  // IsDblQuote(5)
                    new Transition<Action>(0, new Action[] { base.InvalidTransition, }),  // IsNumber(6)
                    new Transition<Action>(0, new Action[] { base.InvalidTransition, }),  // IsPunc(7)
                    new Transition<Action>(0, new Action[] { base.InvalidTransition, }),  // IsSign(8)
                    new Transition<Action>(0, new Action[] { base.InvalidTransition, }),  // IsSlash(9)
                    new Transition<Action>(0, new Action[] { base.InvalidTransition, }),  // IsUnderscore(10)
                    new Transition<Action>(0, new Action[] { base.InvalidTransition, }),  // IsWhiteSpc(11)
                    new Transition<Action>(0, new Action[] { base.InvalidTransition, }),  // No(12)
                    new Transition<Action>(0, new Action[] { base.InvalidTransition, }),  // Yes(13)
                },
                { // CheckingDblPunc(1)
                    new Transition<Action>(0, new Action[] { base.InvalidTransition, }),  // *(0)
                    new Transition<Action>(0, new Action[] { base.InvalidTransition, }),  // EOF(1)
                    new Transition<Action>(0, new Action[] { base.InvalidTransition, }),  // EOL(2)
                    new Transition<Action>(0, new Action[] { base.InvalidTransition, }),  // IsAlpha(3)
                    new Transition<Action>(0, new Action[] { base.InvalidTransition, }),  // IsApost(4)
                    new Transition<Action>(0, new Action[] { base.InvalidTransition, }),  // IsDblQuote(5)
                    new Transition<Action>(0, new Action[] { base.InvalidTransition, }),  // IsNumber(6)
                    new Transition<Action>(0, new Action[] { base.InvalidTransition, }),  // IsPunc(7)
                    new Transition<Action>(0, new Action[] { base.InvalidTransition, }),  // IsSign(8)
                    new Transition<Action>(0, new Action[] { base.InvalidTransition, }),  // IsSlash(9)
                    new Transition<Action>(0, new Action[] { base.InvalidTransition, }),  // IsUnderscore(10)
                    new Transition<Action>(0, new Action[] { base.InvalidTransition, }),  // IsWhiteSpc(11)
                    new Transition<Action>(18, new Action[] { EmitPunctuation, Clear, }),  // No(12)
                    new Transition<Action>(8, new Action[] { Save, Advance, }),  // Yes(13)
                },
                { // CheckingForDblApost(2)
                    new Transition<Action>(0, new Action[] { EmitError, }),  // *(0)
                    new Transition<Action>(0, new Action[] { EmitError, }),  // EOF(1)
                    new Transition<Action>(0, new Action[] { EmitError, }),  // EOL(2)
                    new Transition<Action>(0, new Action[] { EmitError, }),  // IsAlpha(3)
                    new Transition<Action>(16, new Action[] { Save, Advance, }),  // IsApost(4)
                    new Transition<Action>(0, new Action[] { EmitError, }),  // IsDblQuote(5)
                    new Transition<Action>(0, new Action[] { EmitError, }),  // IsNumber(6)
                    new Transition<Action>(0, new Action[] { EmitError, }),  // IsPunc(7)
                    new Transition<Action>(0, new Action[] { EmitError, }),  // IsSign(8)
                    new Transition<Action>(0, new Action[] { EmitError, }),  // IsSlash(9)
                    new Transition<Action>(0, new Action[] { EmitError, }),  // IsUnderscore(10)
                    new Transition<Action>(0, new Action[] { EmitError, }),  // IsWhiteSpc(11)
                    new Transition<Action>(0, new Action[] { EmitError, }),  // No(12)
                    new Transition<Action>(0, new Action[] { EmitError, }),  // Yes(13)
                },
                { // DblQuoteHit(3)
                    new Transition<Action>(18, new Action[] { EmitString, Clear, }),  // *(0)
                    new Transition<Action>(18, new Action[] { EmitString, Clear, }),  // EOF(1)
                    new Transition<Action>(18, new Action[] { EmitString, Clear, }),  // EOL(2)
                    new Transition<Action>(18, new Action[] { EmitString, Clear, }),  // IsAlpha(3)
                    new Transition<Action>(18, new Action[] { EmitString, Clear, }),  // IsApost(4)
                    new Transition<Action>(20, new Action[] { Save, Advance, }),  // IsDblQuote(5)
                    new Transition<Action>(18, new Action[] { EmitString, Clear, }),  // IsNumber(6)
                    new Transition<Action>(18, new Action[] { EmitString, Clear, }),  // IsPunc(7)
                    new Transition<Action>(18, new Action[] { EmitString, Clear, }),  // IsSign(8)
                    new Transition<Action>(18, new Action[] { EmitString, Clear, }),  // IsSlash(9)
                    new Transition<Action>(18, new Action[] { EmitString, Clear, }),  // IsUnderscore(10)
                    new Transition<Action>(18, new Action[] { EmitString, Clear, }),  // IsWhiteSpc(11)
                    new Transition<Action>(18, new Action[] { EmitString, Clear, }),  // No(12)
                    new Transition<Action>(18, new Action[] { EmitString, Clear, }),  // Yes(13)
                },
                { // EndQuotedChar(4)
                    new Transition<Action>(18, new Action[] { EmitChar, Clear, }),  // *(0)
                    new Transition<Action>(18, new Action[] { EmitChar, Clear, }),  // EOF(1)
                    new Transition<Action>(18, new Action[] { EmitChar, Clear, }),  // EOL(2)
                    new Transition<Action>(18, new Action[] { EmitChar, Clear, }),  // IsAlpha(3)
                    new Transition<Action>(18, new Action[] { EmitChar, Clear, }),  // IsApost(4)
                    new Transition<Action>(18, new Action[] { EmitChar, Clear, }),  // IsDblQuote(5)
                    new Transition<Action>(18, new Action[] { EmitChar, Clear, }),  // IsNumber(6)
                    new Transition<Action>(18, new Action[] { EmitChar, Clear, }),  // IsPunc(7)
                    new Transition<Action>(18, new Action[] { EmitChar, Clear, }),  // IsSign(8)
                    new Transition<Action>(18, new Action[] { EmitChar, Clear, }),  // IsSlash(9)
                    new Transition<Action>(18, new Action[] { EmitChar, Clear, }),  // IsUnderscore(10)
                    new Transition<Action>(18, new Action[] { EmitChar, Clear, }),  // IsWhiteSpc(11)
                    new Transition<Action>(18, new Action[] { EmitChar, Clear, }),  // No(12)
                    new Transition<Action>(18, new Action[] { EmitChar, Clear, }),  // Yes(13)
                },
                { // IdStart(5)
                    new Transition<Action>(18, new Action[] { EmitIdentifier, Clear, }),  // *(0)
                    new Transition<Action>(18, new Action[] { EmitIdentifier, Clear, }),  // EOF(1)
                    new Transition<Action>(18, new Action[] { EmitIdentifier, Clear, }),  // EOL(2)
                    new Transition<Action>(5, new Action[] { Save, Advance, }),  // IsAlpha(3)
                    new Transition<Action>(18, new Action[] { EmitIdentifier, Clear, }),  // IsApost(4)
                    new Transition<Action>(18, new Action[] { EmitIdentifier, Clear, }),  // IsDblQuote(5)
                    new Transition<Action>(5, new Action[] { Save, Advance, }),  // IsNumber(6)
                    new Transition<Action>(18, new Action[] { EmitIdentifier, Clear, }),  // IsPunc(7)
                    new Transition<Action>(18, new Action[] { EmitIdentifier, Clear, }),  // IsSign(8)
                    new Transition<Action>(18, new Action[] { EmitIdentifier, Clear, }),  // IsSlash(9)
                    new Transition<Action>(5, new Action[] { Save, Advance, }),  // IsUnderscore(10)
                    new Transition<Action>(18, new Action[] { EmitIdentifier, Clear, }),  // IsWhiteSpc(11)
                    new Transition<Action>(18, new Action[] { EmitIdentifier, Clear, }),  // No(12)
                    new Transition<Action>(18, new Action[] { EmitIdentifier, Clear, }),  // Yes(13)
                },
                { // InBlockComment(6)
                    new Transition<Action>(6, new Action[] { Advance, }),  // *(0)
                    new Transition<Action>(21, new Action[] { EmitError, }),  // EOF(1)
                    new Transition<Action>(6, new Action[] { Advance, }),  // EOL(2)
                    new Transition<Action>(6, new Action[] { Advance, }),  // IsAlpha(3)
                    new Transition<Action>(6, new Action[] { Advance, }),  // IsApost(4)
                    new Transition<Action>(6, new Action[] { Advance, }),  // IsDblQuote(5)
                    new Transition<Action>(6, new Action[] { Advance, }),  // IsNumber(6)
                    new Transition<Action>(6, new Action[] { Advance, }),  // IsPunc(7)
                    new Transition<Action>(6, new Action[] { IsMinus, }),  // IsSign(8)
                    new Transition<Action>(6, new Action[] { Advance, }),  // IsSlash(9)
                    new Transition<Action>(6, new Action[] { Advance, }),  // IsUnderscore(10)
                    new Transition<Action>(6, new Action[] { Advance, }),  // IsWhiteSpc(11)
                    new Transition<Action>(6, new Action[] { Advance, }),  // No(12)
                    new Transition<Action>(12, new Action[] { Advance, }),  // Yes(13)
                },
                { // InComment(7)
                    new Transition<Action>(17, new Action[] { }),  // *(0)
                    new Transition<Action>(17, new Action[] { }),  // EOF(1)
                    new Transition<Action>(17, new Action[] { }),  // EOL(2)
                    new Transition<Action>(17, new Action[] { }),  // IsAlpha(3)
                    new Transition<Action>(17, new Action[] { }),  // IsApost(4)
                    new Transition<Action>(17, new Action[] { }),  // IsDblQuote(5)
                    new Transition<Action>(17, new Action[] { }),  // IsNumber(6)
                    new Transition<Action>(17, new Action[] { }),  // IsPunc(7)
                    new Transition<Action>(10, new Action[] { IsPlus, }),  // IsSign(8)
                    new Transition<Action>(17, new Action[] { }),  // IsSlash(9)
                    new Transition<Action>(17, new Action[] { }),  // IsUnderscore(10)
                    new Transition<Action>(17, new Action[] { }),  // IsWhiteSpc(11)
                    new Transition<Action>(17, new Action[] { }),  // No(12)
                    new Transition<Action>(17, new Action[] { }),  // Yes(13)
                },
                { // IsDblPunc(8)
                    new Transition<Action>(18, new Action[] { EmitPunctuation, Clear, }),  // *(0)
                    new Transition<Action>(18, new Action[] { EmitPunctuation, Clear, }),  // EOF(1)
                    new Transition<Action>(18, new Action[] { EmitPunctuation, Clear, }),  // EOL(2)
                    new Transition<Action>(18, new Action[] { EmitPunctuation, Clear, }),  // IsAlpha(3)
                    new Transition<Action>(18, new Action[] { EmitPunctuation, Clear, }),  // IsApost(4)
                    new Transition<Action>(18, new Action[] { EmitPunctuation, Clear, }),  // IsDblQuote(5)
                    new Transition<Action>(18, new Action[] { EmitPunctuation, Clear, }),  // IsNumber(6)
                    new Transition<Action>(18, new Action[] { EmitPunctuation, Clear, }),  // IsPunc(7)
                    new Transition<Action>(18, new Action[] { EmitPunctuation, Clear, }),  // IsSign(8)
                    new Transition<Action>(18, new Action[] { EmitPunctuation, Clear, }),  // IsSlash(9)
                    new Transition<Action>(18, new Action[] { EmitPunctuation, Clear, }),  // IsUnderscore(10)
                    new Transition<Action>(18, new Action[] { EmitPunctuation, Clear, }),  // IsWhiteSpc(11)
                    new Transition<Action>(18, new Action[] { EmitPunctuation, Clear, }),  // No(12)
                    new Transition<Action>(18, new Action[] { EmitPunctuation, Clear, }),  // Yes(13)
                },
                { // Number(9)
                    new Transition<Action>(18, new Action[] { EmitNumber, Clear, }),  // *(0)
                    new Transition<Action>(18, new Action[] { EmitNumber, Clear, }),  // EOF(1)
                    new Transition<Action>(18, new Action[] { EmitNumber, Clear, }),  // EOL(2)
                    new Transition<Action>(18, new Action[] { EmitNumber, Clear, }),  // IsAlpha(3)
                    new Transition<Action>(18, new Action[] { EmitNumber, Clear, }),  // IsApost(4)
                    new Transition<Action>(18, new Action[] { EmitNumber, Clear, }),  // IsDblQuote(5)
                    new Transition<Action>(9, new Action[] { Save, Advance, }),  // IsNumber(6)
                    new Transition<Action>(18, new Action[] { EmitNumber, Clear, }),  // IsPunc(7)
                    new Transition<Action>(18, new Action[] { EmitNumber, Clear, }),  // IsSign(8)
                    new Transition<Action>(18, new Action[] { EmitNumber, Clear, }),  // IsSlash(9)
                    new Transition<Action>(18, new Action[] { EmitNumber, Clear, }),  // IsUnderscore(10)
                    new Transition<Action>(18, new Action[] { EmitNumber, Clear, }),  // IsWhiteSpc(11)
                    new Transition<Action>(18, new Action[] { EmitNumber, Clear, }),  // No(12)
                    new Transition<Action>(18, new Action[] { EmitNumber, Clear, }),  // Yes(13)
                },
                { // PossibleBlkComment(10)
                    new Transition<Action>(0, new Action[] { base.InvalidTransition, }),  // *(0)
                    new Transition<Action>(0, new Action[] { base.InvalidTransition, }),  // EOF(1)
                    new Transition<Action>(0, new Action[] { base.InvalidTransition, }),  // EOL(2)
                    new Transition<Action>(0, new Action[] { base.InvalidTransition, }),  // IsAlpha(3)
                    new Transition<Action>(0, new Action[] { base.InvalidTransition, }),  // IsApost(4)
                    new Transition<Action>(0, new Action[] { base.InvalidTransition, }),  // IsDblQuote(5)
                    new Transition<Action>(0, new Action[] { base.InvalidTransition, }),  // IsNumber(6)
                    new Transition<Action>(0, new Action[] { base.InvalidTransition, }),  // IsPunc(7)
                    new Transition<Action>(0, new Action[] { base.InvalidTransition, }),  // IsSign(8)
                    new Transition<Action>(0, new Action[] { base.InvalidTransition, }),  // IsSlash(9)
                    new Transition<Action>(0, new Action[] { base.InvalidTransition, }),  // IsUnderscore(10)
                    new Transition<Action>(0, new Action[] { base.InvalidTransition, }),  // IsWhiteSpc(11)
                    new Transition<Action>(17, new Action[] { Advance, }),  // No(12)
                    new Transition<Action>(6, new Action[] { Advance, }),  // Yes(13)
                },
                { // PossibleComment(11)
                    new Transition<Action>(18, new Action[] { EmitPunctuation, Clear, }),  // *(0)
                    new Transition<Action>(18, new Action[] { EmitPunctuation, Clear, }),  // EOF(1)
                    new Transition<Action>(18, new Action[] { EmitPunctuation, Clear, }),  // EOL(2)
                    new Transition<Action>(18, new Action[] { EmitPunctuation, Clear, }),  // IsAlpha(3)
                    new Transition<Action>(18, new Action[] { EmitPunctuation, Clear, }),  // IsApost(4)
                    new Transition<Action>(18, new Action[] { EmitPunctuation, Clear, }),  // IsDblQuote(5)
                    new Transition<Action>(18, new Action[] { EmitPunctuation, Clear, }),  // IsNumber(6)
                    new Transition<Action>(18, new Action[] { EmitPunctuation, Clear, }),  // IsPunc(7)
                    new Transition<Action>(18, new Action[] { EmitPunctuation, Clear, }),  // IsSign(8)
                    new Transition<Action>(7, new Action[] { Clear, Advance, }),  // IsSlash(9)
                    new Transition<Action>(18, new Action[] { EmitPunctuation, Clear, }),  // IsUnderscore(10)
                    new Transition<Action>(18, new Action[] { EmitPunctuation, Clear, }),  // IsWhiteSpc(11)
                    new Transition<Action>(18, new Action[] { EmitPunctuation, Clear, }),  // No(12)
                    new Transition<Action>(18, new Action[] { EmitPunctuation, Clear, }),  // Yes(13)
                },
                { // PossibleEndOfBlock1(12)
                    new Transition<Action>(6, new Action[] { Advance, }),  // *(0)
                    new Transition<Action>(21, new Action[] { EmitError, }),  // EOF(1)
                    new Transition<Action>(6, new Action[] { Advance, }),  // EOL(2)
                    new Transition<Action>(6, new Action[] { Advance, }),  // IsAlpha(3)
                    new Transition<Action>(6, new Action[] { Advance, }),  // IsApost(4)
                    new Transition<Action>(6, new Action[] { Advance, }),  // IsDblQuote(5)
                    new Transition<Action>(6, new Action[] { Advance, }),  // IsNumber(6)
                    new Transition<Action>(6, new Action[] { Advance, }),  // IsPunc(7)
                    new Transition<Action>(6, new Action[] { Advance, }),  // IsSign(8)
                    new Transition<Action>(13, new Action[] { Advance, }),  // IsSlash(9)
                    new Transition<Action>(6, new Action[] { Advance, }),  // IsUnderscore(10)
                    new Transition<Action>(6, new Action[] { Advance, }),  // IsWhiteSpc(11)
                    new Transition<Action>(6, new Action[] { Advance, }),  // No(12)
                    new Transition<Action>(6, new Action[] { Advance, }),  // Yes(13)
                },
                { // PossibleEndOfBlock2(13)
                    new Transition<Action>(6, new Action[] { }),  // *(0)
                    new Transition<Action>(21, new Action[] { EmitError, }),  // EOF(1)
                    new Transition<Action>(6, new Action[] { }),  // EOL(2)
                    new Transition<Action>(6, new Action[] { }),  // IsAlpha(3)
                    new Transition<Action>(6, new Action[] { }),  // IsApost(4)
                    new Transition<Action>(6, new Action[] { }),  // IsDblQuote(5)
                    new Transition<Action>(6, new Action[] { }),  // IsNumber(6)
                    new Transition<Action>(6, new Action[] { }),  // IsPunc(7)
                    new Transition<Action>(6, new Action[] { }),  // IsSign(8)
                    new Transition<Action>(18, new Action[] { Advance, }),  // IsSlash(9)
                    new Transition<Action>(6, new Action[] { }),  // IsUnderscore(10)
                    new Transition<Action>(6, new Action[] { }),  // IsWhiteSpc(11)
                    new Transition<Action>(6, new Action[] { }),  // No(12)
                    new Transition<Action>(6, new Action[] { }),  // Yes(13)
                },
                { // PossibleNumber(14)
                    new Transition<Action>(18, new Action[] { EmitPunctuation, Clear, }),  // *(0)
                    new Transition<Action>(18, new Action[] { EmitPunctuation, Clear, }),  // EOF(1)
                    new Transition<Action>(18, new Action[] { EmitPunctuation, Clear, }),  // EOL(2)
                    new Transition<Action>(18, new Action[] { EmitPunctuation, Clear, }),  // IsAlpha(3)
                    new Transition<Action>(18, new Action[] { EmitPunctuation, Clear, }),  // IsApost(4)
                    new Transition<Action>(18, new Action[] { EmitPunctuation, Clear, }),  // IsDblQuote(5)
                    new Transition<Action>(9, new Action[] { Save, Advance, }),  // IsNumber(6)
                    new Transition<Action>(18, new Action[] { EmitPunctuation, Clear, }),  // IsPunc(7)
                    new Transition<Action>(18, new Action[] { EmitPunctuation, Clear, }),  // IsSign(8)
                    new Transition<Action>(18, new Action[] { EmitPunctuation, Clear, }),  // IsSlash(9)
                    new Transition<Action>(18, new Action[] { EmitPunctuation, Clear, }),  // IsUnderscore(10)
                    new Transition<Action>(18, new Action[] { EmitPunctuation, Clear, }),  // IsWhiteSpc(11)
                    new Transition<Action>(18, new Action[] { EmitPunctuation, Clear, }),  // No(12)
                    new Transition<Action>(18, new Action[] { EmitPunctuation, Clear, }),  // Yes(13)
                },
                { // PosssibleCompoundOp(15)
                    new Transition<Action>(18, new Action[] { EmitPunctuation, Clear, }),  // *(0)
                    new Transition<Action>(18, new Action[] { EmitPunctuation, Clear, }),  // EOF(1)
                    new Transition<Action>(18, new Action[] { EmitPunctuation, Clear, }),  // EOL(2)
                    new Transition<Action>(18, new Action[] { EmitPunctuation, Clear, }),  // IsAlpha(3)
                    new Transition<Action>(18, new Action[] { EmitPunctuation, Clear, }),  // IsApost(4)
                    new Transition<Action>(18, new Action[] { EmitPunctuation, Clear, }),  // IsDblQuote(5)
                    new Transition<Action>(18, new Action[] { EmitPunctuation, Clear, }),  // IsNumber(6)
                    new Transition<Action>(1, new Action[] { CheckForDoublePunc, }),  // IsPunc(7)
                    new Transition<Action>(18, new Action[] { EmitPunctuation, Clear, }),  // IsSign(8)
                    new Transition<Action>(18, new Action[] { EmitPunctuation, Clear, }),  // IsSlash(9)
                    new Transition<Action>(18, new Action[] { EmitPunctuation, Clear, }),  // IsUnderscore(10)
                    new Transition<Action>(18, new Action[] { EmitPunctuation, Clear, }),  // IsWhiteSpc(11)
                    new Transition<Action>(18, new Action[] { EmitPunctuation, Clear, }),  // No(12)
                    new Transition<Action>(18, new Action[] { EmitPunctuation, Clear, }),  // Yes(13)
                },
                { // QuotedChar(16)
                    new Transition<Action>(0, new Action[] { EmitError, }),  // *(0)
                    new Transition<Action>(0, new Action[] { EmitError, }),  // EOF(1)
                    new Transition<Action>(0, new Action[] { EmitError, }),  // EOL(2)
                    new Transition<Action>(0, new Action[] { EmitError, }),  // IsAlpha(3)
                    new Transition<Action>(4, new Action[] { Advance, }),  // IsApost(4)
                    new Transition<Action>(0, new Action[] { EmitError, }),  // IsDblQuote(5)
                    new Transition<Action>(0, new Action[] { EmitError, }),  // IsNumber(6)
                    new Transition<Action>(0, new Action[] { EmitError, }),  // IsPunc(7)
                    new Transition<Action>(0, new Action[] { EmitError, }),  // IsSign(8)
                    new Transition<Action>(0, new Action[] { EmitError, }),  // IsSlash(9)
                    new Transition<Action>(0, new Action[] { EmitError, }),  // IsUnderscore(10)
                    new Transition<Action>(0, new Action[] { EmitError, }),  // IsWhiteSpc(11)
                    new Transition<Action>(0, new Action[] { EmitError, }),  // No(12)
                    new Transition<Action>(0, new Action[] { EmitError, }),  // Yes(13)
                },
                { // SeekingEOL(17)
                    new Transition<Action>(17, new Action[] { Advance, }),  // *(0)
                    new Transition<Action>(18, new Action[] { }),  // EOF(1)
                    new Transition<Action>(18, new Action[] { }),  // EOL(2)
                    new Transition<Action>(17, new Action[] { Advance, }),  // IsAlpha(3)
                    new Transition<Action>(17, new Action[] { Advance, }),  // IsApost(4)
                    new Transition<Action>(17, new Action[] { Advance, }),  // IsDblQuote(5)
                    new Transition<Action>(17, new Action[] { Advance, }),  // IsNumber(6)
                    new Transition<Action>(17, new Action[] { Advance, }),  // IsPunc(7)
                    new Transition<Action>(17, new Action[] { Advance, }),  // IsSign(8)
                    new Transition<Action>(17, new Action[] { Advance, }),  // IsSlash(9)
                    new Transition<Action>(17, new Action[] { Advance, }),  // IsUnderscore(10)
                    new Transition<Action>(17, new Action[] { Advance, }),  // IsWhiteSpc(11)
                    new Transition<Action>(17, new Action[] { Advance, }),  // No(12)
                    new Transition<Action>(17, new Action[] { Advance, }),  // Yes(13)
                },
                { // Start(18)
                    new Transition<Action>(0, new Action[] { base.InvalidTransition, }),  // *(0)
                    new Transition<Action>(18, new Action[] { EmitEOF, }),  // EOF(1)
                    new Transition<Action>(18, new Action[] { Advance, }),  // EOL(2)
                    new Transition<Action>(5, new Action[] { Save, Advance, }),  // IsAlpha(3)
                    new Transition<Action>(19, new Action[] { Advance, }),  // IsApost(4)
                    new Transition<Action>(20, new Action[] { Advance, }),  // IsDblQuote(5)
                    new Transition<Action>(9, new Action[] { Save, Advance, }),  // IsNumber(6)
                    new Transition<Action>(15, new Action[] { Save, Advance, }),  // IsPunc(7)
                    new Transition<Action>(15, new Action[] { Save, Advance, }),  // IsSign(8)
                    new Transition<Action>(11, new Action[] { Save, Advance, }),  // IsSlash(9)
                    new Transition<Action>(5, new Action[] { Save, Advance, }),  // IsUnderscore(10)
                    new Transition<Action>(18, new Action[] { Advance, }),  // IsWhiteSpc(11)
                    new Transition<Action>(0, new Action[] { base.InvalidTransition, }),  // No(12)
                    new Transition<Action>(0, new Action[] { base.InvalidTransition, }),  // Yes(13)
                },
                { // StartChar(19)
                    new Transition<Action>(16, new Action[] { Save, Advance, }),  // *(0)
                    new Transition<Action>(21, new Action[] { EmitError, }),  // EOF(1)
                    new Transition<Action>(0, new Action[] { EmitError, }),  // EOL(2)
                    new Transition<Action>(16, new Action[] { Save, Advance, }),  // IsAlpha(3)
                    new Transition<Action>(2, new Action[] { Advance, }),  // IsApost(4)
                    new Transition<Action>(16, new Action[] { Save, Advance, }),  // IsDblQuote(5)
                    new Transition<Action>(16, new Action[] { Save, Advance, }),  // IsNumber(6)
                    new Transition<Action>(16, new Action[] { Save, Advance, }),  // IsPunc(7)
                    new Transition<Action>(16, new Action[] { Save, Advance, }),  // IsSign(8)
                    new Transition<Action>(16, new Action[] { Save, Advance, }),  // IsSlash(9)
                    new Transition<Action>(16, new Action[] { Save, Advance, }),  // IsUnderscore(10)
                    new Transition<Action>(16, new Action[] { Save, Advance, }),  // IsWhiteSpc(11)
                    new Transition<Action>(16, new Action[] { Save, Advance, }),  // No(12)
                    new Transition<Action>(16, new Action[] { Save, Advance, }),  // Yes(13)
                },
                { // StartString(20)
                    new Transition<Action>(20, new Action[] { Save, Advance, }),  // *(0)
                    new Transition<Action>(21, new Action[] { EmitError, }),  // EOF(1)
                    new Transition<Action>(0, new Action[] { EmitError, }),  // EOL(2)
                    new Transition<Action>(20, new Action[] { Save, Advance, }),  // IsAlpha(3)
                    new Transition<Action>(20, new Action[] { Save, Advance, }),  // IsApost(4)
                    new Transition<Action>(3, new Action[] { Advance, }),  // IsDblQuote(5)
                    new Transition<Action>(20, new Action[] { Save, Advance, }),  // IsNumber(6)
                    new Transition<Action>(20, new Action[] { Save, Advance, }),  // IsPunc(7)
                    new Transition<Action>(20, new Action[] { Save, Advance, }),  // IsSign(8)
                    new Transition<Action>(20, new Action[] { Save, Advance, }),  // IsSlash(9)
                    new Transition<Action>(20, new Action[] { Save, Advance, }),  // IsUnderscore(10)
                    new Transition<Action>(20, new Action[] { Save, Advance, }),  // IsWhiteSpc(11)
                    new Transition<Action>(20, new Action[] { Save, Advance, }),  // No(12)
                    new Transition<Action>(20, new Action[] { Save, Advance, }),  // Yes(13)
                },
                { // UnexpectedEOF(21)
                    new Transition<Action>(0, new Action[] { base.InvalidTransition, }),  // *(0)
                    new Transition<Action>(0, new Action[] { base.InvalidTransition, }),  // EOF(1)
                    new Transition<Action>(0, new Action[] { base.InvalidTransition, }),  // EOL(2)
                    new Transition<Action>(0, new Action[] { base.InvalidTransition, }),  // IsAlpha(3)
                    new Transition<Action>(0, new Action[] { base.InvalidTransition, }),  // IsApost(4)
                    new Transition<Action>(0, new Action[] { base.InvalidTransition, }),  // IsDblQuote(5)
                    new Transition<Action>(0, new Action[] { base.InvalidTransition, }),  // IsNumber(6)
                    new Transition<Action>(0, new Action[] { base.InvalidTransition, }),  // IsPunc(7)
                    new Transition<Action>(0, new Action[] { base.InvalidTransition, }),  // IsSign(8)
                    new Transition<Action>(0, new Action[] { base.InvalidTransition, }),  // IsSlash(9)
                    new Transition<Action>(0, new Action[] { base.InvalidTransition, }),  // IsUnderscore(10)
                    new Transition<Action>(0, new Action[] { base.InvalidTransition, }),  // IsWhiteSpc(11)
                    new Transition<Action>(0, new Action[] { base.InvalidTransition, }),  // No(12)
                    new Transition<Action>(0, new Action[] { base.InvalidTransition, }),  // Yes(13)
                },
            };
        }

        /// <summary>
        /// Invoked to execute the state machine.
        /// </summary>
        /// <param name="e">Provides an optional event type to post at normal priority before starting execution</param>
        /// <returns>A value of type R</returns>
        /// <exception cref="System.InvalidOperationException">Thrown if an event is chosen for 
        /// execution and no transition from the current state maches the event.
        /// </exception>
        /// <remarks>
        /// The state machine runs until one of the following conditions is met:
        /// - There are no remaining events to process
        /// - A stop or error state is entered
        /// - An event is encountered and no transition matches the event type
        /// - An action raises an exception
        /// For each state, the next event to be processed is chosen from the head of the
        /// internal event queue, and if no event is found, then the external event queue.
        /// </remarks>
        public Lexeme Execute(EventTypes? e = null)
        {
            return  base.Execute(e.HasValue ? (int)e.Value : default(int?));
        }

        /// <summary>
        /// Invoked by an action routine to post an internal (high-priority) event.
        /// <param name=eventType>Identifies the event to be posted</param>
        /// <exception cref="ArgumentOutOfRangeException">If the eventType is not valid</exception>
        /// </summary>
        protected void PostHighPriorityEvent(EventTypes eventType)
        {
            PostHighPriorityEvent((int)eventType);
        }

        /// <summary>
        /// Invoked by any code to post an external (lower-priority) event.
        /// <param name=eventType>Identifies the event to be posted</param>
        /// <exception cref="ArgumentOutOfRangeException">If the eventType is not valid</exception>
        /// </summary>
        public void PostNormalPriorityEvent(EventTypes eventType)
        {
            PostNormalPriorityEvent((int)eventType);
        }
    }
}