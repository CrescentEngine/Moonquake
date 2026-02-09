// Copyright (C) 2026 ychgen, all rights reserved.

namespace Moonquake.DSL.Directives
{
    public class DeclareSchemaDirective : Directive
    {
        public DeclareSchemaDirective()
        {
            ValidContexts = [ EvaluationContext.GlobalScope ];
            Flags         = DirectiveFlags.Bodily;
            
            Overloads[[ ASTType.String ]] = Invoke;
        }

        protected void Invoke(ExecutionContext Context, DirectiveAST AST)
        {
            string ConstructName = AST.Parameters.Compound[0].As<StringAST>().Resolved;
            if (Context.Schemas.ContainsKey(ConstructName))
            {
                throw new Exception($"{Name}() directive error: Attempting to declare a schema with name '{ConstructName}', but a schema with that name already exists.");
            }
            Constructs.Schema Subject = new Constructs.Schema
            {
                Name = ConstructName,
                Body = AST.Body
            };

            Context.PushFrame(EvaluationContext.SchemaScope, Subject);
            {
                foreach (AST Node in Subject.Body!.Compound)
                {
                    if (Node.Type == ASTType.Directive)
                    {
                        DirectiveAST Dir = (DirectiveAST) Node;
                        if (Dir.DirectiveName == DirectiveNames.DEFER_EXEC)
                        {
                            if (Subject.Defer is not null)
                            {
                                throw new Exception($"{Name}() directive error: Schema body contains multiple {DirectiveNames.DEFER_EXEC}() directives. Schemas can only contain one {DirectiveNames.DEFER_EXEC}() directive statement in its entire body.");
                            }
                            if (Dir.Body is null)
                            {
                                throw new Exception($"{Name}() directive error: {DirectiveNames.DEFER_EXEC}() directive has no body.");
                            }
                            Subject.Defer = Dir;
                        }
                    }
                }
            }
            Context.PopFrame();

            Context.Schemas[ConstructName] = Subject;
        }
    }
}
