// Copyright (C) 2026 ychgen, all rights reserved.

using System;

namespace Moonquake.DSL
{
    public struct SourceInfo
    {
        public string File;
        public int StartIndex, EndIndex;
        public int Line;
    }

    public class Lexer
    {
        private string Source = "";
        private int Index = 0;

        // for diagnostic SourceInfo generation
        private string SourceFile = "";
        private int CurrentLine = 1;

        public bool IsDone() => Index >= Source.Length;

        public Lexer()
        {
        }
        public Lexer(string InSourceFile, string InSrc, int InStartPos = 0)
        {
            SourceFile = InSourceFile;

            if (InStartPos >= InSrc.Length)
            {
                throw new Exception
                (
                    $"Lexer ctor error: Given source string has a length of {InSrc.Length}, however a start index of {InStartPos} was given. Index out of bounds."
                );
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
                return new Token("", TokenType.EOF, GetDoneInfo());
            }
            if (char.IsWhiteSpace(Cur()))
            {
                SkipWhitespace();
                if (IsDone()) // Have to recheck
                {
                    return new Token("", TokenType.EOF, GetDoneInfo());
                }
            }

            if (Cur() == '#')
            {
                SkipComment();
                if (IsDone()) // Have to recheck
                {
                    return new Token("", TokenType.EOF, GetDoneInfo());
                }
            }
            
            if (Cur() == '"')
            {
                return CollectString();
            }

            if (char.IsLetter(Cur()))
            {
                return CollectIdentifier();
            }

            SourceInfo SrcInfo;
            SrcInfo.File = SourceFile;
            SrcInfo.Line = CurrentLine;
            SrcInfo.StartIndex = Index;
            char CurBeforeChk = Cur();
            SrcInfo.EndIndex = Index++; // advance
            switch (CurBeforeChk)
            {
            case '(': return new Token(CurBeforeChk, TokenType.LeftParentheses, SrcInfo);
            case ')': return new Token(CurBeforeChk, TokenType.RightParantheses, SrcInfo);
            case '[': return new Token(CurBeforeChk, TokenType.LeftBracket, SrcInfo);
            case ']': return new Token(CurBeforeChk, TokenType.RightBracket, SrcInfo);
            case '{': return new Token(CurBeforeChk, TokenType.LeftBrace, SrcInfo);
            case '}': return new Token(CurBeforeChk, TokenType.RightBrace, SrcInfo);
            case '=': return new Token(CurBeforeChk, TokenType.Equal, SrcInfo);
            case ',': return new Token(CurBeforeChk, TokenType.Comma, SrcInfo);
            case ';': return new Token(CurBeforeChk, TokenType.Semicolon, SrcInfo);
            case '~': return new Token(CurBeforeChk, TokenType.Tilde, SrcInfo);
            case '+': return new Token(CurBeforeChk, TokenType.Plus, SrcInfo);
            case '-': return new Token(CurBeforeChk, TokenType.Minus, SrcInfo);
            default:  break;
            }

            throw new Exception
            (
                $"Lexer.Lex() error: Unexpected token '{CurBeforeChk}' met in file '{SourceFile}' at line {CurrentLine} at global index {Index}. This token is not one of the standard Moonquake Build Description Language tokens."
            );
        }

        private void SkipWhitespace()
        {
            int NonspaceIndex = StringUtil.FindFirstNotOf(Source, StringUtil.Whitespace, Index);
            if (NonspaceIndex == -1)
            {
                CurrentLine += Source.AsSpan(Index).Count('\n');
                Index = Source.Length;
                return;
            }

            ReadOnlySpan<char> WhitespaceChunk = Source.AsSpan().Slice(Index, NonspaceIndex - Index);
            CurrentLine += WhitespaceChunk.Count('\n'); // adjust line counter accordingly
            Index = NonspaceIndex;
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
                CurrentLine++; // comments occupy the entire line starting from its first char to the endl
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

            SourceInfo SrcInfo;
            SrcInfo.File = SourceFile;
            SrcInfo.StartIndex = Index;
            SrcInfo.EndIndex = EndIndex;
            SrcInfo.Line = CurrentLine;

            Token result = new Token(Source.Substring(Index, EndIndex - Index), TokenType.Identifier, SrcInfo);
            Index = EndIndex;
            return result;
        }

        private Token CollectString()
        {
            SourceInfo SrcInfo;
            SrcInfo.File = SourceFile;
            SrcInfo.StartIndex = Index++; // Advance from the opening quote.
            SrcInfo.Line = CurrentLine;

            string Literal = "";
            bool bSuccess = false;
            do
            {
                if (char.IsWhiteSpace(Cur()))
                {
                    throw new Exception($"Lexer.CollectString() error: String literal at line {CurrentLine} in file '{SourceFile}' contains a whitespace character after the opening and before the closing quote (if one exists.");
                }
                if (Cur() == '\\')
                {
                    Index++;
                    if (IsDone())
                    {
                        throw new Exception($"Lexer.CollectString() error: String literal at line {CurrentLine} in file '{SourceFile}' contains escape sequence at index {Index-1} but source string finished unexpectedly.");
                    }
                    switch (Cur())
                    {
                    case 'n':  Literal += '\n'; break;
                    case 't':  Literal += '\t'; break;
                    case 'v':  Literal += '\v'; break;
                    case 'b':  Literal += '\b'; break;
                    case 'r':  Literal += '\r'; break;
                    case 'f':  Literal += '\f'; break;
                    case 'a':  Literal += '\a'; break;
                    case '\\': Literal += '\\'; break;
                    case '\'': Literal += '\''; break;
                    case '"':  Literal += '"';  break;
                    default:
                    {
                        throw new Exception($"Lexer.CollectString() error: String literal at line {CurrentLine} in file '{SourceFile}' contains an invalid escape sequence (\\{Cur()}) at index {Index-1}.");
                    }
                    }
                    Index++;
                    continue;
                }
                else if (Cur() == '"')
                {
                    bSuccess = true;
                    break;
                }

                Literal += Cur();
                Index++;
            } while (!IsDone());

            if (!bSuccess)
            {
                Index = Source.Length; // already the case but why not do it explicitly
                throw new Exception($"Lexer.CollectString() error: String literal at line {CurrentLine} in file '{SourceFile}' has no closing quote. Going EOF.");
            }

            SrcInfo.EndIndex = Index++; // Skip closing quote.
            return new Token(Literal, TokenType.String, SrcInfo);

        // Old method
        //    int ReSearchIndex = -1;
        //
        //    Index++; // Skip opening quote
        //ReSearch:
        //    int ClosingQuoteIndex = Source.IndexOf('"', ReSearchIndex == -1 ? Index : ReSearchIndex);
        //    if (ClosingQuoteIndex == -1)
        //    {
        //        Console.WriteLine
        //        (
        //            $"Lexer.CollectString() error: Quoted string literal that began at position {Index-1} has no matching closing quote. Returning IVLD token."
        //        );
        //        Index = Source.Length; // Lexing finished
        //        return new Token("", TokenType.Invalid);
        //    }
        //    // TODO: Support all escape sequences
        //    if (Source[ClosingQuoteIndex - 1] == '\\') // Handle \" (we only support \" escape sequence, wildcard sequence is handled by Visitor on the fly)
        //    {
        //        ReSearchIndex = ClosingQuoteIndex + 1;
        //        goto ReSearch;
        //    }
        //    Token result = new Token(Source.Substring(Index, ClosingQuoteIndex - Index), TokenType.String);
        //    result.Value = result.Value.Replace("\\\"", "\""); // Inefficient, should do fragment by fragment on the fly but it works anyway.
        //    Index = ClosingQuoteIndex + 1;
        //    return result;
        }

        private char Cur() => Source[Index];

        private SourceInfo GetDoneInfo()
        {
            SourceInfo DoneInfo;
            DoneInfo.File = SourceFile;
            DoneInfo.StartIndex = DoneInfo.EndIndex = Index;
            DoneInfo.Line = CurrentLine;
            return DoneInfo;
        }
    }
}
