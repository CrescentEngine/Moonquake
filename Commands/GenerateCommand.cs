// Copyright (C) 2026 ychgen, all rights reserved.

using System.ComponentModel;
using Moonquake.CodeGen;
using Moonquake.DSL;
using Moonquake.DSL.Constructs;
using Moonquake.Orchestra;
using Spectre.Console.Cli;

namespace Moonquake.Commands
{
    class GenerateSettings : CommandSettings
    {
        [CommandArgument(0, "<for>")]
        [Description("Which sort of project files to generate, must be either \"VisualStudio\" or \"Makefile\".")]
        public string For { get; set; } = "";

        [CommandArgument(1, "<input>")]
        [Description("Input description file")]
        public string Input { get; set; } = "";
        
        [CommandArgument(2, "<root>")]
        [Description("Root to generate project files of, must be eventually declared in <input> or its includes")]
        public string Root { get; set; } = "";
        
        [CommandOption("-d|--directory")]
        [Description("Specifies that the given <input> is a directory, and the actual filepath is '<input>/<input>.mqroot'")]
        public bool bDirectory { get; set; } = false;
    }

    class GenerateCommand : Command<GenerateSettings>
    {
        public static Root? CanonicalRoot = null;

        public override int Execute(CommandContext Context, GenerateSettings Settings, CancellationToken CancellationToken)
        {
            // See deeper into the function to see what these affect.
            Moonquake.BuildOrder.RootGenerateTarget = Settings.Root;
            Moonquake.BuildOrder.Type = BuildType.Generate;
            
            ProjectFileGenerator Generator;
            switch (Settings.For)
            {
            case "VisualStudio":
            {
                Generator = new VisualStudioProjectFileGenerator();
                break;
            }
            case "Makefile":
            {
                Generator = new MakefileProjectFileGenerator();
                break;
            }
            default:
            {
                Console.WriteLine($"Cannot generate project files for '{Settings.For}', invalid value provided.");
                return 1;
            }
            }

            string Filepath;
            CompoundAST? SourceRoot = DescriptionFileInputHelper.Help(Settings.Input, Settings.bDirectory, out Filepath);
            if (SourceRoot is null) return 1;

            // create before visiting, important because we must contain original unresolved copy of src.
            ConfigurationResolver CfgResolver = new ConfigurationResolver(Filepath, SourceRoot);

            Visitor SrcVisitor;
            try
            {
                SrcVisitor = new Visitor(Filepath);
                SrcVisitor.Visit(SourceRoot);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Visitor runtime execution failed, this description file is not valid and therefore project files cannot be generated. {e.Message}");
                return 1;
            }

            // Because at the start of the function we set BuildOrder.Type = Generate, DeclareRootDirective checks the name of any root
            // declared after recording it and if the name matches BuildOrder.RootGenerateTarget, it populates GenerateCommand.CanonicalRoot
            // with the declared root and immediately terminates the ExecutionContext, not interpreting more than needed information.
            // Visitor returns after seeing the environment was terminated and this code gets executed.
            if (CanonicalRoot is null)
            {
                Console.WriteLine($"During the entirety of the execution of '{Filepath}', no canonical root such as '{Settings.Root}' was ever declared.");
                return 1;
            }
            // Now that we got canonical root, we return to normal build so DeclRoot never prematurely returns.
            Moonquake.BuildOrder.Type = BuildType.Build;

            Dictionary<TestBuildConfig, BuildRoot> Resolutions;
            try
            {
                Resolutions = CfgResolver.ResolveForAll(SrcVisitor.Context, CanonicalRoot);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Couldn't resolve build configurations for canonical root '{Settings.Root}'. {e.Message}");
                return 1;
            }

            try
            {
                Generator.Generate(Resolutions);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Couldn't generate project files for canonical root '{Settings.Root}'. {e.Message}");
                return 1;
            }

            return 0;
        }
    }
}
