// Copyright (C) 2026 ychgen, all rights reserved.

using System.Collections.Immutable;

namespace Moonquake.DSL.Directives
{
    [Flags]
    public enum DirectiveFlags
    {
        None        = 0 << 0,
        Bodily      = 1 << 0,
        Conditional = 1 << 1
    }

    public delegate void DirectiveOverload(ExecutionContext Context, DirectiveAST AST);
    public class Directive
    {
        // Populated by DirectiveRegistry.
        public string Name = "__unnamed_directive";
        
        // These are populated by children Directive classes.
        protected Dictionary<ImmutableArray<ASTType>, DirectiveOverload> Overloads = new(new AstTypeArrayComparer());
        protected EvaluationContext[] ValidContexts = [];
        protected DirectiveFlags Flags = DirectiveFlags.None;

        public bool HasOverload(ASTType[] InOverload) => Overloads.ContainsKey(InOverload.ToImmutableArray());
        public DirectiveOverload? GetOverload(ASTType[] InOverload)
        {
            DirectiveOverload? Overload;
            Overloads.TryGetValue(InOverload.ToImmutableArray(), out Overload);
            return Overload;
        }
        public EvaluationContext[] GetValidContexts() { return ValidContexts; }

        public bool HasFlags(DirectiveFlags InFlags) => (Flags & InFlags) == InFlags;
        public bool IsBodily() => HasFlags(DirectiveFlags.Bodily);
        public bool IsConditional() => HasFlags(DirectiveFlags.Conditional);

        protected void Overload(ASTType[] InOverload, DirectiveOverload InHandler)
        {
            Overloads[InOverload.ToImmutableArray()] = InHandler;
        }
    }
}
