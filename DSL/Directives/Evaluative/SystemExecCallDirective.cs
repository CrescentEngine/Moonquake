// Copyright (C) 2026 ychgen, all rights reserved.

using System.Diagnostics;

namespace Moonquake.DSL.Directives
{
    public class SystemExecCallDirective : SystemCallDirective
    {
        public SystemExecCallDirective() : base()
        {
        }
        protected override string EvaluteProcessName(string InProcessName) => Moonquake.BuildOrder.Platform switch
        {
            Platforms.Windows => InProcessName + ".exe",
            _ => InProcessName
        };
    }
}
