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
    }
}
