// Copyright (C) 2026 ychgen, all rights reserved.

using System.Diagnostics;

namespace Moonquake.DSL.Directives
{
    public class SystemCallDirective : Directive
    {
        public SystemCallDirective()
        {
            ValidContexts =
            [
                EvaluationContext.GlobalScope,
                EvaluationContext.RootScope,
                EvaluationContext.SchemaScope,
                EvaluationContext.ModuleScope,
                EvaluationContext.DeferredScope
            ];
            Flags = DirectiveFlags.Inert;
            
            Overload([ ASTType.String, ASTType.String ], Invoke);
            Overload([ ASTType.String, ASTType.String, ASTType.Array ], InvokeWithCLI);
        }

        protected void Invoke(ExecutionContext Context, DirectiveAST AST)
        {
            ArrayAST InjectParameter = new ArrayAST();
            Context.Visit(InjectParameter);
            AST.Parameters.Compound.Add(InjectParameter);
            InvokeWithCLI(Context, AST);
        }

        protected void InvokeWithCLI(ExecutionContext Context, DirectiveAST AST)
        {
            string   ProcessName = AST.Parameters.Compound[0].As<StringAST>().Resolved;
            string   ErrorAction = AST.Parameters.Compound[1].As<StringAST>().Resolved;
            string[] Arguments   = AST.Parameters.Compound[2].As<ArrayAST>().ConstructResolvedStringArray();

            if (ErrorAction != "Raise" && ErrorAction != "Continue")
            {
                throw new Exception($"{Name}() directive error: 2nd Parameter 'ErrorAction' was populated with an invalid value '{ErrorAction}'. Only 'Raise' and 'Continue' are allowed.");
            }

            try
            {
                ProcessStartInfo CmdStartInfo = new ProcessStartInfo
                {
                    FileName = EvaluteProcessName(ProcessName),
                    Arguments = string.Join(" ", Arguments),
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using Process Proc = Process.Start(CmdStartInfo)!;
                string Stdout = Proc.StandardOutput.ReadToEnd();
                string Stderr = Proc.StandardError.ReadToEnd();
                Proc.WaitForExit();

                if (!string.IsNullOrEmpty(Stdout))
                {
                    Console.Write(Stdout);
                }
                if (!string.IsNullOrEmpty(Stderr))
                {
                    Console.Write(Stderr);
                }
            }
            catch
            {
                if (ErrorAction == "Raise")
                {
                    throw;
                }
            }
        }

        protected virtual string EvaluteProcessName(string InProcessName) => InProcessName;
    }
}
