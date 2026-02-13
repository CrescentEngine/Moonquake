// Copyright (C) 2026 ychgen, all rights reserved.

using System.Runtime.InteropServices;
using Moonquake.Toolchain;
using Spectre.Console.Cli;

namespace Moonquake
{
    public enum Architectures
    {
        x64,
        ARM64,
        x86
    }
    public enum Platforms
    {
        Windows,
        Linux
    }

    public enum BuildType
    {
        Build,
        Validate,
        Generate
    }
    public struct BuildOrder
    {
        public string Root;
        public string Configuration;
        public Architectures Architecture;
        public Platforms Platform;

        public BuildType Type;
        public bool bDisableIncludes;
        public string? RootGenerateTarget;

        public BuildOrder Clone()
        {
            return new BuildOrder
            {
                Root = Root,
                Configuration = Configuration,
                Architecture = Architecture,
                Platform = Platform,
                Type = Type,
                bDisableIncludes = bDisableIncludes,
                RootGenerateTarget = RootGenerateTarget
            };
        }
    }

    public static class Moonquake
    {
        public static BuildOrder BuildOrder = new BuildOrder();
        
        private static int Main(string[] Arguments)
        {
            // TODO: This needs total revamp but simple for now
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Backend.Instance = new VCBackend();
            }
            else
            {
                Console.WriteLine("You're hosted on an unsupported platform. Moonquake only works with Windows.");
                return 1;
            }

            CommandApp App = new CommandApp();
            App.Configure(Config =>
            {
                Config.AddCommand<Commands.ValidateCommand>("validate")
                      .WithDescription("Validates if a description file is valid by syntax and by statements relative to their context. In this mode every conditional directive is entered unconditionally to check for errors.");
                
                Config.AddCommand<Commands.BuildCommand>("build")
                      .WithDescription("Builds the Root with given name after interpreting the given description file.");
                      
                Config.AddCommand<Commands.GenerateCommand>("generate")
                      .WithDescription("Generates project files for the Root with given name after interpreting the given description file.");
            });
            return App.Run(Arguments);
        }
    }
}
