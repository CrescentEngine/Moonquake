// Copyright (C) 2026 ychgen, all rights reserved.

using System.Collections.Immutable;
using System.Text;

namespace Moonquake.DSL
{
    enum EvaluationContext
    {
        GlobalScope,
        RootScope,
        SchemaScope,
        ModuleScope
    }
    struct VisitorContext
    {
        public EvaluationContext    EvalContext = EvaluationContext.GlobalScope;
        public Construct.Construct? Construct   = null;

        public VisitorContext()
        {
        }
    }

    enum DirectiveResult
    {
        // Executed successfully
        Successful,
        // Directive internal execution failed
        Failure,
        // No such overload exists based on given parameters
        InvalidOverload,
        // Directive functionality not yet implemented within BuildTool
        Unimplemented
    }
    struct DirectiveHandler
    {
        public Dictionary<ImmutableArray<ASTType>, Func<DirectiveAST, DirectiveResult>> Overloads;
        public EvaluationContext[] ValidContexts;
        public bool Bodily;

        public DirectiveHandler(Dictionary<ImmutableArray<ASTType>, Func<DirectiveAST, DirectiveResult>> InOverloads, EvaluationContext[] InValidContexts, bool InBodily)
        {
            Overloads = InOverloads;
            ValidContexts = InValidContexts;
            Bodily = InBodily;
        }
    }
    class DirectiveBuilder
    {
        private readonly Dictionary<ImmutableArray<ASTType>, Func<DirectiveAST, DirectiveResult>> _overloads = new(new AstTypeArrayComparer());
        private readonly List<EvaluationContext> _contexts = new();
        private bool _bodily = false;

        public DirectiveBuilder On(Func<DirectiveAST, DirectiveResult> fn, params ASTType[] args)
        {
            _overloads[ImmutableArray.CreateRange(args)] = fn;
            return this;
        }

        public DirectiveBuilder In(params EvaluationContext[] contexts)
        {
            _contexts.AddRange(contexts);
            return this;
        }

        public DirectiveBuilder Bodily()
        {
            _bodily = true;
            return this;
        }

        public DirectiveHandler Build() => new(_overloads, _contexts.ToArray(), _bodily);
    }

    public class Visitor
    {
        /// <summary>
        /// Context is only singular and not a stack because MQBDL doesn't support nested constructs.
        /// Maybe update in the future if nested constructs are added but it's highly unlikely.
        /// </summary>
        private VisitorContext       Context = new VisitorContext();
        private List<Construct.Root> Roots   = new List<Construct.Root>();
        private Dictionary<string, DirectiveHandler> DirectiveDict = new();
        private LanguageVersion DeclaredVersion = LanguageVersion.Invalid;

        public Visitor()
        {
            DirectiveDict[Directives.DECLARE_VERSION] =
            new DirectiveBuilder().On(Directive_DeclVersion, ASTType.String).In(EvaluationContext.GlobalScope).Build();

            DirectiveDict[Directives.DECLARE_ROOT] =
            new DirectiveBuilder().On(Directive_DeclRoot, ASTType.String).In(EvaluationContext.GlobalScope).Bodily().Build();

            DirectiveDict[Directives.DECLARE_SCHEMA] =
            new DirectiveBuilder().On(Directive_DeclSchema_Dflt, ASTType.String)
                                  .On(Directive_DeclSchema_Inherit, ASTType.String, ASTType.String)
                                  .In(EvaluationContext.GlobalScope).Bodily().Build();

            DirectiveDict[Directives.DECLARE_MODULE] =
            new DirectiveBuilder().On(Directive_DeclModule_Dflt, ASTType.String)
                                  .On(Directive_DeclModule_Schemed, ASTType.String, ASTType.String)
                                  .In(EvaluationContext.GlobalScope).Bodily().Build();
        }

        // Call this after you call Visit(MasterCompoundAST) to acquire roots and every other construct that contains every information.
        public List<Construct.Root> GetRoots() => Roots;

        /// <summary>
        /// Resets the Visitor into a clean state.
        /// </summary>
        public void Reset()
        {
            Context.EvalContext = EvaluationContext.GlobalScope;
            Context.Construct = null;
            Roots.Clear();
        }

        public void Visit(AST Node)
        {
            switch (Node.Type)
            {
            case ASTType.Compound:
            {
                VisitCompound((CompoundAST) Node);
                break;
            }
            case ASTType.String:
            {
                VisitString((StringAST) Node);
                break;
            }
            case ASTType.Directive:
            {
                VisitDirective((DirectiveAST) Node);
                break;
            }
            case ASTType.FieldAssignment:
            {
                VisitFieldAssignment((FieldAssignmentAST) Node);
                break;
            }
            case ASTType.FieldAppendment:
            {
                VisitFieldAppendment((FieldAppendmentAST) Node);
                break;
            }
            case ASTType.FieldErasure:
            {
                VisitFieldErasure((FieldErasureAST) Node);
                break;
            }
            case ASTType.FieldUnassignment:
            {
                VisitFieldUnassignment((FieldUnassignmentAST) Node);
                break;
            }
            case ASTType.Array:
            {
                VisitArray((ArrayAST) Node);
                break;
            }
            }
        }

        private void VisitCompound(CompoundAST Node)
        {
            foreach (AST Subnode in Node.Compound)
            {
                Visit(Subnode);
            }
        }

        private void VisitString(StringAST Node)
        {
            // We swear by God don't touch shit if Resolved already contains anything, because it means it was resolved or hacked around for a good reason.
            // Parser never populates Resolved anyway, it only populates Literal.
            if (Node.Resolved != "")
            {
                return;
            }
            StringBuilder Builder = new StringBuilder(Node.Literal);

            if (Context.Construct is not null)
            {
                switch (Context.EvalContext)
                {
                case EvaluationContext.ModuleScope:
                {
                    Construct.Module Mod = (Construct.Module) Context.Construct;

                    Builder.Replace("%ModuleName%", Mod.Name.Resolved);
                    Builder.Replace("%ModulePath%", Mod.Path.Resolved);
                    Builder.Replace("%Configuration%", Moonquake.BuildOrder.Configuration);
                    Builder.Replace("%Platform%", Moonquake.BuildOrder.Platform.ToString());
                    Builder.Replace("%Architecture%", Moonquake.BuildOrder.Architecture.ToString());
                    
                    break;
                }
                default:
                {
                    break;
                }
                }
            }

            Node.Resolved = Builder.ToString();
        }

        private void VisitDirective(DirectiveAST Node)
        {
            if (DeclaredVersion == LanguageVersion.Invalid)
            {
                if (Node.DirectiveName != Directives.DECLARE_VERSION)
                {
                    Console.WriteLine(
                        $"Visitor.Visit() error: The first statement to be made within a MQBDL file must be a {Directives.DECLARE_VERSION}() directive."
                    );
                    Environment.Exit(1);
                }
            }

            ASTType[] OverloadRequested = new ASTType[Node.Parameters.Compound.Count];
            for (int i = 0; i < Node.Parameters.Compound.Count; i++)
            {
                AST Parameter = Node.Parameters.Compound[i];
                OverloadRequested[i] = Parameter.Type;
                Visit(Parameter);
            }

            if (!DirectiveDict.ContainsKey(Node.DirectiveName))
            {
                Console.WriteLine($"Visitor.VisitDirective() error: Visiting directive named '{Node.DirectiveName}', but no such directive exists at all.");
                Environment.Exit(1);
            }
            DirectiveHandler Handler = DirectiveDict[Node.DirectiveName];

            if (!Handler.ValidContexts.Contains(Context.EvalContext))
            {
                Console.WriteLine($"Visitor.VisitDirective() error: Directive '{Node.DirectiveName}' cannot be executed within a {Context.EvalContext} context.");
                Environment.Exit(1);
            }

            if (Handler.Bodily)
            {
                if (Node.Body is null)
                {
                    Console.WriteLine($"Visitor.VisitDirective() errror: Directive '{Node.DirectiveName}' cannot have a body.");
                    Environment.Exit(1);
                }
            }
            else if (Node.Body is not null)
            {
                Console.WriteLine($"Visitor.VisitDirective() errror: Directive '{Node.DirectiveName}' must have a body.");
                Environment.Exit(1);
            }

            DirectiveResult Result = DirectiveResult.InvalidOverload;
            if (Handler.Overloads.ContainsKey(OverloadRequested.ToImmutableArray())) // TODO: Improve writing, I am not in the mood rn.
            {
                Result = Handler.Overloads[OverloadRequested.ToImmutableArray()](Node);
            }
            
            if (Result != DirectiveResult.Successful)
            {
                Console.WriteLine($"Visitor.VisitDirective() error: Directive '{Node.DirectiveName}' failed with error code '{Result}'.");
                Environment.Exit(1);
            }
        }

        private void VisitFieldAssignment(FieldAssignmentAST Node)
        {
            
        }

        private void VisitFieldAppendment(FieldAppendmentAST Node)
        {
            
        }

        private void VisitFieldErasure(FieldErasureAST Node)
        {
            
        }

        private void VisitFieldUnassignment(FieldUnassignmentAST Node)
        {
            
        }

        private void VisitArray(ArrayAST Node)
        {
            foreach (StringAST String in Node.Value)
            {
                Visit(String);
            }
        }

        private DirectiveResult Directive_DeclVersion(DirectiveAST Directive)
        {
            if (DeclaredVersion != LanguageVersion.Invalid)
            {
                Console.WriteLine($"DeclVersion() directive error: Language version was already declared as {DeclaredVersion} before.");
                return DirectiveResult.Failure;
            }

            string VersionString = ((StringAST) Directive.Parameters.Compound[0]).Resolved;
            try
            {
                DeclaredVersion = LanguageVersion.FromString(VersionString);
            }
            catch
            {
                Console.WriteLine($"DeclVersion() directive error: Language version string '{VersionString}' couldn't be parsed.");
                return DirectiveResult.Failure;
            }

            return DirectiveResult.Successful;
        }

        private DirectiveResult Directive_DeclRoot(DirectiveAST Directive)
        {
            return DirectiveResult.Unimplemented;
        }

        private DirectiveResult Directive_DeclSchema_Dflt(DirectiveAST Directive)
        {
            return DirectiveResult.Unimplemented;
        }

        private DirectiveResult Directive_DeclSchema_Inherit(DirectiveAST Directive)
        {
            return DirectiveResult.Unimplemented;
        }

        private DirectiveResult Directive_DeclModule_Dflt(DirectiveAST Directive)
        {
            return DirectiveResult.Unimplemented;
        }

        private DirectiveResult Directive_DeclModule_Schemed(DirectiveAST Directive)
        {
            return DirectiveResult.Unimplemented;
        }
    }
}
