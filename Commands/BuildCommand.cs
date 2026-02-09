// Copyright (C) 2026 ychgen, all rights reserved.

using System.ComponentModel;
using Spectre.Console.Cli;

namespace Moonquake.Commands
{
    class BuildSettings : CommandSettings
    {
        [CommandArgument(0, "<input>")]
        [Description("Input description file")]
        public string Input { get; set; } = "";
    }

    class BuildCommand : Command<BuildSettings>
    {
        public override int Execute(CommandContext context, BuildSettings settings, CancellationToken cancellationToken)
        {
            return 0;
        }
    }
}
