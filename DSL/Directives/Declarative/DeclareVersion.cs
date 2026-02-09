// Copyright (C) 2026 ychgen, all rights reserved.

namespace Moonquake.DSL.Directives
{
    public class DeclareVersionDirective : Directive
    {
        public DeclareVersionDirective()
        {
            ValidContexts = [ EvaluationContext.GlobalScope ];
            Overloads[[ ASTType.String ]] = Invoke;
        }

        protected void Invoke(ExecutionContext Context, DirectiveAST AST)
        {
            if (Context.DeclaredVersion != LanguageVersion.Invalid)
            {
                throw new Exception($"{Name}() directive error: Language version was already declared as '{Context.DeclaredVersion}' before.");
            }

            string VersionString = AST.Parameters.Compound[0].As<StringAST>().Resolved;
            try
            {
                Context.DeclaredVersion = LanguageVersion.FromString(VersionString);
            }
            catch
            {
                throw new Exception($"{Name}() directive error: Language version string '{VersionString}' couldn't be parsed.");
            }
        }
    }
}
