// Copyright (C) 2026 ychgen, all rights reserved.

using System.ComponentModel;
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
        public override int Execute(CommandContext context, ValidateSettings settings, CancellationToken cancellationToken)
        {
            Moonquake.BuildOrder.bValidationMode  = true;
            Moonquake.BuildOrder.bDisableIncludes = settings.bIncludeDisable;

            string Filepath = settings.Input;
            if (settings.bDirectory)
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
                    Console.WriteLine($"Couldn't construct eventual path from given bDirectory path input '{settings.Input}'.");
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

            DSL.Parser  Parser  = new DSL.Parser(new DSL.Lexer(Filepath, FileContent));
            DSL.Visitor Visitor = new DSL.Visitor(Filepath);

            DSL.CompoundAST SourceRoot = Parser.Parse();
            Visitor.Visit(SourceRoot);
            
            Console.WriteLine($"This description file is valid (with includes {(settings.bIncludeDisable ? "disabled" : "enabled")}).");
            Moonquake.BuildOrder.bValidationMode = false;
            Moonquake.BuildOrder.bDisableIncludes = false;
            return 0;
        }
    }
}
