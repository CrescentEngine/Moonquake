// Copyright (C) 2026 ychgen, all rights reserved.

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

    public struct BuildOrder
    {
        public string Root;
        public string Configuration;
        public Architectures Architecture;
        public Platforms Platform;

        public bool bValidationMode;
        public bool bDisableIncludes;
    }

    public static class Moonquake
    {
        public static BuildOrder BuildOrder = new BuildOrder();
        
        private static int Main(string[] Arguments)
        {
            CommandApp App = new CommandApp();
            App.Configure(Config =>
            {
                Config.AddCommand<Commands.ValidateCommand>("validate")
                      .WithDescription("Validates if a description file is valid by syntax and by statements relative to their context. In this mode every conditional directive is entered unconditionally to check for errors.");
                
                Config.AddCommand<Commands.BuildCommand>("build")
                      .WithDescription("Builds the Root with given name after interpreting the given description file.");
            });
            return App.Run(Arguments);
        }
    }
}
