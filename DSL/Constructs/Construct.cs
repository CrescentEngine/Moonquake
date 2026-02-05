// Copyright (C) 2026 ychgen, all rights reserved.

namespace Moonquake.DSL.Construct
{
    public enum ConstructType
    {
        Root,
        Module,
        Schema
    }
    public abstract class Construct
    {
        public StringAST Name = new StringAST();
        
        public abstract ConstructType GetConstructType();
    }
}
