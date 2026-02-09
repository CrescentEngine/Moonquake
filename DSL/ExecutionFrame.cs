// Copyright (C) 2026 ychgen, all rights reserved.

namespace Moonquake.DSL
{
    public enum EvaluationContext
    {
        GlobalScope,
        RootScope,
        SchemaScope,
        ModuleScope,
        DeferredScope
    }
    public struct ExecutionFrame
    {
        public EvaluationContext     EvalContext = EvaluationContext.GlobalScope;
        public Constructs.Construct? Construct   = null;

        public ExecutionFrame()
        {
        }
        public ExecutionFrame(EvaluationContext InEvalContext, Constructs.Construct? InConstruct)
        {
            EvalContext = InEvalContext;
            Construct   = InConstruct;
        }
    }
}
