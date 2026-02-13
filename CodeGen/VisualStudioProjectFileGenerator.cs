// Copyright (C) 2026 ychgen, all rights reserved.

using Microsoft.Build.Construction;
using Microsoft.Build.Evaluation;
using Moonquake.Orchestra;

namespace Moonquake.CodeGen
{
    struct GenerateProjectResult
    {
        public string ProjectName;
        public string ProjectGuid;
        public string ProjectFilepath;
    }
    enum SlnGlobalSectionEvalTime
    {
        Unspecified,
        PreSolution,
        PostSolution
    }
    struct SlnGlobalSection
    {
        public string Name;
        public string[] Lines;
        public SlnGlobalSectionEvalTime EvalTime;

        public string EvalTimeToStr() => EvalTime switch { SlnGlobalSectionEvalTime.PreSolution => "preSolution", SlnGlobalSectionEvalTime.PostSolution => "postSolution", _ => throw new Exception("Invalid SlnGlobalSectionEvalTime") };
        public string Str()
        {
            if (Lines.Length == 0)
            {
                return $"\tGlobalSection({Name}) = {EvalTimeToStr()}\n\tEndGlobalSection";
            }
            else
            {
                return $"\tGlobalSection({Name}) = {EvalTimeToStr()}\n\t\t{string.Join("\n\t\t", Lines)}\n\tEndGlobalSection";
            }
        }
    }
    class SlnGlobalSectionBuilder
    {
        private string _name = "";
        private readonly List<string> _lines = [];
        private SlnGlobalSectionEvalTime _evalt = SlnGlobalSectionEvalTime.Unspecified;

        public SlnGlobalSectionBuilder Name(string Name)
        {
            _name = Name;
            return this;
        }

        public SlnGlobalSectionBuilder Line(string Line)
        {
            _lines.Add(Line);
            return this;
        }

        public SlnGlobalSectionBuilder PreSolution()
        {
            _evalt = SlnGlobalSectionEvalTime.PreSolution;
            return this;
        }

        public SlnGlobalSectionBuilder PostSolution()
        {
            _evalt = SlnGlobalSectionEvalTime.PreSolution;
            return this;
        }

        public SlnGlobalSection Build() => new SlnGlobalSection { Name = _name, Lines = _lines.ToArray(), EvalTime = _evalt };
    }
    public class VisualStudioProjectFileGenerator : ProjectFileGenerator
    {
        public override ProjectFilesType GetProjectFilesType() => ProjectFilesType.VisualStudio;

        public VisualStudioProjectFileGenerator()
        {
        }
        public string ArchStr(Architectures Arch) => Arch switch
        {
            Architectures.x86   => "Win32",
            Architectures.x64   => "x64",
            Architectures.ARM64 => "ARM64",
            _ => throw new Exception("Invalid architecture")
        };

        public override void Generate(Dictionary<TestBuildConfig, BuildRoot> Resolutions)
        {
            Dictionary<string, GenerateProjectResult> Generated = new();
            foreach (var Resolution in Resolutions)
            {
                TestBuildConfig Cfg = Resolution.Key;
                BuildRoot Root = Resolution.Value;

                foreach (var (ModName, Mod) in Root.Modules)
                {
                    if (!Generated.ContainsKey(ModName))
                    {
                        Generated[ModName] = GenerateProject(ModName, Resolutions);
                    }
                }
            }
            GenerateSolution(Generated, Resolutions);
        }

        private void GenerateSolution(Dictionary<string, GenerateProjectResult> Projects, Dictionary<TestBuildConfig, BuildRoot> InResolutions)
        {
            string FileHeader = "\nMicrosoft Visual Studio Solution File, Format Version 12.00\n# Visual Studio Version 17\nVisualStudioVersion = 17.6.33424.112\nMinimumVisualStudioVersion = 10.0.40219.1\n";
            string ProjectsDefinition = "";
            foreach (var (ProjectName, Values) in Projects)
            {
                ProjectsDefinition += $"Project(\"{{8BC9CEB8-8B4A-11D0-8D11-00A0C91BC942}}\") = \"{Values.ProjectName}\", \"{Values.ProjectFilepath}\", \"{Values.ProjectGuid}\"\nEndProject\n";
            }

            List<string> GlobalSections = new();
            {
                SlnGlobalSectionBuilder Builder = new SlnGlobalSectionBuilder().Name("SolutionConfigurationPlatforms").PreSolution();
                foreach (TestBuildConfig Cfg in InResolutions.Keys)
                {
                    Builder.Line($"{Cfg.Config}|{ArchStr(Cfg.Arch)} = {Cfg.Config}|{ArchStr(Cfg.Arch)}");
                }
                GlobalSections.Add(Builder.Build().Str());
            }
            {
                SlnGlobalSectionBuilder Builder = new SlnGlobalSectionBuilder().Name("ProjectConfigurationPlatforms").PostSolution();
                foreach (var (Cfg, RR) in InResolutions)
                {
                    foreach (var (ProjectName, Values) in Projects)
                    {
                        string Match = $"{Cfg.Config}|{ArchStr(Cfg.Arch)}";
                        string Heading = $"{Values.ProjectGuid}.{Match}";
                        Builder.Line($"{Heading}.ActiveCfg = {Match}");
                        Builder.Line($"{Heading}.Build.0 = {(RR.Modules.ContainsKey(ProjectName) ? Match : "")}");
                    }
                }
                GlobalSections.Add(Builder.Build().Str());
            }

            BuildRoot ExampleRoot = InResolutions.First().Value;
            string Filepath = Path.Combine(ExampleRoot.RootPath, ExampleRoot.Name + ".sln");

            File.WriteAllText(Filepath, $"{FileHeader}{ProjectsDefinition}GlobalSection\n{string.Join("\n", GlobalSections)}\nEndGlobal");
        }

        private GenerateProjectResult GenerateProject(string InModuleName, Dictionary<TestBuildConfig, BuildRoot> InResolutions)
        {
            BuildModule ExampleModule = InResolutions.First().Value.Modules[InModuleName];
            
            GenerateProjectResult Result;
            Result.ProjectName = InModuleName;
            Result.ProjectGuid = Guid.NewGuid().ToString("B").ToUpper();
            Result.ProjectFilepath = Path.Combine(ExampleModule.ModulePath, InModuleName + ".vcxproj");

            ProjectRootElement Proj = ProjectRootElement.Create();
            var ProjectConfigurations = Proj.AddItemGroup();
            ProjectConfigurations.Label = "ProjectConfigurations";
            foreach (var (Cfg, _) in InResolutions)
            {
                var ProjectConfiguration = ProjectConfigurations.AddItem("ProjectConfiguration", $"{Cfg.Config}|{ArchStr(Cfg.Arch)}");
                ProjectConfiguration.AddMetadata("Configuration", Cfg.Config, expressAsAttribute: false);
                ProjectConfiguration.AddMetadata("Platform", ArchStr(Cfg.Arch), expressAsAttribute: false);
            }

            var MainPG = Proj.AddPropertyGroup();
            MainPG.Label = "Globals";
            MainPG.SetProperty("ProjectGuid", Result.ProjectGuid);
            MainPG.SetProperty("ProjectName", InModuleName);
            MainPG.SetProperty("RootNamespace", InModuleName);
            MainPG.SetProperty("ConfigurationType", "Makefile"); // NMake
            MainPG.SetProperty("PlatformToolset", "v145"); // VS2026

            // NMake build commands
            MainPG.SetProperty("NMakeBuildCommandLine", "echo Building $(Configuration)|$(Platform)");
            MainPG.SetProperty("NMakeReBuildCommandLine", "echo Rebuilding $(Configuration)|$(Platform)");
            MainPG.SetProperty("NMakeCleanCommandLine", "echo Cleaning $(Configuration)|$(Platform)");

            foreach (var (Cfg, RR) in InResolutions)
            {
                foreach (BuildModule Resolution in RR.Modules.Values)
                {
                    if (Resolution.Name != InModuleName)
                    {
                        continue;
                    }
                    string Condition = $"'$(Configuration)|$(Platform)'=='{Cfg.Config}|{ArchStr(Cfg.Arch)}'";

                    var Group = Proj.AddPropertyGroup();
                    Group.Condition = Condition;
                    Group.AddProperty("TargetName", Resolution.OutputName);
                    Group.AddProperty("OutDir", Resolution.OutputPath);
                    Group.AddProperty("IntDir", Resolution.ObjectPath);

                    if (Resolution.IncludePaths.Count > 0)
                    {
                        Group.AddProperty("IncludePath", string.Join(";", Resolution.IncludePaths));
                    }
                    if (Resolution.Definitions.Count > 0)
                    {
                        Group.AddProperty("NMakePreprocessorDefinitions", string.Join(";", Resolution.Definitions));
                    }
                    if (Resolution.TranslationUnits.Count > 0)
                    {
                        var ItemGroup = Proj.AddItemGroup();
                        ItemGroup.Condition = Condition;
                        foreach (string TU in Resolution.TranslationUnits)
                        {
                            ItemGroup.AddItem("ClCompile", TU);
                        }
                    }
                    if (Resolution.HeaderFiles.Count > 0)
                    {
                        var ItemGroup = Proj.AddItemGroup();
                        ItemGroup.Condition = Condition;
                        foreach (string Header in Resolution.HeaderFiles)
                        {
                            ItemGroup.AddItem("ClInclude", Header);
                        }
                    }
                }
            }

            Proj.AddImport(@"$(VCTargetsPath)\Microsoft.Cpp.Default.props");
            Proj.AddImport(@"$(VCTargetsPath)\Microsoft.Cpp.props");
            Proj.AddImport(@"$(VCTargetsPath)\Microsoft.Cpp.targets");

            Proj.Save(Result.ProjectFilepath);
            return Result;
        }
    }
}
