// Copyright (C) 2026 ychgen, all rights reserved.

namespace Moonquake.DSL.Directives
{
    public class IfAnyPatternMatchDirective : Directive
    {
        public IfAnyPatternMatchDirective()
        {
            ValidContexts = [ EvaluationContext.RootScope, EvaluationContext.ModuleScope, EvaluationContext.SchemaScope, EvaluationContext.DeferredScope ];
            Flags         =   DirectiveFlags.Bodily | DirectiveFlags.Conditional;

            Overloads[[ ASTType.String, ASTType.Array ]] = Invoke;
        }

        protected void Invoke(ExecutionContext Context, DirectiveAST AST)
        {
            // No evaluation, unconditionally enter if we are in validation mode.
            if (Moonquake.BuildOrder.bValidationMode)
            {
                Context.Visit(AST.Body!);
                return;
            }

            string   Expr     = AST.Parameters.Compound[0].As<StringAST>().Resolved;
            string[] Patterns = AST.Parameters.Compound[1].As<ArrayAST>().ConstructResolvedStringArray();

            foreach (string Pattern in Patterns)
            {
                if (StringUtil.IsMatch(Expr, Pattern))
                {
                    Context.Visit(AST.Body!);
                    break;
                }
            }
        }
    }
}
