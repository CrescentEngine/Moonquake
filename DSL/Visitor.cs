// Copyright (C) 2026 ychgen, all rights reserved.

namespace Moonquake.DSL
{
    public enum EvaluationContext
    {
        GlobalScope,
        RootScope,
        SchemaScope,
        ModuleScope
    }
    public struct VisitorContext
    {
        public EvaluationContext    EvalContext = EvaluationContext.GlobalScope;
        public Construct.Construct? Construct   = null;

        public VisitorContext()
        {
        }
    }

    public class Visitor
    {
        private VisitorContext Context    = new VisitorContext();
        public List<Construct.Root> Roots = new List<Construct.Root>();

        public Visitor()
        {
        }

        public void Reset()
        {
            Context.EvalContext = EvaluationContext.GlobalScope;
            Context.Construct = null;
            Roots = new List<Construct.Root>();
        }

        public void Visit(AST Node)
        {
            switch (Node.Type)
            {
            case ASTType.Compound:
            {
                VisitCompound((CompoundAST) Node);
                break;
            }
            case ASTType.String:
            {
                VisitString((StringAST) Node);
                break;
            }
            case ASTType.Directive:
            {
                VisitDirective((DirectiveAST) Node);
                break;
            }
            case ASTType.FieldAssignment:
            {
                VisitFieldAssignment((FieldAssignmentAST) Node);
                break;
            }
            case ASTType.FieldAppendment:
            {
                VisitFieldAppendment((FieldAppendmentAST) Node);
                break;
            }
            case ASTType.FieldErasure:
            {
                VisitFieldErasure((FieldErasureAST) Node);
                break;
            }
            case ASTType.FieldUnassignment:
            {
                VisitFieldUnassignment((FieldUnassignmentAST) Node);
                break;
            }
            case ASTType.Array:
            {
                VisitArray((ArrayAST) Node);
                break;
            }
            }
        }

        private void VisitCompound(CompoundAST Node)
        {
            foreach (AST Subnode in Node.Compound)
            {
                Visit(Subnode);
            }
        }

        private void VisitString(StringAST Node)
        {
            return;
        }

        private void VisitDirective(DirectiveAST Node)
        {
            
        }

        private void VisitFieldAssignment(FieldAssignmentAST Node)
        {
            
        }

        private void VisitFieldAppendment(FieldAppendmentAST Node)
        {
            
        }

        private void VisitFieldErasure(FieldErasureAST Node)
        {
            
        }

        private void VisitFieldUnassignment(FieldUnassignmentAST Node)
        {
            
        }

        private void VisitArray(ArrayAST Node)
        {
            
        }
    }
}
