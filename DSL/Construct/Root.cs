// Copyright (C) 2026 ychgen, all rights reserved.

namespace Moonquake.DSL.Construct
{
    public class Root : Construct
    {
        public List<string> Configurations = new List<string>();
        public List<string> Platforms      = new List<string>();
        public List<string> Architectures  = new List<string>();
        public List<string> Projects       = new List<string>();
        public string       StartProject   = "";

        public override ConstructType GetConstructType() => ConstructType.Root;
    }
}
