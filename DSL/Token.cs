// Copyright (C) 2026 ychgen, all rights reserved.

namespace Moonquake.DSL
{
    public enum TokenType
    {
        Invalid,
        Identifier,
        String,
        LeftParentheses,
        RightParantheses,
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
        EOF
    }

    public class Token
    {
        public string Value = "";
        public TokenType Type = TokenType.Invalid;

        public Token()
        {
        }

        public Token(char InValue, TokenType InType)
        {
            Value = InValue.ToString();
            Type  = InType;
        }

        public Token(string InValue, TokenType InType)
        {
            Value = InValue;
            Type  = InType;
        }

        public string GetValue() => Value;
        public TokenType GetTokenType() => Type;
    }
}
