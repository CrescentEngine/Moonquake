// Copyright (C) 2026 ychgen, all rights reserved.

using System.Collections.Immutable;
using System.Text;

namespace Moonquake.DSL
{
    public class Visitor
    {
        public ExecutionContext Context { get; }

        public Visitor(string InFilepath)
        {
            Context = new ExecutionContext(InFilepath, Visit);
        }
        public Visitor(ExecutionContext InContext)
        {
            Context = InContext;
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
            case ASTType.DubiousAssignment:
            {
                VisitDubiousAssignment((DubiousAssignmentAST) Node);
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
            // We swear by God don't touch shit if Resolved already contains anything, because it means it was resolved or hacked around for a good reason.
            // Parser never populates Resolved anyway, it only populates Literal.
            if (Node.Resolved != "")
            {
                return;
            }
            StringBuilder Builder = new StringBuilder(Node.Literal);

            if (Context.Frame.Construct is not null)
            {
                switch (Context.Frame.EvalContext)
                {
                case EvaluationContext.SchemaScope:
                case EvaluationContext.ModuleScope:
                {
                    Constructs.Module Mod = (Constructs.Module) Context.Frame.Construct;

                    Builder.Replace("%ModuleName%", Mod.Name);
                    Builder.Replace("%ModulePath%", Mod.Str("Path"));
                    Builder.Replace("%Configuration%", Moonquake.BuildOrder.Configuration);
                    Builder.Replace("%Platform%", Moonquake.BuildOrder.Platform.ToString());
                    Builder.Replace("%Architecture%", Moonquake.BuildOrder.Architecture.ToString());
                    
                    break;
                }
                default:
                {
                    break;
                }
                }
            }

            Node.Resolved = Builder.ToString();
        }

        private void VisitDirective(DirectiveAST Node)
        {
            if (Context.DeclaredVersion == LanguageVersion.Invalid)
            {
                if (Node.DirectiveName != DirectiveNames.DECLARE_VERSION)
                {
                    throw new Exception(
                        $"Visitor.Visit() error: The first statement to be made in a root description file must be a {DirectiveNames.DECLARE_VERSION}() directive."
                    );
                }
            }

            ASTType[] OverloadRequested = new ASTType[Node.Parameters.Compound.Count];
            for (int i = 0; i < Node.Parameters.Compound.Count; i++)
            {
                AST Parameter = Node.Parameters.Compound[i];
                OverloadRequested[i] = Parameter.Type;
                Visit(Parameter);
            }

            Directives.Directive? Directive;
            if (!Directives.DirectiveRegistry.TryGetDirective(Node.DirectiveName, out Directive) || Directive is null)
            {
                throw new Exception($"Visitor.VisitDirective() error: Visiting directive named '{Node.DirectiveName}()', but no such directive exists at all.");
            }

            if (!Directive.GetValidContexts().Contains(Context.Frame.EvalContext))
            {
                if (Directive.GetValidContexts().Length == 1)
                {
                    throw new Exception($"Visitor.VisitDirective() error: Directive '{Node.DirectiveName}()' can only be executed within a '{Directive.GetValidContexts()[0]}' context, but code is currently in a '{Context.Frame.EvalContext}'.");
                }
                else
                {
                    string ValidContextsText = $"{Directive.GetValidContexts()[0]}";
                    for (int i = 1; i < Directive.GetValidContexts().Length; i++)
                    {
                        string Context = Directive.GetValidContexts()[i].ToString();
                        if (i + 1 != Directive.GetValidContexts().Length)
                        {
                            ValidContextsText += $", {Context}";
                        }
                        else
                        {
                            ValidContextsText += $" and {Context}";
                        }
                    }
                    throw new Exception($"Visitor.VisitDirective() error: Directive '{Node.DirectiveName}()' can only be executed within '{ValidContextsText}' contexts, but code is currently in a '{Context.Frame.EvalContext}'.");
                }
            }

            if (Directive.IsBodily())
            {
                if (Node.Body is null)
                {
                    throw new Exception($"Visitor.VisitDirective() errror: Directive '{Node.DirectiveName}()' cannot have a body.");
                }
            }
            else if (Node.Body is not null)
            {
                throw new Exception($"Visitor.VisitDirective() errror: Directive '{Node.DirectiveName}()' must have a body.");
            }

            if (Context.Frame.EvalContext == EvaluationContext.DeferredScope && !Directive.IsConditional())
            {
                throw new Exception($"Visitor.VisitDirective() error: Directive '{Node.DirectiveName}' is not a conditional directive and therefore cannot be called in a deferred context.");
            }
            
            if (!Directive.HasOverload(OverloadRequested))
            {
                throw new Exception($"Visitor.VisitDirective(): Directive '{Node.DirectiveName}()' has no overload that accepts '{string.Join(", ", OverloadRequested)}'.");
            }
            Directive.GetOverload(OverloadRequested)!(Context, Node);
        }

        private void VisitFieldAssignment(FieldAssignmentAST Node)
        {
            if (Context.Frame.EvalContext == EvaluationContext.DeferredScope)
            {
                throw new Exception($"Visitor.VisitFieldAssignment() error: Cannot assign fields in deferred scope.");
            }
            if (Context.Frame.Construct is null)
            {
                throw new Exception($"Visitor.VisitFieldAssignment() error: Cannot assign to a field on global scope, no construct is in context.");
            }
            Visit(Node.Value); // Visit RHS (Right Hand Side, thing we are assigning the field.).
            Constructs.FieldMutationResult Result = Context.Frame.Construct.AssignField(Node.FieldName, Node.Value);
            if (Result != Constructs.FieldMutationResult.Successful)
            {
                throw new Exception($"Visitor.VisitFieldAssignment() error: Assignment to field '{Node.FieldName}' failed with error code '{Result}'.");
            }
        }

        private void VisitFieldAppendment(FieldAppendmentAST Node)
        {
            if (Context.Frame.EvalContext == EvaluationContext.DeferredScope)
            {
                throw new Exception($"Visitor.VisitFieldAppendment() error: Cannot append to fields in deferred scope.");
            }
            if (Context.Frame.Construct is null)
            {
                throw new Exception($"Visitor.VisitFieldAppendment() error: Cannot append to a field on global scope, no construct is in context.");
            }
            Visit(Node.Value); // Visit RHS (Right Hand Side, thing we are appending to the field.).
            Constructs.FieldMutationResult Result = Context.Frame.Construct.AppendToField(Node.FieldName, Node.Value);
            if (Result != Constructs.FieldMutationResult.Successful)
            {
                throw new Exception($"Visitor.VisitFieldAppendment() error: Appendment to field '{Node.FieldName}' failed with error code '{Result}'.");
            }
        }

        private void VisitFieldErasure(FieldErasureAST Node)
        {
            if (Context.Frame.EvalContext == EvaluationContext.DeferredScope)
            {
                throw new Exception($"Visitor.VisitFieldErasure() error: Cannot erase from fields in deferred scope.");
            }
            if (Context.Frame.Construct is null)
            {
                throw new Exception($"Visitor.VisitFieldErasure() error: Cannot erase from a field on global scope, no construct is in context.");
            }
            Visit(Node.Value); // Visit RHS (Right Hand Side, thing we are erasing from the field.).
            Constructs.FieldMutationResult Result = Context.Frame.Construct.EraseFromField(Node.FieldName, Node.Value);
            if (Result != Constructs.FieldMutationResult.Successful)
            {
                throw new Exception($"Visitor.VisitFieldErasure() error: Erasure from field '{Node.FieldName}' failed with error code '{Result}'.");
            }
        }

        private void VisitFieldUnassignment(FieldUnassignmentAST Node)
        {
            if (Context.Frame.EvalContext == EvaluationContext.DeferredScope)
            {
                throw new Exception($"Visitor.VisitFieldUnassignment() error: Cannot unassign fields in deferred scope.");
            }
            if (Context.Frame.Construct is null)
            {
                throw new Exception($"Visitor.VisitFieldUnassignment() error: Cannot unassign a field on global scope, no construct is in context.");
            }
            Constructs.FieldMutationResult Result = Context.Frame.Construct.UnassignField(Node.FieldName);
            if (Result != Constructs.FieldMutationResult.Successful)
            {
                throw new Exception($"Visitor.VisitFieldUnassignment() error: Unassignment of field '{Node.FieldName}' failed with error code '{Result}'.");
            }
        }

        private void VisitDubiousAssignment(DubiousAssignmentAST Node)
        {
            if (Context.Frame.Construct is null)
            {
                throw new Exception($"Visitor.VisitDubiousAssignment() error: Cannot dubiously assign a field on global scope, no construct is in context.");
            }
            if (Context.Frame.EvalContext != EvaluationContext.DeferredScope)
            {
                throw new Exception($"Visitor.VisitDubiousAssignment() error: Dubious assignment is only possible within the body of a Defer() directive's deferred scope.");
            }
            Visit(Node.Value); // Visit RHS (Right Hand Side, thing we are dubiously assigning the field.).
            Constructs.FieldMutationResult Result = Context.Frame.Construct.DubiousAssign(Node.FieldName, Node.Value);
            if
            (
                Result != Constructs.FieldMutationResult.Successful    &&
                Result != Constructs.FieldMutationResult.FieldNotUnset &&
                Result != Constructs.FieldMutationResult.FieldProtected
            )
            {
                throw new Exception($"Visitor.VisitDubiousAssignment() error: Dubious assignment of field '{Node.FieldName}' failed with error code '{Result}'.");
            }
        }

        private void VisitArray(ArrayAST Node)
        {
            foreach (StringAST String in Node.Value)
            {
                Visit(String);
            }
        }
    }
}
