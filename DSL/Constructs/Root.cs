// Copyright (C) 2026 ychgen, all rights reserved.

namespace Moonquake.DSL.Construct
{
    public class Root : Construct
    {
        public List<StringAST> Configurations = new List<StringAST>();
        public List<StringAST> Platforms      = new List<StringAST>();
        public List<StringAST> Architectures  = new List<StringAST>();
        public List<StringAST> Projects       = new List<StringAST>();
        public StringAST       StartProject   = new StringAST();

        public override ConstructType GetConstructType() => ConstructType.Root;
    }
}   
