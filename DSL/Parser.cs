// Copyright (C) 2026 ychgen, all rights reserved.

using Moonquake.DSL;

namespace Moonquake
{
    public class Parser
    {
        private Lexer Lexer = new Lexer();
        private Token PrevToken = new Token();
        private Token Token = new Token();

        public Parser()
        {
        }
        public Parser(Lexer InLexer)
        {
            Lexer = InLexer;
            PrevToken = Token = Lexer.Lex();
        }
        
        public CompoundAST Parse()
        {
            return ParseStatements();
        }

        private void Eat(TokenType Type)
        {
            if (Token.Type == Type)
            {
                PrevToken = Token;
                Token = Lexer.Lex();
            }
            else
            {
                Console.WriteLine($"Parser.Eat() error: Expected '{Type}' token, but got '{Token.Value}' of token type '{Token.Type}'!");
                Environment.Exit(1);
            }
        }

        private CompoundAST ParseStatements()
        {
            CompoundAST compound = new CompoundAST();
            compound.Compound.Add(ParseStatement());

            while (Token.Type == TokenType.Semicolon)
            {
                Eat(TokenType.Semicolon);
                compound.Compound.Add(ParseStatement());
            }

            return compound;
        }

        private AST ParseStatement()
        {
            switch (Token.Type)
            {
            case TokenType.Tilde:      return ParseFieldUnassignment();
            case TokenType.Identifier: return ParseIdentifier();
            }
            return new NoopAST();
        }

        private AST ParseExpr()
        {
            switch (Token.Type)
            {
            case TokenType.String:      return ParseString();
            case TokenType.LeftBracket: return ParseArray();
            }
            // TODO: Take care of this
            return new NoopAST();
        }

        private StringAST ParseString()
        {
            StringAST AST = new StringAST(Token.Value);
            Eat(TokenType.String);
            return AST;
        }

        private ArrayAST ParseArray()
        {
            ArrayAST AST = new ArrayAST();
            Eat(TokenType.LeftBracket); // opening bracket

            if (Token.Type != TokenType.RightBracket)
            {
                // See below for ParseString()
                AST.Value.Add(ParseString());
                while (Token.Type == TokenType.Comma)
                {
                    Eat(TokenType.Comma);
                    // If our DSL was flexible we'd use ParseExpr().
                    // However we only support arrays of string literals so we use ParseString().
                    StringAST String = ParseString();
                    AST.Value.Add(String);
                }
            }

            Eat(TokenType.RightBracket); // closing bracket
            return AST;
        }

        private AST ParseIdentifier()
        {
            Eat(TokenType.Identifier);

            switch (Token.Type)
            {
            case TokenType.LeftParentheses: return ParseDirective();
            case TokenType.Equal:           return ParseFieldAssignment();
            case TokenType.Plus:            return ParseFieldAppendment();
            case TokenType.Minus:           return ParseFieldErasure();
            }

            // TODO: Take care of this
            return new NoopAST();
        }

        private FieldAssignmentAST ParseFieldAssignment()
        {
            FieldAssignmentAST AST = new FieldAssignmentAST();
            AST.FieldName = PrevToken.Value;

            Eat(TokenType.Equal);
            AST.Value = ParseExpr();

            return AST;
        }

        private FieldAppendmentAST ParseFieldAppendment()
        {
            FieldAppendmentAST AST = new FieldAppendmentAST();
            AST.FieldName = PrevToken.Value;

            Eat(TokenType.Plus);
            Eat(TokenType.Equal);
            
            AST.Value = ParseExpr();
            return AST;
        }

        private FieldErasureAST ParseFieldErasure()
        {
            FieldErasureAST AST = new FieldErasureAST();
            AST.FieldName = PrevToken.Value;

            Eat(TokenType.Minus);
            Eat(TokenType.Equal);
            
            AST.Value = ParseExpr();
            return AST;
        }

        private FieldUnassignmentAST ParseFieldUnassignment()
        {
            Eat(TokenType.Tilde);

            FieldUnassignmentAST AST = new FieldUnassignmentAST();
            AST.FieldName = Token.Value;
            Eat(TokenType.Identifier);

            return AST;
        }

        private DirectiveAST ParseDirective()
        {
            DirectiveAST AST = new DirectiveAST();
            AST.DirectiveName = PrevToken.Value;

            Eat(TokenType.LeftParentheses);
            if (Token.Type != TokenType.RightParantheses)
            {
                AST.Parameters.Compound.Add(ParseExpr());
                while (Token.Type == TokenType.Comma)
                {
                    Eat(TokenType.Comma);
                    AST.Parameters.Compound.Add(ParseExpr());
                }
            }
            Eat(TokenType.RightParantheses);

            // Left curly brace? If so this directive has a body.
            if (Token.Type == TokenType.LeftBrace)
            {
                Eat(TokenType.LeftBrace);
                AST.Body = ParseStatements(); // parse statements within body
                // Why do we have a trailing NOOP?
                // ParseStatements() parses nice and good until parsing the last statement.
                // It then again sees a semicolon and eats it. Then calls ParseStatement()
                // However since current token is an RBRACE, it returns a NOOP AST.
                // After that the ParseStatements() loop ends successfully.
                //AST.Body.Compound.RemoveAt(AST.Body.Compound.Count); // Remove trailing NOOP
                // Let's not remove, it isn't harmful.

                Eat(TokenType.RightBrace);
            }

            return AST;
        }
    }
}
