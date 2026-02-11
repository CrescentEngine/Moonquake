// Copyright (C) 2026 ychgen, all rights reserved.

namespace Moonquake
{
    public enum ModuleLanguageStandard
    {
        Cpp14,
        Cpp17,
        Cpp20
    }
    public enum ModuleOutputType
    {
        ConsoleExecutable,
        WindowedExecutable,
        StaticLibrary,
        DynamicLibrary
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
}
