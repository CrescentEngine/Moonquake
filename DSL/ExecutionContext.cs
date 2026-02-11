// Copyright (C) 2026 ychgen, all rights reserved.

namespace Moonquake.DSL
{
    public class ExecutionContext
    {
        public LanguageVersion DeclaredVersion = LanguageVersion.Invalid;
        public string Filepath = "";

        public const string BUILTIN_MODL = "BuiltinDefaultModule",
                            BUILTIN_ROOT = "BuiltinDefaultRoot",
                            BUILTIN_SCHM = "BuiltinDefaultSchema";
        public Dictionary<string, Constructs.Root>   Roots   { get; } = new();
        public Dictionary<string, Constructs.Module> Modules { get; } = new();
        public Dictionary<string, Constructs.Schema> Schemas { get; } = new();

        public Action<AST> Visit { get; }

        private Stack<ExecutionFrame> FrameStack = new();
        public ExecutionFrame Frame => FrameStack.Peek();

        public ExecutionContext(string InFilepath, Action<AST> InVisit)
        {
            Filepath = InFilepath;
            Visit    = InVisit;

            Roots  [BUILTIN_ROOT] = new Constructs.Root   { Name = BUILTIN_ROOT };
            Schemas[BUILTIN_SCHM] = new Constructs.Schema { Name = BUILTIN_SCHM };
            Modules[BUILTIN_MODL] = new Constructs.Module { Name = BUILTIN_MODL };

            PushFrame(EvaluationContext.GlobalScope, null);
        }

        public void PushFrame(EvaluationContext EvalContext, Constructs.Construct? Construct)
        {
            FrameStack.Push(new ExecutionFrame(EvalContext, Construct));
        }

        public void PopFrame()
        {
            if (FrameStack.Count == 0)
            {
                throw new Exception("Execution frame stack underflow!");
            }
            FrameStack.Pop();
        }

        public void ValidateRootReferences(string NameOfRootToValidate)
        {
            Constructs.Root? Root;
            if (!Roots.TryGetValue(NameOfRootToValidate, out Root))
            {
                throw new Exception($"Validation of Root '{NameOfRootToValidate}' was requested, but no such root even exists.");
            }

            foreach (string ModuleName in Root.Arr(Constructs.RootFieldNames.MODULES))
            {
                if (!Modules.ContainsKey(ModuleName))
                {
                    throw new Exception($"Validation of Root '{NameOfRootToValidate}' failed, at least one of the modules specified in {NameOfRootToValidate}.Modules, which is '{ModuleName}', was never declared.");
                }
            }
        }
    }
}
