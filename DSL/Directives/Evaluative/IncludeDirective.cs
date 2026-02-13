// Copyright (C) 2026 ychgen, all rights reserved.

namespace Moonquake.DSL.Directives
{
    public class IncludeDirective : Directive
    {
        public IncludeDirective()
        {
            ValidContexts = [ EvaluationContext.GlobalScope ];
            Overloads[[ ASTType.String ]] = Invoke;
        }

        protected void Invoke(ExecutionContext Context, DirectiveAST AST)
        {
            // No-op if includes were disabled.
            if (Moonquake.BuildOrder.bDisableIncludes)
            {
                return;
            }
            string ToInclude = AST.Parameters.Compound[0].As<StringAST>().Resolved;

            if (File.Exists(ToInclude))
            {
                if (!ToInclude.EndsWith(".mqmod"))
                {
                    throw new Exception($"{Name}() directive error: Filepath inclusions must end with '.mqmod'. Given '{ToInclude}' doesn't satisfy that.");
                }
                goto FileDetected;
            }

            if (!Directory.Exists(ToInclude))
            {
                throw new Exception($"{Name}() directive error: No such directory as '{ToInclude}' exists. If you meant to include a file, it must end in '.mqmod'.");
            }

            ToInclude = Path.Combine(ToInclude, Path.GetFileName(ToInclude.TrimEnd(Path.DirectorySeparatorChar))) + ".mqmod";
            if (!File.Exists(ToInclude))
            {
                throw new Exception($"{Name}() directive error: File to include '{ToInclude}' (deduced from directory-file inclusion relation rule) doesn't exist.");
            }
        FileDetected:
            ToInclude = Path.GetFullPath(ToInclude);
            if (ToInclude == Context.Filepath)
            {
                throw new Exception($"{Name}() directive error: Description file '{Context.Filepath}' is including itself.");
            }
            if (Context.IncludedFiles.Contains(ToInclude))
            {
                throw new Exception($"{Name}() directive error: Circular inclusion of description file '{ToInclude}' detected. Last include attempt done in description file '{Context.Filepath}'.");
            }
            
            Context.IncludedFiles.Push(ToInclude);
            {
                string Content = File.ReadAllText(ToInclude);
                Parser InclParser = new Parser(new Lexer(ToInclude, Content));

                string PrevFilepath = Context.Filepath;
                Context.Filepath = ToInclude;
                // Switch wdir so everything resolves relative to that description file nicely.
                Directory.SetCurrentDirectory(Path.GetDirectoryName(ToInclude)!);
                Context.Visit(InclParser.Parse());
                Context.Filepath = PrevFilepath;
                // Back to the original path.
                Directory.SetCurrentDirectory(Path.GetDirectoryName(PrevFilepath)!);
            }
            Context.IncludedFiles.Pop();
        }
    }
}
