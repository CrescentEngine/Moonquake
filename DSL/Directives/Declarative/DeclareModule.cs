// Copyright (C) 2026 ychgen, all rights reserved.

namespace Moonquake.DSL.Directives
{
    public class DeclareModuleDirective : Directive
    {
        public DeclareModuleDirective()
        {
            ValidContexts = [ EvaluationContext.GlobalScope ];
            Flags         =   DirectiveFlags.Bodily;

            Overloads[[ ASTType.String                 ]] = Invoke;
            Overloads[[ ASTType.String, ASTType.String ]] = InstancedInvoke;
        }

        protected void Invoke(ExecutionContext Context, DirectiveAST AST)
        {
            StringAST InjectParameter = new StringAST(ExecutionContext.BUILTIN_SCHM);
            Context.Visit(InjectParameter);

            AST.Parameters.Compound.Add(InjectParameter);
            InstancedInvoke(Context, AST);
        }

        protected void InstancedInvoke(ExecutionContext Context, DirectiveAST AST)
        {
            string ConstructName = AST.Parameters.Compound[0].As<StringAST>().Resolved;
            string TemplateName  = AST.Parameters.Compound[1].As<StringAST>().Resolved;
            if (Context.Modules.ContainsKey(ConstructName))
            {
                throw new Exception($"{Name}() directive error: Attempting to declare a module with name '{ConstructName}', but a module with that name already exists.");
            }
            if (!Context.Schemas.ContainsKey(TemplateName))
            {
                throw new Exception($"{Name}() directive error: Attempting to declare a module with name '{ConstructName}' templated from schema '{TemplateName}', but no such Schema construct exists.");
            }
            Constructs.Schema Template = Context.Schemas[TemplateName];
            Constructs.Module Subject = new Constructs.Module
            {
                Name     = ConstructName,
                Template = Template
            };

            Context.PushFrame(EvaluationContext.SchemaScope, Subject);
            {
                Context.Visit(Template.Body!);
            }
            Context.PopFrame();
            Context.PushFrame(EvaluationContext.ModuleScope, Subject);
            {
                Context.Visit(AST.Body!);
            }
            Context.PopFrame();
            if (Template.Defer is not null)
            {
                Context.PushFrame(EvaluationContext.DeferredScope, Subject);
                {
                    Context.Visit(Template.Defer.Body!);
                }
                Context.PopFrame();
            }

            Context.Modules[ConstructName] = Subject;
        }
    }
}
