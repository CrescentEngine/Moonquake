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
            Constructs.Module Module = (Constructs.Module) Context.Frame.Construct!;
            foreach (var KVP in Module.Fields)
            {
                KVP.Value.Protect();
            }
        }
    }
}
