// Copyright (C) 2026 ychgen, all rights reserved.

using System.Reflection;

namespace Moonquake.DSL.Constructs
{
    public class Root : Construct
    {
        public Root(string InFilepath)
        {
            Filepath = InFilepath;

            NewString(RootFieldNames.ORIGIN, Path.GetDirectoryName(InFilepath)!).Since(1, 1, 0);
            NewArray(RootFieldNames.CONFIGS, [ "Debug", "Release" ]);
            NewArray(RootFieldNames.ARCHS, ["x64"]);
            NewArray(RootFieldNames.PLATFORMS, [ Moonquake.BuildOrder.Platform.ToString() ]);
            NewArray(RootFieldNames.MODULES);
            NewString(RootFieldNames.ENTRYMOD);
            
            string ExecPath = Assembly.GetExecutingAssembly().Location;

            NewString(RootFieldNames.BUILD, $"{ExecPath} build {InFilepath} {Name} {Moonquake.BuildOrder.Configuration}");
            NewString(RootFieldNames.REBUILD, $"{ExecPath} rebuild {InFilepath} {Name} {Moonquake.BuildOrder.Configuration}");
            NewString(RootFieldNames.CLEAN, $"{ExecPath} clean {InFilepath} {Name}");
        }

        public override ConstructType GetConstructType() => ConstructType.Root;
    }
}   
