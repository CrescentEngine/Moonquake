// Copyright (C) 2026 ychgen, all rights reserved.

using System.ComponentModel;
using System.Runtime.InteropServices;
using Moonquake.DSL;
using Moonquake.DSL.Constructs;
using Moonquake.Orchestra;
using Spectre.Console.Cli;

namespace Moonquake.Commands
{
    class BuildSettings : CommandSettings
    {
        [CommandArgument(0, "<input>")]
        [Description("Input description file")]
        public string Input { get; set; } = "";
        
        [CommandArgument(1, "<root>")]
        [Description("Root to build, must be eventually declared in <input> or its includes")]
        public string Root { get; set; } = "";
        
        [CommandArgument(2, "<config>")]
        [Description("Build configuration, must have a matching value within <root>.Configurations")]
        public string Config { get; set; } = "";
        
        [CommandOption("-d|--directory")]
        [Description("Specifies that the given <input> is a directory, and the actual filepath is '<input>/<input>.mqroot'")]
        public bool bDirectory { get; set; } = false;
    }

    class BuildCommand : Command<BuildSettings>
    {
        public override int Execute(CommandContext Context, BuildSettings Settings, CancellationToken CancellationToken)
        {
            Moonquake.BuildOrder.Configuration = Settings.Config;

            switch (RuntimeInformation.OSArchitecture)
            {
            case Architecture.X86:
            {
                Moonquake.BuildOrder.Architecture = Architectures.x86;
                break;
            }
            case Architecture.X64:
            {
                Moonquake.BuildOrder.Architecture = Architectures.x64;
                break;
            }
            case Architecture.Arm64:
            {
                Moonquake.BuildOrder.Architecture = Architectures.ARM64;
                break;
            }
            default:
            {
                Console.WriteLine($"Moonquake does not support architecture '{RuntimeInformation.OSDescription}'.");
                return 1;
            }
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Moonquake.BuildOrder.Platform = Platforms.Windows;
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                Moonquake.BuildOrder.Platform = Platforms.Windows;
            }
            else
            {
                Console.WriteLine($"Moonquake does not support platform '{RuntimeInformation.OSDescription}'.");
                return 0;
            }

            string Filepath = Settings.Input;
            if (Settings.bDirectory)
            {
                if (!Directory.Exists(Filepath))
                {
                    Console.WriteLine($"No such directory as '{Filepath}' exists.");
                    return 1;
                }
                try
                {
                    Filepath = Path.Combine(Filepath, Path.GetFileName(Filepath.TrimEnd(Path.DirectorySeparatorChar))) + ".mqroot";
                }
                catch
                {
                    Console.WriteLine($"Couldn't construct eventual path from given bDirectory path input '{Settings.Input}'.");
                    return 1;
                }
            }
            if (!File.Exists(Filepath))
            {
                Console.WriteLine($"Description file '{Filepath}' doesn't exist.");
                return 1;
            }
            
            string FileContent;
            try
            {
                FileContent = File.ReadAllText(Filepath);
            }
            catch
            {
                Console.WriteLine($"Failed to read description file '{Filepath}'.");
                return 1;
            }

            // Get absolute path.
            Filepath = Path.GetFullPath(Filepath);
            // Switch working directory.
            Directory.SetCurrentDirectory(Path.GetDirectoryName(Filepath)!);

            Parser SrcParser;
            CompoundAST SourceRoot;
            Visitor SrcVisitor;
            try
            {
                SrcParser = new Parser(new DSL.Lexer(Filepath, FileContent));
                SourceRoot = SrcParser.Parse();
            }
            catch
            {
                Console.WriteLine($"Parser failed, cannot continue execution without a valid source of truth.");
                return 1;
            }
            try
            {
                SrcVisitor = new Visitor(Filepath);
                SrcVisitor.Visit(SourceRoot);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Visitor runtime execution failed, cannot continue build with a corrupt execution world. {e}");
                return 1;
            }

            Root? RootToBuild;
            if (!SrcVisitor.Context.Roots.TryGetValue(Settings.Root, out RootToBuild))
            {
                Console.WriteLine($"No such root as '{Settings.Root}' was declared at any time during the execution of '{Filepath}'.");
                return 1;
            }

            if (!RootToBuild.Arr(RootFieldNames.CONFIGS).Contains(Settings.Config))
            {
                Console.WriteLine($"Root to build '{Settings.Root}' has no such configuration as '{Settings.Config}'.");
                return 1;
            }
            if (!RootToBuild.Arr(RootFieldNames.PLATFORMS).Contains(Moonquake.BuildOrder.Platform.ToString()))
            {
                Console.WriteLine($"Root to build '{Settings.Root}' doesn't support platform '{Moonquake.BuildOrder.Platform}'.");
                return 1;
            }
            if (!RootToBuild.Arr(RootFieldNames.ARCHS).Contains(Moonquake.BuildOrder.Architecture.ToString()))
            {
                Console.WriteLine($"Root to build '{Settings.Root}' doesn't support architecture '{Moonquake.BuildOrder.Architecture}'.");
                return 1;
            }

            try
            {
                SrcVisitor.Context.ValidateRootReferences(Settings.Root);
            }
            catch
            {
                Console.WriteLine($"References of root to build '{Settings.Root}' could not be validated.");
                return 1;
            }

            BuildRoot FinalRoot = BuildFinalizer.FinalizeRoot(SrcVisitor.Context, RootToBuild);
            BuildGraph Graph;
            try
            {
                Graph = new BuildGraph(FinalRoot);
            }
            catch
            {
                Console.WriteLine($"Couldn't construct a build graph for root '{Settings.Root}'.");
                return 1;
            }

            try
            {
                Graph.Build();
            }
            catch (Exception e)
            {
                Console.WriteLine($"Build for root '{Settings.Root}' failed: {e}");
            }

            return 0;
        }
    }
}
