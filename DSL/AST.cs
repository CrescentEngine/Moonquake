// Copyright (C) 2026 ychgen, all rights reserved.

using System.Collections.Immutable;

namespace Moonquake.DSL
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
        DubiousAssignment, // strfield   ?= str-expr; OR arrayfield ?= array-expr;
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

        /// <summary>
        /// Never call without making sure type is same.
        /// Used to simplify syntax.
        /// Check AST.Type (of type ASTType) to ensure same type.
        /// </summary>
        public T As<T>() where T : AST
        {
            return ((T) this);
        }
        public virtual AST Clone()
        {
            // Base AST only copies Type and SrcInfo shallowly
            return (AST) MemberwiseClone();
        }
    }

    public class NoopAST : AST
    {
    }

    public class ExpressionAST : AST
    {
    }

    public class StatementAST : AST
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

        public override AST Clone()
        {
            CompoundAST Clone = new CompoundAST
            {
                SrcInfo = SrcInfo, // shallow copy; deep copy if needed
                Compound = Compound.Select(n => n.Clone()).ToList()
            };
            return Clone;
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
        public override AST Clone()
        {
            return new StringAST
            {
                Literal = Literal,
                Resolved = Resolved,
                SrcInfo = SrcInfo
            };
        }
    }

    public class DirectiveAST : StatementAST
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

    public class FieldAssignmentAST : StatementAST
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
        public override AST Clone()
        {
            return new FieldAssignmentAST
            {
                FieldName = FieldName,
                Value = (ExpressionAST) Value.Clone(),
                SrcInfo = SrcInfo
            };
        }
    }

    public class FieldAppendmentAST : StatementAST
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
        public override AST Clone()
        {
            return new FieldAppendmentAST
            {
                FieldName = FieldName,
                Value = (ExpressionAST) Value.Clone(),
                SrcInfo = SrcInfo
            };
        }
    }

    public class FieldErasureAST : StatementAST
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
        public override AST Clone()
        {
            return new FieldErasureAST
            {
                FieldName = FieldName,
                Value = (ExpressionAST) Value.Clone(),
                SrcInfo = SrcInfo
            };
        }
    }
    
    public class FieldUnassignmentAST : StatementAST
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
        public override AST Clone()
        {
            return new FieldUnassignmentAST
            {
                FieldName = FieldName,
                SrcInfo = SrcInfo
            };
        }
    }

    public class DubiousAssignmentAST : StatementAST
    {
        public string FieldName = "";
        public ExpressionAST Value = new ExpressionAST();

        public DubiousAssignmentAST()
        {
            Type = ASTType.DubiousAssignment;
        }
        public DubiousAssignmentAST(string InFieldName, ExpressionAST InValue) : this()
        {
            FieldName = InFieldName;
            Value = InValue;
        }
        public override AST Clone()
        {
            return new DubiousAssignmentAST
            {
                FieldName = FieldName,
                Value = (ExpressionAST) Value.Clone(),
                SrcInfo = SrcInfo
            };
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
        public override AST Clone()
        {
            return new ArrayAST
            {
                Value = Value.Select(s => (StringAST)s.Clone()).ToList(),
                SrcInfo = SrcInfo
            };
        }
    }
}
