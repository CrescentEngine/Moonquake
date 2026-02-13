// Copyright (C) 2026 ychgen, all rights reserved.

using Moonquake.Orchestra;

namespace Moonquake.CodeGen
{
    public class MakefileProjectFileGenerator : ProjectFileGenerator
    {
        public override ProjectFilesType GetProjectFilesType() => ProjectFilesType.Makefile;
        public override void Generate(Dictionary<TestBuildConfig, BuildRoot> Resolutions)
        {
            
        }
    }
}
