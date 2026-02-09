// Copyright (C) 2026 ychgen, all rights reserved.

namespace Moonquake.DSL.Directives
{
    public class DeclareRootDirective : Directive
    {
        public DeclareRootDirective()
        {
            ValidContexts = [ EvaluationContext.GlobalScope ];
            Flags         = DirectiveFlags.Bodily;
            
            Overloads[[ ASTType.String ]] = Invoke;
        }

        protected void Invoke(ExecutionContext Context, DirectiveAST AST)
        {
            string ConstructName = AST.Parameters.Compound[0].As<StringAST>().Resolved;
            if (Context.Roots.ContainsKey(ConstructName))
            {
                throw new Exception($"{Name}() directive error : Attempting to declare a root with name '{ConstructName}', but a root with that name already exists.");
            }
            Constructs.Root Subject = new Constructs.Root
            {
                Name = ConstructName
            };

            Context.PushFrame(EvaluationContext.RootScope, Subject);
            {
                Context.Visit(AST.Body!);
            }
            Context.PopFrame();
            
            Context.Roots[ConstructName] = Subject;
        }
    }
}
