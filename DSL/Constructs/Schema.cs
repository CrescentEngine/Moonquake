// Copyright (C) 2026 ychgen, all rights reserved.

namespace Moonquake.DSL.Constructs
{
    public class Schema : Construct
    {
        public CompoundAST?  Body;
        public DirectiveAST? Defer;

        public Schema()
        {
        }
        
        public override ConstructType GetConstructType() => ConstructType.Schema;
    }
}
