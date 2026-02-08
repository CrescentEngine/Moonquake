// Copyright (C) 2026 ychgen, all rights reserved.

namespace Moonquake.DSL.Constructs
{
    public enum ModuleLanguageStandard
    {
        Cpp11, // NOTE: With MSVC, Cpp14 will be used! It doesn't support explicit C++11 mode.
        Cpp14,
        Cpp17,
        Cpp20,
        Cpp23
    }
    public enum ModuleOutputType
    {
        ConsoleExecutable,
        WindowedExecutable,
        StaticLibrary,
        SharedLibrary
    }
    public enum ModuleRuntimeLibraries
    {
        UseDebug,
        UseRelease
    }
    public enum ModuleOptimization
    {
        Off,
        Balanced,
        Smallest,
        Fastest,
        Full
    }

    public class Module : Construct
    {
        public Schema? Template;

        public Module()
        {
            NewString("Path");
            NewConstraint<ModuleLanguageStandard>("LanguageStandard", ModuleLanguageStandard.Cpp14);
            NewConstraint<ModuleOutputType>("OutputType", ModuleOutputType.ConsoleExecutable);
            NewString("OutputName");
            NewString("OutputPath");
            NewString("IntermediatePath");
            NewArray("RootSourcePaths");
            NewArray("Sources");
            NewArray("GlobalMacros");
            NewConstraint<ModuleRuntimeLibraries>("RuntimeLibraries", ModuleRuntimeLibraries.UseDebug);
            NewConstraint<ModuleOptimization>("Optimization", ModuleOptimization.Off);
            NewBoolean("bDebugSymbols");
        }

        public override ConstructType GetConstructType() => ConstructType.Module;
    }
}
