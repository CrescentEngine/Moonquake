// Copyright (C) 2026 ychgen, all rights reserved.

namespace Moonquake.DSL.Directives
{
    public static class DirectiveRegistry
    {
        private static Dictionary<string, Directive> Registry = new Dictionary<string, Directive>
        {
            /** Conditional */
            { DirectiveNames.IF_PMATCH,       new IfPatternMatchDirective()    },
            { DirectiveNames.IF_PNOMATCH,     new IfNotPatternMatchDirective() },
            { DirectiveNames.IF_PANYMATCH,    new IfAnyPatternMatchDirective() },

            /** Declarative */
            { DirectiveNames.DECLARE_VERSION, new DeclareVersionDirective()    },
            { DirectiveNames.DECLARE_ROOT,    new DeclareRootDirective()       },
            { DirectiveNames.DECLARE_SCHEMA,  new DeclareSchemaDirective()     },
            { DirectiveNames.DECLARE_MODULE,  new DeclareModuleDirective()     },

            /** Evaluative */
            { DirectiveNames.INCLUDE_MODULE,  new IncludeDirective()           },

            /** Lazy */
            { DirectiveNames.DEFER_EXEC,      new DeferDirective()             },

            /** Module */
            { DirectiveNames.FIELD_PROTECT,   new ProtectFieldDirective()      },
            { DirectiveNames.FIELDS_PROTECT,  new ProtectAllFieldsDirective()  },
        };

        public static IReadOnlyDictionary<string, Directive> All => Registry;
        public static bool TryGetDirective(string Name, out Directive? Dir) => Registry.TryGetValue(Name, out Dir);
    }
}
