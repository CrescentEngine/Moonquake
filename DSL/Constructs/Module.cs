// Copyright (C) 2026 ychgen, all rights reserved.

using System.Reflection;

namespace Moonquake.DSL.Constructs
{
    public class Module : Construct
    {
        public Root Root;
        public Schema? Template;

        public Module(Root InRoot, string InName, string InFilepath)
        {
            Root = InRoot;
            Name = InName;
            Filepath = InFilepath;

            // Path
            NewString(ModuleFieldNames.ORIGIN, Path.GetDirectoryName(Filepath)!);
            // LanguageStandard
            NewConstraint<ModuleLanguageStandard>(ModuleFieldNames.STDCPP, ModuleLanguageStandard.Cpp14);
            // OutputType
            NewConstraint<ModuleOutputType>(ModuleFieldNames.BINTYPE, ModuleOutputType.ConsoleExecutable);
            // OutputName
            NewString(ModuleFieldNames.BINNAME, InName);
            // OutputPath
            NewString(ModuleFieldNames.BINPATH);
            // IntermediatePath
            NewString(ModuleFieldNames.OBJPATH);
            // RootSourcePaths
            NewArray(ModuleFieldNames.SRCDIRS);
            // SourceFiles
            NewArray(ModuleFieldNames.SRCS);
            // Definitions
            NewArray(ModuleFieldNames.DEFINES);
            // RuntimeLibraries
            NewConstraint<ModuleRuntimeLibraries>(ModuleFieldNames.RTLIBS, ModuleRuntimeLibraries.UseDebug);
            // Optimization
            NewConstraint<ModuleOptimization>(ModuleFieldNames.OLEVEL, ModuleOptimization.Off);
            // bGenerateSymbols
            NewBoolean(ModuleFieldNames.SYMBOLS);
            // Linkages
            NewArray(ModuleFieldNames.LINKS);
            // LibLinkages
            NewArray(ModuleFieldNames.LIBLINK);
            // DependsOn
            NewArray(ModuleFieldNames.PREREQ);
            // IncludePaths
            NewArray(ModuleFieldNames.INCDIRS);
            // ExposedIncludePaths
            NewArray(ModuleFieldNames.EXPINCL).Since(1, 1, 0);
        }

        public override ConstructType GetConstructType() => ConstructType.Module;
    }
}
