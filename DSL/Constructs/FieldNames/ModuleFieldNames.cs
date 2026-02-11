// Copyright (C) 2026 ychgen, all rights reserved.

namespace Moonquake.DSL.Constructs
{
    public static class ModuleFieldNames
    {
        public const string ORIGIN  = "Path";
        public const string STDCPP  = "LanguageStandard";
        public const string BINTYPE = "OutputType";
        public const string BINNAME = "OutputName";
        public const string BINPATH = "OutputPath";
        public const string OBJPATH = "IntermediatePath";
        public const string SRCDIRS = "RootSourcePaths";
        public const string SRCS    = "SourceFiles";
        public const string DEFINES = "Definitions";
        public const string RTLIBS  = "RuntimeLibraries";
        public const string OLEVEL  = "Optimization";
        public const string SYMBOLS = "bDebugSymbols";
        public const string LINKS   = "Linkages";
        public const string LIBLINK = "Libraries";
        public const string PREREQ  = "DependsOn";
        public const string INCDIRS = "IncludePaths";
    }
}
