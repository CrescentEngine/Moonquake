// Copyright (C) 2026 ychgen, all rights reserved.

namespace Moonquake.DSL.Directives
{
    public class DeferDirective : Directive
    {
        public DeferDirective()
        {
            ValidContexts = [ EvaluationContext.SchemaScope ];
            Flags         =   DirectiveFlags.Bodily;
            
            Overload([], Invoke);
        }

        protected void Invoke(ExecutionContext Context, DirectiveAST AST)
        {
            // Defer is not really "invoked", DECLARE_SCHEMA directive just takes a reference to its body.
        }
    }
}
