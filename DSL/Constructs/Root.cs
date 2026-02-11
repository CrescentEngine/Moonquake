// Copyright (C) 2026 ychgen, all rights reserved.

namespace Moonquake.DSL.Constructs
{
    public class Root : Construct
    {
        public Root()
        {
            NewArray(RootFieldNames.CONFIGS, [ "Debug", "Release" ]);
            NewArray(RootFieldNames.ARCHS, ["x64"]);
            NewArray(RootFieldNames.PLATFORMS, [ Moonquake.BuildOrder.Platform.ToString() ]);
            NewArray(RootFieldNames.MODULES);
            NewString(RootFieldNames.ENTRYMOD);
        }

        public override ConstructType GetConstructType() => ConstructType.Root;
    }
}   
