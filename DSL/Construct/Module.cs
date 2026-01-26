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
        ConsoleApplication,
        WindowedApplication,
        StaticLibrary,
        SharedLibrary
    }

    public class Module
    {
        public ModuleLanguageStandard LanguageStandard;
        public ModuleOutputType OutputType;
        public string OutputName = "";
        public string OutputPath = "";
        public string IntermediatePath = "";
        public List<string> RootSourceDirectories = new List<string>();
    }
}
