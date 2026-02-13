// Copyright (C) 2026 ychgen, all rights reserved.

using System.ComponentModel;
using Moonquake.DSL;
using Spectre.Console.Cli;

namespace Moonquake.Commands
{
    class ValidateSettings : CommandSettings
    {
        [CommandArgument(0, "<input>")]
        [Description("Input description file.")]
        public string Input { get; set; } = "";
        
        [CommandOption("-d|--directory")]
        [Description("Specifies that the given <input> is a directory, and the actual filepath is '<input>/<input>.mqroot'")]
        public bool bDirectory { get; set; } = false;

        [CommandOption("-i|--include-disable")]
        [Description("Converts Include() directives to no-ops, so that only the given description file is validated.")]
        public bool bIncludeDisable { get; set; } = false;
    }

    class ValidateCommand : Command<ValidateSettings>
    {
        public override int Execute(CommandContext Context, ValidateSettings Settings, CancellationToken CancellationToken)
        {
            Moonquake.BuildOrder.Type = BuildType.Validate;
            Moonquake.BuildOrder.bDisableIncludes = Settings.bIncludeDisable;

            string Filepath;
            CompoundAST? SourceRoot = DescriptionFileInputHelper.Help(Settings.Input, Settings.bDirectory, out Filepath);
            if (SourceRoot is null) return 1;

            Visitor SrcVisitor;
            try
            {
                SrcVisitor = new Visitor(Filepath);
                SrcVisitor.Visit(SourceRoot);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Visitor runtime execution failed, this description file is not valid. {e}");
                return 1;
            }
            
            Console.WriteLine($"This description file is valid (with includes {(Settings.bIncludeDisable ? "disabled" : "enabled")}).");
            return 0;
        }
    }
}
