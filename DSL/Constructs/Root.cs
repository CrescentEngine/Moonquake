// Copyright (C) 2026 ychgen, all rights reserved.

namespace Moonquake.DSL.Constructs
{
    public class Root : Construct
    {
        public Root(string InFilepath)
        {
            Filepath = InFilepath;

            NewString(RootFieldNames.ORIGIN, Path.GetDirectoryName(InFilepath)!);
            NewArray(RootFieldNames.CONFIGS, [ "Debug", "Release" ]);
            NewArray(RootFieldNames.ARCHS, ["x64"]);
            NewArray(RootFieldNames.PLATFORMS, [ Moonquake.BuildOrder.Platform.ToString() ]);
            NewArray(RootFieldNames.MODULES);
            NewString(RootFieldNames.ENTRYMOD);
        }

        public override ConstructType GetConstructType() => ConstructType.Root;
    }
}   
