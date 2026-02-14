// Copyright (C) 2026 ychgen, all rights reserved.

using System.Globalization;
using Moonquake.CodeGen;

namespace Moonquake.Orchestra
{
    /// <summary>
    /// Constructs.Module finalizes into this structural form.
    /// It contains fully resolved paths and the finalized proper structure ready for build.
    /// </summary>
    public class BuildModule
    {
        public string Name = "";
        public string ModulePath = "";
        // Output binary name.
        public string OutputName = "";
        public string OutputPath = "";
        public string ObjectPath = "";
        public ModuleOutputType OutputType = ModuleOutputType.ConsoleExecutable;
        public ModuleRuntimeLibraries RuntimeLibraries = ModuleRuntimeLibraries.UseDebug;

        public ModuleLanguageStandard CppStandard = ModuleLanguageStandard.Cpp14;
        public ModuleOptimization     OptimizationLevel = ModuleOptimization.Off;
        public bool bGenerateSymbols = false;

        public List<string> HeaderFiles         = new();
        public List<string> TranslationUnits    = new();
        public List<string> ExposedIncludePaths = new();
        public List<string> IncludePaths        = new();
        public List<string> Definitions         = new();

        /// <summary>
        /// Raw dependencies are direct links against arbitrary libraries outside build system scope.
        /// Such as winmm.lib on Windows for example.
        /// </summary>
        public List<string> Libraries = new();

        /// <summary>
        /// The modules that need to be built in order for this module to be able to be built.
        /// </summary>
        public Dictionary<string, BuildModule> Dependencies = new();
        
        /// <summary>
        /// Subset of Dependencies list that encapsulates the modules to actually link against, not just depend on.
        /// </summary>
        public Dictionary<string, BuildModule> Linkages     = new();

        public BuildModule()
        {
            switch (Moonquake.BuildOrder.Platform)
            {
            case Platforms.Windows:
            {
                Definitions.Add("DLLIMPORT=__declspec(dllimport)");
                Definitions.Add("DLLEXPORT=__declspec(dllexport)");
                break;
            }
            case Platforms.Linux:
            {
                Definitions.Add("DLLIMPORT");
                Definitions.Add("DLLEXPORT=__attribute__((visibility(\"default\")))");
                break;
            }
            }
        }

        public static string GetAPIMacro(string InModuleName) => $"{InModuleName.Replace(" ", "").ToUpper(CultureInfo.InvariantCulture)}_API";
        public string GetGeneratedIntermediatesPath(TestBuildConfig Cfg) => $"{ModulePath}/Intermediate/{Cfg.Config}-{Cfg.Arch}/Include";
    }
}
