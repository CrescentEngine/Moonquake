// Copyright (C) 2026 ychgen, all rights reserved.

namespace Moonquake.DSL.Directives
{
    public static class DirectiveRegistry
    {
        private static Dictionary<string, Directive> Registry = new();

        public static void Init()
        {
            if (Registry.Count > 0)
            {
                return;
            }

            /** Conditional */
            Register<IfPatternMatchDirective>(DirectiveNames.IF_PMATCH);
            Register<IfNotPatternMatchDirective>(DirectiveNames.IF_PNOMATCH);
            Register<IfAnyPatternMatchDirective>(DirectiveNames.IF_PANYMATCH);

            /** Declarative */
            Register<DeclareVersionDirective>(DirectiveNames.DECLARE_VERSION);
            Register<DeclareRootDirective>(DirectiveNames.DECLARE_ROOT);
            Register<DeclareSchemaDirective>(DirectiveNames.DECLARE_SCHEMA);
            Register<DeclareModuleDirective>(DirectiveNames.DECLARE_MODULE);

            /** Evaluative */
            Register<IncludeDirective>(DirectiveNames.INCLUDE_MODULE);
            Register<SystemCallDirective>(DirectiveNames.SYSCALL);
            Register<SystemExecCallDirective>(DirectiveNames.EXECALL);

            /** Lazy */
            Register<DeferDirective>(DirectiveNames.DEFER_EXEC);

            /** Module */
            Register<ProtectFieldDirective>(DirectiveNames.FIELD_PROTECT);
            Register<ProtectAllFieldsDirective>(DirectiveNames.FIELDS_PROTECT);
        }

        public static IReadOnlyDictionary<string, Directive> All => Registry;
        public static bool TryGetDirective(string Name, out Directive? Dir) => Registry.TryGetValue(Name, out Dir);

        private static void Register<T>(string InName) where T : Directive, new()
        {
            if (Registry.ContainsKey(InName))
            {
                throw new Exception($"DirectiveRegistry.Register() error: Directive with name '{InName}' was already registered.");
            }
            Registry[InName] = new T { Name = InName };
        }
    }
}
