// Copyright (C) 2026 ychgen, all rights reserved.

namespace Moonquake.DSL.Constructs
{
    public class Root : Construct
    {
        public Root()
        {
            NewArray("Configurations", [ "Debug", "Release" ]);
            NewArray("Architectures", ["x64"]);
            NewArray("Platforms", [ Moonquake.BuildOrder.Platform.ToString() ]);
            NewArray("Modules");
            NewString("MainModule");
        }

        public override ConstructType GetConstructType() => ConstructType.Root;
    }
}   
