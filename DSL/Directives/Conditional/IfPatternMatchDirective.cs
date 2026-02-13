// Copyright (C) 2026 ychgen, all rights reserved.

namespace Moonquake.DSL.Directives
{
    public class IfPatternMatchDirective : Directive
    {
        public IfPatternMatchDirective()
        {
            ValidContexts = [ EvaluationContext.RootScope, EvaluationContext.ModuleScope, EvaluationContext.SchemaScope, EvaluationContext.DeferredScope ];
            Flags         =   DirectiveFlags.Bodily | DirectiveFlags.Conditional;

            Overloads[[ ASTType.String, ASTType.String ]] = Invoke;
        }

        protected void Invoke(ExecutionContext Context, DirectiveAST AST)
        {
            // No evaluation, unconditionally enter if we are in validation mode.
            if (Moonquake.BuildOrder.Type == BuildType.Validate)
            {
                Context.Visit(AST.Body!);
                return;
            }

            string Expr    = AST.Parameters.Compound[0].As<StringAST>().Resolved;
            string Pattern = AST.Parameters.Compound[1].As<StringAST>().Resolved;

            if (StringUtil.IsMatch(Expr, Pattern))
            {
                Context.Visit(AST.Body!);
            }
        }
    }
}
