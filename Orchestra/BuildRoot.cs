// Copyright (C) 2026 ychgen, all rights reserved.

namespace Moonquake.Orchestra
{
    /// <summary>
    /// Constructs.Root finalizes into this structural form.
    /// </summary>
    public class BuildRoot
    {
        // Future idea where Roots might depend on each other.
        // public List<BuildRoot> Dependencies = new();
        public string Name = "";
        public string RootPath = "";

        public string[] Configurations = [];
        public Architectures[] Architectures = [];
        public Platforms[] Platforms = [];

        public Dictionary<string, BuildModule> Modules = new();
        public BuildModule? MainModule;

        public string BuildCommand   = "";
        public string ReBuildCommand = "";
        public string CleanCommand   = "";
    }
}
