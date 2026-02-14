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
            Constructs.Module Module = (Constructs.Module) Context.Frame.Construct!;
            foreach (var KVP in Module.Fields)
            {
                KVP.Value.Protect();
            }
        }
    }
}
