// Copyright (C) 2026 ychgen, all rights reserved.

namespace Moonquake.DSL.Construct
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
        public StringAST Path = new StringAST();
        public ModuleLanguageStandard LanguageStandard;
        public ModuleOutputType OutputType;
        public StringAST OutputName = new StringAST();
        public StringAST OutputPath = new StringAST();
        public StringAST IntermediatePath = new StringAST();
        /// <summary>
        /// Every C/C++ header & translation unit will be recursively discovered in these directories and added to build.
        /// </summary>
        public List<StringAST> RootSourcePaths = new List<StringAST>();
        /// <summary>
        /// Individual C/C++ header & translation units that will be used in build.
        /// </summary>
        public List<StringAST> Sources = new List<StringAST>();
        public ModuleRuntimeLibraries RuntimeLibraries;
        public ModuleOptimization Optimization;
        public bool DebugSymbols;

        public override ConstructType GetConstructType() => ConstructType.Module;
    }
}
