// Copyright (C) 2026 ychgen, all rights reserved.

namespace Moonquake.DSL.Directives
{
    public class ProtectFieldDirective : Directive
    {
        public ProtectFieldDirective()
        {
            ValidContexts = [ EvaluationContext.ModuleScope ];
            
            Overload([ ASTType.String ], Invoke);
        }

        protected void Invoke(ExecutionContext Context, DirectiveAST AST)
        {
            string FieldName = AST.Parameters.Compound[0].As<StringAST>().Resolved;
            Constructs.Module Module = (Constructs.Module) Context.Frame.Construct!;
            Constructs.ConstructField? Field;
            if (!Module.Fields.TryGetValue(FieldName, out Field))
            {
                throw new Exception($"{Name}() directive error: No such field as '{FieldName}' exists in this scope and context.");
            }
            Field.Protect();
        }
    }
}
