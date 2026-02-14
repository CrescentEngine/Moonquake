// Copyright (C) 2026 ychgen, all rights reserved.

using System.ComponentModel;
using Moonquake.CodeGen;
using Moonquake.DSL;
using Moonquake.DSL.Constructs;
using Moonquake.Orchestra;
using Spectre.Console.Cli;

namespace Moonquake.Commands
{
    class CleanSettings : CommandSettings
    {
        [CommandArgument(0, "<input>")]
        [Description("Input description file")]
        public string Input { get; set; } = "";
        
        [CommandArgument(1, "<root>")]
        [Description("Which root to clean up")]
        public string Root { get; set; } = "";
        
        [CommandOption("-d|--directory")]
        [Description("Specifies that the given <input> is a directory, and the actual filepath is '<input>/<input>.mqroot'")]
        public bool bDirectory { get; set; } = false;
    }

    class CleanCommand : Command<CleanSettings>
    {
        public override int Execute(CommandContext Context, CleanSettings Settings, CancellationToken CancellationToken)
        {
            // See deeper into the function to see what these affect.
            Moonquake.BuildOrder.RootGenerateTarget = Settings.Root;
            Moonquake.BuildOrder.Type = BuildType.Clean;
            
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
                Console.WriteLine($"Visitor runtime execution failed, this description file is not valid and therefore project files cannot be cleaned. {e.Message}");
                return 1;
            }

            // Because at the start of the function we set BuildOrder.Type = Generate, DeclareRootDirective checks the name of any root
            // declared after recording it and if the name matches BuildOrder.RootGenerateTarget, it populates Moonquake.CanonicalRoot
            // with the declared root and immediately terminates the ExecutionContext, not interpreting more than needed information.
            // Visitor returns after seeing the environment was terminated and this code gets executed.
            if (Moonquake.CanonicalRoot is null)
            {
                Console.WriteLine($"During the entirety of the execution of '{Filepath}', no canonical root such as '{Settings.Root}' was ever declared.");
                return 1;
            }
            // Now that we got canonical root, unset TargetRootName so no premature return from DeclRoot happens.
            Moonquake.BuildOrder.RootGenerateTarget = null;

            Dictionary<TestBuildConfig, BuildRoot> Resolutions;
            try
            {
                Resolutions = CfgResolver.ResolveForAll(SrcVisitor.Context, Moonquake.CanonicalRoot);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Couldn't resolve build configurations for canonical root '{Settings.Root}'. {e.Message}");
                return 1;
            }

            foreach (var (Cfg, RootResolution) in Resolutions)
            {
                foreach (var (Name, ModRes) in RootResolution.Modules)
                {
                    string GenIntPath = ModRes.GetGeneratedIntermediatesPath(Cfg);
                    try
                    {
                        Directory.Delete(ModRes.OutputPath, true);
                        Directory.Delete(ModRes.ObjectPath, true);
                        Directory.Delete(GenIntPath, true);
                    }
                    catch // Do nothing
                    {
                    }
                }
            }

            return 0;
        }
    }
}
