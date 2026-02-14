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
            Constructs.Root? RootOfSubject = null;
            foreach (Constructs.Root Rt in Context.Roots.Values)
            {
                foreach (string Module in Rt.Arr(Constructs.RootFieldNames.MODULES))
                {
                    if (Module == ConstructName)
                    {
                        RootOfSubject = Rt;
                        goto CheckRoot;
                    }
                }
            }
        CheckRoot:
            if (RootOfSubject is null)
            {
                throw new Exception($"{Name}() directive error: Trying to declare module '{ConstructName}', but no root has included such module in their '{Constructs.RootFieldNames.MODULES}' field.");
            }
            Constructs.Schema Template = Context.Schemas[TemplateName];
            Constructs.Module Subject = new Constructs.Module(RootOfSubject, ConstructName, Context.Filepath)
            {
                Template = Template
            };
            // We reconstruct the field so it considers its natural path its default value.
            // Dubious assignment validity is debatable but we make it work currently.
            Subject.Fields[Constructs.ModuleFieldNames.ORIGIN] = new Constructs.StringField(
                Path.GetDirectoryName(Context.Filepath.TrimEnd(Path.DirectorySeparatorChar))!
            );

            if (Template.Body is not null)
            {
                // If we don't clone, modules that inherit from same Schema will have problems
                // because stuff in there will be resolved (already visited by Visitor)
                CompoundAST Body = (CompoundAST) Template.Body!.Clone();

                Context.PushFrame(EvaluationContext.SchemaScope, Subject);
                {
                    Context.Visit(Body);
                }
                Context.PopFrame();
            }
            Context.PushFrame(EvaluationContext.ModuleScope, Subject);
            {
                Context.Visit(AST.Body!);
            }
            Context.PopFrame();
            if (Template.Defer is not null)
            {
                // If we don't clone, modules that inherit from same Schema will have problems
                // because stuff in there will be resolved (already visited by Visitor)
                DirectiveAST Defer = (DirectiveAST) Template.Defer!.Clone();

                Context.PushFrame(EvaluationContext.DeferredScope, Subject);
                {
                    Context.Visit(Defer);
                }
                Context.PopFrame();
            }

            Context.Modules[ConstructName] = Subject;
        }
    }
}
