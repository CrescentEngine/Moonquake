// Copyright (C) 2026 ychgen, all rights reserved.

namespace Moonquake.DSL.Directives
{
    public class ProtectAllFieldsDirective : Directive
    {
        public ProtectAllFieldsDirective()
        {
            ValidContexts = [ EvaluationContext.ModuleScope ];
            Overload([], Invoke);
        }

        protected void Invoke(ExecutionContext Context, DirectiveAST AST)
        {
            // Defer is not really "invoked", DECLARE_SCHEMA directive just takes a reference to its body.
        }
    }
}
