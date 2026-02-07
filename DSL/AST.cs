// Copyright (C) 2026 ychgen, all rights reserved.

using System.Collections.Immutable;
using Moonquake.DSL;

namespace Moonquake
{
    public enum ASTType
    {
        Noop,              // No operation
        Compound,          // node consisting of multiple subnodes
        String,            // "...content..."
        Directive,         // directivename(...comma-led-exprs(str-expr OR array-expr)...);
                           // OR
                           // directivename(...comma-led-parameters(str-expr OR array-expr)...) { compound-ast };
        FieldAssignment,   // strfield    = str-expr; OR arrayfield  = array-expr;
        FieldAppendment,   // arrayfield += str-expr; OR arrayfield += array-expr;
        FieldErasure,      // arrayfield -= str-expr; OR arrayfield -= array-expr;
        FieldUnassignment, // ~somefield;
        Array              // [ ...comma-led-str-exprs... ]
    }

    class AstTypeArrayComparer : IEqualityComparer<ImmutableArray<ASTType>>
    {
        public bool Equals(ImmutableArray<ASTType> x, ImmutableArray<ASTType> y)
            => x.SequenceEqual(y);

        public int GetHashCode(ImmutableArray<ASTType> obj)
        {
            unchecked
            {
                int hash = 17;
                foreach (var t in obj)
                    hash = hash * 31 + t.GetHashCode();
                return hash;
            }
        }
}
    
    public class AST
    {
        public ASTType Type = ASTType.Noop;
        public SourceInfo SrcInfo;
    }

    public class NoopAST : AST
    {
    }

    public class ExpressionAST : AST
    {
    }

    public class CompoundAST : AST
    {
        public List<AST> Compound = new List<AST>();

        public CompoundAST()
        {
            Type = ASTType.Compound;
        }
        public CompoundAST(AST[] InCompound) : this()
        {
            Compound = new List<AST>(InCompound);
        }
        public CompoundAST(List<AST> InCompound) : this()
        {
            Compound = InCompound;
        }
    }

    public class StringAST : ExpressionAST
    {
        public string Literal  = "";
        public string Resolved = ""; // Filled by Visitor.

        public StringAST()
        {
            Type = ASTType.String;
        }
        public StringAST(string InLiteral) : this()
        {
            Literal = InLiteral;
        }
    }

    public class DirectiveAST : AST
    {
        public string DirectiveName = "";
        public CompoundAST Parameters = new CompoundAST();
        public CompoundAST? Body;

        public DirectiveAST()
        {
            Type = ASTType.Directive;
        }
        public DirectiveAST(string InDirectiveName) : this()
        {
            DirectiveName = InDirectiveName;
        }
        public DirectiveAST(string InDirectiveName, CompoundAST InParameters, CompoundAST? InBody = null) : this()
        {
            DirectiveName = InDirectiveName;
            Parameters = InParameters;
            Body = InBody;
        }
    }

    public class FieldAssignmentAST : AST
    {
        public string FieldName = "";
        public ExpressionAST Value = new ExpressionAST();

        public FieldAssignmentAST()
        {
            Type = ASTType.FieldAssignment;
        }
        public FieldAssignmentAST(string InFieldName, ExpressionAST InValue) : this()
        {
            FieldName = InFieldName;
            Value = InValue;
        }
    }

    public class FieldAppendmentAST : AST
    {
        public string FieldName = "";
        public ExpressionAST Value = new ExpressionAST();

        public FieldAppendmentAST()
        {
            Type = ASTType.FieldAppendment;
        }
        public FieldAppendmentAST(string InFieldName, ExpressionAST InValue) : this()
        {
            FieldName = InFieldName;
            Value = InValue;
        }
    }

    public class FieldErasureAST : AST
    {
        public string FieldName = "";
        public ExpressionAST Value = new ExpressionAST();

        public FieldErasureAST()
        {
            Type = ASTType.FieldErasure;
        }
        public FieldErasureAST(string InFieldName, ExpressionAST InValue) : this()
        {
            FieldName = InFieldName;
            Value = InValue;
        }
    }
    
    public class FieldUnassignmentAST : AST
    {
        public string FieldName = "";

        public FieldUnassignmentAST()
        {
            Type = ASTType.FieldUnassignment;
        }
        public FieldUnassignmentAST(string InFieldName) : this()
        {
            FieldName = InFieldName;
        }
    }

    public class ArrayAST : ExpressionAST
    {
        public List<StringAST> Value = new List<StringAST>();

        public ArrayAST()
        {
            Type = ASTType.Array;
        }
        public ArrayAST(StringAST[] InValue) : this()
        {
            Value = new List<StringAST>(InValue);
        }
        public ArrayAST(List<StringAST> InValue) : this()
        {
            Value = InValue;
        }

        public string[] ConstructLiteralStringArray()
        {
            string[] ArrayValue = new string[Value.Count];
            for (int i = 0; i < Value.Count; i++)
            {
                ArrayValue[i] = Value[i].Literal;
            }
            return ArrayValue;
        }

        public string[] ConstructResolvedStringArray()
        {
            string[] ArrayValue = new string[Value.Count];
            for (int i = 0; i < Value.Count; i++)
            {
                ArrayValue[i] = Value[i].Resolved;
            }
            return ArrayValue;
        }
    }
}
