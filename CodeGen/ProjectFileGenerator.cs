// Copyright (C) 2026 ychgen, all rights reserved.

using Moonquake.Orchestra;

namespace Moonquake.CodeGen
{
    public enum ProjectFilesType
    {
        VisualStudio,
        Makefile
    }
    public abstract class ProjectFileGenerator
    {
        public abstract ProjectFilesType GetProjectFilesType();
        public abstract void Generate(Dictionary<TestBuildConfig, BuildRoot> Resolutions);

        public void DumpDefinitions(Dictionary<TestBuildConfig, BuildRoot> InResolutions)
        {
            foreach (var (Cfg, Root) in InResolutions)
            {
                foreach (var (Name, Resolution) in Root.Modules)
                {
                    string IntPath = Path.Combine(Resolution.GetGeneratedIntermediatesPath(Cfg), $"Definitions.{Name}.h");
                    string Content = "";
                    foreach (string Definition in Resolution.Definitions)
                    {
                        int ValueSet = Definition.IndexOf('=');
                        if (ValueSet == -1)
                        {
                            Content += $"#define {Definition}\n";
                        }
                        else
                        {
                            Content += $"#define {Definition.Substring(0, ValueSet)} {Definition.Substring(ValueSet + 1, Definition.Length - ValueSet - 1)}\n";
                        }
                    }
                    Directory.CreateDirectory(Resolution.GetGeneratedIntermediatesPath(Cfg));
                    File.WriteAllText(IntPath, Content);
                }
            }
        }
    }
}
