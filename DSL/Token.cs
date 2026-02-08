// Copyright (C) 2026 ychgen, all rights reserved.

namespace Moonquake.DSL
{
    public enum TokenType
    {
        Invalid,
        Identifier,
        String,
        LeftParentheses,
        RightParantheses, // TODO: Fix typo, cannot be bothered with it currently
        LeftBracket,
        RightBracket,
        LeftBrace,
        RightBrace,
        Equal,
        Comma,
        Semicolon,
        Tilde,
        Plus,
        Minus,
        Question,
        EOF
    }

    public class Token
    {
        public string Value = "";
        public TokenType Type = TokenType.Invalid;
        public SourceInfo SrcInfo;

        public Token()
        {
        }
        public Token(char InValue, TokenType InType, SourceInfo InSrcInfo)
        {
            Value   = InValue.ToString();
            Type    = InType;
            SrcInfo = InSrcInfo;
        }
        public Token(string InValue, TokenType InType, SourceInfo InSrcInfo)
        {
            Value   = InValue;
            Type    = InType;
            SrcInfo = InSrcInfo;
        }

        public Token Originate(SourceInfo InSrcInfo)
        {
            SrcInfo = InSrcInfo;
            return this;
        }

        public override string ToString() => $"Token: {Value} {Type}";

        public string GetValue() => Value;
        public TokenType GetTokenType() => Type;
    }
}
