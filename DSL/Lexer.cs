// Copyright (C) 2026 ychgen, all rights reserved.

using System;

namespace Moonquake.DSL
{
    public class Lexer
    {
        private string Source = "";
        private int Index = 0;

        public bool IsDone() => Index >= Source.Length;

        public Lexer()
        {
        }
        public Lexer(string InSrc, int InStartPos = 0)
        {
            if (InStartPos >= InSrc.Length)
            {
                Console.WriteLine
                (
                    $"Lexer ctor error: Given source string has a length of {InSrc.Length}, however a start index of {InStartPos} was given. Index out of bounds."
                );
                return;
            }

            Source = InSrc;
            Index  = InStartPos;
        }

        /// <summary>
        /// Lexes the next token from current position into the source string.
        /// </summary>
        /// <returns>Next token within src string.</returns>
        public Token Lex()
        {
            if (IsDone())
            {
                return new Token("", TokenType.EOF);
            }
            if (char.IsWhiteSpace(Cur()))
            {
                SkipWhitespace();
            }
            if (IsDone()) // Have to recheck
            {
                return new Token("", TokenType.EOF);
            }

            if (Cur() == '#')
            {
                SkipComment();
            }
            if (IsDone()) // Have to recheck
            {
                return new Token("", TokenType.EOF);
            }

            if (Cur() == '"')
            {
                return CollectString();
            }

            if (char.IsLetter(Cur()) || Cur() == '_')
            {
                return CollectIdentifier();
            }

            char CurBeforeChk = Cur();
            Index++;
            switch (CurBeforeChk)
            {
            case '(': return new Token(CurBeforeChk, TokenType.LeftParentheses);
            case ')': return new Token(CurBeforeChk, TokenType.RightParantheses);
            case '[': return new Token(CurBeforeChk, TokenType.LeftBracket);
            case ']': return new Token(CurBeforeChk, TokenType.RightBracket);
            case '{': return new Token(CurBeforeChk, TokenType.LeftBrace);
            case '}': return new Token(CurBeforeChk, TokenType.RightBrace);
            case '=': return new Token(CurBeforeChk, TokenType.Equal);
            case ',': return new Token(CurBeforeChk, TokenType.Comma);
            case ';': return new Token(CurBeforeChk, TokenType.Semicolon);
            case '~': return new Token(CurBeforeChk, TokenType.Tilde);
            case '+': return new Token(CurBeforeChk, TokenType.Plus);
            case '-': return new Token(CurBeforeChk, TokenType.Minus);
            default:  break;
            }

            Console.WriteLine
            (
                $"Lexer.Lex() error: Unexpected token '{CurBeforeChk}' met on position {Index-1}. Returning new Token of IVLD type."
            );

            return new Token(Cur(), TokenType.Invalid);
        }

        private void SkipWhitespace()
        {
            int EndIndex = StringUtil.FindFirstNotOf(Source, StringUtil.Whitespace, Index);
            Index = EndIndex != -1 ? EndIndex : Source.Length;
        }

        /// <summary>
        /// Recursively skips all comments.
        /// </summary>
        private void SkipComment()
        {
            do
            {
                int NewLinePos = Source.IndexOf('\n', Index);
                if (NewLinePos == -1)
                {
                    Index = Source.Length;
                    return;
                }
                Index = NewLinePos + 1;
                if (!IsDone() && char.IsWhiteSpace(Cur()))
                {
                    SkipWhitespace();
                }
            } while (Cur() == '#');
        }

        private Token CollectIdentifier()
        {
            int EndIndex = StringUtil.FindFirstNotOf(Source, StringUtil.Alnum + '_', Index);
            if (EndIndex == -1)
            {
                EndIndex = Source.Length;
            }

            Token result = new Token(Source.Substring(Index, EndIndex - Index), TokenType.Identifier);
            Index = EndIndex;
            return result;
        }

        private Token CollectString()
        {
            int ReSearchIndex = -1;

            Index++; // Skip opening quote
        ReSearch:
            int ClosingQuoteIndex = Source.IndexOf('"', ReSearchIndex == -1 ? Index : ReSearchIndex);
            if (ClosingQuoteIndex == -1)
            {
                Console.WriteLine
                (
                    $"Lexer.CollectString() error: Quoted string literal that began at position {Index-1} has no matching closing quote. Returning IVLD token."
                );
                Index = Source.Length; // Lexing finished
                return new Token("", TokenType.Invalid);
            }
            // TODO: Support all escape sequences
            if (Source[ClosingQuoteIndex - 1] == '\\') // Handle \" (we only support \" escape sequence, wildcard sequence is handled by Visitor on the fly)
            {
                ReSearchIndex = ClosingQuoteIndex + 1;
                goto ReSearch;
            }
            Token result = new Token(Source.Substring(Index, ClosingQuoteIndex - Index), TokenType.String);
            result.Value = result.Value.Replace("\\\"", "\""); // Inefficient, should do fragment by fragment on the fly but it works anyway.
            Index = ClosingQuoteIndex + 1;
            return result;
        }

        private char Cur() => Source[Index];
    }
}
