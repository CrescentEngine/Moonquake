// Copyright (C) 2026 ychgen, all rights reserved.

using Moonquake.DSL.Constructs;

namespace Moonquake.Orchestra
{
    public static class BuildFinalizer
    {
        public static BuildRoot FinalizeRoot(DSL.ExecutionContext ExecContext, Root InRoot)
        {
            string PrevWorkingDir = Directory.GetCurrentDirectory();
            Directory.SetCurrentDirectory(Path.GetDirectoryName(InRoot.Filepath.TrimEnd(Path.DirectorySeparatorChar))!);

            BuildRoot Final = new BuildRoot();
            Final.Name = InRoot.Name;
            foreach (string ModuleName in InRoot.Arr(RootFieldNames.MODULES))
            {
                Module? Module;
                if (!ExecContext.Modules.TryGetValue(ModuleName, out Module))
                {
                    throw new Exception($"BuildFinalizer.FinalizeRoot() error: Module '{ModuleName}' specified as apart of Root '{InRoot.Name}' was never declared within the execution context.");
                }
                Final.Modules[ModuleName] = FinalizeModule(ExecContext, InRoot, Module, []);
            }
            string MainModuleName = InRoot.Str(RootFieldNames.ENTRYMOD);
            if (MainModuleName == "")
            {
                foreach (var KVP in Final.Modules)
                {
                    BuildModule Module = KVP.Value;
                    if (Module.OutputType == ModuleOutputType.ConsoleExecutable || Module.OutputType == ModuleOutputType.WindowedExecutable)
                    {
                        Final.MainModule = Module;
                    }
                }
                if (Final.MainModule is null)
                {
                    Console.WriteLine($"BuildFinalizer.FinalizeRoot() warning: No suitable main module could be found for Root '{InRoot.Name}'. Is this expected? If this is supposed to be a root for a library project, then probably yes.");
                }
            }
            else
            {
                BuildModule? Module;
                if (!Final.Modules.TryGetValue(MainModuleName, out Module))
                {
                    throw new Exception($"BuildFinalizer.FinalizeRoot() error: Module '{MainModuleName}', specified as the Main Module of Root '{InRoot.Name}', is not apart of the Root. Add it to the '{DSL.Constructs.RootFieldNames.MODULES}' field.");
                }
                if (Module.OutputType != ModuleOutputType.ConsoleExecutable && Module.OutputType != ModuleOutputType.WindowedExecutable)
                {
                    throw new Exception($"BuildFinalizer.FinalizeRoot() error: Module '{MainModuleName}', specified as the Main Module of Root '{InRoot.Name}', has the output type '{Module.OutputType}'. Only ConsoleExecutable and WindowedExecutable modules can be main modules.");
                }
                Final.MainModule = Module;
            }
            Directory.SetCurrentDirectory(PrevWorkingDir);
            return Final;
        }

        private static BuildModule FinalizeModule(DSL.ExecutionContext ExecContext, Root InRoot, Module InModule, List<string> InDependencyDenyList)
        {
            if (InDependencyDenyList.Contains(InModule.Name))
            {
                throw new Exception($"BuildFinalizer.FinalizeModule() error: Module '{InModule.Name}' is apart of a cyclic dependency list. Related modules in this cycle: {string.Join(", ", InDependencyDenyList)}.");
            }

            // Change wdir to the file's location so every path resolves correctly
            string PrevWorkingDir = Directory.GetCurrentDirectory();
            Directory.SetCurrentDirectory(Path.GetDirectoryName(InModule.Filepath.TrimEnd(Path.DirectorySeparatorChar))!);

            BuildModule Final = new BuildModule
            {
                Name = InModule.Name,
                ModulePath = Path.GetFullPath(InModule.Str(ModuleFieldNames.ORIGIN)),
                OutputName = InModule.Str(ModuleFieldNames.BINNAME),
                OutputPath = Path.GetFullPath(InModule.Str(ModuleFieldNames.BINPATH)),
                ObjectPath = Path.GetFullPath(InModule.Str(ModuleFieldNames.OBJPATH)),
                OutputType = InModule.Con<ModuleOutputType>(ModuleFieldNames.BINTYPE),
                RuntimeLibraries = InModule.Con<ModuleRuntimeLibraries>(ModuleFieldNames.RTLIBS),
                CppStandard = InModule.Con<ModuleLanguageStandard>(ModuleFieldNames.STDCPP),
                OptimizationLevel = InModule.Con<ModuleOptimization>(ModuleFieldNames.OLEVEL),
                bGenerateSymbols = InModule.Bool(ModuleFieldNames.SYMBOLS)
            };

            Action<string>? DoFilesIt = null;
            DoFilesIt = Dir =>
            {
                foreach (string File in Directory.GetFiles(Dir))
                {
                    foreach (string HeaderExtension in StringUtil.HeaderFileExtensions)
                    {
                        if (File.EndsWith(HeaderExtension))
                        {
                            Final.HeaderFiles.Add(File);
                            goto NextFile;
                        }
                    }
                    foreach (string TranslationUnitExtension in StringUtil.TranslationUnitExtensions)
                    {
                        if (File.EndsWith(TranslationUnitExtension))
                        {
                            Final.TranslationUnits.Add(File);
                            goto NextFile;
                        }
                    }
                NextFile:
                    continue;
                }
                foreach (string Subdir in Directory.GetDirectories(Dir))
                {
                    DoFilesIt!(Subdir);
                }
            };

            foreach (string RootSourcePath in InModule.Arr(ModuleFieldNames.SRCDIRS).Distinct())
            {
                DoFilesIt(Path.GetFullPath(RootSourcePath));
            }

            Final.IncludePaths = InModule.Arr(ModuleFieldNames.INCDIRS).Select(Path.GetFullPath).Distinct().ToList();
            foreach (string Definition in InModule.Arr(ModuleFieldNames.DEFINES).Distinct())
            {
                int EqualsPos = Definition.IndexOf('=');
                string DefinitionName = Definition.Substring(0, EqualsPos == -1 ? Definition.Length : EqualsPos);
                if (Final.Definitions.Contains(DefinitionName))
                {
                    throw new Exception($"BuildFinalizer.FinalizeModule() error: Module '{InModule.Name}' has multiple definitions of macro '{DefinitionName}'.");
                }
                Final.Definitions.Add(Definition);
            }

            Final.Libraries = InModule.Arr(ModuleFieldNames.LIBLINK).Distinct().ToList();
            foreach (string Linkage in InModule.Arr(ModuleFieldNames.LINKS).Distinct())
            {
                Module? Mod;
                if (!ExecContext.Modules.TryGetValue(Linkage, out Mod))
                {
                    throw new Exception($"BuildFinalizer.FinalizeModule() error: Module '{InModule.Name}' wants to link against module '{Linkage}', however no such module exists.");
                }
                if (!InRoot.Arr(RootFieldNames.MODULES).Contains(Linkage))
                {
                    throw new Exception($"BuildFinalizer.FinalizeModule() error: Module '{InModule.Name}' wants to link against module '{Linkage}', however no such module exists in root '{InRoot.Name}' which '{InModule.Name}' is apart of. Cross-root module linkages are illegal.");
                }

                InDependencyDenyList.Add(InModule.Name);
                    BuildModule Module = FinalizeModule(ExecContext, InRoot, Mod, InDependencyDenyList);
                InDependencyDenyList.RemoveAt(InDependencyDenyList.Count - 1);

                Final.Linkages[Module.Name] = Module;
            }
            Final.Dependencies = Final.Linkages; // Carry over linkages (they are dependencies as well)
            foreach (string Dependency in InModule.Arr(ModuleFieldNames.PREREQ).Distinct())
            {
                if (InModule.Arr(ModuleFieldNames.LINKS).Contains(Dependency))
                {
                    throw new Exception($"BuildFinalizer.FinalizeModule() error: Module '{InModule.Name}' both depends on and links against module '{Dependency}'. This case is illegal, it must either only link against or only depend on.");
                }

                Module? Mod;
                if (!ExecContext.Modules.TryGetValue(Dependency, out Mod))
                {
                    throw new Exception($"BuildFinalizer.FinalizeModule() error: Module '{InModule.Name}' wants to depend on module '{Dependency}', however no such module exists.");
                }
                if (!InRoot.Arr(RootFieldNames.MODULES).Contains(Dependency))
                {
                    throw new Exception($"BuildFinalizer.FinalizeModule() error: Module '{InModule.Name}' wants to depend on module '{Dependency}', however no such module exists in root '{InRoot.Name}' which '{InModule.Name}' is apart of. Cross-root module dependencies are illegal.");
                }

                InDependencyDenyList.Add(InModule.Name);
                    BuildModule Module = FinalizeModule(ExecContext, InRoot, Mod, InDependencyDenyList);
                InDependencyDenyList.RemoveAt(InDependencyDenyList.Count - 1);

                Final.Dependencies[Module.Name] = Module;
            }

            Directory.SetCurrentDirectory(PrevWorkingDir);
            return Final;
        }
    }
}
